using HTB_Updates_Shared_Resources.Models.Database;
using HTB_Updates_Shared_Resources.Models.Shared;
using SixLabors.Fonts;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Drawing.Processing;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Net.Mime.MediaTypeNames;
using Image = SixLabors.ImageSharp.Image;

namespace HTB_Updates_Shared_Resources.Managers
{
    public static class ImageGeneration
    {
        public static async Task<MemoryStream> GetSolvesImage(string avatarUrl, string username, DiscordUser discordUser, GuildUser guildUser, HTBUser htbUser, Solve solve)
        {
            var collection = new FontCollection();
            var regularFamily = collection.Add("Files/UbuntuMono-Regular.ttf");
            var boldFamily = collection.Add("Files/UbuntuMono-Bold.ttf");
            var heading = boldFamily.CreateFont(50, FontStyle.Bold);
            var body = regularFamily.CreateFont(30);
            var top = regularFamily.CreateFont(25);
            var slogan = regularFamily.CreateFont(20, FontStyle.Bold);

            var backgroundColor = new Rgba32(20, 29, 44);
            var borderColor = new Rgba32(20, 21, 24);

            int offset = 0;
            if (discordUser.Border == true) offset = 10;

            using var image = Image.LoadPixelData<Rgba32>(new Rgba32[] { borderColor }, 1, 1);
            image.Mutate(x => {
                x.Resize(offset * 2 + 800, offset * 2 + 200);
                x.Fill(backgroundColor, new RectangleF(offset, offset, 800, 200));
            }
            );

            using var frameImage = await Image.LoadAsync<Rgba32>("Files/frame.png");
            var framePosition = new Point(offset + 12, offset + 9);

            /*
            if (supporter != null) 
            {
                using var patreonImage = await Image.LoadAsync<Rgba32>("Files/patreon.png");
                patreonImage.Mutate(x => x.Resize(27, 27));
                image.Mutate(x =>
                {
                    x.Fill(borderColor, new RectangleF(offset + 227, offset + 163, 60 + supporter.Slogan.Length * 11, 37));
                    x.DrawImage(patreonImage, new Point(offset + 237, offset + 171), 1);
                    x.DrawText(supporter.Slogan, slogan, SixLabors.ImageSharp.Color.White, new PointF(offset + 277, offset + 173));
                });
            }
            */

            using var userImage = await Image.LoadAsync<Rgba32>("Files/user.png");
            using var rootImage = await Image.LoadAsync<Rgba32>("Files/root.png");
            userImage.Mutate(x => x.Resize(65, 60));
            rootImage.Mutate(x => x.Resize(60, 60));
            var userPosition = new Point(offset + 731, offset + 135);
            var rootPosition = new Point(offset + 733, offset + 135);

            var avatar = userImage;
            if (!string.IsNullOrEmpty(avatarUrl))
            {
                using var httpClient = new HttpClient();
                var avatarResponse = await httpClient.GetAsync(avatarUrl);
                var avatarBytes = await avatarResponse.Content.ReadAsByteArrayAsync();
                avatar = Image.Load<Rgba32>(avatarBytes);
                avatar.Mutate(x => x.Resize(140, 140));
            }
            var avatarPosition = new Point(offset + 32, offset + 29);

            var isMachine = !string.IsNullOrEmpty(solve.MachineAvatar);

            var solveAvatar = userImage;
            if (isMachine)
            {
                using var httpClient = new HttpClient();
                var solveResponse = await httpClient.GetAsync("https://hackthebox.com" + solve.MachineAvatar);
                var solveBytes = await solveResponse.Content.ReadAsByteArrayAsync();
                solveAvatar = Image.Load<Rgba32>(solveBytes);
                solveAvatar.Mutate(x => x.Resize(128, 128));
            }
            var solveAvatarPosition = new Point(offset + 660, offset + 59);

            var bodyText = solve.Type switch
            {
                "challenge" => $"Just solved {solve.Name}",
                "user" => $"Just got user on {solve.Name}",
                "root" => $"Just got root on {solve.Name}",
                _ => "Just solved something unknown"
            };

            image.Mutate(x =>
            {
                if (!string.IsNullOrEmpty(avatarUrl))
                {
                    x.DrawText(new TextOptions(top) { HorizontalAlignment = HorizontalAlignment.Right, Origin = new PointF(782 + offset, 15 + offset) }, $"AKA {username}", SixLabors.ImageSharp.Color.White);
                    x.DrawImage(avatar, avatarPosition, 1);
                }
                x.DrawImage(frameImage, framePosition, 1);
                if (isMachine) x.DrawImage(solveAvatar, solveAvatarPosition, 1);
                if (solve.Type == "user") x.DrawImage(userImage, userPosition, 1);
                if (solve.Type == "root") x.DrawImage(rootImage, rootPosition, 1);
                x.DrawText(htbUser.Username, heading, SixLabors.ImageSharp.Color.White, new PointF(offset + 228, offset + 46));
                x.DrawText(bodyText, body, SixLabors.ImageSharp.Color.White, new PointF(offset + 228, offset + 110));
            });

            var stream = new MemoryStream();
            await image.SaveAsPngAsync(stream);
            stream.Seek(0, SeekOrigin.Begin);

            return stream;
        }
    }
}
