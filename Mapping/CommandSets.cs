﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using SimpleProvider.Attributes;
using SimpleProvider.Constants;
using SimpleProvider.Enumerators;
using SimpleProvider.Extensions;



namespace SimpleProvider.Mapping
{
    /// <summary>
    /// Dynamic SQL Generation
    /// Generates CommandSets and associated Options
    /// </summary>
    public class CommandSets
    {
        private readonly string _op;
        private readonly ProviderType _type;


        /// <summary>
        /// Database Type
        /// </summary>
        /// <param name="type"></param>
        public CommandSets(ProviderType type)
        {
            _type = type;

            _op = type switch
            {
                ProviderType.Oracle => ":",
                _ => "@"
            };
        }
        /// <summary>
        ///  Generate Insert Command Set
        /// </summary>
        /// <param name="record">Object that represents an DB row</param>
        /// <returns>Insert CommandSet</returns>
        public CommandSet CreateInsert(object record)
        {
            if (record == null) throw new ArgumentNullException(nameof(record));
            if (!Validate(record)) throw new Exception("An error occurred in the validation.");

            Definition def = record.GetDefinition(); /* get the definition from the object */
            PropertyMap[] map = record.GetMappings();

            CommandSet cs = new(def.IsReadOnly)
            {
                CommandText = $@"insert into {def.SchemaName}.{def.TableName} ("
            };

            string values = ") values (";

            for (int p = 0; p < map.Length; p++)
            {
                if (map[p].Value.IsScope) continue;
                if (map[p].Value.IsVirtual) continue;

                object value = map[p].Key.GetValue(record) ?? DBNull.Value;
                if (value == DBNull.Value) continue;

                cs.CommandText += $"{map[p].Value.Name}, ";
                values += $"{_op}{map[p].Value.Name}, ";
                cs.Parameters.Add(new Option(map[p].Value.Name, value));
            }

            values = values.Substring(0, values.Length - 2) + ")";
            cs.CommandText = cs.CommandText.Substring(0, cs.CommandText.Length - 2) + values;
            return cs;
        }

        /// <summary>
        ///     Create Select based on the provided object
        /// </summary>
        /// <param name="record">Object that represents an DB row</param>
        /// <returns>Select Command Set</returns>
        public CommandSet CreateSelect(object record)
        {
            if (record == null) throw new ArgumentNullException(nameof(record));

            Definition def = record.GetDefinition();
            PropertyInfo[] props = def.PrimaryKeys?.Length > 0
                ? record.GetProperties(record.GetKeyNames())
                : record.GetProperties();

            CommandSet cs = new(def.IsReadOnly)
            {
                CommandText = _type != ProviderType.Oracle ? $@"select top(1) * from {def.SchemaName}.{def.TableName} where " :
                                                                   $@"select * from {def.SchemaName}.{def.TableName} where "
            };

            for (int p = 0; p < props.Length; p++)
            {
                Column col = props[p].GetColumnDefinition();
                object value = props[p].GetValue(record) ?? DBNull.Value;
                if (col.IsVirtual) continue;
                string name = $@"{_op}{col.Name}";
                bool islast = p == props.Length - 1;
                cs.CommandText += !islast ? $"{col.Name} = {name} and " : $"{col.Name} = {name}";
                cs.Parameters.Add(new Option(name, value));
            }

            if (_type == ProviderType.Oracle)
            {
                cs.CommandText += " and rownum < 2";
            }

            return cs;
        }

