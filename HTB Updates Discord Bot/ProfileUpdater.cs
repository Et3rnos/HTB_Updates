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
    class ProfileUpdater
    {
        private readonly IServiceProvider _services;
        private DatabaseContext context;
        private IHTBApiV4Service htbApiV4Service;

        public ProfileUpdater(IServiceProvider services)
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
                    htbApiV4Service = services.GetRequiredService<IHTBApiV4Service>();
                    context = services.GetRequiredService<DatabaseContext>();
                    context.Database.Migrate();

                    var htbUsers = await context.HTBUsers.ToListAsync();
                    if (!htbUsers.Any())
                    {
                        await Task.Delay(60000);
                        Log.Information($"No HTB users were found");
                        continue;
                    }

                    var runningTasks = new List<Task>();
                    int delay = (24 * 60 * 60 * 1000) / htbUsers.Count;
                    foreach (var htbUser in htbUsers)
                    {
                        runningTasks.Add(CheckForProfileChanges(htbUser));
                        await Task.Delay(delay);
                    }

                    await Task.WhenAll(runningTasks);
                    await context.SaveChangesAsync();
                }
                catch(Exception e)
                {
                    Log.Error(e, "Something went wrong while running the profile updater loop");
                    await Task.Delay(60000);
                }
            }
        }

        public async Task CheckForProfileChanges(HTBUser user)
        {
            string username;
            try
            {
                username = await htbApiV4Service.GetUserNameById(user.HtbId);
            }
            catch (Exception e)
            {
                Log.Error(e, $"There was an error while fetching the username for user id {user.HtbId}");
                return;
            }

            if (username != user.Username) {
                user.Username = username;
            }
        }
    }
}
