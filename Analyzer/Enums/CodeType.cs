namespace SimpleProvider.Analyzer.Enums
{
    /// <summary>
    ///     Type that is contained for the generator
    /// </summary>
    public enum CodeType
    {
        /// <summary>
        ///     Appends Class to the generated code
        /// </summary>
        Class = 0x0000,

        /// <summary>
        ///     Appends Struct to the generated code
        /// </summary>
        Struct = 0x0001,

        /// <summary>
        ///     Appends Interface to the generated code
        /// </summary>
        IFace = 0x0003
    }
}