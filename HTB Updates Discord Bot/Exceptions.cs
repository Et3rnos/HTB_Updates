using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HTB_Updates_Discord_Bot
{
    [Serializable]
    class RateLimitingException : Exception
    {
        public RateLimitingException() { }
        public RateLimitingException(string message) : base(message) { }
    }
}
