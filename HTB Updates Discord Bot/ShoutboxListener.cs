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
using System.Net.Http;
using HtmlAgilityPack;
using PusherClient;
using HTB_Updates_Shared_Resources.Services;
using HTB_Updates_Shared_Resources.Managers;

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
            CheckForSolves(183581);
            CheckForSolves(userId).Wait(5000);
        }

        public async Task CheckForSolves(int userId)
        {
            using var scope = _services.CreateScope();
            var serviceProvider = scope.ServiceProvider;
            var client = serviceProvider.GetRequiredService<DiscordSocketClient>();
            var context = serviceProvider.GetRequiredService<DatabaseContext>();
            var htbApiV4Service = serviceProvider.GetRequiredService<IHTBApiV4Service>();

            var htbUser = context.HTBUsers.Include(x => x.Solves).Include(x => x.GuildUsers).ThenInclude(x => x.Guild).Include(x => x.GuildUsers).ThenInclude(x => x.DiscordUser).FirstOrDefault(x => x.HtbId == userId);
            if (htbUser == null) return;

            List<Solve> currentSolves;
            try
            { currentSolves = await htbApiV4Service.GetSolves(htbUser.HtbId); }
            catch 
            (Exception e) { Log.Error(e, $"There was an error while fetching info for user id {htbUser.HtbId}"); return; }

            int score = currentSolves.Sum(x => x.Points);
            htbUser.Score = score;

            var oldSolves = htbUser.Solves;
            oldSolves = new();

            var newSolves = currentSolves.Except(oldSolves, new SolveComparer()).ToList();
            newSolves.Reverse();

            //This is impossible... right???
            if (!newSolves.Any()) return;

            try
            { htbUser.Username = await htbApiV4Service.GetUserNameById(htbUser.HtbId); }
            catch
            (Exception e) { Log.Error(e, $"There was an error while fetching the username for user id {htbUser.HtbId}"); return; }

            foreach (var dUser in htbUser.GuildUsers)
            {
                foreach (var solve in newSolves)
                {
                    try
                    {
                        var channel = client.GetGuild(dUser.Guild.GuildId).GetTextChannel(dUser.Guild.ChannelId);

                        var user = await client.GetUserAsync(dUser.DiscordUser.DiscordId);
                        string avatarUrl = "";
                        string username = "";
                        if (user != null)
                        {
                            avatarUrl = user.GetAvatarUrl(ImageFormat.Png, 256) ?? user.GetDefaultAvatarUrl();
                            username = user.Username;
                        }
                        using var stream = await ImageGeneration.GetSolvesImage(avatarUrl, username, dUser.DiscordUser, dUser, htbUser, solve);
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
    }
}
