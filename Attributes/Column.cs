using System;

namespace SimpleProvider.Attributes
{
    /// <summary>
    ///     Binds an Database Field to the Properties of an Class
    /// </summary>
    [AttributeUsage(AttributeTargets.Property)]
    public class Column : Attribute
    {
        /// <summary>
        ///     Unique Value and can not be repeated in the Database
        /// </summary>
        public bool IsUnique { get; set; } = false;

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
        /// Contained in the Primary Keys of the Table
        /// </summary>
        public bool IsPrimary { get; set; }
        /// <summary>
        /// Contained in the Foreign Keys of the Table
        /// </summary>
        public bool IsForeign { get; set; }
        /// <summary>
        ///     Property Name is different than that of the Field in the DataTable
        /// </summary>
        public string Name { get; set; } = string.Empty;
    }
}