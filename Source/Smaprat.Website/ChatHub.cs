﻿using Microsoft.AspNet.SignalR;
using Smaprat.Data;
using System;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace Smaprat.Website
{
    /// <summary>
    /// Provides methods that allow clients to send and receive messages.
    /// </summary>
    public class ChatHub : Hub
    {
        /// <summary>
        /// The maximum number of characters allowed for a user name.
        /// </summary>
        public const int MaxCharactersPerName = 20;

        /// <summary>
        /// The maximum number of characters allowed per message.
        /// </summary>
        public const int MaxCharactersPerMessage = 500;

        private IUserRepository _userRepository = null;

        private static readonly NameValidator _nameValidator = new NameValidator();

        /// <summary>
        /// Initializes a new instance of <see cref="ChatHub"/>.
        /// </summary>
        public ChatHub()
        {
            _userRepository = UserRepository.Instance;
        }

        /// <summary>
        /// Sends the specified <paramref name="message"/> to all connected clients, except the caller.
        /// </summary>
        /// <param name="message">The message to send to all connected clients.</param>
        /// <remarks>Messages over <see cref="M:MaxCharactersPerMessage"/> in length are truncated.</remarks>
        public void SendToAll(string message)
        {
            SimulateDelay();

            if (String.IsNullOrWhiteSpace(message))
                return;

            message = TruncateMessage(message);

            IUser from = GetConnectedUser();
            Debug.Assert(from != null);

            Clients.Others.message(from.Name, message);

            Trace.TraceInformation("{0} sent a message", from.Name);
        }

        /// <summary>
        /// Send the specified <paramref name="message"/> to the client with the specified <paramref name="name"/>.
        /// </summary>
        /// <param name="name">The name of the user to send the message to.</param>
        /// <param name="message">The message to send to the specified client.</param>
        /// <remarks>Messages over <see cref="M:MaxCharactersPerMessage"/> in length are truncated.</remarks>
        public void SendToUser(string name, string message)
        {
            SimulateDelay();

            if (String.IsNullOrWhiteSpace(name))
                return;

            message = TruncateMessage(message);

            IUser from = GetConnectedUser();

            IUser to = _userRepository.GetUserByName(name);
            if (to == null)
                throw new HubException("Could not find user. They may have disconnected.");

            Clients.Client(to.ConnectionId).message(from.Name, message);
        }

        /// <summary>
        /// Initialises the users connection information and notifies other users that the current user has joined the chat.
        /// </summary>
        /// <param name="name">The name of the current user.</param>
        /// <remarks>Handles usecases where a new user has joined, and where an existing user has changed their name.</remarks>
        public void InitialiseUser(string name)
        {
            SimulateDelay();

            if (String.IsNullOrWhiteSpace(name))
                return;

            Debug.Assert(_nameValidator != null);
            if (!_nameValidator.IsValid(name))
                throw new HubException("That name is not allowed.");

            name = TruncateName(name);

            Debug.Assert(_userRepository != null);
            IUser exisitngUserDetails = _userRepository.GetUserByConnectionId(Context.ConnectionId); // Must retrieve existing details before we overwrite with new details
            IUser newUserDetails = _userRepository.Create(name, Context.ConnectionId);

            try
            {
                _userRepository.AddOrUpdateUser(newUserDetails);
            }
            catch (NameInUseException nameInUseException)
            {
                throw new HubException(nameInUseException.Message);
            }

            if (exisitngUserDetails == null)
            {
                Clients.Others.notification(name + " has joined the conversation, say Hi");

                Trace.TraceInformation("{0} changed to {1}", Context.ConnectionId, name);
            }
            else if ((exisitngUserDetails != null) && (exisitngUserDetails.Name != name))
            {
                Clients.Others.notification(exisitngUserDetails.Name + " changed their name to " + newUserDetails.Name);
                Trace.TraceInformation("{0} changed to {1}", exisitngUserDetails.Name, name);
            }
        }

        /// <summary>
        /// Called when a client connects to the hubg.
        /// </summary>
        /// <returns>A <see cref="System.Threading.Tasks.Task" />.</returns>
        public override Task OnConnected()
        {
            Trace.TraceInformation("{0} connected", Context.ConnectionId);

            return base.OnConnected();
        }

        /// <summary>
        /// Called when a client disconnects from the hub.
        /// </summary>
        /// <returns>A <see cref="System.Threading.Tasks.Task" />.</returns>
        public override Task OnDisconnected()
        {
            Debug.Assert(_userRepository != null);
            IUser user = _userRepository.GetUserByConnectionId(Context.ConnectionId);
            if (user != null)
            {
                string name = user.Name;
                _userRepository.RemoveUser(user);
                Clients.Others.notification(name + " has left the conversation");
                Trace.TraceInformation("{0} disconnected", name);
            }
            else
            {
                Trace.TraceInformation("{0} disconnected", Context.ConnectionId);
            }

            return base.OnDisconnected();
        }

        /// <summary>
        /// Called when a client reconnects to the hub.
        /// </summary>
        /// <returns>A <see cref="System.Threading.Tasks.Task" />.</returns>
        public override Task OnReconnected()
        {
            Clients.Caller.notification("You have been reconnected");

            return base.OnReconnected();
        }

        /// <summary>
        /// Returns an <see cref="IUser"/> instance representing the current connected user. If no connected user information is found, a <see cref="HubException"/> is thrown.
        /// </summary>
        /// <returns>Returns an <see cref="IUser"/> instance representing the current connected user. If no connected user information is found, a <see cref="HubException"/> is thrown.</returns>
        private IUser GetConnectedUser()
        {
            Debug.Assert(_userRepository != null);
            IUser user = _userRepository.GetUserByConnectionId(Context.ConnectionId);
            if (user == null)
            {
                Trace.TraceError("No user information found (ConnectionId: {0})", Context.ConnectionId);
                throw new HubException("You have not joined the conversation. Enter a name to begin chatting.");
            }

            return user;
        }

        /// <summary>
        /// Truncates a message if it is over <see cref="M:MaxCharactersPerMessage"/> in length.
        /// </summary>
        /// <param name="message">The message to truncate.</param>
        /// <returns>Truncated message if it is over <see cref="M:MaxCharactersPerMessage"/> in length. Otherwise the original message is returned.</returns>
        private string TruncateMessage(string message)
        {
            if (message == null)
                return String.Empty;
            if (message.Length > MaxCharactersPerMessage)
                message = message.Substring(0, MaxCharactersPerMessage);

            return message.Trim();
        }

        /// <summary>
        /// Truncates the name if it is over <see cref="M:MaxCharactersPerName"/> in length.
        /// </summary>
        /// <param name="name">The name to truncate.</param>
        /// <returns>Truncated name if it is over <see cref="M:MaxCharactersPerName"/> in length. Otherwise the original name is returned.</returns>
        private string TruncateName(string name)
        {
            if (name == null)
                return String.Empty;
            if (name.Length > MaxCharactersPerName)
                name = name.Substring(0, MaxCharactersPerName);

            return name.Trim();
        }

        /// <summary>
        /// Simulates delay when running locally.
        /// </summary>
        [Conditional("DEBUG")]
        private void SimulateDelay()
        {
            if (Context.Request.Url.IsLoopback)
                System.Threading.Thread.Sleep(200);
        }
    }
}