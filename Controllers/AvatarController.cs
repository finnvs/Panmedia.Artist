
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Helpers;
using System.Web.Mvc;
using Panmedia.Artist.Handlers;
using Panmedia.Artist.Models;
using Orchard;
using Orchard.ContentManagement;
using Orchard.Users.Models;
using Orchard.Localization;
using Orchard.UI.Notify;
using System.Net;

namespace Panmedia.Artist.Controllers
{
    public class AvatarController : Controller
    {
        private IOrchardServices Services { get; set; }
        public Localizer T { get; set; }
        private const int AvatarStoredWidth = 150;  // ToDo - Change the size of the stored avatar image
        private const int AvatarStoredHeight = 150; // ToDo - Change the size of the stored avatar image
        private const int AvatarScreenWidth = 400;  // ToDo - Change the value of the width of the image on the screen

        private const string TempFolder = "/Panmedia.Artist/Temp";
        private const string MapTempFolder = "~" + TempFolder;
        private const string AvatarPath = "/Panmedia.Artist/Content/Avatars";
        private const string AzureContainerPath = "http://panmediablob.blob.core.windows.net/aasv-container/";

        private readonly string[] _imageFileExtensions = { ".jpg", ".png", ".gif", ".jpeg" };

        public AvatarController(IOrchardServices services) { Services = services; }

        [HttpGet]
        public ActionResult Upload()
        {
            return View();
        }

        [HttpGet]
        public ActionResult _Upload()
        {
            return PartialView();
        }

        [ValidateAntiForgeryToken]
        public ActionResult _Upload(IEnumerable<HttpPostedFileBase> files)
        {
            if (files == null || !files.Any()) return Json(new { success = false, errorMessage = "No file uploaded." });
            var file = files.FirstOrDefault();  // get ONE only
            if (file == null || !IsImage(file)) return Json(new { success = false, errorMessage = "File is of wrong format." });
            if (file.ContentLength <= 0) return Json(new { success = false, errorMessage = "File cannot be zero length." });

            // Store the uploaded file in aasv blob container
            var img = new WebImage(file.InputStream);
            var ratio = img.Height / (double)img.Width;
            img.Resize(AvatarScreenWidth, (int)(AvatarScreenWidth * ratio));
            var fileName = Path.GetFileName(file.FileName); // shorten to real filename
            byte[] imgBytes = img.GetBytes();
            BlobHandler bh = new BlobHandler("aasv-container");
            bh.Upload(imgBytes, fileName);                        
            // bh.Upload(file); // Ouch! meeega giga canvas m billedet, hvis img er ex ca 4 Mb..

            // var webPath = GetTempSavedFilePath(file);  // TODO: save file to Azure aasv blob instead
            // replacing '\' to '//' results in incorrect image url on firefox and IE,
            // therefore replacing '\\' to '/' so that a proper web url is returned.
                        
            var imageUrl = AzureContainerPath + file.FileName; 
            return Json(new { success = true, fileName = imageUrl }); // webPath.Replace("\\", "/") }); // success
        }

        [HttpPost]
        public ActionResult Save(string t, string l, string h, string w, string fileName, int rotateVal = 0)
        {            
            try
            {
                // Calculate dimensions
                var top = Convert.ToInt32(t.Replace("-", "").Replace("px", ""));
                var left = Convert.ToInt32(l.Replace("-", "").Replace("px", ""));
                var height = Convert.ToInt32(h.Replace("-", "").Replace("px", ""));
                var width = Convert.ToInt32(w.Replace("-", "").Replace("px", ""));

                // Get file path to temporary folder - this gets tricky w Azure, ref
                // https://code.msdn.microsoft.com/How-to-store-temp-files-in-d33bbb10 and
                // https://blog.codingoutloud.com/2011/06/12/azure-faq-can-i-write-to-the-file-system-on-windows-azure/
                // var fn = "C:/Users/fvs/Documents/My Web Sites/Orchard.Web-Site/Panmedia.Artist/Temp/" + Path.GetFileName(fileName); // temporary hack
                // var fn = Path.Combine(Server.MapPath(MapTempFolder), Path.GetFileName(fileName)); // original

                // byte[] profileImageBytes = null;
                //Stream profileImageStream = null;
                //WebClient client = new WebClient();
                //client.OpenReadCompleted += (s, e) =>
                //{
                //    byte[] imageBytes = new byte[e.Result.Length];
                //    e.Result.Read(imageBytes, 0, imageBytes.Length);                    
                //    profileImageStream = e.Result;
                //    // profileImageBytes = imageBytes;                    
                //};
                //client.OpenReadAsync(new Uri(fileName));

                WebImage img = null;
                string path = fileName;
                WebRequest request = WebRequest.Create(path);                               
                WebResponse response = request.GetResponse();

                using (Stream stream = response.GetResponseStream())
                {
                    img = new WebImage(stream);                    
                }
                // ...get image and resize it, ...
                // var img = new WebImage(profileImageStream); // url er ikke gangbar mønt, skal være en fil eller et byte array
                img.Resize(width, height);
                // ... crop the part the user selected, ...
                img.Crop(top, left, img.Height - top - AvatarStoredHeight, img.Width - left - AvatarStoredWidth);
                
                // if a rotation was performed clientside, rotate the cropped image accordingly
                if(rotateVal == -90)
                    img.RotateLeft();
                if (rotateVal == 90)
                    img.RotateRight();
                if (rotateVal == 180)
                    img.FlipVertical();

                // ... delete the temporary file,...
                // System.IO.File.Delete(fn);
                // ... and save the new one.

                // Store in blob container - this will overwirte, so no need to delete original.
                BlobHandler bh = new BlobHandler("aasv-container");
                fileName = Path.GetFileName(fileName); // shorten from src attribute to real filename
                byte[] imgBytes =  img.GetBytes(); // byte[] bytes =
                bh.Upload(imgBytes, fileName);

                string profilePicUrl = SetArtistProfileData(fileName);

                // Store image in server file system (not applicable in regard to Azure Blob Storage)
                //var newFileName = Path.Combine(AvatarPath, Path.GetFileName(fn));
                //var newFileLocation = HttpContext.Server.MapPath(newFileName);
                //if (Directory.Exists(Path.GetDirectoryName(newFileLocation)) == false)
                //{
                //    Directory.CreateDirectory(Path.GetDirectoryName(newFileLocation));
                //}
                //img.Save(newFileLocation);

                // var redirect = RedirectToAction("Index/" + Services.WorkContext.CurrentUser.UserName, "Artist"); // gives an object..
                // var redirect = "Profile/" + Services.WorkContext.CurrentUser.ContentItem.Id; // this could be used instead, no email in url

                // There are problems in production with Url.Content("~") - so, just pass Profile/username, 
                // and we can add site root url later in Avatar.js (baseurl var).
                // var redirect = Url.Content("~") + "/Profile/" + Services.WorkContext.CurrentUser.UserName;
                var redirect = "Profile/" + Services.WorkContext.CurrentUser.UserName;
                return Json(new { success = true, avatarFileLocation = profilePicUrl, redirecturl = redirect });
                
                // return RedirectToAction("Index/" + Services.WorkContext.CurrentUser.UserName, "Artist" );
            }
            catch (Exception ex)
            {
                return Json(new { success = false, errorMessage = "Unable to upload file.\nERRORINFO: " + ex.Message });
            }
        }

