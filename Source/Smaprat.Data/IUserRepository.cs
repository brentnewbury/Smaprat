using System.Collections.Generic;
namespace Smaprat.Data
{
    /// <summary>
    /// Defines methods for retriving and persisting user connection information.
    /// </summary>
    public interface IUserRepository
    {
        /// <summary>
        /// Returns a new <see cref="IUser"/> instance. No data is commited to the repository until the <see cref="M:AddOrUpdate(IUser user)"/> method is called.
        /// </summary>
        /// <param name="name">The name of the user to create.</param>
        /// <param name="connectionId">A <see cref="System.String"/> uniquely identifying a user connection.</param>
        /// <param name="groupName">The name of the group the user has joined.</param>
        /// <returns>Returns a new <see cref="IUser"/> instance.</returns>
        IUser Create(string name, string connectionId, string groupName);

        /// <summary>
        /// Retrieves an <see cref="IUser"/> instance identified by the connection identifier.
        /// </summary>
        /// <param name="connectionId">A <see cref="System.String"/> unqiuely identifying a user connection.</param>
        /// <returns>A <see cref="IUser"/> instance containing user connection information. If no user connection information can be found, <see langword="null"/> is returned.</returns>
        IUser GetUserByConnectionId(string connectionId);

        /// <summary>
        /// Retrieves an <see cref="IUser"/> instance identified by the user's name.
        /// </summary>
        /// <param name="name">The name of the user identifying the user connection.</param>
        /// <returns>A <see cref="IUser"/> instance containing user connection information. If no user connection information can be found, <see langword="null"/> is returned.</returns>
        IUser GetUserByName(string name);

        /// <summary>
        /// Adds user connection information into the repository, or updates existing user conneciton information in the repository.
        /// </summary>
        /// <param name="user">The user connection information to add.</param>
        /// <exception cref="ArgumentException">Thrown when either the <see cref="M:IUser.Name"/> or <see cref="M:IUser.ConnectionId"/> is blank.</exception>
        /// <exception cref="NameInUseException">Thrown when either the <see cref="M:IUser.Name"/> is already in use by another connection.</exception>
        void AddOrUpdateUser(IUser user);

        /// <summary>
        /// Removes user connection information from the repository.
        /// </summary>
        /// <param name="user">The user connection information to remove.</param>
        /// <remarks>If the specified <paramref name="user"/> cannot be found, no action is performed.</remarks>
        void RemoveUser(IUser user);

        /// <summary>
        /// Retrieves a collection of <see cref="IUser"/> instances currently in the specified group.
        /// </summary>
        /// <param name="groupName">The name of the group.</param>
        /// <returns>An collection of <see cref="IUser"/> instances currently in the specified group.</returns>
        IEnumerable<IUser> GetUsersInGroup(string groupName);
    }
}
