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
using System.IO;
using SixLabors.ImageSharp;
using Image = SixLabors.ImageSharp.Image;
using SixLabors.Fonts;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using SixLabors.ImageSharp.Drawing.Processing;
using System.Net.Http;
using Websocket.Client;
using Websocket.Client.Models;
using HtmlAgilityPack;
using PusherClient;

namespace HTB_Updates_Discord_Bot
{
    class ShoutboxListener
    {
        private readonly IServiceProvider _services;
        private Pusher pusher;

        public ShoutboxListener(IServiceProvider services)
        {
            _services = services;
        }

        public async Task Run()
        {
            pusher = new("97608bf7532e6f0fe898", new PusherOptions
            {
                Cluster = "eu",
                Encrypted = true
            });
            var ownsChannel = await pusher.SubscribeAsync("owns-channel");
            ownsChannel.Bind("display-info", OnOwn);
            await Task.Delay(2000);
            //CheckForSolves(183581);
            await pusher.ConnectAsync();
        }

        private void OnOwn(PusherEvent eventData)
        {
            string text = JsonConvert.DeserializeObject<dynamic>(eventData.Data).text;
            var doc = new HtmlDocument();
            doc.LoadHtml(text);
            var links = doc.DocumentNode.Descendants("a");
            var profilePage = links.First().GetAttributeValue("href", null);
            var userId = Convert.ToInt32(profilePage.Split("/").Last());
            CheckForSolves(userId).Wait(5000);
        }

        public async Task CheckForSolves(int userId)
        {
            using var scope = _services.CreateScope();
            var serviceProvider = scope.ServiceProvider;
            var client = serviceProvider.GetRequiredService<DiscordSocketClient>();
            var context = serviceProvider.GetRequiredService<DatabaseContext>();
            var htbApiV4Service = serviceProvider.GetRequiredService<IHTBApiV4Service>();

            var htbUser = context.HTBUsers.Include(x => x.Solves).Include(x => x.DiscordUsers).ThenInclude(x => x.Guild).FirstOrDefault(x => x.HtbId == userId);
            if (htbUser == null) return;

            List<Solve> currentSolves;
            try
            { currentSolves = await htbApiV4Service.GetSolves(htbUser.HtbId); }
            catch 
            (Exception e) { Log.Error(e, $"There was an error while fetching info for user id {htbUser.HtbId}"); return; }

            int score = currentSolves.Sum(x => x.Points);
            htbUser.Score = score;

            var oldSolves = htbUser.Solves;
            //oldSolves = new();

            var newSolves = currentSolves.Except(oldSolves, new SolveComparer()).ToList();
            newSolves.Reverse();

            //This is impossible... right???
            if (!newSolves.Any()) return;

            try
            { htbUser.Username = await htbApiV4Service.GetUserNameById(htbUser.HtbId); }
            catch
            (Exception e) { Log.Error(e, $"There was an error while fetching the username for user id {htbUser.HtbId}"); return; }

            var supporters = await context.Supporters.ToListAsync();

            foreach (var dUser in htbUser.DiscordUsers)
            {
                foreach (var solve in newSolves)
                {
                    try
                    {
                        var channel = client.GetGuild(dUser.Guild.GuildId).GetTextChannel(dUser.Guild.ChannelId);

                        using var stream = await GetSolvesImage(client, dUser, htbUser, solve, supporters.FirstOrDefault(x => x.DiscordId == dUser.DiscordId));
                        await channel.SendFileAsync(stream, "solve.png", "");
                    }
                    catch 
                    (Exception e) { Log.Error(e, $"There was an error while sending a message to: {dUser.Guild.GuildId}/{dUser.Guild.ChannelId}"); }
                }
            }
            htbUser.LastUpdated = DateTime.UtcNow;
            htbUser.Solves.AddRange(newSolves);

            await context.SaveChangesAsync();
        }

        public async Task<MemoryStream> GetSolvesImage(DiscordSocketClient client, DiscordUser dUser, HTBUser htbUser, Solve solve, Supporter supporter)
        {
            var collection = new FontCollection();
            var regularFamily = collection.Add("Files/UbuntuMono-Regular.ttf");
            var boldFamily = collection.Add("Files/UbuntuMono-Bold.ttf");
            var heading = boldFamily.CreateFont(50, FontStyle.Bold);
            var body = regularFamily.CreateFont(30);
            var top = regularFamily.CreateFont(25);
            var slogan = regularFamily.CreateFont(20, FontStyle.Bold);

            var backgroundColor = new Rgba32(20, 29, 44);
            var borderColor = new Rgba32(20, 21, 24);

            int offset = 0;
            if (supporter?.Border == true) offset = 10;

            using var image = Image.LoadPixelData(new Rgba32[] { borderColor }, 1, 1);
            image.Mutate(x => {
                x.Resize(offset * 2 + 800, offset * 2 + 200);
                x.Fill(backgroundColor, new RectangleF(offset, offset, 800, 200));
            }
            );

            using var frameImage = await Image.LoadAsync<Rgba32>("Files/frame.png");
            var framePosition = new Point(offset + 12, offset + 9);

            if (supporter != null) 
            {
                using var patreonImage = await Image.LoadAsync<Rgba32>("Files/patreon.png");
                patreonImage.Mutate(x => x.Resize(27, 27));
                image.Mutate(x =>
                {
                    x.Fill(borderColor, new RectangleF(offset + 227, offset + 163, 60 + supporter.Slogan.Length * 11, 37));
                    x.DrawImage(patreonImage, new Point(offset + 237, offset + 171), 1);
                    x.DrawText(supporter.Slogan, slogan, SixLabors.ImageSharp.Color.White, new PointF(offset + 277, offset + 173));
                });
            }

            using var userImage = await Image.LoadAsync<Rgba32>("Files/user.png");
            using var rootImage = await Image.LoadAsync<Rgba32>("Files/root.png");
            userImage.Mutate(x => x.Resize(65, 60));
            rootImage.Mutate(x => x.Resize(60, 60));
            var userPosition = new Point(offset + 731, offset + 135);
            var rootPosition = new Point(offset + 733, offset + 135);

            var user = await client.GetUserAsync(dUser.DiscordId);
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
            var avatarPosition = new Point(offset + 32, offset + 29);

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
            var solveAvatarPosition = new Point(offset + 660, offset + 59);

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
                    x.DrawText(new TextOptions(top) { HorizontalAlignment = HorizontalAlignment.Right, Origin = new PointF(782 + offset, 15 + offset) }, $"AKA {user.Username}", SixLabors.ImageSharp.Color.White);
                    x.DrawImage(avatar, avatarPosition, 1);
                }
                x.DrawImage(frameImage, framePosition, 1);
                if (isMachine) x.DrawImage(solveAvatar, solveAvatarPosition, 1);
                if (solve.Type == "user") x.DrawImage(userImage, userPosition, 1);
                if (solve.Type == "root") x.DrawImage(rootImage, rootPosition, 1);
                x.DrawText(htbUser.Username, heading, SixLabors.ImageSharp.Color.White, new PointF(offset + 228, offset + 46));
                x.DrawText(bodyText, body, SixLabors.ImageSharp.Color.White, new PointF(offset + 228, offset + 110));
            });

            var stream = new MemoryStream();
            await image.SaveAsPngAsync(stream);

            return stream;
        }
    }
}
