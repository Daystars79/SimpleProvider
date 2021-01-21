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
        public int Id { get; set; }

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
                        Size = int.MaxValue;
                        return "string";
                    case "datetime":
                    case "datetime2":
                    case "time":
                    case "date":
                        return Nullable ? "DateTime?" : "DateTime";
                        break;
                    case "varbinary":
                    case "binary":
                    case "image":
                    case "filestream":
                        return Nullable ? "byte[]?" : "byte[]";
                    case "tinyint":
                        return Nullable ? "byte?" : "byte";
                    case "smallint":
                        return Nullable ? "short?" : "short";
                        break;
                    case "int":
                    case "bigint":
                    case "numeric":
                        return Nullable ? "int?" : "int";
                        break;
                    case "decimal":
                    case "money":
                    case "real":
                    case "float":
                        return Nullable ? "decimal?" : "decimal";
                        break;
                    default:
                        return Nullable ? "object?" : "object";
                        break;
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