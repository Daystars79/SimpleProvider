using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Reflection;


#nullable  enable
namespace SimpleProvider.Extensions
{
    using Attributes;


    /// <summary>
    ///     Extensions that are used publicly by the ORM
    /// </summary>
    public static class Extenders
    {
        /// <summary>
        ///     Returns an list of PropertyMap Objects for the Mapper
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="names">Name of properties to be returned.</param>
        internal static List<PropertyMap> GetMappings(this object obj, params string[] names)
        {
            List<PropertyMap> results = new();
            Type type = obj.GetType();
            PropertyInfo[] properties = type.GetProperties().Where(w => w.CanRead && w.CanWrite).ToArray();

            foreach (PropertyInfo pi in properties)
            {
                Column cd = pi.GetColumnDefinition();
                if (cd.IsVirtual) continue;

                if (names?.Length > 0)
                {
                    if (names.Any(n => string.Equals(n, cd.Name, StringComparison.CurrentCultureIgnoreCase))) results.Add(new PropertyMap(pi, cd));
                }
                else
                {
                    results.Add(new PropertyMap(pi, cd));
                }
            }

            return results;
        }

        /// <summary>
        ///     Return PropertyInfo of all properties that have Read / Write
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="names"></param>
        /// <returns>PropertyInfo[]</returns>
        internal static PropertyInfo[] GetProperties(this object obj, params string[]? names)
        {
            Type type = obj.GetType();
            PropertyInfo[] properties = type.GetProperties().Where(w => w.CanRead && w.CanWrite).ToArray();
            List<PropertyInfo> results = new();

            foreach (PropertyInfo pi in properties)
            {
                var cd = pi.GetColumnDefinition();
                if (cd.IsVirtual) continue;

                if (names?.Length > 0)
                {
                    if (names.Any(a => string.Equals(a, cd.Name, StringComparison.CurrentCultureIgnoreCase))) results.Add(pi);
                }
                else
                {
                    results.Add(pi);
                }
            }
            return results.ToArray();
        }

        /// <summary>
        ///     Retrieve the Column Attribute from the PropertyInfo
        /// </summary>
        /// <param name="info"></param>
        /// <returns>Column Attribute</returns>
        internal static Column GetColumnDefinition(this PropertyInfo info)
        {
            Type? nullable = Nullable.GetUnderlyingType(info.PropertyType);

            if (info.GetCustomAttribute(typeof(Column)) is Column attr)
            {
                if (string.IsNullOrEmpty(attr.Name)) attr.Name = info.Name;
                if (attr.DataType == null) attr.DataType = info.PropertyType;
                attr.IsNullable = !(info.PropertyType.IsValueType & (nullable == null));
                return attr;
            }

            return new Column
            {
                Name = info.Name,
                DataType = info.PropertyType,
                IsVirtual = false,
                IsScope = false,
                IsNullable = !(info.PropertyType.IsValueType & (nullable == null))
            };
        }

        /// <summary>
        ///     Retrieve Definition Attribute from an class
        /// </summary>
        /// <param name="obj"></param>
        /// <returns>Definition Attribute</returns>
        internal static Definition GetDefinition(this object obj)
        {
            Type type = obj.GetType();
            Definition? def = type.GetCustomAttribute<Definition>();
            if (def != null) return def;
            return new Definition
            {
                TableName = type.Name,
                SchemaName = "dbo",
                IsReadOnly = false
            };
        }

        internal static string[]? GetKeyNames(this object obj)
        {
            Type type = obj.GetType();
            Definition? attrib = type.GetCustomAttribute<Definition>();
            return attrib?.PrimaryKeys.Select(s => s.ToLower()).ToArray() ?? null;
        }

        #region public Extensions

        /// <summary>
        /// Convert object into an ExpandoObject
        /// </summary>
        /// <param name="obj"></param>
        /// <returns>ExpandoObject</returns>
        public static ExpandoObject ToExpando(this object obj)
        {
            ExpandoObject eo = new();
            PropertyInfo[] props = obj.GetProperties();

            for (int index = 0; index < props.Length; index++)
            {
#if (NET461)
                ((IDictionary<string, object>) eo).Add(props[index].Name, props[index].GetValue(obj));
#else
                eo.TryAdd(props[index].Name, props[index].GetValue(obj));
#endif
            }

            return eo;
        }

        #endregion
    }
}