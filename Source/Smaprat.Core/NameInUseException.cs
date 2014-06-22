using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Smaprat
{
    /// <summary>
    /// Exception that is thrown when a name is already in use.
    /// </summary>
    [Serializable]
    public sealed class NameInUseException : Exception
    {
        private string _name;

        /// <summary>
        /// Gets the name that is in use.
        /// </summary>
        public string Name
        {
            get { return _name; }
            private set { _name = value; }
        }

        /// <summary>
        /// Initializes a new instance of <see cref="NameInUseException"/>.
        /// </summary>
        public NameInUseException()
            : base("The name is already in use.")
        {
            Name = String.Empty;
        }

        /// <summary>
        /// Initializes a new instance of <see cref="NameInUseException"/> specifying the name that is in use.
        /// </summary>
        /// <param name="name">The name that is already in use.</param>
        public NameInUseException(string name)
            : base(String.Format(CultureInfo.CurrentCulture, "The name '{0}' is already in use.", name))
        {
            Name = name ?? String.Empty;
        }

        /// <summary>
        /// Initializes a new instance of <see cref="NameInUseException"/> with serialized data.
        /// </summary>
        /// <param name="info">The <see cref="SerializationInfo" /> that holds the serialized
        /// object data about the exception being thrown.</param>
        /// <param name="context">The <see cref="StreamingContext" /> that contains contextual 
        /// information about the source or destination.</param>
        public NameInUseException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
            if (info != null)
                _name = info.GetString("_name");
        }

        /// <summary>
        /// When overridden in a derived class, sets the <see cref="System.Runtime.Serialization.SerializationInfo" />
        //  with information about the exception.
        /// </summary>
        /// <param name="info">The <see cref="System.Runtime.Serialization.SerializationInfo" /> 
        /// that holds the serialized object data about the exception being thrown.</param>
        /// <param name="context">The <see cref="System.Runtime.Serialization.StreamingContext" />
        /// that contains contextual information about the source or destination.</param>
        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);

            if (info != null)
                info.AddValue("_name", _name);
        }
    }
}
