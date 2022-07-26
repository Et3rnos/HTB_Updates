using Discord;
using Discord.Commands;
using Discord.WebSocket;
using HTB_Updates_Shared_Resources;
using HTB_Updates_Shared_Resources.Models.Database;
using HTB_Updates_Discord_Bot.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace HTB_Updates_Discord_Bot.Modules
{
    [Name("general")]
    public class GeneralModule : ModuleBase<SocketCommandContext>
    {
        private readonly DiscordSocketClient _client;
        private readonly CommandService _commands;
        private readonly DatabaseContext _context;
        private readonly IHTBApiV4Service _htbApiV4Service;
        private readonly IHTBApiV1Service _htbApiV1Service;

        private GeneralModule(DiscordSocketClient client, CommandService commands, DatabaseContext context, IHTBApiV4Service htbApiV4Service, IHTBApiV1Service htbApiV1Service)
        {
            _client = client;
            _commands = commands;
            _context = context;
            _htbApiV4Service = htbApiV4Service;
            _htbApiV1Service = htbApiV1Service;
        }

        [Command("link")]
        [Summary("Links the referred HTB account to your discord one")]
        [RequireContext(ContextType.Guild)]
        public async Task Link(string username)
        {
            //Create the embed
            var eb = new EmbedBuilder();
            eb.WithColor(Color.DarkGreen);

            //Check if the guild is set up before proceed
            var dGuild = await _context.DiscordGuilds.AsAsyncEnumerable().Where(x => x.GuildId == Context.Guild.Id).FirstOrDefaultAsync();
            if (dGuild == null)
            {
                eb.WithTitle("Linking Error");
                eb.WithDescription($"{Context.User.Mention}, this server is not set up yet. Ask admins to run `h.setup` first.");
                await ReplyAsync(embed: eb.Build());
                return;
            }

            //Check if the username is valid before proceed
            int htbId = await _htbApiV1Service.GetHTBUserIdByName(username);
            if (htbId == -1)
            {
                eb.WithTitle("Linking Error");
                eb.WithDescription($"{Context.User.Mention}, there is no HTB user with that name.");
                await ReplyAsync(embed: eb.Build());
                return;
            }

            //Send a temporary message
            eb.WithTitle("Linking In Progress");
            eb.WithDescription($"{Context.User.Mention}, you are being linked to the HTB user \"{username}\".\nThis shouldn't take more than 5 seconds to complete.");
            var msg = await ReplyAsync(embed: eb.Build());

            //Create HTB user if not exists
            var htbUser = await _context.HTBUsers.AsQueryable().FirstOrDefaultAsync(x => x.HtbId == htbId);
            if (htbUser == null)
            {
                htbUser = new HTBUser();

                var solves = await _htbApiV4Service.GetSolves(htbId);

                htbUser.HtbId = htbId;
                htbUser.Username = await _htbApiV4Service.GetUserNameById(htbId);
                htbUser.Solves = solves;
                htbUser.Score = solves.Sum(x => x.Points);
                htbUser.LastUpdated = DateTime.UtcNow;

                await _context.HTBUsers.AddAsync(htbUser);
            }

            //Get the current discord user if exists
            ulong discordId = Context.User.Id;
            ulong guildId = Context.Guild.Id;
            var dUser = await _context.DiscordUsers.AsQueryable().FirstOrDefaultAsync(x => x.DiscordId == discordId && x.Guild.GuildId == guildId);

            if (dUser == null)
            {
                //Create a new discord user
                dUser = new DiscordUser { DiscordId = discordId, Guild = dGuild, HTBUser = htbUser };
                await _context.DiscordUsers.AddAsync(dUser);
            }
            else
            {
                //Update the current discord user
                await _context.Entry(dUser).Reference(x => x.HTBUser).LoadAsync();
                if (dUser.HTBUser != htbUser)
                {
                    var oldHTBUser = dUser.HTBUser;
                    dUser.HTBUser = htbUser;
                    dUser.Verified = false;
                }
            }

            //Clean unlinked htb users
            var htbUsers = await _context.HTBUsers.Where(x => !x.DiscordUsers.Any()).ToListAsync();
            _context.HTBUsers.RemoveRange(htbUsers);

            await _context.SaveChangesAsync();

            //Update the temporary message with the successful message
            eb.WithTitle("Account Successfully Linked");
            eb.WithDescription($"{Context.User.Mention}, you are now linked to the HTB user named \"{htbUser.Username}\".\nConsider verifying your HTB account by messaging me `h.verify <account_identifier>`.\nPlease note that your new solves may take up to 10 minutes to be announced.");
            await msg.ModifyAsync(x => x.Embed = eb.Build());
        }

        [Command("verify")]
        [Summary("Verifies your HTB account")]
        [RequireContext(ContextType.DM)]
        public async Task Verify(string accountId)
        {
            //Create the embed
            var eb = new EmbedBuilder();
            eb.WithColor(Color.DarkGreen);

            var htbId = await _htbApiV1Service.GetHTBIdByAccountId(accountId);

            if (htbId == -1)
            {
                eb.WithTitle("Verify Error");
                eb.WithDescription($"<@{Context.User.Id}>, looks like the account identifier you provided is invalid.");
                await ReplyAsync(embed: eb.Build());
                return;
            }

            var dUsers = await _context.DiscordUsers.AsQueryable()
                .Where(x => x.DiscordId == Context.User.Id && x.HTBUser.HtbId == htbId && x.Verified == false)
                .ToListAsync();

            if (!dUsers.Any())
            {
                eb.WithTitle("Verify Error");
                eb.WithDescription($"<@{Context.User.Id}>, looks like you are not linked to that HTB account in any server or you already verified it.");
                await ReplyAsync(embed: eb.Build());
                return;
            }

            foreach (var dUser in dUsers)
            {
                dUser.Verified = true;
            }

            await _context.SaveChangesAsync();

            eb.WithTitle("Verification Succeeded");
            eb.WithDescription($"<@{Context.User.Id}>, you successfully verified that HTB account and unlocked new features in {dUsers.Count} servers.");
            await ReplyAsync(embed: eb.Build());
        }

        [Command("unlink")]
        [Summary("Unlinks your HTB account from your discord one")]
        [RequireContext(ContextType.Guild)]
        public async Task Unlink()
        {
            //Create the embed
            EmbedBuilder eb = new EmbedBuilder();
            eb.WithColor(Color.DarkGreen);

            //Get the current discord user if exists
            ulong discordId = Context.User.Id;
            ulong guildId = Context.Guild.Id;
            var dUser = await _context.DiscordUsers.AsQueryable().Include(x => x.HTBUser.DiscordUsers).FirstOrDefaultAsync(x => x.DiscordId == discordId && x.Guild.GuildId == guildId);

            if (dUser != null)
            {
                
                _context.DiscordUsers.Remove(dUser);

                //Clean unlinked htb users
                var htbUsers = await _context.HTBUsers.Where(x => !x.DiscordUsers.Any()).ToListAsync();
                _context.HTBUsers.RemoveRange(htbUsers);

                await _context.SaveChangesAsync();

                //Send the success message
                eb.WithTitle("Account Successfully Unlinked");
                eb.WithDescription($"{Context.User.Mention}, your HTB account was unlinked from your discord one.");
            }
            else
            {
                eb.WithTitle("Unlinking Error");
                eb.WithDescription($"{Context.User.Mention}, there is no HTB user linked to your account. Nothing changed.");
            }

            await ReplyAsync(embed: eb.Build());
        }

        [Command("stats")]
        [Summary("Prints the user's last HTB solves")]
        [RequireContext(ContextType.Guild)]
        public async Task Stats(IUser pingedUser = null)
        {
            if (pingedUser == null)
            {
                pingedUser = Context.User;
            }

            //Create the embed
            var eb = new EmbedBuilder();
            eb.WithColor(Color.DarkGreen);

            //Get the target user if exists
            ulong discordId = pingedUser.Id;
            ulong guildId = Context.Guild.Id;
            var dUser = await _context.DiscordUsers.AsQueryable().Include(x => x.HTBUser.Solves).FirstOrDefaultAsync(x => x.DiscordId == discordId && x.Guild.GuildId == guildId);

            //Send error embed if no discord embed was found
            if (dUser == null)
            {
                eb.WithTitle("Stats Error");
                eb.WithDescription($"{Context.User.Mention}, there is no HTB user linked to {pingedUser.Mention}'s account.");
                await ReplyAsync(embed: eb.Build());
                return;
            }

            //Get most recent solves
            var solves = dUser.HTBUser.Solves.OrderByDescending(x => x.Date).Take(10);

            //Build the embed
            eb.WithColor(Color.DarkGreen);
            eb.WithTitle($"Last HTB solves by {dUser.HTBUser.Username}");
            foreach (var solve in solves)
            {
                eb.Description += $":small_blue_diamond: **{solve.Name}** ({solve.Type}) - {(DateTime.UtcNow - solve.Date).Days} days ago\n";
            }

            await ReplyAsync(embed: eb.Build());
        }

        [Command("solves")]
        [Summary("Prints all the users that solved that challenge/machine")]
        [RequireContext(ContextType.Guild)]
        public async Task Solves([Remainder] string name)
        {
            //Create the embed
            var eb = new EmbedBuilder();
            eb.WithColor(Color.DarkGreen);

            //Validate name
            var solve = await _context.Solves.AsQueryable().FirstOrDefaultAsync(x => x.Name.ToLower() == name.ToLower());

            if (solve == null)
            {
                eb.WithTitle("Solves Error");
                eb.WithDescription($"{Context.User.Mention}, either no user solved the challenge/machine you referred or you provided a wrong name.");
                await ReplyAsync(embed: eb.Build());
                return;
            }

            //Set the title based on the solve type
            if (solve.ObjectType == "challenge")
            {
                eb.WithTitle("The following users solved the challenge you referred");
            }
            else if (solve.ObjectType == "machine")
            {
                eb.WithTitle("The following users submitted a flag for the machine you referred");
            }

            //Get the linked discord users in the guild
            ulong guildId = Context.Guild.Id;
            var dUsers = await _context.DiscordUsers.AsQueryable().Include(x => x.HTBUser.Solves).Include(x => x.Guild).Where(x => x.Guild.GuildId == guildId).ToListAsync();

            var htbUsers = dUsers.Select(x => x.HTBUser).Distinct();

            foreach (var htbUser in htbUsers)
            {
                var solves = htbUser.Solves.Where(x => x.Name.ToLower() == name.ToLower()).ToList();
                var linkedUsers = htbUser.DiscordUsers.Where(x => x.Guild.GuildId == guildId).ToList();
                if (solves.Count == 2 && solves.First().ObjectType == "machine")
                {
                    foreach (var lUser in linkedUsers)
                    {
                        eb.Description += $":small_blue_diamond: <@{lUser.DiscordId}> ({htbUser.Username}) - Root\n";
                    }
                }
                else if (solves.Count == 1 && solves.First().ObjectType == "machine" && solves.First().Type == "root")
                {
                    foreach (var lUser in linkedUsers)
                    {
                        eb.Description += $":small_blue_diamond: <@{lUser.DiscordId}> ({htbUser.Username}) - Root\n";
                    }
                }
                else if (solves.Count == 1 && solves.First().ObjectType == "machine" && solves.First().Type == "user")
                {
                    foreach (var lUser in linkedUsers)
                    {
                        eb.Description += $":small_blue_diamond: <@{lUser.DiscordId}> ({htbUser.Username}) - User\n";
                    }
                }
                else if (solves.Count == 1 && solves.First().ObjectType == "challenge")
                {
                    foreach (var lUser in linkedUsers)
                    {
                        eb.Description += $":small_blue_diamond: <@{lUser.DiscordId}> ({htbUser.Username})\n";
                    }
                }
            }

            if (string.IsNullOrEmpty(eb.Description))
            {
                eb.WithTitle("Solves Error");
                eb.WithDescription($"<@{Context.User.Id}>, looks like no one in this server solved that yet.");
            }

            await ReplyAsync(embed: eb.Build());
        }

        [Command("lb")]
        [Summary("Shows this server leaderboard")]
        [RequireContext(ContextType.Guild)]
        public async Task Leaderboard()
        {
            //Create the embed
            var eb = new EmbedBuilder();
            eb.WithColor(Color.DarkGreen);

            //Check if the guild is set up before proceed
            var dGuild = await _context.DiscordGuilds.Include(x => x.DiscordUsers.OrderByDescending(x => x.HTBUser.Score).Take(10)).ThenInclude(x => x.HTBUser).FirstOrDefaultAsync(x => x.GuildId == Context.Guild.Id);
            if (dGuild == null)
            {
                eb.WithTitle("Leaderboard Error");
                eb.WithDescription($"{Context.User.Mention}, this server is not set up yet. Ask admins to run `h.setup` first.");
                await ReplyAsync(embed: eb.Build());
                return;
            }

            //Say whatever you want, I'm not removing this
            var dUsers = dGuild.DiscordUsers.OrderByDescending(x => x.HTBUser.Score).Take(10).ToList();

            if (!dUsers.Any())
            {
                eb.WithTitle("Leaderboard Error");
                eb.WithDescription($"{Context.User.Mention}, this server has no HTB users yet.\nPlease link your HTB user first.");
                await ReplyAsync(embed: eb.Build());
                return;
            }

            eb.WithTitle("Server's Leaderboard");
            foreach (var dUser in dUsers)
            {
                eb.Description += $":small_blue_diamond: <@{dUser.DiscordId}> ({dUser.HTBUser.Username}) {(dUser.Verified ? ":star: " : "")}- {dUser.HTBUser.Score} points\n";
            }

            await ReplyAsync(embed: eb.Build());
        }

        [Command("lb global")]
        [Summary("Shows the global leaderboard")]
        [RequireContext(ContextType.Guild)]
        public async Task LeaderboardGlobal()
        {
            //Create the embed
            var eb = new EmbedBuilder();
            eb.WithColor(Color.DarkGreen);

            var htbUsers = await _context.HTBUsers.Where(x => x.DiscordUsers.Any(x => x.Verified)).OrderByDescending(x => x.Score).Take(10).ToListAsync();

            eb.WithTitle("Global Leaderboard");
            foreach (var user in htbUsers)
            {
                eb.Description += $":small_blue_diamond: {user.Username} - {user.Score} points\n";
            }

            await ReplyAsync(embed: eb.Build());
        }

    }
}
