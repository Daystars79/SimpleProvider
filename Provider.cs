using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Threading.Tasks;
using SimpleProvider.Constants;
using SimpleProvider.Enumerators;
using SimpleProvider.Extensions;
using SimpleProvider.Factory;
using SimpleProvider.Mapping;


#nullable enable
namespace SimpleProvider
{
    using Attributes;

    /// <summary>
    ///     Generic Object Relationship Mapper
    /// </summary>
    public class Provider : IDisposable
    {
        #region Internal Methods

        private int GetScopeIdentity(object record)
        {
            Definition def = record.GetDefinition();
            string cmdtxt = string.Format(Shared.Scope, def.SchemaName, def.TableName);
            return GetValue<int>(cmdtxt);
        }

        #endregion

        #region Transactions

        /// <summary>
        ///  Wrap command sets in an Database Transaction. Any failure will make it roll back.
        /// </summary>
        /// <param name="commands">Commands to execute against the database</param>
        /// <returns>True if the operation completes successfully</returns>
        public bool ProcessCommands(params CommandSet[] commands)
        {
            DbTransaction transaction = Connection.BeginTransaction();

            try
            {
                for (int x = 0; x < commands.Length; x++)
                {
                    CommandSet cs = commands[x];
                    using DbCommand dbCom = _factory.CreateCommand(cs, _timeout, transaction);
                    dbCom.ExecuteNonQuery();
                }

                transaction.Commit();
                return true;
            }
            catch (Exception)
            {
                transaction.Rollback();
                throw;
            }
        }

        #endregion

        #region Internal Fields

        private int _timeout = 30;
        private bool _disposedValue;
        private readonly DbFactory _factory;
        private readonly CommandSets _commands;

        #endregion

        #region Public Properties

        /// <summary>
        ///     Default Timeout for Commands used by the Provider Class
        /// </summary>
        public int CommandTimeout
        {
            get => _timeout;
            set
            {
                if (value < 30) value = 30;
                _timeout = value;
            }
        }
        /// <summary>
        ///     Return the Underlying Connection
        /// </summary>
        public DbConnection Connection => _factory.Connection;

        /// <summary>
        /// Expose the underlying SQL Generator
        /// </summary>
        public CommandSets Commands => _commands;

        /// <summary>
        ///     Default Schema
        /// </summary>
        public string Schema { get; set; } = "dbo";

        #endregion

        #region Constructors

        /// <summary>
        ///  Instantiate the Provider class using the specified connection string and Database Type
        /// </summary>
        /// <param name="connection">Connection String</param>
        /// <param name="ptype">Database Type (Defaul is ProviderType.SqlServer)</param>
        public Provider(string connection, ProviderType ptype = ProviderType.SqlServer)
        {
            _factory = new DbFactory(connection, ptype);
            _commands = new CommandSets(ptype);
        }

        /// <summary>
        /// Instantiate the Provider class using the specified connection string and Database Type
        /// </summary>
        /// <param name="connection">Connection String</param>
        /// <param name="schema">Default Schema (Default is "dbo")</param>
        /// <param name="ptype">Database Type (Defaul is ProviderType.SqlServer)</param>
        public Provider(string connection, string schema, ProviderType ptype = ProviderType.SqlServer)
        {
            Schema = schema;
            _factory = new DbFactory(connection, ptype);
            _commands = new CommandSets(ptype);
        }

        /// <summary>
        /// Instantiate the Provider class using the specified connection string and Database Type
        /// </summary>
        /// <param name="connection">DbConnection instance</param>
        /// <param name="schema">Default Schema (Default is "dbo")</param>
        public Provider(DbConnection connection, string schema)
        {
            if (connection == null)
                throw new ArgumentNullException(nameof(connection), "Instance of DbConnection must be provided");
            Schema = schema;
            _factory = new DbFactory(connection);
            _commands = new CommandSets(_factory.DatabaseType);
        }

        #endregion

        #region Reference Type - Single Object Methods

