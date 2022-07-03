using Discord;
using Discord.Commands;
using Discord.WebSocket;
using HTB_Updates_Discord_Bot.Models;
using HTB_Updates_Discord_Bot.Models.Database;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace HTB_Updates_Discord_Bot.Modules
{
    [Name("extra")]
    public class ExtraModule : ModuleBase<SocketCommandContext>
    {
        private readonly DiscordSocketClient _client;
        private readonly CommandService _commands;
        private readonly DatabaseContext _context;

        private ExtraModule(DiscordSocketClient client, CommandService commands, DatabaseContext context)
        {
            _client = client;
            _commands = commands;
            _context = context;
        }

        [Command("forceunlink")]
        [Summary("Unlinks the HTB account from that discord user")]
        [RequireContext(ContextType.Guild)]
        [RequireOwner(Group = "Permission")]
        [RequireUserPermission(GuildPermission.ManageGuild, Group = "Permission")]
        public async Task ForceUnlink(IUser pingedUser)
        {
            await ForceUnlink(pingedUser.Id);
        }

        [Command("forceunlink")]
        [Summary("Unlinks the HTB account from that discord user")]
        [RequireContext(ContextType.Guild)]
        [RequireOwner(Group = "Permission")]
        [RequireUserPermission(GuildPermission.ManageGuild, Group = "Permission")]
        public async Task ForceUnlink(ulong discordId)
        {
            //Create the embed
            EmbedBuilder eb = new EmbedBuilder();
            eb.WithColor(Color.DarkGreen);

            //Get the pinged discord user if exists
            ulong guildId = Context.Guild.Id;
            var dUser = await _context.DiscordUsers.AsQueryable().Include(x => x.HTBUser.DiscordUsers).FirstOrDefaultAsync(x => x.DiscordId == discordId && x.Guild.GuildId == guildId);

            if (dUser != null)
            {
                if (dUser.HTBUser.DiscordUsers.Count == 1)
                {
                    //Delete the HTB user and the discord one if there is no other discord user linked to that HTB account
                    await _context.Entry(dUser.HTBUser).Collection(x => x.Solves).LoadAsync();
                    _context.HTBUsers.Remove(dUser.HTBUser);
                }
                else
                {
                    //Delete just the discord user
                    _context.DiscordUsers.Remove(dUser);
                }

                await _context.SaveChangesAsync();

                //Send the success message
                eb.WithTitle("Account Successfully Unlinked");
                eb.WithDescription($"{Context.User.Mention}, <@{discordId}>'s HTB account was unlinked from their discord one.");
            }
            else
            {
                eb.WithTitle("Unlinking Error");
                eb.WithDescription($"{Context.User.Mention}, there is no HTB user linked to <@{discordId}>'s account. Nothing changed.");
            }

            await ReplyAsync(embed: eb.Build());
        }

        [Command("setup")]
        [Summary("Sets the channel for HTB updates. You must have Manage Guild permission")]
        [RequireContext(ContextType.Guild)]
        [RequireOwner(Group = "Permission")]
        [RequireUserPermission(GuildPermission.ManageGuild, Group = "Permission")]
        public async Task Setup(IChannel channel = null)
        {
            channel ??= Context.Channel;
            ulong channelId = channel.Id;
            ulong guildId = Context.Guild.Id;

            var guild = await _context.DiscordGuilds.AsAsyncEnumerable().Where(x => x.GuildId == guildId).FirstOrDefaultAsync();
            if (guild == null)
            {
                guild = new DiscordGuild { GuildId = guildId, ChannelId = channelId };
                await _context.DiscordGuilds.AddAsync(guild);
            }
            else
            {
                guild.ChannelId = channelId;
            }

            await _context.SaveChangesAsync();

            await ReplyAsync($"HTB Updates will be announced in <#{channelId}>. Make sure I have Send Messages permission in the referred channel.");
        }

        #region Help

        [Command("help")]
        [Summary("Prints information about available commands")]
        public async Task HelpAsync(string category = null)
        {
            Embed embed;

            if (!string.IsNullOrEmpty(category)) 
                embed = GetCategoryHelpEmbed(_commands, category);
            else 
                embed = GetHelpEmbed(_commands);

            if (embed != null) 
                await ReplyAsync(embed: embed);
            else 
                await ReplyAsync("That's not a valid category name.");
        }

        private Embed GetHelpEmbed(CommandService commandService)
        {
            var eb = new EmbedBuilder();
            eb.WithColor(Color.DarkGreen);
            eb.WithTitle("Available Commands");

            var mainCommandsNames = new string[] { "link" };
            var mainCommands = commandService.Commands.Where(x => mainCommandsNames.Contains(x.Name));

            eb.Description += "\n**Main Commands**";

            foreach (var command in mainCommands)
            {
                eb.Description += $"\n`h.{command.Name}";
                foreach (var parameter in command.Parameters)
                {
                    if (parameter.IsOptional)
                        eb.Description += $" [{parameter.Name}]";
                    else
                        eb.Description += $" <{parameter.Name}>";
                }
                eb.Description += "`";
                var requirePermission = command.Preconditions.OfType<RequireUserPermissionAttribute>().FirstOrDefault();
                if (requirePermission != null)
                {
                    eb.Description += " :star:";
                }
                eb.Description += $"\n➜ {command.Summary}";
                var requireContext = command.Preconditions.OfType<RequireContextAttribute>().FirstOrDefault();
                if (requireContext != null)
                {
                    switch (requireContext.Contexts)
                    {
                        case ContextType.DM:
                            eb.Description += " (DMs only)";
                            break;
                        case ContextType.Guild:
                            eb.Description += " (Guild only)";
                            break;
                    }
                }
            }

            eb.Description += "\n\n**Other Commands**";

            foreach (var module in new string[] { "general", "config", "extra" })
            {
                eb.Description += $"\n`h.help {module}`";
            }

            eb.WithFooter("Starred commands require the user to have certain permissions");

            return eb.Build();
        }

        private Embed GetCategoryHelpEmbed(CommandService commandService, string category)
        {
            var module = commandService.Modules.FirstOrDefault(x => x.Name.ToLower() == category.ToLower());
            if (module == null) return null;

            var eb = new EmbedBuilder();
            eb.WithColor(Color.DarkGreen);
            eb.WithTitle("Available Commands");

            foreach (var command in module.Commands)
            {
                eb.Description += $"\n`h.{command.Name}";
                foreach (var parameter in command.Parameters)
                {
                    if (parameter.IsOptional)
                        eb.Description += $" [{parameter.Name}]";
                    else
                        eb.Description += $" <{parameter.Name}>";
                }
                eb.Description += "`";
                var requirePermission = command.Preconditions.OfType<RequireUserPermissionAttribute>().FirstOrDefault();
                if (requirePermission != null)
                {
                    eb.Description += " :star:";
                }
                eb.Description += $"\n➜ {command.Summary}";
                var requireContext = command.Preconditions.OfType<RequireContextAttribute>().FirstOrDefault();
                if (requireContext != null)
                {
                    switch (requireContext.Contexts)
                    {
                        case ContextType.DM:
                            eb.Description += " (DMs only)";
                            break;
                        case ContextType.Guild:
                            eb.Description += " (Guild only)";
                            break;
                    }
                }
            }

            eb.WithFooter("Starred commands require the user to have certain permissions");

            return eb.Build();
        }

        #endregion
        
        [Command("invite")]
        [Summary("Generates a link that allows you to add this bot to your server")]
        public async Task Invite()
        {
            await ReplyAsync("You can invite me to your server using this link:\n<https://discord.com/api/oauth2/authorize?client_id=806824180074938419&permissions=2048&scope=bot>");
        }

        [Command("about")]
        [Summary("Prints some info about this bot")]
        public async Task About()
        {
            var usersCount = await _context.DiscordUsers.CountAsync();
            var uniqueUsersCount = await _context.HTBUsers.CountAsync();
            var serverMoreLinks = await _context.DiscordGuilds.OrderByDescending(x => x.DiscordUsers.Count).Select(x => new { Guild = x, Count = x.DiscordUsers.Count }).FirstOrDefaultAsync();
            var bestPlayer = await _context.HTBUsers.OrderByDescending(x => x.Score).FirstOrDefaultAsync(x => x.DiscordUsers.Any(x => x.Verified));

            var eb = new EmbedBuilder();
            eb.WithColor(Color.DarkGreen);
            eb.WithThumbnailUrl("https://cdn.discordapp.com/avatars/669798825765896212/572b97a2e8c1dc33265ac51679303c41.png?size=128");
            eb.WithTitle("About");
            eb.WithDescription($@"
This bot was created by Et3rnos#6556.
It announces your Hack The Box solves in a specific channel.
If you want to support me you can visit [my patreon](https://www.patreon.com/et3rnos).

**Interesting Statistics**

Watching `{_client.Guilds.Count} servers` and `{usersCount} users ({uniqueUsersCount} unique)`
Our top server is `{_client.GetGuild(serverMoreLinks.Guild.GuildId)?.Name}` with `{serverMoreLinks.Count} linked users`
Our best verified player is `{bestPlayer?.Username}` with `{bestPlayer?.Score} points`
");
            await ReplyAsync(embed: eb.Build());
        }

        [Command("faq")]
        [Summary("Prints some Frequently Asked Questions")]
        public async Task Faq()
        {
            var eb = new EmbedBuilder();
            eb.WithColor(Color.DarkGreen);
            eb.WithTitle("Frequently Asked Questions");
            eb.AddField("Q: Does this bot work with Fortresses?", "A: No, the bot only announces challenges solves and user and root box claims.");
            eb.AddField("Q: I've solved a box/challenge but the bot didn't announce it.", "A: The bot can take up to 10 minutes to announce a solve.");
            eb.AddField("Q: I waited more than 10 minutes and no announcement was made.", "A: It is probably a misconfiguration. Ask admins to run `h.setup` on a channel I have Send Messages permission.");
            eb.AddField("Q: There was still no announcement!", "A: It may be a bot problem. Message Et3rnos#6556 and he will hopefully fix it.");
            await ReplyAsync(embed: eb.Build());
        }

        /*
        [Command("giveaway")]
        [Summary("Prints information about our giveaway")]
        public async Task Giveaway()
        {
            var uniqueUsersCount = await _context.HTBUsers.CountAsync();

            var eb = new EmbedBuilder();
            eb.WithColor(Color.DarkGreen);
            eb.WithTitle("Giveaway Information");
            eb.WithThumbnailUrl("https://i.imgur.com/3h9wG3Y.png");
            eb.WithDescription($@"
**Prize: ** HTB Vip (1 month)
**Alternative Prize: ** Discord Nitro (1 month)

**Eligibility Rules**
• Be verified
• Solved at least one challenge/box in the previous 30 days to the draw 

**Draw date**
When this bot reaches 200 unique users (currently `{uniqueUsersCount}`)
");
            await ReplyAsync(embed: eb.Build());
        }
        */

        [Command("suggest")]
        [Summary("Suggests a new feature to the bot")]
        public async Task Giveaway([Remainder] string suggestion)
        {
            var eb = new EmbedBuilder();
            eb.WithColor(Color.DarkGreen);

            eb.WithTitle("New suggestion");
            eb.WithDescription(Format.Sanitize(suggestion));
            eb.WithFooter($"From {Context.User.Username} ({Context.User.Id}) in {Context.Guild.Name}");

            var owner = (await _client.GetApplicationInfoAsync()).Owner;
            await owner.SendMessageAsync(embed: eb.Build());

            await ReplyAsync("Your suggestion is safe :thumbsup:");
        }
    }
}
