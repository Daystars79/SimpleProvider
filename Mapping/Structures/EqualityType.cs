namespace SimpleProvider
{
    /// <summary>
    ///     Operation Type for Mapping Parameters
    /// </summary>
    public enum EqualityType
        : short
    {
        /// <summary>
        ///     Not used for mapping
        /// </summary>
        None = 0x0000,
        /// <summary>
        ///     Query equality operate is set to =
        /// </summary>
        Equals = 0x0001,
        /// <summary>
        ///     Query equality operator is set to >
        /// </summary>
        GreaterThan = 0x0002,
        /// <summary>
        ///     Query equality operator is set to less than
        ///     ///
        /// </summary>
        LessThan = 0x0003,
        /// <summary>
        ///     Query equality operator is set to 'like'
        ///     Generated parameter adds 'value%'
        /// </summary>
        StartsWith = 0x0004,
        /// <summary>
        ///     Query equality operator is set to 'like'
        ///     Generated parameter adds '%value'
        /// </summary>
        EndsWith = 0x0005,
        /// <summary>
        ///     Query equality operator is set to 'like'
        ///     Generated parameter adds '%value%'
        /// </summary>
        Contains = 0x0006,
        /// <summary>
        ///     SQL equality operator is set to !=
        /// </summary>
        NotEqual = 0x0007,
        /// <summary>
        /// SQL equality operator is set to "IS NULL"
        /// </summary>
        Is = 0x0008,
        /// <summary>
        /// SQL equality operator is set to "IS NOT NULL"
        /// </summary>
        IsNot = 0x0009
    }
}