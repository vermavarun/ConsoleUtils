using System;
using System.Collections.Generic;

namespace ENGIEImpact.ClientDataFeeds.AzureFunctions.Helper
{
   
    using Microsoft.WindowsAzure.Storage;
    using Microsoft.WindowsAzure.Storage.Auth;
    using Microsoft.WindowsAzure.Storage.Blob;
    using Microsoft.WindowsAzure.Storage.RetryPolicies;
    using System.IO;
    using System.Threading.Tasks;

    public class BlobContainerHelper : IBlobContainerHelper
    {
        #region Properties

        #region private Members

        private readonly object cloudBlobClientLock = new object();
        private readonly object cloudBlobContainerLock = new object();

        private CloudBlobClient cloudBlobClient;
        private CloudBlobContainer cloudBlobContainer;

        #endregion

        #region Public Properties

        /// <summary>
        ///     Singleton object for Storage Credentials for Azure Account
        /// </summary>
        private StorageCredentials StorageCredentials
        {
            get => Singleton<StorageCredentials>.GetInstance("ftpsource",
                "qzDkh/hT4HA4WbO6xaIKvZlM7PCVUZpfN0tUqEFw/WGzEX8Z7bFGDi7yB6lNX5Mcpj5q4UwbpyyWyMJUqX2sQw==");
        }

        /// <summary>
        ///     Singleton object for Storage in Azure Account
        /// </summary>
        private CloudStorageAccount CloudStorageAccount
        {
            get => Singleton<CloudStorageAccount>.GetInstance(StorageCredentials, true);
        }

        /// <summary>
        ///     Singleton object for Blob Client
        /// </summary>
        public CloudBlobClient CloudBlobClient
        {
            get
            {
                if (cloudBlobClient != null)
                {
                    return cloudBlobClient;
                }

                lock (cloudBlobClientLock)
                {
                    if (cloudBlobClient == null)
                    {
                        cloudBlobClient = CloudStorageAccount.CreateCloudBlobClient();
                    }
                }

                return cloudBlobClient;
            }
        }

        /// <summary>
        ///     Singleton object for Blob Container
        /// </summary>
        public CloudBlobContainer CloudBlobContainer
        {
            get
            {
                if (cloudBlobContainer != null)
                {
                    return cloudBlobContainer;
                }

                lock (cloudBlobContainerLock)
                {
                    if (cloudBlobContainer == null)
                    {
                        cloudBlobContainer =
                            CloudBlobClient.GetContainerReference("ftpblob");
                    }
                }

                return cloudBlobContainer;
            }
        }

        #endregion

        #region Public Methods

        /// <summary>
        ///     Method to get File array from Azure Container
        /// </summary>
        /// <returns></returns>
        public async Task<byte[]> GetBlobAsync(string fileLocation) =>
            await GetBlobAsync(CloudBlobContainer.GetBlockBlobReference(fileLocation));

        public async Task<byte[]> GetBlobAsync(ICloudBlob blockBlob) => (await GetBlobStreamAsync(blockBlob)).ToArray();

        /// <summary>
        ///     Method to get File array from Azure Container
        /// </summary>
        /// <returns></returns>
        public async Task<ICloudBlob> GetCloudBlobAsync(string fileLocation, CloudBlobContainer blobContainer)
        {
            CloudBlockBlob blockBlob = (blobContainer ?? CloudBlobContainer).GetBlockBlobReference(fileLocation);
            await blockBlob.FetchAttributesAsync();

            return blockBlob;
        }

        /// <summary>
        ///     Method to get File array from Azure Container
        /// </summary>
        /// <returns></returns>
        public async Task UploadBlobAsync(string fileLocation, byte[] bytes, CloudBlobContainer blobContainer)
        {
            CloudBlockBlob blockBlob = (blobContainer ?? CloudBlobContainer).GetBlockBlobReference(fileLocation);

            // Download Byte array of specified blob in byte array variable
            await blockBlob.UploadFromByteArrayAsync(bytes, 0, bytes.Length);
        }

        public async Task<MemoryStream> GetBlobStreamAsync(ICloudBlob blockBlob)
        {
            MemoryStream memoryStream = new MemoryStream();

            // Download Byte array of specified blob in byte array variable
            await blockBlob.DownloadToStreamAsync(memoryStream);

            return memoryStream;
        }

        public async Task ForEachBlobAsync(CloudBlobContainer container, Action<CloudBlockBlob> action)
        {
            BlobContinuationToken dirToken = null;
            do
            {
                BlobResultSegment dirResult = await container.ListBlobsSegmentedAsync(
                    null,
                    false,
                    BlobListingDetails.None,
                    int.MaxValue,
                    dirToken,
                    new BlobRequestOptions { LocationMode = LocationMode.SecondaryThenPrimary },
                    null);

                dirToken = dirResult.ContinuationToken;

                await ForEachBlobAsync(dirResult.Results, action);
            } while (dirToken != null);
        }

        private async Task ForEachBlobAsync(IEnumerable<IListBlobItem> results,
            Action<CloudBlockBlob> action)
        {
            foreach (IListBlobItem item in results)
            {
                switch (item)
                {
                    case CloudBlobDirectory dir:
                        {
                            Console.WriteLine($"DIRECTORY: {dir.Uri}");
                            BlobContinuationToken blobResultContinuationToken = null;

                            do
                            {
                                BlobResultSegment blobResult = await dir.ListBlobsSegmentedAsync(
                                    false,
                                    BlobListingDetails.None,
                                    int.MaxValue,
                                    blobResultContinuationToken,
                                    new BlobRequestOptions { LocationMode = LocationMode.SecondaryThenPrimary },
                                    null);
                                await ForEachBlobAsync(blobResult.Results, action);
                                blobResultContinuationToken = blobResult.ContinuationToken;
                            } while (blobResultContinuationToken != null);

                            break;
                        }

                    case CloudBlockBlob blob:
                        {
                            action(blob);

                            break;
                        }
                }
            }

            #endregion

            #endregion
        }
    }
}