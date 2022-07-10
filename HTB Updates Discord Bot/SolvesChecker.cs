using Discord.WebSocket;
using System;
using Microsoft.Extensions.DependencyInjection;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HTB_Updates_Shared_Resources;
using Newtonsoft.Json;
using Microsoft.Extensions.Options;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using HTB_Updates_Shared_Resources.Models.Database;
using HTB_Updates_Shared_Resources.Models.Shared;
using Discord;
using Serilog;
using HTB_Updates_Discord_Bot.Services;

namespace HTB_Updates_Discord_Bot
{
    class SolvesChecker
    {
        private readonly IServiceProvider _services;
        private DiscordSocketClient client;
        private DatabaseContext context;
        private IHTBApiV4Service htbApiV4Service;

        public SolvesChecker(IServiceProvider services)
        {
            _services = services;
        }

        public async Task Run()
        {
            while (true)
            {
                try
                {
                    using var scope = _services.CreateScope();
                    var services = scope.ServiceProvider;
                    client = services.GetRequiredService<DiscordSocketClient>();
                    htbApiV4Service = services.GetRequiredService<IHTBApiV4Service>();
                    context = services.GetRequiredService<DatabaseContext>();
                    context.Database.Migrate();

                    var htbUsers = await context.HTBUsers.ToListAsync();
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
                    await context.SaveChangesAsync();
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
                currentSolves = await htbApiV4Service.GetSolves(user.HtbId);
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

            await context.Entry(user).Collection(x => x.Solves).LoadAsync();

            var oldSolves = user.Solves;
            //oldSolves = new List<Solve>();

            if (currentSolves.Count == oldSolves.Count) { return; }

            var newSolves = currentSolves.Except(oldSolves, new SolveComparer()).ToList();
            newSolves.Reverse();

            try
            {
                user.Username = await htbApiV4Service.GetUserNameById(user.HtbId);
            }
            catch (Exception e)
            {
                Log.Error(e, $"There was an error while fetching the username for user id {user.HtbId}");
                return;
            }

            await context.Entry(user).Collection(x => x.DiscordUsers).LoadAsync();

            foreach (var dUser in user.DiscordUsers)
            {
                await context.Entry(dUser).Reference(x => x.Guild).LoadAsync();

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
                var channel = client.GetGuild(dUser.Guild.GuildId).GetTextChannel(dUser.Guild.ChannelId);

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
