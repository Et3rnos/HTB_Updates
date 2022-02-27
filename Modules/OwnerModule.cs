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
    [Name("owner")]
    public class OwnerModule : ModuleBase<SocketCommandContext>
    {
        private readonly DiscordSocketClient _client;
        private readonly CommandService _commands;
        private readonly DatabaseContext _context;

        private OwnerModule(DiscordSocketClient client, CommandService commands, DatabaseContext context)
        {
            _client = client;
            _commands = commands;
            _context = context;
        }

        [Command("announce")]
        [Summary("Announces something in all guilds")]
        [RequireOwner]
        public async Task Announce(string text)
        {
            var eb = new EmbedBuilder
            {
                Color = Color.DarkGreen,
                Title = "Announcement",
                Description = text
            };
            var guilds = await _context.DiscordGuilds.AsQueryable().ToListAsync();
            foreach (var guild in guilds)
            {
                try
                {
                    var channel = _client.GetGuild(guild.GuildId).GetTextChannel(guild.ChannelId);
                    await channel.SendMessageAsync(embed: eb.Build());
                }
                catch { continue; }
            }
        }

        [Command("healthcheck")]
        [Summary("Performs a health check")]
        [RequireOwner]
        public async Task HealthCheck()
        {
            var databaseGuilds = await _context.DiscordGuilds.AsQueryable().ToListAsync();
            var botGuilds = _client.Guilds;

            var uselessGuilds = databaseGuilds.Where(x => !botGuilds.Select(x => x.Id).Contains(x.GuildId));
            if (uselessGuilds.Any())
            {
                await ReplyAsync($"{uselessGuilds.Count()} useless guilds were found:\n```\n{string.Join('\n', uselessGuilds.Select(x => x.GuildId))}\n```");
            } 
            else
            {
                await ReplyAsync("No useless guilds were found.");
            }

            var uselessHtbUsers = await _context.HTBUsers.Where(x => !x.DiscordUsers.Any()).ToListAsync();
            if (uselessHtbUsers.Any())
            {
                await ReplyAsync($"{uselessHtbUsers.Count()} useless HTB users were found:\n```\n{string.Join('\n', uselessHtbUsers.Select(x => x.Username))}\n```");
            }
            else
            {
                await ReplyAsync("No useless HTB users were found.");
            }
        }
    }
}
