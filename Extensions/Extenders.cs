using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;


namespace SimpleProvider.Extensions
{
    using Attributes;


    /// <summary>
    /// 
    /// </summary>
    public static class Extenders
    {
        /// <summary>
        ///  Returns all readable / writable properties and column attributes from the current type
        /// </summary>
        /// <param name="obj">Object</param>
        /// <param name="names"></param>
        /// <returns></returns>
        public static PropertyMap[] GetMappings(this object obj, params string[] names)
        {
            PropertyInfo[] props = obj.GetType().GetProperties();
            bool check = names?.Length > 0;

            props = props.Where(w => w.CanWrite & w.CanRead).ToArray();
            if (check)
            {
                props = props.Where(w =>
                    names.Any(a => string.Equals(a, w.Name, StringComparison.CurrentCultureIgnoreCase))).ToArray();
            }
            PropertyMap[] map = new PropertyMap[props.Length];
            for (int x = 0; x < map.Length; x++)
            {
                map[x] = new PropertyMap(props[x], props[x].GetColumnDefinition());
            }
            /* Remove the virtual columns */
            map = map.Where(w => !w.Value.IsVirtual).ToArray();
            return map;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="names"></param>
        /// <returns></returns>
        public static PropertyInfo[] GetProperties(this object obj, params string[] names)
        {
            Type type = obj.GetType();
            PropertyInfo[] properties = type.GetProperties().Where(w => w.CanRead && w.CanWrite).ToArray();
            List<PropertyInfo> results = new List<PropertyInfo>();

            foreach (PropertyInfo pi in properties)
            {
                Column cd = pi.GetColumnDefinition();
                if (cd.IsVirtual) continue;

                if (names?.Length > 0)
                {
                    if (names.Any(n => string.Equals(n, cd.Name, StringComparison.CurrentCultureIgnoreCase))) results.Add(pi);
                }
                else
                {
                    results.Add(pi);
                }
            }
            return results.ToArray();
        }
        /// <summary>
        /// Return the column defintion for the selected property
        /// </summary>
        /// <param name="info"></param>
        /// <returns></returns>
        public static Column GetColumnDefinition(this PropertyInfo info)
        {
            Type nullable = Nullable.GetUnderlyingType(info.PropertyType);

            if (info.GetCustomAttribute(typeof(Column)) is Column attr)
            {
                if (string.IsNullOrEmpty(attr.Name))
                {
                    attr.Name = info.Name;
                }
                if (attr.DataType == null)
                {
                    attr.DataType = info.PropertyType;
                }
                attr.IsNullable = !(info.PropertyType.IsValueType & nullable == null);
                return attr;
            }
            return new Column
            {
                Name = info.Name,
                DataType = info.PropertyType,
                IsVirtual = false,
                IsScope = false,
                IsNullable = !(info.PropertyType.IsValueType & nullable == null)
            };
        }
        /// <summary>
        /// Return the definition attribute from the selected Type
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public static Definition GetDefinition(this object obj)
        {
            Type type = obj.GetType();
            Definition def = type.GetCustomAttribute<Definition>();
            if (def != null)
            {
                return def;
            }
            return new Definition
            {
                TableName = type.Name,
                SchemaName = "dbo",
                IsReadOnly = false,
            };
        }
        #region public Extensions
        /// <summary>
        /// Return primary key names contained in the Definition Attribute
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public static string[] GetKeyNames(this object obj)
        {
            Type type = obj.GetType();
            Definition attrib = type.GetCustomAttribute<Definition>();
            string[] keys = attrib?.PrimaryKeys.Select(s => s?.ToLower()).ToArray();
            return keys;
        }
        #endregion
    }
}