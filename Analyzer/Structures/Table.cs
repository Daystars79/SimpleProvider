using System;
using System.Collections.Generic;
using System.Linq;

namespace SimpleProvider.Analyzer.Structures
{
    /// <summary>
    /// Structure to Contain Table information from the Database
    /// </summary>
    public class Table
    {
        private IList<Column> _columns = new List<Column>();

        /// <summary>
        ///     Columns
        /// </summary>
        public IList<Column> Columns
        {
            get => _columns;
            set
            {
                if (value == null) return;
                if (Indexes?.Count > 0)
                    for (int index = 0; index < Indexes.Count; index++)
                    {
                        int colId = Indexes[index];
                        for (int x = 0; x < value.Count; x++) value[x].PK = colId == value[x].Id;
                    }
                _columns = value?.OrderBy(ob => ob.Id).ToList();
            }
        }

        /// <summary>
        ///     Database Indexes
        /// </summary>
        public IList<int> Indexes { get; set; } = new List<int>();

        /// <summary>
        ///  View / Table
        /// </summary>
        public bool IsTable => Type == "table";

        /// <summary>
        ///     Table name
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Cleaned Name 
        /// </summary>
        [Attributes.Column(IsVirtual = true)]
        public string ClassName
        {
            get
            {
                string clsname = string.Empty;
                for (int x = 0; x < Name.Length; x++)
                {
                    if (char.IsLetter(Name[x]))
                    {
                        clsname += Name[x];
                    }
                }
                return clsname.Normalize();
            }
        }


        /// <summary>
        ///     Name of the columns contained
        /// </summary>
        public string[] Names => Columns?.Select(s => s.Name).ToArray();

        /// <summary>
        /// Object ID from the Database
        /// </summary>
        public int ObjectId { get; set; }

        /// <summary>
        ///     Schema Name
        /// </summary>
        public string Schema { get; set; }

        /// <summary>
        ///     This is either view or table
        /// </summary>
        public string Type { get; set; }

        /// <summary>
        ///     Display Schema and Table Name
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return $"{Schema}.{Name}";
        }
    }
}