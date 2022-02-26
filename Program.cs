using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using System.Runtime;
using System.Linq;
using Microsoft.Extensions.Configuration;
using System.IO;
using Serilog;
using HTB_Updates_Discord_Bot.Services;

namespace HTB_Updates_Discord_Bot
{
    class Program
    {
        private IConfigurationRoot configuration;
        private readonly DiscordSocketClient _client;
        private readonly CommandService _commands;
        private readonly IServiceProvider _services;

        public static void Main(string[] args)
        => new Program().MainAsync().GetAwaiter().GetResult();

        private Program()
        {
            _client = new DiscordSocketClient(new DiscordSocketConfig
            {
                GatewayIntents = GatewayIntents.All
            });
            _commands = new CommandService(new CommandServiceConfig
            {
                CaseSensitiveCommands = false,
                //Keeping the bot sync for now
                //DefaultRunMode = RunMode.Async
            });
            _client.Log += LoggingManager.LogAsync;
            _commands.Log += LoggingManager.LogAsync;
            _services = ConfigureServices();
        }

        private IServiceProvider ConfigureServices()
        {
            configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetParent(AppContext.BaseDirectory).FullName)
            .AddJsonFile("appsettings.json", false)
            .Build();

            Log.Logger = new LoggerConfiguration()
                .ReadFrom.Configuration(configuration)
                .CreateLogger();

            var connectionString = configuration.GetValue<string>("ConnectionString");

            var map = new ServiceCollection()
                .AddSingleton(configuration)
                .AddSingleton(_client)
                .AddSingleton(_commands)
                .AddScoped<IHTBApiV1Service, HTBApiV1Service>()
                .AddScoped<IHTBApiV4Service, HTBApiV4Service>()
                .AddDbContext<DatabaseContext>(options =>
                    options.UseMySql(
                        connectionString,
                        ServerVersion.AutoDetect(connectionString),
                        x => x.MigrationsHistoryTable("HTBUpdates_EFMigrationsHistory")
                    )
                );

            return map.BuildServiceProvider();
        }

        private async Task MainAsync() {
            await InitCommands();

            await _client.LoginAsync(TokenType.Bot, configuration.GetValue<string>("Token"));
            await _client.StartAsync();

            await _client.SetGameAsync("h.help", type: ActivityType.Playing);

            await Task.Delay(5000);

            using var scope = _services.CreateScope();
            var solvesChecker = new SolvesChecker(_client, scope.ServiceProvider);
            var profileUpdater = new ProfileUpdater(scope.ServiceProvider);
            _ = solvesChecker.Run();
            await Task.Delay(5000);
            _ = profileUpdater.Run();
            await Task.Delay(Timeout.Infinite);
        }

        private async Task InitCommands()
        {
            await _commands.AddModulesAsync(Assembly.GetEntryAssembly(), _services);

            _client.MessageReceived += HandleCommandAsync;
            _client.LeftGuild += HandleLeftGuild;
            _client.UserLeft += HandleUserLeft;
        }

        private async Task HandleCommandAsync(SocketMessage messageParam)
        {
            if (messageParam is not SocketUserMessage message) return;

            int argPos = 0;

            if (!(message.HasStringPrefix("h.", ref argPos) ||
                message.HasMentionPrefix(_client.CurrentUser, ref argPos)) ||
                message.Author.IsBot)
                return;

            var context = new SocketCommandContext(_client, message);

            using var scope = _services.CreateScope();
            await _commands.ExecuteAsync(context, argPos, scope.ServiceProvider);
        }

        private async Task HandleLeftGuild(SocketGuild socketGuild)
        {
            using var scope = _services.CreateScope();
            var serviceProvider = scope.ServiceProvider;

            var context = serviceProvider.GetRequiredService<DatabaseContext>();
            var guild = await context.DiscordGuilds.FirstOrDefaultAsync(x => x.GuildId == socketGuild.Id);
            if (guild == null) return;

            context.DiscordGuilds.Remove(guild);

            //Clean unlinked htb users
            var htbUsers = await context.HTBUsers.Where(x => !x.DiscordUsers.Any()).ToListAsync();
            context.HTBUsers.RemoveRange(htbUsers);

            await context.SaveChangesAsync();
            Log.Information($"This bot was removed from {socketGuild.Name} guild ({socketGuild.Id})");
        }

        private async Task HandleUserLeft(SocketGuildUser socketGuildUser)
        {
            using var scope = _services.CreateScope();
            var serviceProvider = scope.ServiceProvider;

            var context = serviceProvider.GetRequiredService<DatabaseContext>();
            var discordUser = await context.DiscordUsers.FirstOrDefaultAsync(x => x.DiscordId == socketGuildUser.Id && x.Guild.GuildId == socketGuildUser.Guild.Id);
            if (discordUser == null) return;

            context.DiscordUsers.Remove(discordUser);

            //Clean unlinked htb users
            var htbUsers = await context.HTBUsers.Where(x => !x.DiscordUsers.Any()).ToListAsync();
            context.HTBUsers.RemoveRange(htbUsers);

            await context.SaveChangesAsync();
            Log.Information($"User {socketGuildUser.Username} ({socketGuildUser.Id}) left {socketGuildUser.Guild.Name} ({socketGuildUser.Guild.Id})");
        }
    }
}