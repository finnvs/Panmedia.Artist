using System;
using System.Collections.Generic;
using System.Configuration;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using System.Web;
using Orchard.Logging;

namespace Panmedia.Artist.Handlers
{
    public class BlobHandler
    {
        // Retrieve storage account from app settings in web config of Orchard.Web project
        CloudStorageAccount storageAccount = CloudStorageAccount.Parse(
            ConfigurationManager.AppSettings["Orchard.Azure.Media.StorageConnectionString"]);


        private string imageDirectoryUrl;
        public ILogger Logger { get; set; }

        /// <summary>
        /// Receives the users Id for where the pictures are and creates
        /// a blob storage with that name if it does not exist.
        /// </summary>
        /// <param name="imageDirectoryUrl"></param>
        public BlobHandler(string imageDirectoryUrl)
        {
            Logger = NullLogger.Instance;

            this.imageDirectoryUrl = imageDirectoryUrl;
            // Create the blob client.
            CloudBlobClient blobClient = storageAccount.CreateCloudBlobClient();
            // Retrieve a reference to a container.
            CloudBlobContainer container =
            blobClient.GetContainerReference(imageDirectoryUrl);
            // Create the container if it doesn't already exist.
            container.CreateIfNotExists();
            //Make available to everyone
            container.SetPermissions(
            new BlobContainerPermissions
            {
                PublicAccess = BlobContainerPublicAccessType.Blob
            });

            // log call for server side panic relief
            Logger.Information("BlobHandler CTOR was executed");

        }

        // Bulk upload files
        public void Upload(IEnumerable<HttpPostedFileBase> files)
        {
            // Create the blob client
            CloudBlobClient blobClient = storageAccount.CreateCloudBlobClient();
            // Retrieve a reference to a container
            CloudBlobContainer container =
            blobClient.GetContainerReference(imageDirectoryUrl);
            if (files != null)
                foreach (var f in files)
                {
                    if (f != null)
                    {
                        CloudBlockBlob blockBlob =
                        container.GetBlockBlobReference(f.FileName);
                        blockBlob.UploadFromStream(f.InputStream);
                    }
                }
        }

        // Upload a single file
        public void Upload(HttpPostedFileBase singleFile)
        {
            // Create the blob client
            CloudBlobClient blobClient = storageAccount.CreateCloudBlobClient();
            // Retrieve a reference to a container
            CloudBlobContainer container =
                  blobClient.GetContainerReference(imageDirectoryUrl);
            if (singleFile != null)
            {
                CloudBlockBlob blockBlob =
                container.GetBlockBlobReference(singleFile.FileName);
                blockBlob.UploadFromStream(singleFile.InputStream);
            }

            // log call for server side panic relief
            Logger.Information("BlobHandler upload method was executed for file " + singleFile.FileName);
        }

        // Upload a single phile as a byte array (cropped image)
        public void Upload(byte[] singleFile, string filename)
        {
            // Create the blob client
            CloudBlobClient blobClient = storageAccount.CreateCloudBlobClient();
            // Retrieve a reference to a container
            CloudBlobContainer container =
                  blobClient.GetContainerReference(imageDirectoryUrl);
            if (singleFile != null)
            {
                CloudBlockBlob blockBlob =
                container.GetBlockBlobReference(filename);
                var byteCount = Buffer.ByteLength(singleFile);
                blockBlob.UploadFromByteArray(singleFile, 0, byteCount);
            }

            // log call for server side panic relief
            Logger.Information("BlobHandler upload byte array method was executed for file " + filename);
        }


        /// <summary>
        /// Method to generate a SAS token - not relevant for this example, as we are operating server side here,
        /// and the web app has full and unrestricted access to the blob storage key 
        /// </summary>
        /// <param name="container"></param>
        /// <returns></returns>
        private static string GetContainerSasUri(CloudBlobContainer container)
        {
            //Set the expiry time and permissions for the container.
            //In this case no start time is specified, so the shared access signature becomes valid immediately.
            SharedAccessBlobPolicy sasConstraints = new SharedAccessBlobPolicy();
            sasConstraints.SharedAccessExpiryTime = DateTime.UtcNow.AddHours(24);
            sasConstraints.Permissions = SharedAccessBlobPermissions.Write | SharedAccessBlobPermissions.List;

            //Generate the shared access signature on the container, setting the constraints directly on the signature.
            string sasContainerToken = container.GetSharedAccessSignature(sasConstraints);

            //Return the URI string for the container, including the SAS token.
            return container.Uri + sasContainerToken;
        }

        public List<string> GetBlobs()
        {
            // Create the blob client
            CloudBlobClient blobClient = storageAccount.CreateCloudBlobClient();
            // Retrieve reference to a previously created container
            CloudBlobContainer container =
            blobClient.GetContainerReference(imageDirectoryUrl);
            List<string> blobs = new List<string>();
            // Loop over blobs within the container and output the URI for each blob
            foreach (var blobItem in container.ListBlobs())
                blobs.Add(blobItem.Uri.ToString());
            return blobs;
        }
    }
}

