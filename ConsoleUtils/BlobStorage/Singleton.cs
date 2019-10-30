using System;

namespace ENGIEImpact.ClientDataFeeds.AzureFunctions.Helper
{
    public class Singleton<T> where T : class
    {
        #region Fields

        private static readonly object lockObject = new object();

        private static T instance;

        #endregion

        #region Constructors & Destructors

        private Singleton()
        {
        }

        #endregion

        #region Methods

        #region Public Methods

        public static T GetInstance(params object[] args)
        {
            if (instance != null)
            {
                return instance;
            }

            lock (lockObject)
            {
                return instance ?? (instance = Activator.CreateInstance(typeof(T), args) as T);
            }
        }

        #endregion

        #endregion
    }
}
