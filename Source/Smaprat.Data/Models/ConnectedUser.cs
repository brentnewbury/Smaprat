using System;

namespace Smaprat.Data.Models
{
    /// <summary>
    /// Represents a connected user.
    /// </summary>
    internal class ConnectedUser : IUser
    {
        /// <summary>
        /// Gets the name of the user.
        /// </summary>
        public string Name { get; private set; }

        /// <summary>
        /// Gets a <see cref="System.String"/> uniquely identifying the connection.
        /// </summary>
        public string ConnectionId { get; private set; }

        /// <summary>
        /// Initializes a new instance of <see cref="Smaprat.Data.ConnectedUser"/>.
        /// </summary>
        private ConnectedUser()
        {
        }

        /// <summary>
        /// Initializes a new instance of <see cref="Smaprat.Data.ConnectedUser"/>.
        /// </summary>
        /// <param name="name">Name of the user</param>
        /// <param name="connectionId">A <see cref="System.String"/> uniquely identifying the connection.</param>
        public ConnectedUser(string name, string connectionId)
        {
            if (String.IsNullOrWhiteSpace(name))
                throw new ArgumentNullException("name");
            if (String.IsNullOrWhiteSpace(connectionId))
                throw new ArgumentNullException("connectionId");

            Name = name;
            ConnectionId = connectionId;
        }
    }
}
