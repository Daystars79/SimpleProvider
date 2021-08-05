using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Dynamic;
using SimpleProvider.Extensions;
using System.Reflection;

namespace SimpleProvider.Mapping
{
    using Attributes;
    internal static class Mapper
    {
        #region Record Mapping

        internal static dynamic DynamicMap(IDataRecord idr)
        {
            dynamic result = new ExpandoObject();
            for (int x = 0; x < idr.FieldCount; x++) ((IDictionary<string, object>)result).Add(idr.GetName(x), idr[x]);
            return result;
        }

        /// <summary>
        ///     Maps an IDataRecord to an object of the specified type at run time.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="idr"></param>
        /// <returns></returns>
        internal static T Map<T>(IDataRecord idr) where T : class, new()
        {
            T result = Activator.CreateInstance<T>();
            string[] columnNames = new string[idr.FieldCount];

            for (int col = 0; col < idr.FieldCount; col++)
            {
                if (idr.IsDBNull(col)) continue; /* Only map the columns that have values */
                columnNames[col] = idr.GetName(col).ToLower();
            }

            /* Use the extender to get the relevant properties */
            ConcurrentBag<PropertyMap> mappings = new(result.GetMappings(columnNames));
            if (mappings.Count <= 0) return null;


            foreach (PropertyMap pm in mappings)
            {
                PropertyInfo pi = pm.Key;
                Column col = pm.Value;

                string name = string.Equals(col.Name, pi.Name, StringComparison.CurrentCultureIgnoreCase) ? pi.Name : col.Name;
                Type nullable = Nullable.GetUnderlyingType(pi.PropertyType);

                if (idr[name] == DBNull.Value) continue;

                object value = idr[name];


                if (nullable != null)
                {
                    if (col.DataType == typeof(char?))
                    {
                        if (ReferenceEquals(value, "")) continue;
                    }

                    pi.SetValue(result, Convert.ChangeType(value, nullable), null);
                    continue; /* Step to the next property */
                }
                if (pi.PropertyType.IsEnum)
                {
                    pi.SetValue(result, Enum.ToObject(pi.PropertyType, value));
                    continue;
                }

                pi.SetValue(result, Convert.ChangeType(value, pi.PropertyType));
            }

            return result;
        }

        /// <summary>
        ///     Maps an IDataRecord to an object of the specified type at run time.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="idr"></param>
        /// <returns></returns>
        internal static T Map<T>(DbDataRecord idr) where T : class, new()
        {
            T result = Activator.CreateInstance<T>();
            string[] columns = new string[idr.FieldCount];

            for (int col = 0; col < idr.FieldCount; col++)
            {
                if (idr.IsDBNull(col)) continue; /* Only map the columns that have values */
                columns[col] = idr.GetName(col).ToLower();
            }

            /* Use the extender to get the relevant properties */
            ConcurrentBag<PropertyMap> mappings = new(result.GetMappings(columns));

            if (mappings.Count <= 0) return null;


            foreach(PropertyMap pm in mappings)
            {
                PropertyInfo pi = pm.Key;
                Column col = pm.Value;

                string name = string.Equals(col.Name, pi.Name, StringComparison.CurrentCultureIgnoreCase) ? pi.Name : col.Name;
                Type nullable = Nullable.GetUnderlyingType(pi.PropertyType);

                if (idr[name] == DBNull.Value) continue;

                object value = idr[name];

                if (nullable != null)
                {
                    if (col.DataType == typeof(char?))
                    {
                        if (ReferenceEquals(value, "")) continue;
                    }

                    pi.SetValue(result, Convert.ChangeType(value, nullable), null);
                    continue; /* Step to the next property */
                }

                if (pi.PropertyType.IsEnum)
                {
                    pi.SetValue(result, Enum.ToObject(pi.PropertyType, value));
                    continue;
                }

                pi.SetValue(result, Convert.ChangeType(value, pi.PropertyType));
            }
            return result;
        }

        /// <summary>
        ///     Map used to specify the type for more dynamic code.
        /// </summary>
        /// <param name="idr">DbDataRecord from DbDataReader</param>
        /// <param name="type">Type of the object to be returned</param>
        /// <returns>Returns instance of the specified type populated from the database.</returns>
        internal static object Map(IDataRecord idr, Type type)
        {
            if (type == null) return null;

