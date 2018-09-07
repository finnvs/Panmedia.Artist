using Panmedia.Artist.Handlers;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;

namespace Panmedia.Artist.Controllers
{
    public class ImageController : Controller
    {
        [HttpPost]
        public ActionResult CropImage(
            string imagePath,
            // byte[] imageArray,
            int? cropPointX,
            int? cropPointY,
            int? imageCropWidth,
            int? imageCropHeight)
        {
            if (string.IsNullOrEmpty(imagePath)
            // if (imageArray == null
                || !cropPointX.HasValue
                || !cropPointY.HasValue
                || !imageCropWidth.HasValue
                || !imageCropHeight.HasValue)
            {
                return new HttpStatusCodeResult((int)HttpStatusCode.BadRequest);
            }

            // byte[] imageBytes = imageArray;
            byte[] imageBytes = System.IO.File.ReadAllBytes(Server.MapPath(imagePath));
            byte[] croppedImage = ImageHandler.CropImage(imageBytes, cropPointX.Value, cropPointY.Value, imageCropWidth.Value, imageCropHeight.Value);

            string tempFolderName = Server.MapPath("~/" + ConfigurationManager.AppSettings["Image.TempFolderName"]);

            string fileNameWithoutExtension = Path.GetFileNameWithoutExtension(imagePath);
            string fileName = Path.GetFileName(imagePath).Replace(fileNameWithoutExtension, fileNameWithoutExtension + "_cropped");

            try
            {
                // FileHelper.SaveFile(croppedImage, Path.Combine(tempFolderName, fileName));               
                BlobHandler bh = new BlobHandler("aasv-container");
                bh.Upload(croppedImage, fileName);
            }
            catch (Exception)
            {
                //Log an error     
                return new HttpStatusCodeResult((int)HttpStatusCode.InternalServerError);
            }

            string photoPath = string.Concat("/", ConfigurationManager.AppSettings["Image.TempFolderName"], "/", fileName);
            return Json(new { photoPath = photoPath }, JsonRequestBehavior.AllowGet);
        }
    }
}