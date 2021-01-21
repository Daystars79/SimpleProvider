namespace SimpleProvider.Enumerators
{
    /// <summary>
    ///     Specifies the Database Type
    /// </summary>
    public enum ProviderType : short
    {
        /// <summary>
        ///     Microsoft Sql Server - This is the default
        /// </summary>
        SqlServer = 0,

        /// <summary>
        ///     MySql Server
        /// </summary>
        MySql = 1,

        /// <summary>
        ///     Sqlite Server
        /// </summary>
        Sqlite = 2,

        /// <summary>
        ///     PostGress Sql Server
        /// </summary>
        PostGres = 3,

        /// <summary>
        ///     Oracle Database
        /// </summary>
        Oracle = 4
    }
}