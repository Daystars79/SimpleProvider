using System;
using System.ComponentModel;

namespace SimpleProvider.Attributes
{
    /// <summary>
    ///     Binds an Database Field to the Properties of an Class
    /// </summary>
    [AttributeUsage(AttributeTargets.Property)]
    public class Column : Attribute
    {

        /// <summary>
        ///     Property allows nullable types
        /// </summary>
        public bool IsNullable { get; set; } = true;

        /// <summary>
        ///     Is an database generated value
        /// </summary>
        public bool IsScope { get; set; } = false;

        /// <summary>
        ///     Property does not exist in the Database Table
        /// </summary>
        public bool IsVirtual { get; set; } = false;

        /// <summary>
        ///     Type of Data Contained
        /// </summary>
        public Type DataType { get; set; }

        /// <summary>
        ///     Maximum Length in bytes of the property
        /// </summary>
        public int Length { get; set; } = -1;

        /// <summary>
        ///     Property Name is different than that of the Field in the DataTable
        /// </summary>
        public string Name { get; set; } = string.Empty;

    }
}