using Microsoft.WindowsAzure;
using Smaprat.Data.Repositories;
using System;
using System.Configuration;
using System.Diagnostics;

namespace Smaprat.Data
{
    /// <summary>
    /// Type to access the <see cref="IUserRepository"/> instance which is used to retrieve and persist user connection information.
    /// </summary>
    public sealed class UserRepository
    {
        /// <summary>
        /// Name of the setting which contains the connection string to Azure Table Storage.
        /// </summary>
        /// <remarks>If blank, in-memory storage will be used instead.</remarks>
        private const string TableStorageConnectionStringSettingName = "site:StorageConnectionString";

        private static volatile IUserRepository _userRepositoryInstance;
        private static object syncRoot = new object();

        /// <summary>
        /// Gets an instance of an <see cref="IUserRepository"/> which is used to retrieve and persist user connection information.
        /// </summary>
        public static IUserRepository Instance
        {
            get
            {
                if (_userRepositoryInstance == null)
                {
                    lock (syncRoot)
                    {
                        if (_userRepositoryInstance == null)
                            _userRepositoryInstance = Create();
                    }
                }

                return _userRepositoryInstance;
            }
        }

        /// <summary>
        /// Initializes a new <see cref="UserRepository"/>.
        /// </summary>
        private UserRepository()
        { }

        /// <summary>
        /// Creates a new instance of an <see cref="IUserRepository"/>.
        /// </summary>
        /// <returns>A new instance of an <see cref="IUserRepository"/>.</returns>
        /// <remarks>If a table storage connection string has been configured, a <see cref="AzureTableStorageUserRepository"/> instance is returned, otherwise 
        /// an <see cref="InMemoryUserRepository"/> is returned.</remarks>
        private static IUserRepository Create()
        {
            string tableStorageConnectionString = 
                ConfigurationManager.AppSettings[TableStorageConnectionStringSettingName];

            if (!String.IsNullOrWhiteSpace(tableStorageConnectionString))
                return CreateAzureTableStorageRepository(tableStorageConnectionString);
            else
                return CreateInMemoryRepository();
        }

        /// <summary>
        /// Creates a new instance of an <see cref="AzureTableStorageUserRepository"/>.
        /// </summary>
        /// <returns>A new instance of an <see cref="AzureTableStorageUserRepository"/>.</returns>
        private static IUserRepository CreateAzureTableStorageRepository(string tableStorageConnectionString)
        {
            Trace.TraceInformation("Creating Azure Table Storage user repository.");
            return new AzureTableStorageUserRepository(tableStorageConnectionString);
        }

        /// <summary>
        /// Creates a new instance of an <see cref="InMemoryUserRepository"/>.
        /// </summary>
        /// <returns>A new instance of an <see cref="InMemoryUserRepository"/>.</returns>
        private static IUserRepository CreateInMemoryRepository()
        {
            Trace.TraceInformation("Creating In-Memory repository");
            return new InMemoryUserRepository();
        }
    }
}