        /// <summary>
        ///  Used internally
        /// </summary>
        /// <param name="record"></param>
        /// <returns></returns>
        internal CommandSet CreateExists(object record)
        {
            if (record == null) throw new ArgumentNullException(nameof(record));

            Definition def = record.GetDefinition();
            PropertyMap[] props = def.PrimaryKeys?.Length > 0
                ? record.GetMappings(def.PrimaryKeys)
                : record.GetMappings();

            CommandSet cs = new()
            {
                CommandText =
                    $@"select count(tab.{props[0].Value.Name}) from {def.SchemaName}.{def.TableName} tab"
            };

            for (int p = 0; p < props.Length; p++)
            {
                PropertyInfo pi = props[p].Key;
                Column column = props[p].Value;
                cs.Parameters.Add(new Option(column.Name, pi.GetValue(record)));
            }
            cs.CommandText += CreateWhere(cs.Parameters.ToArray());
            return cs;
        }
        /// <summary>
        ///     Creates Exists Command Set
        /// </summary>
        /// <typeparam name="T">class, new()</typeparam>
        /// <param name="args">Options for use in the "Where" clause</param>
        /// <returns>Exists Commandset</returns>
        public CommandSet CreateExists<T>(params Option[] args) where T : class, new()
        {
            T reference = Activator.CreateInstance<T>();
            Definition def = reference.GetDefinition();

            CommandSet cs = new()
            {
                CommandText = $@"select count(tab.{def.PrimaryKeys[0]}) from {def.SchemaName}.{def.TableName} tab"
            };
            cs.Parameters.AddRange(args);
            cs.CommandText += CreateWhere(args);
            return cs;
        }
        /// <summary>
        /// Return an collection of CommandSets related to the specified object 
        /// </summary>
        /// <param name="record"></param>
        /// <param name="record2">Optional parameter to generate update string</param>
        /// <returns></returns>
        public CommandSet[] CreateCommandSets(object record, object record2 = null)
        {
            if (record == null) throw new ArgumentNullException(nameof(record));
            Type type = record.GetType();
            if (type == null) throw new Exception("Unable to materialize the specified type.");

            bool hasUpgrade = record2 != null;

            List<CommandSet> sets = new();
            sets.Add(CreateInsert(record));
            sets.Add(CreateDelete(record));
            sets.Add(CreateExists(record));
            sets.Add(CreateSelect(record));
            sets.Add(CreateTop(1000, type));
            CommandSet update = hasUpgrade ? CreateUpdate(record, record2) : null;

            sets[0].Name = "Insert";
            sets[1].Name = "Delete";
            sets[2].Name = "Exists";
            sets[3].Name = "Select";
            sets[4].Name = "Top (1000)";
            if (update == null) return sets.ToArray();
            update.Name = "Update";
            sets.Add(update);
            return sets.ToArray();
        }
        /// <summary>
        ///     Create Select Command Set
        /// </summary>
        /// <typeparam name="T">class, new()</typeparam>
        /// <param name="args">Options for use in the "WHERE" clause</param>
        /// <returns>Select Commandset</returns>
        public CommandSet CreateSelect<T>(params Option[] args) where T : class, new()
        {
            T t = Activator.CreateInstance<T>();
            Definition def = t.GetDefinition();
            CommandSet cs = new()
            {
                CommandText = $"select tab.* from {def.SchemaName}.{def.TableName} tab"
            };
            cs.CommandText += CreateWhere(args) + OrderBy(args);
            cs.Parameters.AddRange(args);
            return cs;
        }

        /// <summary>
        /// Commandset that selects the specified amount of records.
        /// </summary>
        /// <typeparam name="T">class, new</typeparam>
        /// <param name="number">Number of records to return</param>
        /// <param name="args">Options for use in the "WHERE" clause</param>
        /// <returns></returns>
        public CommandSet CreateTop<T>(int number, params Option[] args)
        {
            T t = Activator.CreateInstance<T>();

            if (t == null) throw new Exception(@"Unable to materialize the specified type.");

            Definition def = t.GetDefinition();
            CommandSet cs = new CommandSet
            {
                CommandText = string.Format(_type == ProviderType.Oracle ? Shared.OracleTop : Shared.SqlTop, def.SchemaName, def.TableName, number)
            };
            cs.CommandText += CreateWhere(args) + OrderBy(args);
            return cs;
        }

