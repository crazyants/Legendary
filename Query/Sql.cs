﻿using System;
using System.Collections.Generic;
using System.Text;
using System.Data.SqlClient;
using System.Threading.Tasks;
using Dapper;

namespace Legendary.Query
{
    public class Sql
    {

        private SqlConnection conn = new SqlConnection();
        private SqlCommand cmd = new SqlCommand();
        private string connString = "";

        public Sql(string connectionString)
        {
            connString = connectionString;
        }


        private void Start()
        {
            if (conn.State == System.Data.ConnectionState.Closed)
            {
                conn.ConnectionString = connString;
                conn.Open();
                cmd.Connection = conn;
            }
        }

        private string GetStoredProc(string storedproc, Dictionary<string, object> parameters = null)
        {
            var sql = new StringBuilder("EXEC " + storedproc);
            if (parameters != null)
            {
                var x = 0;
                foreach (var parm in parameters)
                {
                    sql.Append(" " + (x > 0 ? "," : "") + "@" + parm.Key + "=@" + parm.Key);
                    x++;
                }
            }
            return sql.ToString();
        }

        private List<SqlParameter> GetSqlParameters(Dictionary<string, object> parameters = null)
        {
            var parms = new List<SqlParameter>();
            foreach (var parm in parameters)
            {
                parms.Add(new SqlParameter("@" + parm.Key, parm.Value));
            }
            return parms;
        }

        public SqlDataReader ExecuteReader(string storedproc, Dictionary<string, object> parameters = null)
        {
            Start();
            cmd.Parameters.Clear();
            cmd.CommandText = GetStoredProc(storedproc, parameters);
            try
            {
                if (parameters != null) { GetSqlParameters(parameters).ForEach(a => cmd.Parameters.Add(a)); }
                return cmd.ExecuteReader();
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public void ExecuteNonQuery(string storedproc, Dictionary<string, object> parameters = null)
        {
            Start();
            cmd.Parameters.Clear();
            cmd.CommandText = GetStoredProc(storedproc, parameters);
            try
            {
                if (parameters != null) { GetSqlParameters(parameters).ForEach(a => cmd.Parameters.Add(a)); }
                cmd.ExecuteNonQuery();
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public T ExecuteScalar<T>(string storedproc, Dictionary<string, object> parameters = null)
        {
            Start();
            cmd.Parameters.Clear();
            cmd.CommandText = GetStoredProc(storedproc, parameters);
            try
            {
                if (parameters != null) { cmd.Parameters.AddRange(GetSqlParameters(parameters).ToArray()); }
                return (T)cmd.ExecuteScalar();
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public async Task<int> ExecuteNonQueryAsync(string storedproc, Dictionary<string, object> parameters = null)
        {
            using (var newConnection = new SqlConnection(connString))
            using (var newCommand = new SqlCommand(GetStoredProc(storedproc, parameters), newConnection))
            {
                try
                {
                    if (parameters != null) newCommand.Parameters.AddRange(GetSqlParameters(parameters).ToArray());
                    await newConnection.OpenAsync().ConfigureAwait(false);
                    return await newCommand.ExecuteNonQueryAsync().ConfigureAwait(false);

                }
                catch (Exception ex)
                {
                    throw ex;
                }
            }
        }

        public List<T> Populate<T>(string storedproc, Dictionary<string, object> parameters = null)
        {
            Start();
            return conn.Query<T>(GetStoredProc(storedproc, parameters), parameters).AsList<T>();
        }

        public SqlMapper.GridReader PopulateMultiple(string storedproc, Dictionary<string, object> parameters = null)
        {
            Start();
            return conn.QueryMultiple(GetStoredProc(storedproc, parameters), parameters);
        }
    }
}
