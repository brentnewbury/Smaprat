using Microsoft.WindowsAzure.Storage.Table;
using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Web;

namespace Smaprat.Data
{
    /// <summary>
    /// Represents user connection information.
    /// </summary>
    public class ConnectionEntity : TableEntity, IUser
    {
        /// <summary>
        /// Gets the name of the user.
        /// </summary>
        public string Name
        {
            get { return PartitionKey; }
        }

        /// <summary>
        /// Gets a <see cref="System.String"/> uniquely identifying the connection.
        /// </summary>
        public string ConnectionId
        {
            get { return RowKey; }
        }

        /// <summary>
        /// Initializes a new instance of <see cref="Smaprat.Website.ConnectionEntity"/>.
        /// </summary>
        public ConnectionEntity()
        {
        }

        /// <summary>
        /// Initializes a new instance of <see cref="Smaprat.Website.ConnectionEntity"/>.
        /// </summary>
        /// <param name="username">The username associated with the connection.</param>
        /// <param name="connectionId">A <see cref="System.String"/> uniquely identifying the connection.</param>
        public ConnectionEntity(string username, string connectionId)
        {
            Contract.Requires<ArgumentException>(String.IsNullOrWhiteSpace(username), "username");
            Contract.Requires<ArgumentException>(String.IsNullOrWhiteSpace(connectionId), "connectionId");

            PartitionKey = username;
            RowKey = connectionId;
        }
    }
}