using System;
using System.Reflection;
using Renci.SshNet.Security;
using SimpleProvider.Attributes;

namespace SimpleProvider
{
    internal interface IValuePair<T, TU>
    {
        public T Key { get; set; }
        public TU Value { get; set; }
    }
    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <typeparam name="TU"></typeparam>
    public class ValuePair<T, TU> : IValuePair<T, TU>
    {
        /// <summary>
        /// Data Key
        /// </summary>
        public T Key { get; set; }
        /// <summary>
        /// Data Value
        /// </summary>
        public TU Value { get; set; }

        /// <summary>
        /// Empty Constructor
        /// </summary>
        public ValuePair()
        {

        }
        /// <summary>
        /// Instance created with Key and Value Set
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        public ValuePair(T key, TU value)
        {
            Key = key;
            Value = value;
        }
    }
    /// <summary>
    /// Value Pair with PropertyInfo / Column Attribute Information
    /// </summary>
    public class PropertyMap : IValuePair<PropertyInfo, Column>
    {
        public PropertyInfo Key { get; set; }
        public Column Value { get; set; }

        public PropertyMap()
        {

        }
        public PropertyMap(PropertyInfo key, Column value)
        {
            Key = key;
            Value = value;
        }
    }
    internal class ChangeValue
    {
        dynamic _old;
        dynamic _new;

        public string FieldName { get; set; }
        public dynamic OldValue
        {
            get => _old ?? DBNull.Value;
            set => _old = value;
        }
        public dynamic NewValue
        {
            get => _new ?? DBNull.Value;
            set => _new = value;
        }

        public ChangeValue()
        {

        }
        public ChangeValue(string name, dynamic from, dynamic to)
        {
            FieldName = name;
            OldValue = from;
            NewValue = to;
        }
    }

}