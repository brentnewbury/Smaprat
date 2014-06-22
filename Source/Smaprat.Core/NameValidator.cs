using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Smaprat
{
    /// <summary>
    /// Validates user names.
    /// </summary>
    public class NameValidator
    {
        private static readonly string[] disallowedNames = new string[]
        {
            "me",
            "admin",
            "administrator",
            "server",
            "host"
        };

        /// <summary>
        /// Initializes a new instance of <see cref="NameValidator"/>.
        /// </summary>
        public NameValidator()
        { }

        /// <summary>
        /// Determines if the specified <paramref name="name"/> is valid.
        /// </summary>
        /// <param name="name">The user name to validate.</param>
        /// <returns><see langword="true" /> if the specified <paramref name="name"/> is valid, otherwise <see langword="false" /> is returned.</returns>
        public bool IsValid(string name)
        {
            if (String.IsNullOrWhiteSpace(name))
                return false;

            if (disallowedNames.Contains<string>(name, StringComparer.InvariantCultureIgnoreCase))
                return false;

            return true;
        }
    }
}
