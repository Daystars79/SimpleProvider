using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace SimpleProvider
{
    /// <summary>
    ///     CommandSet - Prepares the Command Text and Parameter information for the Mapper
    /// </summary>
    public class CommandSet
    {

        /// <summary>
        /// Name for the Command Set
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        ///  Empty instance of the commandset
        /// </summary>
        public CommandSet(bool readOnly = false)
        {
            ReadOnly = readOnly;
        }

        /// <summary>
        ///  CommandSet with the specified Query and Parameters
        /// </summary>
        /// <param name="commandtext">Query</param>
        /// <param name="parameters">Parameters</param>
        public CommandSet(string commandtext, params Option[] parameters)
        {
            CommandText = commandtext;
            Parameters = new List<Option>(parameters);
        }

        /// <summary>
        ///     SQL Command Text
        /// </summary>
        public string CommandText { get; set; }

        /// <summary>
        ///     List of Mapping Parameters this should match the parameters contained in the SQL
        /// </summary>
        public List<Option> Parameters { get; } = new();

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
        /// <summary>
        /// Underlying definition is marked as readonly.
        /// </summary>
        public bool ReadOnly { get; }

        /// <summary>
        /// Add an option to the command set.
        /// </summary>
        /// <param name="option"></param>
        public void Add(Option option)
        {
            if (option == null) return;
            if (option.FieldName == string.Empty) return;
            if (Parameters.All(a => string.Equals(a.FieldName, option.FieldName, StringComparison.InvariantCultureIgnoreCase))) Parameters.Add(option);
        }
        /// <summary>
        /// Add an range of parameters to the collection
        /// </summary>
        /// <param name="options"></param>
        public void AddRange(IEnumerable<Option> options)
        {
            if (options == null) return;
            Option[] enumerable = options as Option[] ?? options.ToArray();
            for (int x = 0; x < enumerable.Count(); x++)
            {
                Add(enumerable[x]);
            }
        }
    }
}