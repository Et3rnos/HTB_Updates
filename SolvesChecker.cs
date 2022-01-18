using Discord.WebSocket;
using System;
using Microsoft.Extensions.DependencyInjection;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HTB_Updates_Discord_Bot.Models;
using Newtonsoft.Json;
using Microsoft.Extensions.Options;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using HTB_Updates_Discord_Bot.Models.Database;
using HTB_Updates_Discord_Bot.Models.Shared;
using Discord;
using Serilog;
using HTB_Updates_Discord_Bot.Services;

namespace HTB_Updates_Discord_Bot
{
    class SolvesChecker
    {
        private readonly DiscordSocketClient _client;
        private readonly DatabaseContext _context;
        private readonly IHTBApiV4Service _htbApiV4Service;

        public SolvesChecker(DiscordSocketClient client, IServiceProvider services)
        {
            _client = client;
            _htbApiV4Service = services.GetRequiredService<IHTBApiV4Service>();
            _context = services.GetRequiredService<DatabaseContext>();
            _context.Database.Migrate();
        }

        public async Task Run()
        {
            while (true)
            {
                try
                {
                    Log.Information("Starting loop");
                    var start = DateTime.UtcNow;

                    var htbUsers = await _context.HTBUsers.AsQueryable().ToListAsync();

                    if (!htbUsers.Any())
                    {
                        await Task.Delay(5000);
                        Log.Information($"No HTB users were found");
                        continue;
                    }

                    var runningTasks = new List<Task>();

                    int delay = (10 * 60 * 1000) / htbUsers.Count;

                    foreach (var htbUser in htbUsers)
                    {
                        runningTasks.Add(CheckForSolves(htbUser));
                        await Task.Delay(delay);
                    }

                    await Task.WhenAll(runningTasks);
                    await _context.SaveChangesAsync();
                    Log.Information($"Ending loop after {(DateTime.UtcNow - start).TotalSeconds}s");
                }
                catch(Exception e)
                {
                    Log.Error(e, "Something went wrong while running the solves checker loop");
                    await Task.Delay(5000);
                }
            }
        }

        public async Task CheckForSolves(HTBUser user)
        {
            List<Solve> currentSolves;
            try
            {
                currentSolves = await _htbApiV4Service.GetSolves(user.HtbId);
            }
            catch (Exception e)
            {
                Log.Error(e, $"There was an error while fetching info for user id {user.HtbId}");
                return;
            }

            int score = currentSolves.Sum(x => x.Points);

            if (score != user.Score) {
                user.Score = score;
            }

            await _context.Entry(user).Collection(x => x.Solves).LoadAsync();

            var oldSolves = user.Solves;
            //oldSolves = new List<Solve>();

            if (currentSolves.Count == oldSolves.Count) { return; }

            var newSolves = currentSolves.Except(oldSolves, new SolveComparer()).ToList();
            newSolves.Reverse();

            try
            {
                user.Username = await _htbApiV4Service.GetUserNameById(user.HtbId);
            }
            catch (Exception e)
            {
                Log.Error(e, $"There was an error while fetching the username for user id {user.HtbId}");
                return;
            }

            await _context.Entry(user).Collection(x => x.DiscordUsers).LoadAsync();

            foreach (var dUser in user.DiscordUsers)
            {
                await _context.Entry(dUser).Reference(x => x.Guild).LoadAsync();

                foreach (var solve in newSolves) {
                    await AnnounceSolve(dUser, user, solve);
                }
            }
            user.LastUpdated = DateTime.UtcNow;
            user.Solves.AddRange(newSolves);
        }

        public async Task AnnounceSolve(DiscordUser dUser, HTBUser htbUser, Solve solve)
        {
            try
            {
                var channel = _client.GetGuild(dUser.Guild.GuildId).GetTextChannel(dUser.Guild.ChannelId);

                var eb = new EmbedBuilder();
                eb.WithColor(Color.DarkGreen);

                switch (solve.Type) {
                    case "challenge":
                        eb.WithTitle($"{htbUser.Username} just solved {solve.Name}");
                        break;
                    case "user":
                        eb.WithTitle($"{htbUser.Username} just got user on {solve.Name}");
                        break;
                    case "root":
                        eb.WithTitle($"{htbUser.Username} just got root on {solve.Name}");
                        break;
                }

                eb.Description += $"**Discord User:** <@{dUser.DiscordId}>\n";

                if (!string.IsNullOrEmpty(solve.ChallengeCategory)) {
                    eb.Description += $"**Challenge Category:** {solve.ChallengeCategory}\n";
                }
                eb.Description += $"**Points Gained:** {solve.Points}\n";

                if (!string.IsNullOrEmpty(solve.MachineAvatar)) {
                    eb.WithThumbnailUrl($"https://hackthebox.com{solve.MachineAvatar}");
                }

                await channel.SendMessageAsync(embed: eb.Build());
            }
            catch (Exception e)
            {
                Log.Error(e, $"There was an error while sending a message to: {dUser.Guild.GuildId}/{dUser.Guild.ChannelId}");
            }
        }
    }
}
