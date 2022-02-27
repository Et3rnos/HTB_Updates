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
using HTB_Updates_Discord_Bot.Models.Api;

namespace HTB_Updates_Discord_Bot
{
    class ReleasesChecker
    {
        private readonly IServiceProvider _services;
        private DiscordSocketClient client;
        private DatabaseContext context;
        private IHTBApiV4Service htbApiV4Service;

        private List<UnreleasedMachine> unreleasedMachines = new List<UnreleasedMachine>();

        public ReleasesChecker(IServiceProvider services)
        {
            _services = services;
        }

        public async Task Run()
        {
            _ = ReleaseSchedule();
            while (true)
            {
                try
                {
                    using var scope = _services.CreateScope();
                    var services = scope.ServiceProvider;
                    htbApiV4Service = services.GetRequiredService<IHTBApiV4Service>();

                    unreleasedMachines = await htbApiV4Service.GetUnreleasedMachines();

                    await Task.Delay(3600 * 1000);
                }
                catch (Exception e)
                {
                    Log.Error(e, "Something went wrong while updating upcomming releases");
                    await Task.Delay(60 * 1000);
                }
            }
        }

        private async Task ReleaseSchedule()
        {
            while (true)
            {
                try
                {
                    using var scope = _services.CreateScope();
                    var services = scope.ServiceProvider;
                    client = services.GetRequiredService<DiscordSocketClient>();
                    context = services.GetRequiredService<DatabaseContext>();

                    foreach (var machine in unreleasedMachines.ToList())
                    {
                        if (machine.Release > DateTime.UtcNow) continue;

                        var guilds = await context.DiscordGuilds.ToListAsync();
                        foreach (var guild in guilds)
                        {
                            await AnnounceNewMachine(guild, machine);
                        }

                        unreleasedMachines.Remove(machine);
                    }

                    await Task.Delay(1000);
                }
                catch (Exception e)
                {
                    Log.Error(e, "Something went wrong while updating upcomming releases");
                    await Task.Delay(1000);
                }
            }
        }

        public async Task AnnounceNewMachine(DiscordGuild guild, UnreleasedMachine machine)
        {
            try
            {
                var channel = client.GetGuild(guild.GuildId).GetTextChannel(guild.ChannelId);

                var eb = new EmbedBuilder();
                eb.WithColor(Color.DarkGreen);

                eb.WithTitle("A new machine was released!");

                eb.Description += $"**Name:** {machine.Name}\n";
                eb.Description += $"**OS:** {machine.Os}\n";
                eb.Description += $"**Difficulty:** {machine.DifficultyText}\n";

                if (!string.IsNullOrEmpty(machine.Avatar))
                {
                    eb.WithThumbnailUrl($"https://hackthebox.com{machine.Avatar}");
                }

                await channel.SendMessageAsync(embed: eb.Build());
            }
            catch (Exception e)
            {
                Log.Error(e, $"There was an error while sending a message to: {guild.GuildId}/{guild.ChannelId}");
            }
        }
    }
}