        private bool IsImage(HttpPostedFileBase file)
        {
            if (file == null) return false;
            return file.ContentType.Contains("image") ||
                _imageFileExtensions.Any(item => file.FileName.EndsWith(item, StringComparison.OrdinalIgnoreCase));
        }

        private string GetTempSavedFilePath(HttpPostedFileBase file)
        {
            // Define destination
            var serverPath = HttpContext.Server.MapPath(TempFolder);
            if (Directory.Exists(serverPath) == false)
            {
                Directory.CreateDirectory(serverPath);
            }

            // Generate unique file name
            var fileName = Path.GetFileName(file.FileName);
            fileName = SaveTemporaryAvatarFileImage(file, serverPath, fileName);

            // Clean up old files after every save
            CleanUpTempFolder(1);
            return Path.Combine(TempFolder, fileName);
        }

        private static string SaveTemporaryAvatarFileImage(HttpPostedFileBase file, string serverPath, string fileName)
        {
            var img = new WebImage(file.InputStream);
            var ratio = img.Height / (double)img.Width;
            img.Resize(AvatarScreenWidth, (int)(AvatarScreenWidth * ratio));

            var fullFileName = Path.Combine(serverPath, fileName);
            if (System.IO.File.Exists(fullFileName))
            {
                System.IO.File.Delete(fullFileName);
            }

            img.Save(fullFileName);
            return Path.GetFileName(img.FileName);
        }

        private void CleanUpTempFolder(int hoursOld)
        {
            try
            {
                var currentUtcNow = DateTime.UtcNow;
                var serverPath = HttpContext.Server.MapPath("/Temp");
                if (!Directory.Exists(serverPath)) return;
                var fileEntries = Directory.GetFiles(serverPath);
                foreach (var fileEntry in fileEntries)
                {
                    var fileCreationTime = System.IO.File.GetCreationTimeUtc(fileEntry);
                    var res = currentUtcNow - fileCreationTime;
                    if (res.TotalHours > hoursOld)
                    {
                        System.IO.File.Delete(fileEntry);
                    }
                }
            }
            catch
            {
                // Deliberately empty.
            }
        }

        // TODO: remove magic string for blob storage, make it configurable via web config or in Orchards admin panel
        private string SetArtistProfileData(string fileName)
        {
            try
            {
                var id = Services.WorkContext.CurrentUser.ContentItem.Id;
                var artist = Services.ContentManager.Get(id);
                if (artist == null)
                {
                    Services.Notifier.Information(T("Profilen kunne ikke hentes - kontakt venligst webmaster@sang-skriver.dk."));
                    return "no profile data";
                }
                var artistProfilePart = artist.As<ProfilePart>();
                // var artistUserPart = artist.As<UserPart>();                
                artistProfilePart.ProfilePictureURL = AzureContainerPath + fileName;
                artistProfilePart.ProfilePictureName = fileName;
                //artistProfilePart.ArtistContentItemId = artist.Id; 
                //artistProfilePart.UserContentItemId = artistUserPart.Id;
                                
                Services.Notifier.Information(T("Dit profilbillede er redigeret"));
                // Publish content item
                Services.ContentManager.Publish(artist);
                return artistProfilePart.ProfilePictureURL;
            }
            catch (Exception e)
            {
                Services.Notifier.Information(T("Der var et problem med at opdatere profildata: " + e.Message));
                return "Exception - no profile data";
            }

        }
    }
}