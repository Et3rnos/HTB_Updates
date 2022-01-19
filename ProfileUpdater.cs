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

namespace HTB_Updates_Discord_Bot
{
    class ProfileUpdater
    {
        private readonly DatabaseContext _context;
        private readonly IHTBApiV4Service _htbApiV4Service;

        public ProfileUpdater(IServiceProvider services)
        {
            _htbApiV4Service = services.GetRequiredService<IHTBApiV4Service>();
            _context = services.GetRequiredService<DatabaseContext>();
            _context.Database.Migrate();
        }

        public async Task Run()
        {
            while (true)
            {
                try
                {
                    Log.Information("Starting loop");
                    var start = DateTime.UtcNow;

                    var htbUsers = await _context.HTBUsers.AsQueryable().ToListAsync();

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
                    await _context.SaveChangesAsync();
                    Log.Information($"Ending loop after {(DateTime.UtcNow - start).TotalSeconds}s");
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
                username = await _htbApiV4Service.GetUserNameById(user.HtbId);
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