        /// <summary>
        ///  Commandset that selects the specified amount of records.
        /// </summary>
        /// <param name="number">Number of records to return</param>
        /// <param name="type">type to generate query for.</param>
        /// <param name="args">Options for use in the "WHERE" clause</param>
        /// <returns></returns>
        public CommandSet CreateTop(int number, Type type, params Option[] args)
        {
            object t = Activator.CreateInstance(type);
            if (t == null) throw new Exception(@"Unable to materialize the specified type.");

            Definition def = t.GetDefinition();
            CommandSet cs = new()
            {
                CommandText = string.Format(_type == ProviderType.Oracle ? Shared.OracleTop : Shared.SqlTop,
                def.SchemaName, def.TableName, number)
            };

            if ((args.Length > 0) & (_type != ProviderType.Oracle))
            {
                Option[] mps = args.Where(w => w.Type != EqualityType.None).ToArray();
                for (int mp = 0; mp < mps.Length; mp++)
                {
                    bool isLast = mp == mps.Length - 1;
                    cs.CommandText += !isLast
                        ? $"{mps[mp].FieldName} {mps[mp].Operator} {_op}{mps[mp].FieldName} and "
                        : $"{mps[mp].FieldName} {mps[mp].Operator} {_op}{mps[mp].FieldName}";
                }

                cs.Parameters.AddRange(mps);
            }

            cs.CommandText += OrderBy(args);
            return cs;
        }

        /// <summary>
        ///  Create Update Commandset - Runs an comparison of the local object versus the database record.
        /// </summary>
        /// <param name="target">Changed Record</param>
        /// <param name="source">Database Record</param>
        /// <param name="args">Options - Used to overed the where clause</param>
        /// <returns>Update Commandset</returns>
        public CommandSet CreateUpdate(object target, object source, params Option[] args)
        {
            if (target == null) throw new ArgumentNullException(nameof(target));
            if (source == null) throw new ArgumentNullException(nameof(source));
            if (!Validate(target)) throw new ArgumentException("Record validation failed");
            if (target.GetType() != source.GetType()) throw new Exception("Objects are not of the same type.");
            if (ReferenceEquals(target, source)) throw new Exception("Target and Source are the same object");

            ICollection<ChangeValue> changes = Compare(source, target);
            if (changes.Count <= 0) return null;
            Definition def = target.GetDefinition();
            CommandSet cs = new(def.IsReadOnly)
            {
                CommandText = string.Format(Shared.Update, def.SchemaName, def.TableName)
            };
            PropertyMap[] props = target.GetMappings(changes.Select(s => s.FieldName).ToArray());
            for (int pm = 0; pm < props.Length; pm++)
            {
                Column col = props[pm].Value;
                /* These can not be updated */
                if (col.IsVirtual) continue;
                if (col.IsScope) continue;

                bool isLast = pm == props.Length - 1;
                cs.CommandText += isLast
                    ? $"{col.Name} = {_op}u_{pm}"
                    : $"{col.Name} = {_op}u_{pm},";
                cs.Parameters.Add(new Option($"u_{pm}", props[pm].Key.GetValue(target)));
            }

            if (args?.Length > 0)
            {
                args = args.Where(a => a.Type != EqualityType.None).ToArray();
                cs.CommandText += CreateWhere(args);
                return cs;
            }

            PropertyInfo[] keys = target.GetProperties(def.PrimaryKeys);
            if (!(keys?.Length > 0)) return cs;
            {
                string where = " where ";

                for (int index = 0; index < keys.Length; index++)
                {
                    bool isLast = index == keys.Length - 1;
                    @where += isLast
                        ? $"{keys[index].Name} = {_op}{keys[index].Name}"
                        : $"{keys[index].Name} = {_op}{keys[index].Name} and ";
                    cs.Parameters.Add(new Option(keys[index].Name, keys[index].GetValue(target)));
                }

                cs.CommandText += @where;
            }
            return cs;
        }
        /// <summary>
        /// Create Delete Commandset - Delete is generated from the Keys contained in the Definition Attribute
        /// </summary>
        /// <param name="record"></param>
        /// <returns>Delete Commandset</returns>
        public CommandSet CreateDelete(object record)
        {
            if (record == null) throw new ArgumentException(nameof(record));
            Definition def = record.GetDefinition();
            PropertyInfo[] properties = record.GetProperties(def.PrimaryKeys);
            CommandSet cs = new(def.IsReadOnly) { CommandText = $@"delete {def.SchemaName}.{def.TableName} " };

            for (int index = 0; index < properties.Length; index++)
            {
                PropertyInfo pi = properties[index];
                Column col = pi.GetColumnDefinition();
                if (index == 0) cs.CommandText += $"where {col.Name} = {_op}{col.Name}";
                else cs.CommandText += $" and {col.Name} = {_op}{col.Name}";
                cs.Parameters.Add(new Option(col.Name, pi.GetValue(record)));
            }

            return cs;
        }

