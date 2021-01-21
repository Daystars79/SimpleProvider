using System;
using System.Collections.Generic;
using System.Linq;
using SimpleProvider.Analyzer.Extension;
using SimpleProvider.Analyzer.Structures;
using SimpleProvider.Constants;
using SimpleProvider.Enumerators;



#nullable enable
namespace SimpleProvider.Analyzer
{
    /// <summary>
    ///     Simple Database Scanner
    /// </summary>
    public class Scanner
    {
        private readonly string _connection;
        private readonly ProviderType _type;

        /// <summary>
        ///     Create an instance of th DB Scanner (If initial catalog is left blank it will scan the entire DB Instance)
        /// </summary>
        /// <param name="connectionString">Valid Connection string to an Database</param>
        /// <param name="ptype">Provider Type that specifies the type of Database</param>
        public Scanner(string connectionString, ProviderType ptype = ProviderType.SqlServer)
        {
            _connection = connectionString;
            _type = ptype;
        }

        /// <summary>
        ///     Return an list of the Databases contained in an Sql Instance
        /// </summary>
        /// <returns></returns>
        public string[] GetDatabaseNames()
        {
            try
            {
                using Provider uow = new(_connection, _type);
                string[]? values = uow.GetValues<string>(Shared.Databases)?.ToArray();
                if (values != null)
                {
                    return values.OrderBy(ob => ob).ToArray();
                }
                return new string[0];
            }
            catch (Exception)
            {
                throw new Exception("Invalid connection string");
            }
        }

        /// <summary>
        ///     Return an collection of Table Information contained in an DB Instance
        /// </summary>
        /// <returns></returns>
        public Table[] LoadTableInformation(Action? step = null, Action<int>? count = null, Action? complete = null)
        {
            using Provider uow = new(_connection, _type);
            try
            {
                Table[]? tables = uow.GetRecords<Table>(Shared.Tables)?.OrderBy(ob => ob.Name).ToArray();

                if (tables == null) return new Table[0]; /* Return empty instance */

                for (int index = 0; index < tables.Length; index++)
                {
                    Option mp = new Option("object_id", tables[index].ObjectId);
                    tables[index].Columns = uow.GetRecords<Column>(tables[index].IsTable ? Shared.TableColumns : Shared.ViewColumns, mp);
                    step?.Invoke();
                }
                return tables.OrderBy(ob => ob.Schema).ThenBy(tb => tb.Name).ToArray();
            }
            finally
            {
                complete?.Invoke();
            }
        }

        /// <summary>
        ///     Return an RowCount from the specified table
        /// </summary>
        /// <param name="tableName"></param>
        /// <returns></returns>
        public int GetRowCount(string tableName)
        {
            if (string.IsNullOrEmpty(tableName)) return -1;

            const string cmdTxt = @"SELECT ISNULL(p.rows, -1) [Rows]
FROM sys.partitions p INNER JOIN 
     sys.tables t ON t.object_id = p.object_id
WHERE t.name = @table_name";

            using Provider uow = new(_connection, _type);
            Option mp = new("table_name", tableName);
            return uow.GetValue<int>(cmdTxt, mp);

        }
        private string GenerateHeader()
        {
            return $"using System;{Environment.NewLine}" +
                   $"using System.Collections.Generic;{Environment.NewLine}" +
                   $"using System.Linq;{Environment.NewLine}" +
                   $"using System.Text;{Environment.NewLine}" +
                   $"using System.Threading.Tasks;{Environment.NewLine}" +
                   $"using SimpleProvider;{Environment.NewLine} +" +
                   $"using SimpleProvider.Attributes;{Environment.NewLine}";
        }

        private string CreateDefinition(Table table)
        {
            if (table?.Columns == null) return string.Empty;
            if (table.Columns?.Count <= 0)
                return $@"[Definition(""{table.Name}"", SchemaName = ""{table.Schema}"")]{Environment.NewLine}";
            List<Column> columns = new();
            columns.AddRange(table.Columns.Where(w => w.PK));

            if (columns.Count > 0)
            {
                columns = columns.OrderBy(ob => ob.Id).ToList();
                string keys = string.Empty;
                for (int x = 0; x < columns.Count; x++) keys += $@"""{columns[x].Name}"",";
                keys = keys.Substring(0, keys.Length - 1);
                return $@"[Definition(""{table.Name}"", {keys}, SchemaName = ""{table.Schema}"")]{Environment.NewLine}";
            }

            return $@"[Definition(""{table.Name}"", SchemaName = ""{table.Schema}"")]{Environment.NewLine}";
        }

        private string GenerateClass(Table table)
        {
            if (table == null) return string.Empty;
            if (table.Columns.Count <= 0) return string.Empty;

            string clsStart =
                $@"{CreateDefinition(table)}public class {table.Name}{Environment.NewLine}{'{'}{Environment.NewLine}";

            string clsEnd = "}";
            string output = string.Empty;

            output += clsStart;

            foreach (var c in table.Columns)
            {
                string dt = string.Empty;
                string type = c.DataType.ToLower();
                switch (type)
                {
                    case "bit":
                        dt = c.Nullable ? "bool?" : "bool";
                        break;
                    case "varchar2":
                    case "varchar":
                    case "nvarchar":
                        if (c.Size == 1) dt = c.Nullable ? "char?" : "char";
                        else dt = "string";
                        break;
                    case "char":
                    case "nchar":
                        if (c.Size > 1)
                            dt = "string";
                        else
                            dt = c.Nullable ? "char?" : "char";
                        break;
                    case "datetime":
                    case "datetime2":
                    case "time":
                    case "date":
                        dt = c.Nullable ? "DateTime?" : "DateTime";
                        break;
                    case "varbinary":
                    case "binary":
                    case "image":
                    case "filestream":
                        dt = c.Nullable ? "byte[]?" : "byte[]";
                        break;
                    case "tinyint":
                        dt = c.Nullable ? "byte?" : "byte";
                        break;
                    case "smallint":
                        dt = c.Nullable ? "short?" : "short";
                        break;
                    case "int":
                    case "bigint":
                    case "numeric":
                        dt = c.Nullable ? "int?" : "int";
                        break;
                    case "decimal":
                    case "money":
                    case "real":
                    case "float":
                        dt = c.Nullable ? "decimal?" : "decimal";
                        break;
                    default:
                        dt = c.Nullable ? "object?" : "object";
                        break;
                }

                /* This needs to apply attributes */
                if (c.Identity)
                {
                    output += $"     [Column(IsScope = true)]{Environment.NewLine}";
                    output += $"     public {dt} {c.Name?.ToPascalCase()} " + "{ get; set; }" + Environment.NewLine;
                    continue;
                }

                if (dt == "string")
                {
                    output += $"     [Column(Length = {c.Size})]{Environment.NewLine}";
                    output += $"     public {dt} {c.Name?.ToPascalCase()} " + "{ get; set; }" + Environment.NewLine;
                    continue;
                }

                if (!c.Nullable)
                {
                    output += $"     [Column(IsNullable = false, Length = {c.Size})]{Environment.NewLine}";
                    output += $"     public {dt} {c.Name?.ToPascalCase()} " + "{ get; set; }" + Environment.NewLine;
                    continue;
                }


                /* Default behavior */
                output += $"     public {dt} {c.Name?.ToPascalCase()} " + "{ get; set; }" + Environment.NewLine;
            }

            output += Environment.NewLine;
            output += ' ' + clsEnd;
            return output;
        }
    }
}