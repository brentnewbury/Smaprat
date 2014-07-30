namespace Smaprat.Data
{
    /// <summary>
    /// Represents a connected user.
    /// </summary>
    public interface IUser
    {
        /// <summary>
        /// Gets the name of the user.
        /// </summary>
        string Name { get; }

        /// <summary>
        /// Gets a <see cref="System.String"/> uniquely identifying the connection.
        /// </summary>
        string ConnectionId { get; }

        /// <summary>
        /// Gets the name of the group the user has joined.
        /// </summary>
        string GroupName { get; }
    }
}
