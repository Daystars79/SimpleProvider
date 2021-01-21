using System;

namespace SimpleProvider.Attributes
{
    /// <summary>
    ///     Maps an class to an Database Table or View
    /// </summary>
    [AttributeUsage(AttributeTargets.Class)]
    public class Definition : Attribute
    {
        /// <summary>
        ///     Empty Definition
        /// </summary>
        public Definition()
        {
        }

        /// <summary>
        ///     Default Constructor
        /// </summary>
        /// <param name="table">Table Name</param>
        /// <param name="schema">Schema Name</param>
        /// <param name="key_names">Key Names</param>
        public Definition(string table, string schema, params string[] key_names)
        {
            TableName = table;
            SchemaName = schema;
            PrimaryKeys = key_names;
            IsReadOnly = false;
            IsView = false;
        }

        /// <summary>
        ///     Table Name
        /// </summary>
        public string TableName { get; set; }

        /// <summary>
        ///     Schema Name
        /// </summary>
        public string SchemaName { get; set; } = "dbo"; /* Use the MS-SQL Default */

        /// <summary>
        ///     Primary Keys or Combination of Properties to make an Unique Identifier
        /// </summary>
        public string[] PrimaryKeys { get; set; } = new string[0];

        /// <summary>
        ///     Readonly Table
        /// </summary>
        public bool IsReadOnly { get; set; }

        /// <summary>
        ///     ReadOnly View
        /// </summary>
        public bool IsView { get; set; }

        internal string[] KeysToLower
        {
            get
            {
                var lowerKeys = new string[PrimaryKeys.Length];
                for (var x = 0; x < PrimaryKeys.Length; x++) lowerKeys[x] = PrimaryKeys[x].ToLower();
                return lowerKeys;
            }
        }
    }
}