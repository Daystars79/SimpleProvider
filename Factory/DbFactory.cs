using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Reflection;
using SimpleProvider.Constants;
using SimpleProvider.Enumerators;
using SimpleProvider.Extensions;

namespace SimpleProvider.Factory
{
    using Attributes;
#nullable  enable
    internal class DbFactory : IDisposable
    {
        private bool _disposedValue;

        private readonly string _operator;
        private readonly ProviderType _type;

        public ProviderType DatabaseType => _type;


        public DbFactory(string connectionString, ProviderType type)
        {
            _type = type;
            _operator = _type == ProviderType.Oracle ? ":" : "@";
            Connection = CreateConnection(connectionString);
            Connection.Open();
        }

        public DbFactory(DbConnection connection)
        {
            Connection = connection;
            switch (Connection)
            {
                case Oracle.ManagedDataAccess.Client.OracleConnection:
                    _type = ProviderType.Oracle;
                    break;
                case System.Data.SQLite.SQLiteConnection:
                    _type = ProviderType.Sqlite;
                    break;
                case Npgsql.NpgsqlConnection:
                    _type = ProviderType.PostGres;
                    break;
                case MySql.Data.MySqlClient.MySqlConnection:
                    _type = ProviderType.MySql;
                    break;
                default:
                    _type = ProviderType.SqlServer;
                    break;
            }
            _operator = _type == ProviderType.Oracle ? ":" : "@";
            if (Connection.State != ConnectionState.Open) Connection.Open();
        }

        public DbConnection Connection { get; }

        #region Connection and Command

        internal DbConnection CreateConnection(string connectionString)
        {
            if (connectionString == null)
                throw new ArgumentNullException(nameof(connectionString),
                    "Connection string provided is null or empty");

            return _type switch
            {
                ProviderType.Oracle => new Oracle.ManagedDataAccess.Client.OracleConnection(connectionString),
                ProviderType.Sqlite => new System.Data.SQLite.SQLiteConnection(connectionString),
                ProviderType.PostGres => new Npgsql.NpgsqlConnection(connectionString),
                ProviderType.MySql => new MySql.Data.MySqlClient.MySqlConnection(connectionString),
                _ => new System.Data.SqlClient.SqlConnection(connectionString) { StatisticsEnabled = true },
            };
        }

        internal DbCommand CreateCommand()
        {
            DbCommand command = _type switch
            {
                ProviderType.MySql => new MySql.Data.MySqlClient.MySqlCommand(),
                ProviderType.Sqlite => new System.Data.SQLite.SQLiteCommand(),
                ProviderType.PostGres => new Npgsql.NpgsqlCommand(),
                ProviderType.Oracle => new Oracle.ManagedDataAccess.Client.OracleCommand(),
                _ => new System.Data.SqlClient.SqlCommand(),
            };
            command.Connection = Connection;
            return command;
        }
        internal DbCommand CreateCommand(string commandtext)
        {
            DbCommand command = CreateCommand();
            command.CommandText = commandtext;
            return command;
        }

        internal DbCommand CreateCommand(CommandSet cs, int timeout = 60, DbTransaction? transaction = null)
        {
            DbCommand command = _type switch
            {
                ProviderType.MySql => new MySql.Data.MySqlClient.MySqlCommand { CommandText = cs.CommandText },
                ProviderType.Sqlite => new System.Data.SQLite.SQLiteCommand { CommandText = cs.CommandText },
                ProviderType.PostGres => new Npgsql.NpgsqlCommand { CommandText = cs.CommandText },
                ProviderType.Oracle => new Oracle.ManagedDataAccess.Client.OracleCommand { CommandText = cs.CommandText },
                _ => new System.Data.SqlClient.SqlCommand { CommandText = cs.CommandText }
            };

            if (cs.HasParameters) command.Parameters.AddRange(CreateParameters(cs.Parameters));
            command.Connection = Connection;
            command.CommandTimeout = timeout;
            command.Transaction = transaction;
            return command;
        }

        #endregion

        #region Parameters

        internal DbParameter CreateParameter(string fieldName, dynamic value)
        {
            fieldName = fieldName.Replace(_operator, "");
            string name = $"{_operator}{fieldName}";
            return _type switch
            {
                ProviderType.Oracle => new Oracle.ManagedDataAccess.Client.OracleParameter
                {
                    ParameterName = fieldName,
                    Value = value
                },
                ProviderType.MySql => new MySql.Data.MySqlClient.MySqlParameter
                {
                    ParameterName = name,
                    Value = value
                },
                ProviderType.PostGres => new Npgsql.NpgsqlParameter
                {
                    ParameterName = name,
                    Value = value
                },
                ProviderType.Sqlite => new System.Data.SQLite.SQLiteParameter
                {
                    ParameterName = name,
                    Value = value
                },
                _ => new System.Data.SqlClient.SqlParameter
                {
                    ParameterName = fieldName,
                    Value = value
                },
            };
        }

        internal DbParameter CreateParameter(object value, PropertyInfo pi)
        {
            Column column = pi.GetColumnDefinition();
            string name = $"{_operator}{column.Name}";


            return _type switch
            {
                ProviderType.Oracle => new Oracle.ManagedDataAccess.Client.OracleParameter
                {
                    ParameterName = name,
                    Size = column.Length,
                    Value = value
                },
                ProviderType.MySql => new MySql.Data.MySqlClient.MySqlParameter
                {
                    ParameterName = name,
                    Size = column.Length,
                    Value = value
                },
                ProviderType.PostGres => new Npgsql.NpgsqlParameter
                {
                    ParameterName = name,
                    Size = column.Length,
                    Value = value
                },
                ProviderType.Sqlite => new System.Data.SQLite.SQLiteParameter
                {
                    ParameterName = name,
                    Size = column.Length,
                    Value = value
                },
                _ => new System.Data.SqlClient.SqlParameter
                {
                    ParameterName = name,
                    Size = column.Length,
                    Value = value
                },
            };
        }

        internal DbParameter[] CreateParameters(ICollection<Option> values)
        {
            DbParameter[] parameters = new DbParameter[values.Count];
            int index = 0;
            foreach (var opt in values)
        {
                parameters[index] = CreateParameter($"{_operator}{opt.FieldName}", opt.Value);
                index++;
            }
            return parameters;
        }

        #endregion Parameters

        #region Disposable Interfaces

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

        public void Dispose()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        #endregion
    }
}