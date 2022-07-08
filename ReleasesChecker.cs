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
using SixLabors.ImageSharp;
using Image = SixLabors.ImageSharp.Image;
using SixLabors.Fonts;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using SixLabors.ImageSharp.Drawing.Processing;
using System.Net.Http;
using System.Numerics;

namespace HTB_Updates_Discord_Bot
{
    class ReleasesChecker
    {
        private readonly IServiceProvider _services;
        private DiscordSocketClient client;
        private DatabaseContext context;
        private IHTBApiV4Service htbApiV4Service;

        private List<UnreleasedMachine> unreleasedMachines = new();

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

                    unreleasedMachines = unreleasedMachines.UnionBy(await htbApiV4Service.GetUnreleasedMachines(), x => x.Id).ToList();

                    await Task.Delay(TimeSpan.FromHours(1));
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

                    foreach (var machine in unreleasedMachines)
                    {
                        if (machine.Release > DateTime.UtcNow) continue;

                        var guilds = await context.DiscordGuilds.ToListAsync();
                        foreach (var guild in guilds)
                        {
                            await AnnounceNewMachine(guild, machine);
                        }

                        unreleasedMachines.Remove(machine);
                        break;
                    }

                    await Task.Delay(TimeSpan.FromSeconds(1));
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

                await MakeMachineCard(machine);

                await channel.SendFileAsync("Files/modified.png", "A new machine was released!");
            }
            catch (Exception e)
            {
                Log.Error(e, $"There was an error while sending a message to: {guild.GuildId}/{guild.ChannelId}");
            }
        }

        public async Task MakeMachineCard(UnreleasedMachine machine)
        {
            using var image = await SixLabors.ImageSharp.Image.LoadAsync<Rgba32>("Files/card.png");
            var collection = new FontCollection();
            var family = collection.Add("Files/UbuntuMono-Regular.ttf");
            var body = family.CreateFont(16);
            var heading = family.CreateFont(30);

            using var client = new HttpClient();
            var response = await client.GetAsync("https://hackthebox.com" + machine.Avatar);
            var avatarBytes = await response.Content.ReadAsByteArrayAsync();
            var avatar = Image.Load<Rgba32>(avatarBytes);
            avatar.Mutate(x => x.Resize(300, 300));

            Image osImage;
            if (machine.Os == "Windows") osImage = await Image.LoadAsync<Rgba32>("Files/win.png");
            else osImage = await Image.LoadAsync<Rgba32>("Files/linux.png");
            var height = (int)(((float)osImage.Size().Height / osImage.Size().Width) * 100);
            osImage.Mutate(x => x.Resize(100, height));

            var color = SixLabors.ImageSharp.Color.FromRgb(204, 204, 204);

            image.Mutate(x =>
            {
                x.DrawText(new TextOptions(heading) { HorizontalAlignment = HorizontalAlignment.Center, Origin = new PointF(510, 40) }, machine.Name, SixLabors.ImageSharp.Color.White);
                x.DrawText(machine.Os, body, color, new PointF(510, 95.5f));
                x.DrawText(machine.DifficultyText, body, color, new PointF(510, 135.5f));
                x.DrawText("-", body, color, new PointF(510, 175.5f));
                x.DrawText(machine.Release.ToShortDateString(), body, color, new PointF(510, 215.5f));
                x.DrawText("-", body, color, new PointF(510, 254.5f));
                x.DrawImage(avatar, new Point(31, 31), 1);
                x.DrawImage(osImage, new Point(231, 331 - height), 1);
            });
            await image.SaveAsPngAsync("Files/modified.png");
        }
    }
}