        /// <summary>
        ///     Create delete command set
        /// </summary>
        /// <typeparam name="T">class, new()</typeparam>
        /// <param name="args">Options for use in the "WHERE" clause</param>
        /// <returns></returns>
        public CommandSet CreateDelete<T>(params Option[] args) where T : class, new()
        {
            T reference = Activator.CreateInstance<T>();
            Definition def = reference.GetDefinition();
            CommandSet cs = new(def.IsReadOnly)
            {
                CommandText = $"delete {def.SchemaName}.{def.TableName}"
            };
            cs.Parameters.AddRange(args);
            args = args.Where(w => w.Type != EqualityType.None).ToArray();
            cs.CommandText += CreateWhere(args);
            return cs;
        }

        /// <summary>
        ///  Commandset for Stored Procedures
        /// </summary>
        /// <param name="procname">Procedure Name</param>
        /// <param name="schema">Database Schema</param>
        /// <param name="parameters">Values for the parameters</param>
        /// <returns></returns>
        public CommandSet CreateExecute(string procname, string schema = "dbo", params object[] parameters)
        {
            if (string.IsNullOrEmpty(procname)) throw new ArgumentNullException(procname);
            CommandSet cs = new() { CommandText = $@"exec {schema}.{procname}" };

            if (parameters.Length <= 0) return cs;
            for (int index = 0; index < parameters.Length; index++)
            {
                bool isLast = index == parameters.Length - 1;

                cs.CommandText += isLast ? $" {_op}p_{index}" : $" {_op}p_{index},";
                Option param = new($"p_{index}", parameters[index]);
                cs.Parameters.Add(param);
            }

            return cs;
        }

        /// <summary>
        /// Command Set for getting an row count from the DB
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="fieldName"></param>
        /// <param name="distinct"></param>
        /// <param name="options"></param>
        /// <returns></returns>
        public CommandSet CreateCount<T>(string fieldName, bool distinct, params Option[] options) where T : class, new()
        {
            if (string.IsNullOrWhiteSpace(fieldName)) throw new ArgumentNullException(nameof(fieldName));
            string dist = distinct ? "distinct" : "";
            T item = Activator.CreateInstance<T>();
            Definition def = item.GetDefinition();

            CommandSet cs = new()
            {
                CommandText = @$"select count({dist} tab.{fieldName}) from {def.SchemaName}.{def.TableName} tab"
            };
            cs.CommandText += CreateWhere(options);
            cs.CommandText += OrderBy(options);

            cs.Parameters.AddRange(options);

            return cs;
        }

        #region Where Clause
        /// <summary>
        /// Generate the where clause for the command sets
        /// </summary>
        /// <param name="options"></param>
        /// <returns></returns>
        private string CreateWhere(Option[] options)
        {
            if (options == null) return string.Empty;

            options = options.Where(w => w.Type != EqualityType.None).ToArray();
            if (options.Length <= 0) return string.Empty;

            string where = " where ";

            for (int opt = 0; opt < options.Length; opt++)
            {
                bool islast = (opt == options.Length - 1);
                string andOr = options[opt].UseOrStatement ? " or " : " and ";

                string param = $"{_op}{options[opt].FieldName}";



                switch (options[opt].Type)
                {
                    case EqualityType.Is:
                    case EqualityType.IsNot:
                        where += islast ? $"{options[opt].FieldName} {options[opt].Operator}"
                                        : $"{options[opt].FieldName} {options[opt].Operator} {andOr} ";
                        break;
                    default:
                        if (where.Contains(param))
                        {
                            param = $"{param}_{opt}";
                            where += islast
                                ? $"{options[opt].FieldName} {options[opt].Operator} {param}"
                                : $"{options[opt].FieldName} {options[opt].Operator} {param} {andOr} ";
                            options[opt].FieldName = param;
                            break;
                        }
                        where += islast
                            ? $"{options[opt].FieldName} {options[opt].Operator} {param}"
                            : $"{options[opt].FieldName} {options[opt].Operator} {param} {andOr} ";
                        break;
                }
            }
            return where;
        }
        #endregion

