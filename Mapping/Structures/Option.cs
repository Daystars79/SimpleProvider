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
        private EqualityType _opt;
        private dynamic _value;
        private bool _isOrdered;


        /// <summary>
        /// Constructor for an OrderBy option
        /// </summary>
        /// <param name="name"></param>
        public Option(string name)
        {
            FieldName = name;
            Value = null;
            _opt = EqualityType.None;
        }

        /// <summary>
        /// Default Option
        /// </summary>
        /// <param name="name">Field Name</param>
        /// <param name="value"></param>
        public Option(string name, dynamic value)
        {
            FieldName = name;
            Value = value;
            _opt = EqualityType.Equals;
        }

        /// <summary>
        ///     Create Parameter Instance
        /// </summary>
        /// <param name="fieldname">Field Name</param>
        /// <param name="value">Value</param>
        /// <param name="mt">Equality Type</param>
        public Option(string fieldname, dynamic value, EqualityType mt)
        {
            FieldName = fieldname;
            Value = value;
            _opt = mt;
        }
        /// <summary>
        /// Create OrderedBy and Parameter Instance
        /// </summary>
        /// <param name="fieldname">Field Name</param>
        /// <param name="value">Value</param>
        /// <param name="isAscending">Using ascending sort (default is descending)</param>
        /// <param name="mt">Equality Type</param>
        public Option(string fieldname, dynamic value, bool isAscending, EqualityType mt)
        {
            FieldName = fieldname;
            Value = value;
            OrderBy = true;
            IsAscending = isAscending;
            _opt = mt;
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
        /// Use in the Order By Clause
        /// </summary>
        public bool OrderBy
        {
            get => _isOrdered;
            set
            {
                if (value) _opt = EqualityType.None;
                _isOrdered = value;
            }
        }
        /// <summary>
        /// Use "or" instead of "and" when this is set to true.
        /// </summary>
        public bool UseOrStatement { get; set; }

        /// <summary>
        /// Return the underlying operation type as an string for use in query generation
        /// </summary>
        public string Operator
        {
            get
            {
                return _opt switch
                {
                    EqualityType.None => null,
                    EqualityType.Equals => @" = ",
                    EqualityType.NotEqual => @" != ",
                    EqualityType.GreaterThan => @" > ",
                    EqualityType.LessThan => @" < ",
                    EqualityType.Is => @" IS NULL",
                    EqualityType.IsNot => @" IS NOT NULL ",
                    _ => @" like "
                };
            }
        }
        /// <summary>
        /// Underlying Equality Type 
        /// </summary>
        public EqualityType Type => _opt;
        /// <summary>
        /// Value
        /// </summary>
        public dynamic Value
        {
            get
            {
                return _opt switch
                {
                    EqualityType.None => null,
                    EqualityType.StartsWith => $"{_value}%",
                    EqualityType.EndsWith => $"%{_value}",
                    EqualityType.Contains => $"%{_value}%",
                    _ => _value
                };
            }
            set => _value = value ?? DBNull.Value;
        }
        /// <summary>
        /// Display the Field Name and Value
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return $"Field: {_key} | Value: {_value}";
        }
    }
}