using Discord;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Serilog;

namespace HTB_Updates_Discord_Bot
{
    internal static class LoggingManager
    {
        public async static Task LogAsync(LogMessage message)
        {
            switch (message.Severity)
            {
                case LogSeverity.Debug:
                    Log.Debug(message.ToString(prependTimestamp: false));
                    break;
                case LogSeverity.Verbose:
                    Log.Debug(message.ToString(prependTimestamp: false));
                    break;
                case LogSeverity.Info:
                    Log.Information(message.ToString(prependTimestamp: false));
                    break;
                case LogSeverity.Warning:
                    Log.Warning(message.ToString(prependTimestamp: false));
                    break;
                case LogSeverity.Error:
                    Log.Error(message.ToString(prependTimestamp: false));
                    break;
                case LogSeverity.Critical:
                    Log.Fatal(message.ToString(prependTimestamp: false));
                    break;
            }
        }
    }
}
