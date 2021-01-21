using System;


namespace SimpleProvider.Attributes
{
    /// <summary>
    /// Defines an Foreign Key Constraint 
    /// </summary>
    [AttributeUsage(AttributeTargets.Property)]
    public class Relationship : Attribute
    {
        /// <summary>
        /// Source Name 
        /// </summary>
        public string Source { get; set; }
        /// <summary>
        /// Column or Property Name
        /// </summary>
        public string ColumnName { get; set; }
    }
}
