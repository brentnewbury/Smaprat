using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Table;
using Smaprat.Data.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace Smaprat.Data.Repositories
{
    /// <summary>
    /// Retrieves and persists user connection information in Microsoft Azure Table Storage.
    /// </summary>
    /// <remarks>
    /// Azure Table Storage will perist user connection information through server restarts and application deployment/publishing.
    /// </remarks>
    internal class AzureTableStorageUserRepository : IUserRepository
    {
        CloudTable _table = null;

        /// <summary>
        /// Initializes a new instance of <see cref="AzureTableStorageUserRepository"/>.
        /// </summary>
        private AzureTableStorageUserRepository()
        { }

        /// <summary>
        /// Initializes a new instance of <see cref="AzureTableStorageUserRepository"/>.
        /// </summary>
        /// <param name="settingName">Name of the configuration setting containing the connection string to table storage.</param>
        public AzureTableStorageUserRepository(string connectionString)
        {
            if (String.IsNullOrWhiteSpace(connectionString))
                throw new ArgumentException("No Azure Table Storage connection string provided.");

            CloudStorageAccount storageAccount = CloudStorageAccount.Parse(connectionString);
            CloudTableClient tableClient = storageAccount.CreateCloudTableClient();
            _table = tableClient.GetTableReference("connections");
            if (_table.CreateIfNotExists())
                Trace.TraceInformation("Azure Storage Table created.");
        }

        /// <summary>
        /// Returns a new <see cref="IUser"/> instance. No data is commited to the repository until the <see cref="M:AddOrUpdate(IUser user)"/> method is called.
        /// </summary>
        /// <param name="name">The name of the user to create.</param>
        /// <param name="connectionId">A <see cref="System.String"/> uniquely identifying a user connection.</param>
        /// <returns>Returns a new <see cref="IUser"/> instance.</returns>
        public IUser Create(string name, string connectionId)
        {
            return new ConnectedUserEntity(name, connectionId);
        }

        /// <summary>
        /// Retrieves an <see cref="IUser"/> instance identified by the connection identifier.
        /// </summary>
        /// <param name="connectionId">A <see cref="System.String"/> unqiuely identifying a user connection.</param>
        /// <returns>A <see cref="IUser"/> instance containing user connection information. If no user connection information can be found, <see langword="null"/> is returned.</returns>
        public IUser GetUserByConnectionId(string connectionId)
        {
            if (String.IsNullOrWhiteSpace(connectionId))
                throw new ArgumentException("Connection identifier cannot be empty.");

            var query = new TableQuery<ConnectedUserEntity>()
                .Where(
                    TableQuery.GenerateFilterCondition(
                        "PartitionKey", QueryComparisons.Equal, connectionId));

            return _table.ExecuteQuery(query).FirstOrDefault();
        }

        /// <summary>
        /// Retrieves an <see cref="IUser"/> instance identified by the user's name.
        /// </summary>
        /// <param name="name">The name of the user identifying the user connection.</param>
        /// <returns>A <see cref="IUser"/> instance containing user connection information. If no user connection information can be found, <see langword="null"/> is returned.</returns>
        public IUser GetUserByName(string name)
        {
            if (String.IsNullOrWhiteSpace(name))
                throw new ArgumentException("User name cannot be empty.");

            var query = new TableQuery<ConnectedUserEntity>()
                .Where(
                    TableQuery.GenerateFilterCondition(
                        "RowKey", QueryComparisons.Equal, name.ToLower()));

            return _table.ExecuteQuery(query).FirstOrDefault();
        }

        /// <summary>
        /// Adds user connection information into the repository, or updates existing user conneciton information in the repository.
        /// </summary>
        /// <param name="user">The user connection information to add.</param>
        /// <exception cref="ArgumentException">Thrown when either the <see cref="M:IUser.Name"/> or <see cref="M:IUser.ConnectionId"/> is blank.</exception>
        /// <excepion cref="NameInUseException">Thrown when either the <see cref="M:IUser.Name"/> is already in use by another connection.</exception>
        public void AddOrUpdateUser(IUser user)
        {
            if (user == null)
                throw new ArgumentNullException("user");
            if (String.IsNullOrWhiteSpace(user.Name))
                throw new ArgumentException("User name cannot be empty.");
            if (String.IsNullOrWhiteSpace(user.ConnectionId))
                throw new ArgumentException("Connection identifier cannot be empty.");

            // Ensure the name isn't already in use
            ConnectedUserEntity existingUser = (ConnectedUserEntity)GetUserByName(user.Name);
            if ((existingUser != null) && (existingUser.ConnectionId != user.ConnectionId))
                throw new NameInUseException(existingUser.Name);

            TableBatchOperation batchOperation = new TableBatchOperation();

            // Delete existing user, because we can't update the name as it's a row key
            existingUser = (ConnectedUserEntity)GetUserByConnectionId(user.ConnectionId);
            if (existingUser != null)
            {
                var deleteOperation = TableOperation.Delete(existingUser);
                batchOperation.Add(deleteOperation);
            }

            var insertOperation = TableOperation.Insert((ConnectedUserEntity)user);
            batchOperation.Add(insertOperation);

            _table.ExecuteBatch(batchOperation);
        }

        /// <summary>
        /// Removes user connection information from the repository.
        /// </summary>
        /// <param name="user">The user connection information to remove.</param>
        /// <remarks>If the specified <paramref name="user"/> cannot be found, no action is performed.</remarks>
        public void RemoveUser(IUser user)
        {
            if (user == null)
                throw new ArgumentNullException("user");
            if (String.IsNullOrWhiteSpace(user.Name))
                throw new ArgumentException("User name cannot be empty.");
            if (String.IsNullOrWhiteSpace(user.ConnectionId))
                throw new ArgumentException("Connection identifier cannot be empty.");

            ConnectedUserEntity existingUser = (ConnectedUserEntity)GetUserByConnectionId(user.ConnectionId);
            if (existingUser != null)
                _table.Execute(TableOperation.Delete(existingUser));
        }

        /// <summary>
        /// Retrieves a collection of <see cref="IUser"/> instances currently connected.
        /// </summary>
        /// <returns>A collection of <see cref="IUser"/> instances currently connected.</returns>
        public IEnumerable<IUser> GetUsers()
        {
            return from entry in _table.CreateQuery<ConnectedUserEntity>()
                   select entry;
        }
    }
}