            object result = Activator.CreateInstance(type);
            if (result == null) throw new Exception(@"Unable to materialize the specified type.");

            string[] columnNames = new string[idr.FieldCount];

            for (int col = 0; col < idr.FieldCount; col++)
            {
                if (idr.IsDBNull(col)) continue; /* Only map the columns that have values */
                columnNames[col] = idr.GetName(col).ToLower();
            }

            /* Use the extender to get the relevant properties */
            ConcurrentBag<PropertyMap> mappings = new (result.GetMappings(columnNames));
            if (mappings.Count <= 0) return null;


            foreach(PropertyMap pm in mappings)
            {
                PropertyInfo pi = pm.Key;
                Column col = pm.Value;

                string name = string.Equals(col.Name, pi.Name, StringComparison.CurrentCultureIgnoreCase) ? pi.Name : col.Name;
                Type nullable = Nullable.GetUnderlyingType(pi.PropertyType);

                if (idr[name] == DBNull.Value) continue;

                dynamic value = idr[name];

                if (nullable != null)
                {
                    if (col.DataType == typeof(char?))
                    {
                        if (value == "") continue;
                    }

                    pi.SetValue(result, Convert.ChangeType(value, nullable), null);
                    continue; /* Step to the next property */
                }

                if (pi.PropertyType.IsEnum)
                {
                    pi.SetValue(result, Enum.ToObject(pi.PropertyType, value));
                    continue;
                }

                pi.SetValue(result, Convert.ChangeType(value, pi.PropertyType));
            }

            return result;
        }

        /// <summary>
        ///     Map used to specify the type for more dynamic code.
        /// </summary>
        /// <param name="idr">DbDataRecord from DbDataReader</param>
        /// <param name="type">Type of the object to be returned</param>
        /// <returns>Returns instance of the specified type populated from the database.</returns>
        internal static object Map(DbDataRecord idr, Type type)
        {
            object result = Activator.CreateInstance(type);
            if (result == null) throw new Exception(@"Unable to materialize the specified type.");

            string[] columNames = new string[idr.FieldCount];

            for (int col = 0; col < idr.FieldCount; col++)
            {
                if (idr.IsDBNull(col)) continue; /* Only map the columns that have values */
                columNames[col] = idr.GetName(col).ToLower();
            }

            /* Use the extender to get the relevant properties */
            ConcurrentBag<PropertyMap> mappings = new(result.GetMappings(columNames));
            if (mappings.Count <= 0) return null;


            foreach(PropertyMap pm in mappings)
            {
                PropertyInfo pi = pm.Key;
                Column col = pm.Value;

                string name = string.Equals(col.Name, pi.Name, StringComparison.CurrentCultureIgnoreCase) ? pi.Name : col.Name;
                Type nullable = Nullable.GetUnderlyingType(pi.PropertyType);

                if (idr[name] == DBNull.Value) continue;

                object value = idr[name];

                if (nullable != null)
                {
                    if (col.DataType == typeof(char?))
                    {
                        if (ReferenceEquals(value, "")) continue;
                    }

                    pi.SetValue(result, Convert.ChangeType(value, nullable), null);
                    continue; /* Step to the next property */
                }

                if (pi.PropertyType.IsEnum)
                {
                    pi.SetValue(result, Enum.ToObject(pi.PropertyType, value));
                    continue;
                }

                pi.SetValue(result, Convert.ChangeType(value, pi.PropertyType));
            }

            return result;
        }

        internal static T MapValue<T>(DbDataReader idr)
        {
            if (idr == null) throw new ArgumentNullException(nameof(idr));
            try
            {
                return (T)Convert.ChangeType(idr[0], typeof(T));
            }
            finally
            {
                idr.Close();
            }
        }

        internal static IList<T> MapValues<T>(DbDataReader idr)
        {
            if (idr == null) throw new ArgumentNullException(nameof(idr));
            List<T> results = new List<T>();
            try
            {
                if (!idr.HasRows) return null;

                while (idr.Read())
                {
                    object dbValue = idr[0];
                    if (dbValue.GetType() != typeof(T)) dbValue = Convert.ChangeType(dbValue.GetType(), typeof(T));
                    results.Add((T)dbValue);
                }

                return results;
            }
            finally
            {
                idr.Close();
            }
        }

        #endregion
    }
}