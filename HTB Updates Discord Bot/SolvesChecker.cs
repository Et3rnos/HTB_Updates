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
using System.IO;
using SixLabors.ImageSharp;
using Image = SixLabors.ImageSharp.Image;
using SixLabors.Fonts;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using SixLabors.ImageSharp.Drawing.Processing;
using System.Net.Http;
using HTB_Updates_Shared_Resources.Services;

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
                    int delay = (24 * 60 * 60 * 1000) / htbUsers.Count;
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

            await context.Entry(user).Collection(x => x.GuildUsers).LoadAsync();

            foreach (var dUser in user.GuildUsers)
            {
                await context.Entry(dUser).Reference(x => x.Guild).LoadAsync();
                await context.Entry(dUser).Reference(x => x.DiscordUser).LoadAsync();

                foreach (var solve in newSolves) {
                    await AnnounceSolve(dUser, user, solve);
                }
            }
            user.LastUpdated = DateTime.UtcNow;
            user.Solves.AddRange(newSolves);
        }

        public async Task AnnounceSolve(GuildUser dUser, HTBUser htbUser, Solve solve)
        {
            try
            {
                var channel = client.GetGuild(dUser.Guild.GuildId).GetTextChannel(dUser.Guild.ChannelId);

                using var stream = await GetSolvesImage(dUser, htbUser, solve);
                await channel.SendFileAsync(stream, "solve.png", "");
            }
            catch (Exception e)
            {
                Log.Error(e, $"There was an error while sending a message to: {dUser.Guild.GuildId}/{dUser.Guild.ChannelId}");
            }
        }

        public async Task<MemoryStream> GetSolvesImage(GuildUser dUser, HTBUser htbUser, Solve solve)
        {
            //var framePath = dUser.Verified ? "Files/gold_frame.png" : "Files/frame.png";
            var framePath = "Files/frame.png";

            using var image = await Image.LoadAsync<Rgba32>("Files/solve.png");
            using var frame = await Image.LoadAsync<Rgba32>(framePath);
            using var userImage = await Image.LoadAsync<Rgba32>("Files/user.png");
            using var rootImage = await Image.LoadAsync<Rgba32>("Files/root.png");
            userImage.Mutate(x => x.Resize(65, 60));
            rootImage.Mutate(x => x.Resize(60, 60));

            var user = await client.GetUserAsync(dUser.DiscordUser.DiscordId);
            var avatar = userImage;
            if (user != null)
            {
                var avatarUrl = user.GetAvatarUrl(ImageFormat.Png, 256) ?? user.GetDefaultAvatarUrl();

                using var httpClient = new HttpClient();
                var avatarResponse = await httpClient.GetAsync(avatarUrl);
                var avatarBytes = await avatarResponse.Content.ReadAsByteArrayAsync();
                avatar = Image.Load<Rgba32>(avatarBytes);
                avatar.Mutate(x => x.Resize(140, 140));
            }

            var isMachine = !string.IsNullOrEmpty(solve.MachineAvatar);

            var solveAvatar = userImage;
            if (isMachine)
            {
                using var httpClient = new HttpClient();
                var solveResponse = await httpClient.GetAsync("https://hackthebox.com" + solve.MachineAvatar);
                var solveBytes = await solveResponse.Content.ReadAsByteArrayAsync();
                solveAvatar = Image.Load<Rgba32>(solveBytes);
                solveAvatar.Mutate(x => x.Resize(128, 128));
            }

            var collection = new FontCollection();
            var regularFamily = collection.Add("Files/UbuntuMono-Regular.ttf");
            var boldFamily = collection.Add("Files/UbuntuMono-Bold.ttf");
            var body = regularFamily.CreateFont(30);
            var top = regularFamily.CreateFont(25);
            var heading = boldFamily.CreateFont(50, FontStyle.Bold);

            var bodyText = solve.Type switch
            {
                "challenge" => $"Just solved {solve.Name}",
                "user" => $"Just got user on {solve.Name}",
                "root" => $"Just got root on {solve.Name}",
                _ => "Just solved something unknown"
            };

            image.Mutate(x =>
            {
                if (user != null)
                {
                    x.DrawText(new TextOptions(top) { HorizontalAlignment = HorizontalAlignment.Right, Origin = new PointF(782, 15) }, $"AKA {user.Username}", SixLabors.ImageSharp.Color.White);
                    x.DrawImage(avatar, new Point(32, 29), 1);
                }
                x.DrawImage(frame, new Point(12, 9), 1);
                if (isMachine) x.DrawImage(solveAvatar, new Point(660, 59), 1);
                if (solve.Type == "user") x.DrawImage(userImage, new Point(731, 135), 1);
                if (solve.Type == "root") x.DrawImage(rootImage, new Point(733, 135), 1);
                x.DrawText(htbUser.Username, heading, SixLabors.ImageSharp.Color.White, new PointF(228, 46));
                x.DrawText(bodyText, body, SixLabors.ImageSharp.Color.White, new PointF(228, 110));
            });

            var stream = new MemoryStream();
            await image.SaveAsPngAsync(stream);

            return stream;
        }
    }
}