        /// <summary>
        ///     Return an single object
        /// </summary>
        /// <param name="command">DbCommand (SqlCommand, OracleCommand, etc)</param>
        /// <param name="type">Reference Type to be returned</param>
        /// <returns></returns>
        public object? GetRecord(DbCommand command, Type type)
        {
            using (command)
            {
                command.Connection = Connection;
                command.CommandTimeout = CommandTimeout;

                using (DbDataReader dbReader = command.ExecuteReader())
                {
                    try
                    {
                        if (dbReader.HasRows)
                        {
                            DbDataRecord[] collection = dbReader.AsParallel().Cast<DbDataRecord>().ToArray();
                            return Mapper.Map(collection[0], type);
                        }
                    }
                    finally
                    {
                        dbReader.Close();
                    }
                }
            }

            return null;
        }

        /// <summary>
        ///     Get the Database Copy of the record
        /// </summary>
        /// <param name="record">Database Record</param>
        /// <param name="type">Object Type</param>
        /// <returns></returns>
        public object? GetRecord(object record, Type type)
        {
            CommandSet cs = _commands.CreateSelect(record);
            using DbCommand command = _factory.CreateCommand(cs);
            return GetRecord(command, type);
        }

        /// <summary>
        ///     Return single instance of T based
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="commandtext"></param>
        /// <returns></returns>
        public T? GetRecord<T>(string commandtext) where T : class, new()
        {
            if (string.IsNullOrEmpty(commandtext)) throw new ArgumentNullException(nameof(commandtext));
            CommandSet cs = new CommandSet
            {
                CommandText = commandtext
            };
            using DbCommand command = _factory.CreateCommand(cs);
            using DbDataReader ddr = command.ExecuteReader();
            if (!ddr.HasRows) return null;
            while (ddr.Read()) return Mapper.Map<T>(ddr);

            return null;
        }

        /// <summary>
        ///     Return single instance of T from the database
        /// </summary>
        /// <typeparam name="T">T : class, new()</typeparam>
        /// <param name="command">DbCommand (SqlCommand, OracleCommand, etc)</param>
        /// <returns></returns>
        public T? GetRecord<T>(DbCommand command) where T : class, new()
        {
            using (command)
            {
                command.Connection = Connection;
                command.CommandTimeout = CommandTimeout;

                using (DbDataReader dbReader = command.ExecuteReader())
                {
                    try
                    {
                        if (dbReader.HasRows)
                        {
                            DbDataRecord[] collection = dbReader.AsParallel().Cast<DbDataRecord>().ToArray();
                            return Mapper.Map<T>(collection[0]);
                        }
                    }
                    finally
                    {
                        dbReader.Close();
                    }
                }
            }

            return null;
        }

        /// <summary>
        ///     Return single instance of T from the database
        /// </summary>
        /// <typeparam name="T">T : class, new()</typeparam>
        /// <param name="args">Collection of Mapping Parameters</param>
        /// <returns></returns>
        public T? GetRecord<T>(params Option[] args) where T : class, new()
        {
            CommandSet cs = _commands.CreateSelect<T>(args);
            using DbCommand dc = _factory.CreateCommand(cs);
            return GetRecord<T>(dc);
        }
        #endregion

        #region Reference Type - Collection Methods

        /// <summary>
        ///     Return an collection of objects that match the query provided to the command.
        /// </summary>
        /// <param name="command">DbCommand Interface.</param>
        /// <returns></returns>
        public dynamic GetDynamicRecords(DbCommand command)
        {
            ConcurrentBag<dynamic> results = new ConcurrentBag<dynamic>();
            DbDataRecord[] collection;
            using (command)
            {
                command.Connection = Connection;
                command.CommandTimeout = CommandTimeout;

                using (DbDataReader dbReader = command.ExecuteReader())
                {
                    try
                    {
                        if (dbReader.HasRows)
                        {
                            collection = dbReader.AsParallel().Cast<DbDataRecord>().ToArray();
                            Parallel.For(0,
                                collection.Length,
                                index =>
                                {
                                    dynamic result = Mapper.DynamicMap(collection[index]);
                                    if (result != null) results.Add(result);
                                });
                        }
                    }
                    finally
                    {
                        dbReader.Close();
                    }
                }
                return results.ToArray();
            }
        }

