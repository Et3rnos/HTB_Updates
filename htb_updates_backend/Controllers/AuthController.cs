using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;
using System.IdentityModel.Tokens.Jwt;
using static System.Runtime.InteropServices.JavaScript.JSType;
using System.Net.Http.Headers;
using Newtonsoft.Json;
using System.Security.Claims;
using Microsoft.IdentityModel.Tokens;
using System.Text;

namespace htb_updates_backend.Controllers
{
    [ApiController]
    [Route("/api/auth")]
    public class AuthController : Controller
    {
        private readonly IConfiguration _configuration;

        public AuthController(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public class LoginModel
        {
            [Required]
            public string Code { get; set; }
        }

        [HttpPost]
        [Route("login")]
        public async Task<IActionResult> PostLoginAsync([FromBody] LoginModel model)
        {
            var clientId = _configuration["Discord:ClientId"];
            var clientSecret = _configuration["Discord:ClientSecret"];
            var redirectUri = _configuration["Discord:RedirectUri"];

            var client = new HttpClient();
            var values = new Dictionary<string, string>
            {
                { "client_id", clientId },
                { "client_secret", clientSecret },
                { "redirect_uri", redirectUri },
                { "grant_type", "authorization_code" },
                { "code", model.Code },
                { "scope", "identify" }
            };
            var content = new FormUrlEncodedContent(values);
            var response = await client.PostAsync("https://discordapp.com/api/oauth2/token", content);
            var responseString = await response.Content.ReadAsStringAsync();
            dynamic data = JsonConvert.DeserializeObject(responseString);
            Console.WriteLine(responseString);
            if (string.IsNullOrEmpty((string)data.access_token))
                return BadRequest(new { error = "Invalid code supplied" });

            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", (string)data.access_token);
            response = await client.GetAsync("https://discordapp.com/api/users/@me");
            responseString = await response.Content.ReadAsStringAsync();
            data = JsonConvert.DeserializeObject(responseString);
            if (string.IsNullOrEmpty((string)data.id))
                return BadRequest(new { error = "An error occured while fetching your data" });

            ulong discordId = (ulong)data.id;
            string username = (string)data.username;
            int discriminator = (int)data.discriminator;

            var authClaims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, discordId.ToString()),
                new Claim(ClaimTypes.Name, username),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new Claim("Discriminator", discriminator.ToString())
            };

            var authSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["JWT:Secret"]));

            var token = new JwtSecurityToken(
                issuer: _configuration["JWT:ValidIssuer"],
                audience: _configuration["JWT:ValidAudience"],
                expires: DateTime.Now.AddDays(1),
                claims: authClaims,
                signingCredentials: new SigningCredentials(authSigningKey, SecurityAlgorithms.HmacSha256)
            );

            var tokenString = new JwtSecurityTokenHandler().WriteToken(token);

            return Ok(new
            {
                token = tokenString
            });
        }
    }
}
