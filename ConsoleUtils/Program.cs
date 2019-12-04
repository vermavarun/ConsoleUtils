namespace ConsoleUtils
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Net;
    using System.Threading;
    using System.Threading.Tasks;
    using System.Timers;
    using ENGIEImpact.ClientDataFeeds.AzureFunctions.Helper;
    using Microsoft.WindowsAzure.Storage.Blob;
    using Microsoft.WindowsAzure.Storage.RetryPolicies;
    using Renci.SshNet;
    using static System.Net.Mime.MediaTypeNames;
    using Timer = System.Threading.Timer;

    internal class Program
    {

        #region Fields

        private static IBlobContainerHelper blobContainerHelper;
        private static CloudBlobContainer ftpBlobContainer;

        #endregion

        #region Methods

        #region Private Methods

        private static async Task<int> DownloadAndUploadFTPInChunks()
        {
            ServicePointManager.Expect100Continue = false;

            blobContainerHelper = new BlobContainerHelper();
            string containerName = "ftpblob";
            ftpBlobContainer =
                blobContainerHelper.CloudBlobClient.GetContainerReference(containerName);
            string blobName = "2. Setting up the development environment.mp4";
            ICloudBlob fetchedblob = blobContainerHelper
                .GetCloudBlobAsync(blobName, ftpBlobContainer).GetAwaiter().GetResult();

            int segmentSize = 1 * 1024 * 1024; //1 MB chunk

            long blobLengthRemaining = fetchedblob.Properties.Length;
            long startPosition = 0;

            FtpWebRequest request =
                (FtpWebRequest)WebRequest.Create("ftp://13.66.250.56/" + blobName);
            request.Credentials = new NetworkCredential("user1", "user1");
            request.Method = WebRequestMethods.Ftp.UploadFile;
            request.Timeout = int.MaxValue;
            Stream requestStream = request.GetRequestStream();
            BlobRequestOptions blobRequestOptions = new BlobRequestOptions
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
                        await fetchedblob.DownloadRangeToStreamAsync(ms, startPosition, blockSize, null,
                            blobRequestOptions, null);
                        ms.Position = 0;
                        ms.Read(blobContents, 0, blobContents.Length);

                        requestStream.Write(blobContents, 0, (int)blockSize);
                    }

                    startPosition += blockSize;
                    blobLengthRemaining -= blockSize;
                    Console.Clear();
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


        private static async Task<int> DownloadAndUploadSFTPInChunks()
        {
            ServicePointManager.Expect100Continue = false;

            blobContainerHelper = new BlobContainerHelper();
            string containerName = "ftpblob";
            ftpBlobContainer =
                blobContainerHelper.CloudBlobClient.GetContainerReference(containerName);
            string blobName = "2017-Scrum-Guide-US.pdf";
            ICloudBlob fetchedblob = blobContainerHelper
                .GetCloudBlobAsync(blobName, ftpBlobContainer).GetAwaiter().GetResult();

            int segmentSize = 1 * 1024 * 1024; //1 MB chunk

            long blobLengthRemaining = fetchedblob.Properties.Length;
            long startPosition = 0;

            //FtpWebRequest request = (FtpWebRequest)WebRequest.Create("ftp://13.66.250.56/" + blobName);

            //request.Credentials = new NetworkCredential("user1", "user1");
            //request.Method = WebRequestMethods.Ftp.UploadFile;
            //request.Timeout = int.MaxValue;

            var client = new SftpClient("52.183.65.75", 22, "user1", "user1");
            client.Connect();
            if (!client.IsConnected)
            {
                Console.WriteLine("Not able to connect sftp");
                return 1;
            }

            Stream requestStream = client.OpenWrite(blobName); // .GetRequestStream();
            BlobRequestOptions blobRequestOptions = new BlobRequestOptions
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
                        await fetchedblob.DownloadRangeToStreamAsync(ms, startPosition, blockSize, null,
                            blobRequestOptions, null);
                        ms.Position = 0;
                        ms.Read(blobContents, 0, blobContents.Length);

                        requestStream.Write(blobContents, 0, (int)blockSize);
                    }

                    startPosition += blockSize;
                    blobLengthRemaining -= blockSize;
                    Console.Clear();
                    Console.WriteLine($"Remaining bytes : {blobLengthRemaining}");
                } while (blobLengthRemaining > 0);

                requestStream.Close();
                //using (FtpWebResponse response = (FtpWebResponse)request.GetResponse())
                //{
                //    Console.WriteLine($"Upload File Complete, status {response.StatusDescription}");
                //}



                return 1;
            }
            catch (Exception ex)
            {
                client.Disconnect();
                client.Dispose();
                Console.WriteLine("Error");
            }
            finally
            {
                client.Disconnect();
                client.Dispose();
                requestStream.Close();
            }

            return 1;
        }

        private static void Main(string[] args)
        {
            //DownloadAndUploadFTPInChunks().Wait();
            //DownloadAndUploadSFTPInChunks().Wait();
            UploadFileInBlobInChunksAsync().Wait();

        }

        private static async Task<int> UploadFileInBlobInChunksAsync()
        {
            blobContainerHelper = new BlobContainerHelper();
            string containerName = "ftpblob";
            ftpBlobContainer =
              blobContainerHelper.CloudBlobClient.GetContainerReference(containerName);
            string blobName = "koinaam.txt";
            //ICloudBlob fetchedblob = blobContainerHelper
            //    .GetCloudBlobAsync(blobName, ftpBlobContainer).GetAwaiter().GetResult();

            CloudAppendBlob blob = ftpBlobContainer.GetAppendBlobReference(blobName);

            bool ifexsist = await blob.ExistsAsync();
           
          

            List<string> mylist;
            for (int i = 0; i <= 100; i++)
            {
                if (ifexsist)
                {

                    mylist = new List<string>(new string[] { "element" + i, "element" + i, "element" + i });
                    var result = String.Join(", ", mylist.ToArray()) + Environment.NewLine;

                    //byte[] dataAsBytes = mylist
                    //      .SelectMany(s => System.Text.Encoding.ASCII.GetBytes(s))
                    //      .ToArray();

                    byte[] dataAsBytes = System.Text.Encoding.ASCII.GetBytes(result);
                    await blob.AppendFromByteArrayAsync(dataAsBytes, 0, dataAsBytes.Length);
                }
                else
                {
                    await blob.CreateOrReplaceAsync();
                    ifexsist = true;
                }

              
            }
            return 1;
        }

        #endregion

        #endregion
    }
}