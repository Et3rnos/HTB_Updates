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

namespace HTB_Updates_Discord_Bot
{
    class ShoutboxListener
    {
        private readonly IServiceProvider _services;
        private WebsocketClient ws;

        public ShoutboxListener(IServiceProvider services)
        {
            _services = services;
        }

        public async Task Run()
        {
            ws = new (new Uri("wss://ws-eu.pusher.com/app/97608bf7532e6f0fe898?protocol=7&client=js&version=5.1.1&flash=false"));
            ws.ReconnectionHappened.Subscribe(OnReconnect);
            ws.MessageReceived.Subscribe(OnMessage);
            ws.ReconnectTimeout = TimeSpan.FromSeconds(30);
            await ws.Start();
        }

        private void OnReconnect(ReconnectionInfo info)
        {
            ws.Send("{\"event\":\"pusher:subscribe\",\"data\":{\"auth\":\"\",\"channel\":\"owns-channel\"}}");
        }

        private void OnMessage(ResponseMessage message)
        {
            dynamic json = JsonConvert.DeserializeObject(message.Text);
            if ((string)json.channel != "owns-channel" || (string)json.@event != "display-info") return;
            dynamic data = JsonConvert.DeserializeObject((string)json.data);
            string text = data.text;
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

            var newSolves = currentSolves.Except(oldSolves, new SolveComparer()).ToList();
            newSolves.Reverse();

            //This is impossible... right???
            if (!newSolves.Any()) return;

            try
            { htbUser.Username = await htbApiV4Service.GetUserNameById(htbUser.HtbId); }
            catch
            (Exception e) { Log.Error(e, $"There was an error while fetching the username for user id {htbUser.HtbId}"); return; }

            foreach (var dUser in htbUser.DiscordUsers)
            {
                foreach (var solve in newSolves)
                {
                    try
                    {
                        var channel = client.GetGuild(dUser.Guild.GuildId).GetTextChannel(dUser.Guild.ChannelId);

                        using var stream = await GetSolvesImage(client, dUser, htbUser, solve);
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

        public async Task<MemoryStream> GetSolvesImage(DiscordSocketClient client, DiscordUser dUser, HTBUser htbUser, Solve solve)
        {
            //var framePath = dUser.Verified ? "Files/gold_frame.png" : "Files/frame.png";
            var framePath = "Files/frame.png";

            using var image = await Image.LoadAsync<Rgba32>("Files/solve.png");
            using var frame = await Image.LoadAsync<Rgba32>(framePath);
            using var userImage = await Image.LoadAsync<Rgba32>("Files/user.png");
            using var rootImage = await Image.LoadAsync<Rgba32>("Files/root.png");
            userImage.Mutate(x => x.Resize(65, 60));
            rootImage.Mutate(x => x.Resize(60, 60));

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
