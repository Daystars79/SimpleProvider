using System;

namespace SimpleProvider.Analyzer.Structures
{
    /// <summary>
    /// Table Column information
    /// </summary>
    public class Column
    {
        /// <summary>
        /// Column Name
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        ///     Column ID from the sys.columns table
        /// </summary>
        public long Id { get; set; }

        /// <summary>
        /// Size in Bytes
        /// </summary>
        public int Size { get; set; }

        /// <summary>
        ///     Data Type
        /// </summary>
        public string DataType { get; set; }

        /// <summary>
        /// .NET Equivalent of the DB Data Type
        /// </summary>
        [Attributes.Column(IsVirtual = true)]
        public string NetType
        {
            get
            {
                if (string.IsNullOrEmpty(DataType)) return "int";
                switch (DataType.ToLower())
                {
                    case "bit":
                        return Nullable ? "bool?" : "bool";
                    case "varchar2":
                    case "varchar":
                    case "nvarchar":
                    case "char":
                    case "nchar":
                        if (Size == 1) return Nullable ? "char?" : "char";
                        else return "string";
                    case "text":
                    case "ntext":
                        Size = int.MaxValue;
                        return "string";
                    case "time":
                        return "TimeSpan";
                    case "datetime":
                    case "datetime2":
                    case "date":
                    case "smalldatetime":
                        return Nullable ? "DateTime?" : "DateTime";
                    case "datetimeoffset":
                        return "DateTimeOffset";
                    case "varbinary":
                    case "binary":
                    case "image":
                    case "filestream":
                    case "rowversion":
                    case "timestamp":
                        return "byte[]"; /* This is nullable by default */
                    case "tinyint":
                        return Nullable ? "byte?" : "byte";
                    case "smallint":
                        return Nullable ? "short?" : "short";
                    case "int":
                        return Nullable ? "int?" : "int";
                    case "bigint":
                        return Nullable ? "long?" : "long";
                    case "numeric":
                        return Nullable ? "int?" : "int";
                    case "decimal":
                    case "money":
                    case "smallmoney":
                        return Nullable ? "decimal?" : "decimal";
                    case "real":
                        return "Single";
                    case "float":
                        return "double";
                    case "uniqueidentifier":
                        return "Guid";
                    default:
                        return Nullable ? "object?" : "object";
                }
            }
        }

        /// <summary>
        ///     Nullable?
        /// </summary>
        public bool Nullable { get; set; } = false;

        /// <summary>
        ///     Scope Identity Column
        /// </summary>
        public bool Identity { get; set; } = false;

        /// <summary>
        ///     Foreign Key
        /// </summary>
        public bool FK { get; set; } = false;

        /// <summary>
        ///     Primary Key
        /// </summary>
        public bool PK { get; set; } = false;
    }
}