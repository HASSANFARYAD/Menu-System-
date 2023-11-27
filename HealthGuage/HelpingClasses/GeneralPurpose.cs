using HealthGuage.Models;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;
using System.Security.Claims;
using Template.HelpingClasses;
using SixLabors.ImageSharp.Formats.Jpeg;
using Color = SixLabors.ImageSharp.Color;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;


namespace HealthGuage.HelpingClasses
{
    public class GeneralPurpose
    {
        private readonly HttpContext hcontext;
        public GeneralPurpose(IHttpContextAccessor haccess)
        {
            hcontext = haccess.HttpContext;
        }

        public UserDto? GetUserClaims()
        {
            string? userId = hcontext?.User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Sid)?.Value;
            string? encId = hcontext?.User.Claims.FirstOrDefault(c => c.Type == "EncId")?.Value;
            string? name = hcontext?.User.Claims.FirstOrDefault(c => c.Type == "UserName")?.Value;
            string? email = hcontext?.User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Email)?.Value;
            string? role = hcontext?.User.Claims.FirstOrDefault(c => c.Type == "Role")?.Value;
            string? profile = hcontext?.User.Claims.FirstOrDefault(c => c.Type == "Profile")?.Value;

            UserDto? loggedInUser = null;
            if (userId != null)
            {
                loggedInUser = new UserDto()
                {
                    Id = userId,
                    EncId = encId,
                    Name = name,
                    Email = email,
                    Role = Convert.ToInt32(role),
                    Profile = string.IsNullOrEmpty(profile) ? "" : profile
                };
            }
            
            return loggedInUser;
        }

        public async Task<bool> SetUserClaims(User user)
        {
            try
            {
                List<Claim> claims = new List<Claim>()
                {
                    new Claim(ClaimTypes.Sid, user.Id.ToString()),
                    new Claim("EncId", StringCipher.EncryptId(user.Id)),
                    new Claim("UserName", user.Name),
                    new Claim(ClaimTypes.Email, user.Email),
                    new Claim("Role", user.Role.ToString()),
                    new Claim("Profile", string.IsNullOrEmpty(user.ProfilePicture) ? "" : user.ProfilePicture.ToString()),
                };


                var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                var principal = new ClaimsPrincipal(identity);

                await hcontext.SignInAsync(
                    CookieAuthenticationDefaults.AuthenticationScheme, 
                    principal, 
                    new AuthenticationProperties { IsPersistent = true }
                );

                return true;
            }
            catch 
            {
                return false;
            }
        }

        public static DateTime DateTimeNow()
        {
            return DateTime.Now;
        }

        public static async Task<string> UploadProfilePicture(IFormFile file, string oldProfile = "", string way = "")
        {
            try
            {
                // Define the root folder where you want to save the file
                string rootFolder = Path.Combine(Directory.GetCurrentDirectory(), @"wwwroot\UserProfile\");
                string fileFirstName = "ProfileImage";
                if (!string.IsNullOrEmpty(way))
                {
                    rootFolder = Path.Combine(Directory.GetCurrentDirectory(), @"wwwroot\UserTile\");
                    fileFirstName = "TileImage";
                }

                if (!Directory.Exists(rootFolder))
                {
                    Directory.CreateDirectory(rootFolder);
                }

                // Generate a unique filename for the uploaded file
                string fileName = fileFirstName + DateTime.Now.Ticks + Path.GetExtension(file.FileName);

                // Combine the root folder and the generated filename
                string filePath = Path.Combine(rootFolder, fileName);

                if (!string.IsNullOrEmpty(oldProfile))
                {
                    string[] getName = oldProfile.Split("/");

                    // Check if a profile picture already exists for the user
                    string existingProfilePicturePath = Path.Combine(rootFolder, getName.Last());
                    if (System.IO.File.Exists(existingProfilePicturePath))
                    {
                        System.IO.File.Delete(existingProfilePicturePath);
                    }
                }
                
                //if (!string.IsNullOrEmpty(way))
                //{
                //    // Resize the image before saving
                //    using (var image = SixLabors.ImageSharp.Image.Load(file.OpenReadStream()))
                //    {
                //        var imageEncoder = new JpegEncoder
                //        {
                //            Quality = 100 // Set JPEG quality to 100 (maximum quality)
                //        };

                //        var width = 220;
                //        var height = 220;
                //        if (image.Width > image.Height)
                //        {
                //            height = (int)((image.Height / (double)image.Width) * width);
                //        }
                //        else
                //        {
                //            width = (int)((image.Width / (double)image.Height) * height);
                //        }

                //        image.Mutate(x => x.Resize(new ResizeOptions
                //        {
                //            //Size = new Size(220, 220), // Resize to 220x220 pixels
                //            Size = new Size(width, height), // Resize to 220x220 pixels
                //            Mode = ResizeMode.Max,
                //            //Position = AnchorPositionMode.Center,
                //        }));
                //        //Save the resized image with high quality
                //        image.Save(filePath, imageEncoder);

                //        //// Resize the image to fit within 220x220
                //        //image.Mutate(x => x.Resize(new ResizeOptions
                //        //{
                //        //    Size = new Size(220, 220),
                //        //    Mode = ResizeMode.BoxPad,
                //        //    Position = AnchorPositionMode.Center,
                //        //}));

                //        //// Create a new image with the desired size and fill it with the background color
                //        //using (var newImage = new Image<Rgba32>(configuration: new Configuration(), 220, 220, Color.White))
                //        //{
                //        //    // Overlay the resized image onto the new image
                //        //    newImage.Mutate(ctx => ctx.DrawImage(image, new Point(0, 0), 1f));

                //        //    // Save the final image
                //        //    newImage.Save(filePath, imageEncoder);
                //        //}
                //    }
                //}
                //else
                //{
                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await file.CopyToAsync(stream);
                    }
                //}

                

                // You can save the filePath to a database or return it as needed
                Console.WriteLine($"File uploaded to: {filePath}");

                if (!string.IsNullOrEmpty(way))
                {
                    return "/UserTile/" + fileName;
                }
                else
                {
                    return "/UserProfile/" + fileName;
                }
                
            }
            catch (Exception ex)
            {
                return null;
            }
        }

    }
}
