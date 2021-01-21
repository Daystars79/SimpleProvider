using System.Collections.Generic;
using System.Reflection;

namespace SimpleProvider
{
    /// <summary>
    ///     CommandSet - Prepares the Command Text and Parameter information for the Mapper
    /// </summary>
    public class CommandSet
    {
        /// <summary>
        ///     Empty instance of the commandset
        /// </summary>
        public CommandSet()
        {
        }

        /// <summary>
        ///  CommandSet with the specified Query and Parameters
        /// </summary>
        /// <param name="commandtext">Query</param>
        /// <param name="parameters">Parameters</param>
        public CommandSet(string commandtext, params Option[] parameters)
        {
            CommandText = commandtext;
            Parameters.AddRange(parameters);
        }

        /// <summary>
        ///     SQL Command Text
        /// </summary>
        public string CommandText { get; set; }

        /// <summary>
        ///     List of Mapping Parameters this should match the parameters contained in the SQL
        /// </summary>
        public List<Option> Parameters { get; set; } = new();

        /// <summary>
        /// Passes the IdentityScope back from the Database
        /// </summary>
        public PropertyInfo Scope { get; set; } = null;

        /// <summary>
        /// CommandSet contains an IdentityScope
        /// </summary>
        public bool HasScope => Scope != null;

        /// <summary>
        /// CommandSet contains Parameter Data
        /// </summary>
        public bool HasParameters => Parameters.Count > 0;
    }
}