        /// <summary>
        ///  Return collection of T using the provided mapping parameters. Schema / Table names are generated from the
        ///  Definition Attribute
        /// </summary>
        /// <typeparam name="T">T : class, new()</typeparam>
        /// <param name="args">Fields and values to check for.</param>
        /// <returns></returns>
        public IList<T> GetRecords<T>(params Option[] args) where T : class, new()
        {
            CommandSet cs = _commands.CreateSelect<T>(args);
            using DbCommand command = _factory.CreateCommand(cs, _timeout);
            return GetRecords<T>(command);
        }

        /// <summary>
        ///     Return collection of T using the provided query and mapping parameters
        /// </summary>
        /// <typeparam name="T">T : class, new()</typeparam>
        /// <param name="cmdtxt">Query to execute</param>
        /// <param name="parameters">Collection of parameters</param>
        /// <returns></returns>
        public IList<T> GetRecords<T>(string cmdtxt, params Option[] parameters) where T : class, new()
        {
            CommandSet cs = new(cmdtxt, parameters);
            using DbCommand dc = _factory.CreateCommand(cs, _timeout);
            return GetRecords<T>(dc);
        }

        /// <summary>
        ///  Returns collection of T using the provided DbCommand
        /// </summary>
        /// <typeparam name="T">T : class, new()</typeparam>
        /// <param name="command">DbCommand</param>
        /// <param name="step">Notify the host application an record has been loaded.</param>
        /// <param name="complete">Notify the host application the operation is completed</param>
        /// <returns></returns>
        public IList<T> GetRecords<T>(DbCommand command, Action? step = null, Action? complete = null)
            where T : class, new()
        {
            ConcurrentBag<T> results = new();
            DbDataRecord[] collection;
            try
            {
                using (command)
                {
                    command.Connection = Connection;
                    command.CommandTimeout = CommandTimeout;

                    using (DbDataReader dbReader = command.ExecuteReader())
                    {
                        try
                        {
                            if (dbReader.HasRows)
                            {
                                collection = dbReader.AsParallel().Cast<DbDataRecord>().ToArray();
                                Parallel.For(0,
                                    collection.Length,
                                    index =>
                                    {
                                        T? result = Mapper.Map<T>(collection[index]);
                                        if (result != null) results.Add(result);
                                        step?.Invoke();
                                    });
                            }
                        }
                        finally
                        {
                            dbReader.Close();
                        }
                    }
                    return results.ToArray();
                }
            }
            finally
            {
                complete?.Invoke();
            }
        }

        /// <summary>
        ///     Return the specified amount of records of the specified type
        /// </summary>
        /// <typeparam name="T">T : class, new()</typeparam>
        /// <param name="count">Number of rows to return</param>
        /// <param name="args">Parameters for the where clause</param>
        /// <returns></returns>
        public IList<T> GetTopRecords<T>(int count, params Option[] args) where T : class, new()
        {
            CommandSet cs = _commands.CreateTop<T>(count, args);
            using DbCommand dc = _factory.CreateCommand(cs.CommandText);
            return GetRecords<T>(dc);
        }

        /// <summary>
        ///     Returns the specified amount of records from the Database
        /// </summary>
        /// <param name="count">Number of records to retrieve</param>
        /// <param name="type">Reference Type</param>
        /// <param name="args"></param>
        /// <returns></returns>
        public IList<object>? GetTopRecords(int count, Type type, params Option[] args)
        {
            ConcurrentBag<object> results = new();
            CommandSet cs = _commands.CreateTop(count, type, args);
            using DbCommand dc = _factory.CreateCommand(cs, _timeout);
            using DbDataReader ddr = dc.ExecuteReader();
            if (!ddr.HasRows) return null;
            while (ddr.Read())
            {
                object? result = Mapper.Map(ddr, type);
                if (result != null) results.Add(result);
            }

            return results.ToList();
        }

        #endregion

        #region Value Methods

