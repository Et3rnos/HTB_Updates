using Discord;
using Discord.Commands;
using Discord.WebSocket;
using HTB_Updates_Shared_Resources;
using HTB_Updates_Shared_Resources.Models.Database;
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
    [Name("config")]
    public class ConfigModule : ModuleBase<SocketCommandContext>
    {
        private readonly DiscordSocketClient _client;
        private readonly CommandService _commands;
        private readonly DatabaseContext _context;

        private ConfigModule(DiscordSocketClient client, CommandService commands, DatabaseContext context)
        {
            _client = client;
            _commands = commands;
            _context = context;
        }

        [Command("config")]
        [Summary("Change or view this server configuration")]
        [RequireContext(ContextType.Guild)]
        [RequireOwner(Group = "Permission")]
        [RequireUserPermission(GuildPermission.ManageGuild, Group = "Permission")]
        public async Task Config()
        {
            var guild = await _context.DiscordGuilds.FirstOrDefaultAsync(x => x.GuildId == Context.Guild.Id);
            if (guild == null)
            {
                await ReplyAsync("Please run `h.setup` first");
                return;
            }

            var embed = GetConfigEmbed(guild);
            await ReplyAsync(embed: embed);
        }

        [Command("config")]
        [Summary("Change or view this server configuration")]
        [RequireContext(ContextType.Guild)]
        [RequireOwner(Group = "Permission")]
        [RequireUserPermission(GuildPermission.ManageGuild, Group = "Permission")]
        public async Task Config(string key, string value)
        {
            var guild = await _context.DiscordGuilds.FirstOrDefaultAsync(x => x.GuildId == Context.Guild.Id);
            if (guild == null)
            {
                await ReplyAsync("Please run `h.setup` first");
                return;
            }

            if (key == "optional_announcements")
            {
                if (value == "enabled") guild.OptionalAnnouncements = true;
                else if (value == "disabled") guild.OptionalAnnouncements = false;
                else await ReplyAsync("Invalid value");
            }
            else
            {
                await ReplyAsync("Invalid key name");
            }
            await _context.SaveChangesAsync();
            var embed = GetConfigEmbed(guild);
            await ReplyAsync(embed: embed);
        }

        private Embed GetConfigEmbed(DiscordGuild guild)
        {
            var eb = new EmbedBuilder { Color = Color.DarkGreen };
            eb.WithTitle("Server Configuration");
            eb.Description = $"**optional_announcements:** `{(guild.OptionalAnnouncements ? "enabled" : "disabled")}` (enabled/disabled)\n";


            eb.Description += "\n**Want to change something?**\n";
            eb.Description += "Run `h.config config_key new_value`";
            return eb.Build();
        }
    }
}
