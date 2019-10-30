using System;

namespace ENGIEImpact.ClientDataFeeds.AzureFunctions.Helper
{
    using Microsoft.WindowsAzure.Storage.Blob;
    using System.IO;
    using System.Threading.Tasks;

    public interface IBlobContainerHelper
    {
        #region Properties

        #region Public Properties

        /// <summary>
        ///     Singleton object for Blob Client
        /// </summary>
        CloudBlobClient CloudBlobClient { get; }

        /// <summary>
        ///     Singleton object for Blob Container
        /// </summary>
        CloudBlobContainer CloudBlobContainer { get; }

        #endregion

        #endregion

        #region Methods

        #region Public Methods

        Task ForEachBlobAsync(CloudBlobContainer container, Action<CloudBlockBlob> action);

        /// <summary>
        ///     Method to get File array from Azure Container
        /// </summary>
        /// <returns></returns>
        Task<byte[]> GetBlobAsync(string fileLocation);

        Task<byte[]> GetBlobAsync(ICloudBlob blockBlob);

        Task<MemoryStream> GetBlobStreamAsync(ICloudBlob tifCloudBlob);

        /// <summary>
        ///     Method to get File array from Azure Container
        /// </summary>
        /// <returns></returns>
        Task<ICloudBlob> GetCloudBlobAsync(string fileLocation, CloudBlobContainer blobContainer);

        /// <summary>
        ///     Method to get File array from Azure Container
        /// </summary>
        /// <returns></returns>
        Task UploadBlobAsync(string fileLocation, byte[] bytes, CloudBlobContainer blobContainer);

        #endregion

        #endregion
    }
}
