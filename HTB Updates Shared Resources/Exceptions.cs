using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HTB_Updates_Shared_Resources
{
    [Serializable]
    class RateLimitingException : Exception
    {
        public RateLimitingException() { }
        public RateLimitingException(string message) : base(message) { }
    }
}
