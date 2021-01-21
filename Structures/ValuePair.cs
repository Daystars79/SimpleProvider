using System;
using System.Reflection;
using SimpleProvider.Attributes;

namespace SimpleProvider
{
    /// <summary>
    ///     Base Key
    /// </summary>
    /// <typeparam name="T">Key Type</typeparam>
    /// <typeparam name="TU">Value Type</typeparam>
    public abstract class ValuePairBase<T, TU>
    {
        internal T Key;
        internal TU Value;

        /// <summary>
        ///     Empty Constructor
        /// </summary>
        protected ValuePairBase()
        {
        }

        /// <summary>
        ///     Instance with Key and Value
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        protected ValuePairBase(T key, TU value)
        {
            Key = key;
            Value = value;
        }

        /// <summary>
        ///     Return Key / Value in an formatted string.
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return $"Key: {Key} : Value: {Value}";
        }
    }

    internal class PropertyMap : ValuePairBase<PropertyInfo, Column>
    {
        public PropertyMap(PropertyInfo key, Column value) : base(key, value)
        {
        }

        public new PropertyInfo Key => base.Key;
        public new Column Value => base.Value;
    }

    /// <summary>
    ///     Used to track changes to Objects for posting Updates to the DB.
    /// </summary>
    public class ChangeValue
    {
        private dynamic _new;
        private dynamic _old;

        /// <summary>
        /// </summary>
        /// <param name="name">Field Name</param>
        /// <param name="from">Original Value</param>
        /// <param name="to">New Value</param>
        public ChangeValue(string name, dynamic from, dynamic to)
        {
            FieldName = name;
            OldValue = from;
            NewValue = to;
        }

        /// <summary>
        ///     Field Name in the Database
        /// </summary>
        public string FieldName { get; set; }

        /// <summary>
        ///     Original Value
        /// </summary>
        public dynamic OldValue
        {
            get => _old ?? DBNull.Value;
            set => _old = value;
        }

        /// <summary>
        ///     New Value
        /// </summary>
        public dynamic NewValue
        {
            get => _new ?? DBNull.Value;
            set => _new = value;
        }
    }
}