        /// <summary>
        ///     Retrieve an Value Type from the Database
        /// </summary>
        /// <typeparam name="T">Value Type</typeparam>
        /// <param name="cmdtxt">Command Text for the DBCommand</param>
        /// <returns></returns>
        public T? GetValue<T>(string cmdtxt)
        {
            CommandSet cs = new(cmdtxt);
            using DbCommand command = _factory.CreateCommand(cs, _timeout);
            using DbDataReader ddr = command.ExecuteReader();
            if (!ddr.HasRows) return default;
            while (ddr.Read()) return Mapper.MapValue<T>(ddr);
            return default;
        }

        /// <summary>
        ///     Return an Value Type from the Database
        /// </summary>
        /// <typeparam name="T">Value Type</typeparam>
        /// <param name="cmdtxt">Query String</param>
        /// <param name="args">Parameters</param>
        /// <returns></returns>
        public T? GetValue<T>(string cmdtxt, params Option[] args)
        {
            CommandSet cs = new(cmdtxt, args);
            using DbCommand command = _factory.CreateCommand(cs, _timeout);
            return GetValue<T>(command);
        }

        /// <summary>
        ///     Return a Value type from the database
        /// </summary>
        /// <typeparam name="T">Value Type</typeparam>
        /// <param name="dbCommand">DbCommand (e.g. SqlCommand, OracleCommand)</param>
        /// <returns></returns>
        public T? GetValue<T>(DbCommand dbCommand)
        {
            dbCommand.CommandTimeout = _timeout;
            dbCommand.Connection = Connection;
            using DbDataReader ddr = dbCommand.ExecuteReader();
            if (!ddr.HasRows) return default;
            while (ddr.Read()) return Mapper.MapValue<T>(ddr);
            return default;
        }

        /// <summary>
        ///     Return an collection of T from the database
        /// </summary>
        /// <typeparam name="T">Value Type (int, char, bool etc....)</typeparam>
        /// <param name="dbCommand">DbCommand (e.g. SqlCommand, OracleCommand)</param>
        /// <returns></returns>
        public IList<T>? GetValues<T>(DbCommand dbCommand)
        {
            dbCommand.Connection = Connection;
            dbCommand.CommandTimeout = _timeout;
            using DbDataReader ddr = dbCommand.ExecuteReader();
            if (!ddr.HasRows) return null;
            return Mapper.MapValues<T>(ddr)?.ToList();
        }

        /// <summary>
        ///     Return an collection of T from the database
        /// </summary>
        /// <typeparam name="T">Value Type (int, char, bool etc....)</typeparam>
        /// <param name="cmdtxt">Query Text</param>
        /// <param name="args">Parameters</param>
        /// <returns></returns>
        public IList<T>? GetValues<T>(string cmdtxt, params Option[] args)
        {
            CommandSet cs = new CommandSet(cmdtxt, args);
            using DbCommand command = _factory.CreateCommand(cs, _timeout);
            return GetValues<T>(command);
        }

        #endregion

        #region Insert, Update, and Delete Methods

        /// <summary>
        ///     Insert an record into the database
        /// </summary>
        /// <param name="record"></param>
        /// <returns>True if the insert is successful</returns>
        public bool Insert(object record)
        {
            if (record == null) throw new ArgumentNullException(nameof(record));
            if (Exists(record)) return false;

            CommandSet cs = _commands.CreateInsert(record);
            if (cs.ReadOnly) return false;

            using DbCommand command = _factory.CreateCommand(cs, _timeout);
            int result = command.ExecuteNonQuery();
            if (result <= 0) return false;
            if (!cs.HasScope) return true;

            int value = GetScopeIdentity(record);
            cs.Scope.SetValue(record, value);
            return true;
        }

        /// <summary>
        ///     Insert an record into the database
        /// </summary>
        /// <param name="command">DbCommand with Query and Parameters set</param>
        /// <returns>True if the insert is successful</returns>
        public bool Insert(DbCommand command)
        {
            command.Connection = Connection;
            command.CommandTimeout = _timeout;
            return command.ExecuteNonQuery() > 0;
        }

