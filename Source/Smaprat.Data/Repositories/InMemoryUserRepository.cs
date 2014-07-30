using Smaprat.Data.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Smaprat.Data.Repositories
{
    /// <summary>
    /// Retrieves and persists user connection information in memory.
    /// </summary>
    /// <remarks>
    /// If the server is restarted, all user connection information is lost.
    /// If the application is hosted on a webfarm or cloud infrastructure, information is not shared between each host.
    /// </remarks>
    internal class InMemoryUserRepository : IUserRepository
    {
        private Dictionary<string, IUser> _users = new Dictionary<string, IUser>();

        /// <summary>
        /// Initializes a new instance of <see cref="InMemoryUserRepository"/>.
        /// </summary>
        public InMemoryUserRepository()
        { }

        /// <summary>
        /// Returns a new <see cref="IUser"/> instance. No data is commited to the repository until the <see cref="M:AddOrUpdate(IUser user)"/> method is called.
        /// </summary>
        /// <param name="name">The name of the user to create.</param>
        /// <param name="connectionId">A <see cref="System.String"/> uniquely identifying a user connection.</param>
        /// <param name="groupName">The name of the group the user has joined.</param>
        /// <returns>Returns a new <see cref="IUser"/> instance.</returns>
        public IUser Create(string name, string connectionId, string groupName)
        {
            return new ConnectedUser(name, connectionId, groupName);
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

            lock (_users)
            {
                if (_users.ContainsKey(connectionId))
                    return _users[connectionId];
            }

            return null;
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

            lock (_users)
            {
                return _users.FirstOrDefault(u => u.Value.Name.Equals(name, StringComparison.OrdinalIgnoreCase)).Value;
            }
        }

        /// <summary>
        /// Adds user connection information into the repository, or updates existing user conneciton information in the repository.
        /// </summary>
        /// <param name="user">The user connection information to add.</param>
        /// <excepion cref="ArgumentException">Thrown when either the <see cref="M:IUser.Name"/> or <see cref="M:IUser.ConnectionId"/> is blank.</exception>
        /// <excepion cref="NameInUseException">Thrown when either the <see cref="M:IUser.Name"/> is already in use.</exception>
        public void AddOrUpdateUser(IUser user)
        {
            if (user == null)
                throw new ArgumentNullException("user");
            if (String.IsNullOrWhiteSpace(user.Name))
                throw new ArgumentException("User name cannot be empty.");
            if (String.IsNullOrWhiteSpace(user.ConnectionId))
                throw new ArgumentException("Connection identifier cannot be empty.");

            lock (_users)
            {
                // Ensure the name isn't already in use
                IUser existingUser = GetUserByName(user.Name);
                if ((existingUser != null) && (existingUser.ConnectionId != user.ConnectionId))
                    throw new NameInUseException(existingUser.Name);

                if (_users.ContainsKey(user.ConnectionId))
                    _users[user.ConnectionId] = user;
                else
                    _users.Add(user.ConnectionId, user);
            }
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

            lock (_users)
            {
                // No exception is thrown by Dictionary<TKey, TValue> if the specified entry cannot be found (see http://msdn.microsoft.com/en-us/library/kabs04ac(v=vs.110).aspx)
                _users.Remove(user.ConnectionId);
            }
        }

        /// <summary>
        /// Retrieves a collection of <see cref="IUser"/> instances currently in the specified group.
        /// </summary>
        /// <param name="groupName">The name of the group.</param>
        /// <returns>An collection of <see cref="IUser"/> instances currently in the specified group.</returns>
        public IEnumerable<IUser> GetUsersInGroup(string groupName)
        {
            lock (_users)
            {
                return _users.Values.Where(u => u.GroupName == groupName).ToList();
            }
        }
    }
}
