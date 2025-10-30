using System.Data;
using Microsoft.Data.SqlClient;

namespace Employee_Management_System.Services
{
    public class DatabaseService
    {
        private readonly string _connectionString;

        public DatabaseService()
        {
            _connectionString = System.Configuration.ConfigurationManager.ConnectionStrings["EMS"].ConnectionString;
        }

        /// <summary>
        /// Executes a stored procedure and returns a DataTable
        /// </summary>
        public DataTable ExecuteStoredProcedure(string procedureName, SqlParameter[] parameters = null)
        {
            DataTable dataTable = new DataTable();

            try
            {
                using (SqlConnection connection = new SqlConnection(_connectionString))
                {
                    using (SqlCommand command = new SqlCommand(procedureName, connection))
                    {
                        command.CommandType = CommandType.StoredProcedure;

                        if (parameters != null)
                        {
                            command.Parameters.AddRange(parameters);
                        }

                        connection.Open();

                        using (SqlDataAdapter adapter = new SqlDataAdapter(command))
                        {
                            adapter.Fill(dataTable);
                        }
                    }
                }
            }
            catch (SqlException ex)
            {
                throw new Exception($"Database error executing {procedureName}: {ex.Message}", ex);
            }
            catch (Exception ex)
            {
                throw new Exception($"Error executing {procedureName}: {ex.Message}", ex);
            }

            return dataTable;
        }

        /// <summary>
        /// Executes a SQL query and returns a DataTable
        /// </summary>
        public DataTable ExecuteQuery(string query, SqlParameter[] parameters = null)
        {
            DataTable dataTable = new DataTable();

            try
            {
                using (SqlConnection connection = new SqlConnection(_connectionString))
                {
                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        command.CommandType = CommandType.Text;

                        if (parameters != null)
                        {
                            command.Parameters.AddRange(parameters);
                        }

                        connection.Open();

                        using (SqlDataAdapter adapter = new SqlDataAdapter(command))
                        {
                            adapter.Fill(dataTable);
                        }
                    }
                }
            }
            catch (SqlException ex)
            {
                throw new Exception($"Database error: {ex.Message}", ex);
            }
            catch (Exception ex)
            {
                throw new Exception($"Error executing query: {ex.Message}", ex);
            }

            return dataTable;
        }

        /// <summary>
        /// Executes a stored procedure without returning results
        /// </summary>
        public int ExecuteNonQuery(string procedureName, SqlParameter[] parameters = null)
        {
            int rowsAffected = 0;

            try
            {
                using (SqlConnection connection = new SqlConnection(_connectionString))
                {
                    using (SqlCommand command = new SqlCommand(procedureName, connection))
                    {
                        command.CommandType = CommandType.StoredProcedure;

                        if (parameters != null)
                        {
                            command.Parameters.AddRange(parameters);
                        }

                        connection.Open();
                        rowsAffected = command.ExecuteNonQuery();
                    }
                }
            }
            catch (SqlException ex)
            {
                throw new Exception($"Database error executing {procedureName}: {ex.Message}", ex);
            }
            catch (Exception ex)
            {
                throw new Exception($"Error executing {procedureName}: {ex.Message}", ex);
            }

            return rowsAffected;
        }

        /// <summary>
        /// Tests the database connection
        /// </summary>
        public bool TestConnection()
        {
            try
            {
                using (SqlConnection connection = new SqlConnection(_connectionString))
                {
                    connection.Open();
                    return true;
                }
            }
            catch
            {
                return false;
            }
        }
    }
}