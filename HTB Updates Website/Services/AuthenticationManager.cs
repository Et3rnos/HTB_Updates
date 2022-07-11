using Serilog;
using System.Security.Cryptography;
using System.Text;

namespace HTB_Updates_Website.Services
{
    public interface IAuthenticationManager
    {
        public string GetToken(string password);
        public bool ValidateRequest(HttpContext context);
    }

    public class AuthenticationManager : IAuthenticationManager
    {
        private readonly IConfiguration _configuration;

        private static string token;

        public AuthenticationManager(IServiceProvider serviceProvider)
        {
            _configuration = serviceProvider.GetRequiredService<IConfiguration>();
        }

        public string GetToken(string password)
        {
            if (password == _configuration.GetValue<string>("Password"))
            {
                token = GenerateToken();
                return token;
            }
            return null;
        }

        public bool ValidateRequest(HttpContext context)
        {
            if (string.IsNullOrEmpty(token)) return false;
            context.Request.Cookies.TryGetValue("token", out string contextToken);
            return contextToken == token;
        }

        private string GenerateToken()
        {
            int size = 32;
            char[] chars = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ1234567890".ToCharArray();

            byte[] data = new byte[4 * size];
            using (var crypto = RandomNumberGenerator.Create())
            {
                crypto.GetBytes(data);
            }
            var result = new StringBuilder(size);
            for (int i = 0; i < size; i++)
            {
                var rnd = BitConverter.ToUInt32(data, i * 4);
                var idx = rnd % chars.Length;

                result.Append(chars[idx]);
            }

            return result.ToString();
        }
    }
}
