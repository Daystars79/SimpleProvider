using System;

namespace SimpleProvider
{
    /// <summary>
    ///     Options for the sql generator contained in the mapper.
    ///     Used to pass parameters to the database and orderby information
    /// </summary>
    public class Option
    {
        private string _key = string.Empty;
        private dynamic _value;

        /// <summary>
        ///     Create OrderBy Instance
        /// </summary>
        /// <param name="name">Field Name</param>
        /// <param name="ascending">Using ascending sort (default is descending)</param>
        public Option(string name, bool ascending = false)
        {
            FieldName = name;
            IsOrderBy = true;
            IsAscending = ascending;
            Type = EqualityType.None;
        }

        /// <summary>
        ///     Create Parameter Instance
        /// </summary>
        /// <param name="fieldname">Field Name</param>
        /// <param name="value">Value</param>
        /// <param name="mt">Equality Type</param>
        public Option(string fieldname, dynamic value, EqualityType mt = EqualityType.Equals)
        {
            FieldName = fieldname;
            Value = value;
            Type = mt;
        }

        /// <summary>
        ///     Create OrderedBy and Parameter Instance
        /// </summary>
        /// <param name="fieldname">Field Name</param>
        /// <param name="value">Value</param>
        /// <param name="ascending">Using ascending sort (default is descending)</param>
        /// <param name="mt">Equality Type</param>
        public Option(string fieldname, dynamic value, bool ascending, EqualityType mt = EqualityType.Equals)
        {
            FieldName = fieldname;
            Value = value;
            IsOrderBy = true;
            IsAscending = ascending;
            Type = mt;
        }

        /// <summary>
        ///     Parameter Name or Field Name
        /// </summary>
        public string FieldName
        {
            get => _key;
            set
            {
                if (!string.IsNullOrEmpty(value)) value = value.Replace("@", "");
                _key = value;
            }
        }

        /// <summary>
        ///     Order By Direction
        /// </summary>
        public bool IsAscending { get; set; }


        /// <summary>
        ///     Use in the Order By Clause
        /// </summary>
        public bool IsOrderBy { get; set; }

        /// <summary>
        ///     Return the underlying operation type as an string for use in query generation
        /// </summary>
        public string Operator
        {
            get
            {
                switch (Type)
                {
                    case EqualityType.None:
                        return null;
                    case EqualityType.Equals:
                        return @" = ";
                    case EqualityType.NotEqual:
                        return @" != ";
                    case EqualityType.GreaterThan:
                        return @" > ";
                    case EqualityType.LessThan:
                        return @" < ";
                    default:
                        return @" like ";
                }
            }
        }
        /// <summary>
        /// Return the equality type used in the SQL Generation 
        /// </summary>
        public EqualityType Type { get; } 

        /// <summary>
        ///     Value
        /// </summary>
        public dynamic Value
        {
            get
            {
                switch (Type)
                {
                    case EqualityType.None:
                        return null;
                    case EqualityType.StartsWith:
                        return $"{_value}%";
                    case EqualityType.EndsWith:
                        return $"%{_value}";
                    case EqualityType.Contains:
                        return $"%{_value}%";
                    default:
                        return _value;
                }
            }
            set => _value = value ?? DBNull.Value;
        }

        /// <summary>
        ///     Returns an string containing the Key and the Value
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return $"Field: {_key} | Value: {_value}";
        }
    }
}