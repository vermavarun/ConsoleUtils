using System;

namespace ConsoleUtils
{
    using Microsoft.WindowsAPICodePack.Shell.PropertySystem;
    using System.IO;
    using System.Linq;
    using System.Net;
    using System.Threading.Tasks;
    using Microsoft.WindowsAPICodePack.Shell;
    using Microsoft.WindowsAzure.Storage;
    using System.Globalization;
    using ENGIEImpact.ClientDataFeeds.AzureFunctions.Helper;
    using Microsoft.WindowsAzure.Storage.Auth;
    using Microsoft.WindowsAzure.Storage.Blob;
    using Microsoft.WindowsAzure.Storage.RetryPolicies;
    
    class Program
    {
        private static IBlobContainerHelper blobContainerHelper;
        private static CloudBlobContainer ftpBlobContainer;
        static void Main(string[] args)
        {
         
             DownloadAndUploadFTPInChunks().Wait();
        }


      
        private static async Task<int> DownloadAndUploadFTPInChunks()
        {
            System.Net.ServicePointManager.Expect100Continue = false;

            blobContainerHelper = new BlobContainerHelper();
            var containerName = "ftpblob";
            ftpBlobContainer =
                blobContainerHelper.CloudBlobClient.GetContainerReference(containerName);
            var blobName = "2. Setting up the development environment.mp4";
            ICloudBlob fetchedblob = blobContainerHelper
                .GetCloudBlobAsync(blobName, ftpBlobContainer).GetAwaiter().GetResult();
           


            // var cloudStorageAccount = CloudStorageAccount.DevelopmentStorageAccount;
           
           
            int segmentSize = 1 * 1024 * 1024;//1 MB chunk
                                              // var blobContainer = cloudStorageAccount.CreateCloudBlobClient().GetContainerReference(containerName);
                                              //  var fetchedblob = blobContainer.GetBlockBlobReference(blobName);

                                              string saveFileName = @"D:\"+ blobName;

            long blobLengthRemaining = fetchedblob.Properties.Length;
            long startPosition = 0;

            FtpWebRequest request =
                (FtpWebRequest)WebRequest.Create("ftp://13.66.250.56/"+ blobName);
            request.Credentials = new NetworkCredential("user1", "user1");
            request.Method = WebRequestMethods.Ftp.UploadFile;
            request.Timeout = int.MaxValue;
            Stream requestStream = request.GetRequestStream();
            var blobRequestOptions = new BlobRequestOptions
            {
                RetryPolicy = new ExponentialRetry(TimeSpan.FromSeconds(60), 10),
                MaximumExecutionTime = TimeSpan.FromMinutes(60),
                ServerTimeout = TimeSpan.FromMinutes(60)
            };
            try
            {
                do
                {
                    long blockSize = Math.Min(segmentSize, blobLengthRemaining);
                    byte[] blobContents = new byte[blockSize];
                    using (MemoryStream ms = new MemoryStream())
                    {
                        await fetchedblob.DownloadRangeToStreamAsync(ms, startPosition, blockSize,null, blobRequestOptions,null);
                        ms.Position = 0;
                        ms.Read(blobContents, 0, blobContents.Length);
                        ////using (FileStream fs = new FileStream(saveFileName, FileMode.OpenOrCreate))
                        ////{
                        ////    fs.Position = startPosition;
                        ////    fs.Write(blobContents, 0, blobContents.Length);
                        ////}

                        //Stream ftpStream = request.GetRequestStream();
                        // ftpStream.Write(blobContents, 0, (int)blockSize);

                        //using (requestStream = request.GetRequestStream())
                        //{
                        
                        requestStream.Write(blobContents, 0, (int)blockSize);
                        //}
                    }

                    startPosition += blockSize;
                    blobLengthRemaining -= blockSize; Console.Clear();
                    Console.WriteLine($"Remaining bytes : {blobLengthRemaining}");
                    
                } while (blobLengthRemaining > 0);
                requestStream.Close();
                using (FtpWebResponse response = (FtpWebResponse)request.GetResponse())
                {
                    Console.WriteLine($"Upload File Complete, status {response.StatusDescription}");

                }
                return 1;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error");
            }
            finally
            {

                requestStream.Close();
            }

            return 1;
        }




    }
}
