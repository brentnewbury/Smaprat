using Microsoft.WindowsAzure.Storage.Table;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Smaprat.Data.Models
{
    /// <summary>
    /// Represents a connected user backed by a table entity in Microsoft Table Storage.
    /// </summary>
    internal class ConnectedUserEntity : TableEntity, IUser
    {
        /// <summary>
        /// Gets the name of the user.
        /// </summary>
        public string Name
        {
            get;
            set;
        }

        /// <summary>
        /// Gets a <see cref="System.String"/> uniquely identifying the connection.
        /// </summary>
        [IgnoreProperty]
        public string ConnectionId
        {
            get
            {
                return RowKey;
            }
        }

        /// <summary>
        /// Gets the name of the group the user has joined.
        /// </summary>
        public string GroupName
        {
            get;
            set; 
        }

        /// <summary>
        /// Initializes a new instance of <see cref="Smaprat.Data.ConnectedUserEntity"/>.
        /// </summary>
        public ConnectedUserEntity()
        {
        }

        /// <summary>
        /// Initializes a new instance of <see cref="Smaprat.Data.ConnectedUserEntity"/>.
        /// </summary>
        /// <param name="name">Name of the user</param>
        /// <param name="connectionId">A <see cref="System.String"/> uniquely identifying the connection.</param>
        /// <param name="groupName">The name of the group the user has joined.</param>
        public ConnectedUserEntity(string name, string connectionId, string groupName)
        {
            if (String.IsNullOrWhiteSpace(name))
                throw new ArgumentNullException("name");
            if (String.IsNullOrWhiteSpace(connectionId))
                throw new ArgumentNullException("connectionId");
            if (String.IsNullOrWhiteSpace(groupName))
                throw new ArgumentNullException("groupName");

            PartitionKey = name.ToLower(); // Normalized version of the name for case-insensitive searches (at time of writing, this doesn't exist in Table Storage)
            RowKey = connectionId;
            Name = name;
            GroupName = groupName;
        }
    }
}