        /// <summary>
        ///     Update Database Record
        /// </summary>
        /// <param name="command">DbCommand (SqlCommand, OracleCommand etc...)</param>
        /// <returns>True if the update is successful</returns>
        public bool Update(DbCommand command)
        {
            command.Connection = Connection;
            command.CommandTimeout = _timeout;
            return command.ExecuteNonQuery() > 0;
        }

        /// <summary>
        ///     Updates the database record, if it does not exist then it inserts the record.
        ///     Automatically creates the query based on changes of the object to be update versus what is contained in the
        ///     database
        /// </summary>
        /// <param name="record">Object to be updated</param>
        /// <param name="args">Optional parameters to override the default behavior</param>
        /// <returns>True if the operation completes successfully.</returns>
        public bool Update(object record, params Option[] args)
        {
            if (record == null) throw new ArgumentNullException(nameof(record));
            if (!Exists(record)) return Insert(record);

            CommandSet cs = _commands.CreateSelect(record);
            using DbCommand dbc = _factory.CreateCommand(cs);
            object? dbRef = GetRecord(dbc, record.GetType());
            if (dbRef == null) return Insert(record); /* Record wasn't found proceed with an insert */

            cs = _commands.CreateUpdate(record, dbRef, args);

            if (cs == null) return false;
            if (cs.ReadOnly) return false;

            using DbCommand updateCommand = _factory.CreateCommand(cs, _timeout);
            return updateCommand.ExecuteNonQuery() > 0;
        }

        /// <summary>
        ///     Delete the specified record from the database
        /// </summary>
        /// <param name="record"></param>
        /// <returns>Returns true if the record is deleted</returns>
        public bool Delete(object record)
        {
            if (record == null) throw new ArgumentNullException(nameof(record));
            if (!Exists(record)) return false;
            CommandSet cs = _commands.CreateDelete(record);

            if (cs.ReadOnly) return false;

            using DbCommand command = _factory.CreateCommand(cs);
            return command.ExecuteNonQuery() > 0;
        }

        #endregion

        #region Execution Methods

        /// <summary>
        ///     Verify if an record exists in the database
        /// </summary>
        /// <param name="record"></param>
        /// <returns>True if the record exists</returns>
        public bool Exists(object record)
        {
            if (record == null) throw new ArgumentNullException(nameof(record));
            CommandSet cs = _commands.CreateExists(record);

            using DbCommand command = _factory.CreateCommand(cs.CommandText);
            if (cs.HasParameters) command.Parameters.AddRange(_factory.CreateParameters(cs.Parameters));
            return GetValue<int>(command) > 0;
        }

        /// <summary>
        ///     Execute NonQuery using the DbCommand
        /// </summary>
        /// <param name="command">DbCommand (SqlCommand, OracleCommand, etc....)</param>
        /// <returns>Number of records effected</returns>
        public int Execute(DbCommand command)
        {
            command.Connection = Connection;
            command.CommandTimeout = _timeout;
            return command.ExecuteNonQuery();
        }

        /// <summary>
        /// </summary>
        /// <param name="cmdtxt"></param>
        /// <param name="args"></param>
        /// <returns></returns>
        public int Execute(string cmdtxt, params Option[] args)
        {
            if (string.IsNullOrEmpty(cmdtxt)) throw new ArgumentNullException(nameof(cmdtxt));
            CommandSet cs = new CommandSet
            {
                CommandText = cmdtxt
            };
            cs.AddRange(args);
            using DbCommand command = _factory.CreateCommand(cs);
            return command.ExecuteNonQuery();
        }

        #endregion


        #region Disposable

        /// <summary>
        /// </summary>
        /// <param name="disposing"></param>
        protected virtual void Dispose(bool disposing)
        {
            if (!_disposedValue)
            {
                if (disposing)
                {
                    // TODO: dispose managed state (managed objects)
                }

                // TODO: free unmanaged resources (unmanaged objects) and override finalizer
                // TODO: set large fields to null
                _disposedValue = true;
            }
        }

        /// <summary>
        /// </summary>
        public void Dispose()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        #endregion
    }
}