        #region Sorting

        internal string OrderBy(params Option[] parameters)
        {
            if (parameters.Length <= 0) return string.Empty;

            parameters = parameters.Where(w => w.OrderBy).ToArray();
            if (parameters.Length <= 0) return null;

            string sort = " order by ";

            for (int mp = 0; mp < parameters.Length; mp++)
            {
                bool isLast = mp == parameters.Length - 1;
                string dir = parameters[mp].IsAscending ? "asc" : "desc";
                sort += isLast ? $"{parameters[mp].FieldName} {dir}" : $"{parameters[mp].FieldName} {dir}, ";
            }
            return sort;
        }

        #endregion

        #region Validation

        internal bool Validate(object record)
        {
            if (record == null)
                throw new ArgumentNullException(nameof(record));

            PropertyMap[] mapping = record.GetMappings();

            for (int x = 0; x < mapping.Length; x++)
            {
                PropertyInfo prop = mapping[x].Key;
                Column column = mapping[x].Value;

                object value = prop.GetValue(record);

                if (column.IsScope) continue;
                if (column.Length < 0) continue;
                if (column.IsVirtual) continue;

                if (value == null)
                {
                    if (column.IsNullable) continue;
                    throw new Exception($"{prop.Name} contains an null value.");
                }

                Type underlying = Nullable.GetUnderlyingType(column.DataType);

                if (underlying == null)
                {
                    if (column.DataType != value.GetType())
                        throw new Exception($"{prop.Name} - Invalid Data Type");
                }
                else
                {
                    if (underlying != value.GetType())
                        throw new Exception($"{prop.Name} - Invalid Data Type");
                }

                switch (value.GetType())
                {
                    case { } when value is string s:
                        if (s.Length > column.Length)
                            throw new Exception($"{prop.Name} - Exceeds the size allowed for the field");
                        break;
                    case { } when value is byte[] bytes:
                        if (bytes.Length > column.Length)
                            throw new Exception($"{prop.Name} - Exceeds the size allowed for the field");
                        break;
                    case { } when value is char[] chars:
                        if (chars.Length > column.Length)
                            throw new Exception($"{prop.Name} - Exceeds the size allowed for the field");
                        break;
                }
            }

            return true;
        }

        private bool HasEquality(object a, object b)
        {

            if (a == null & b != null) return false;
            if ((a != null) & (b == null)) return false;
            if ((a == null) & (b == null)) return true;

            if (ReferenceEquals(a, b))
                return true;

            /* Remove padding to verify if the string fields are the same */
            if (a is string & b is string)
            {
                a = $"{a}".Trim();
                b = $"{b}".Trim();
            }
            return a.Equals(b);
        }

        internal ICollection<ChangeValue> Compare(object source, object target)
        {
            if (target == null) return null;
            if (source == null) return null;

            if (target.GetType() != source.GetType()) throw new Exception("Source and Target must be of the same type.");

            HashSet<ChangeValue> changes = new HashSet<ChangeValue>();
            PropertyMap[] mapping = target.GetMappings();

            for (int index = 0; index < mapping.Length; index++)
            {
                PropertyMap map = mapping[index];
                dynamic sourceValue = map.Key.GetValue(source);
                dynamic targetValue = map.Key.GetValue(target);

                if (sourceValue != null && sourceValue.GetType() != typeof(DBNull))
                {
                    if (HasEquality(sourceValue, targetValue)) continue;
                    ChangeValue dbChange = new ChangeValue(map.Value.Name, sourceValue, targetValue);
                    changes.Add(dbChange);
                }
                else if (targetValue != null)
                {
                    ChangeValue dbChange = new ChangeValue(map.Value.Name, null, targetValue);
                    changes.Add(dbChange);
                }
            }

            return changes;
        }

        #endregion
    }
}