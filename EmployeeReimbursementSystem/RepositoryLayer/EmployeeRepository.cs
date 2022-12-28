using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Data.SqlClient;

using ModelLayer;

namespace RepositoryLayer
{
    public interface IEmployeeRepository {
        Employee UpdateEmployee(int id, int roleId);
        Employee UpdateEmployee(int id, string info);
        Employee PostEmployee(string email, string password, int roleId);
        Employee GetEmployee(string email);
        Employee GetEmployee(int id);
        Employee LoginEmployee(string email, string password);
    }

    public class EmployeeRepository : IEmployeeRepository {
        // Injecting a logger
        private readonly IRepositoryLogger _logger;
        private string _conString;
        public EmployeeRepository(IRepositoryLogger logger) {
            this._logger = logger;
            this._conString = File.ReadAllText("../../ConString.txt");
        } 

        // Update an employee's role, email, or password
        public Employee UpdateEmployee(int id, int roleId) {
            using(SqlConnection connection = new SqlConnection(_conString)) {
                string updateEmployeeQuery = "UPDATE Employee SET RoleId = @RoleId WHERE EmployeeId = @Id;";
                SqlCommand command = new SqlCommand(updateEmployeeQuery, connection);
                command.Parameters.AddWithValue("@RoleId", roleId);
                command.Parameters.AddWithValue("@Id", id); 

                return ExecuteUpdate(connection, command, id, "UpdateRole");
            } 
        }

        public Employee UpdateEmployee(int id, string info) {
            string regex = @"^([a-zA-Z0-9_\-\.]+)@([a-zA-Z0-9_\-\.]+)\.([a-zA-Z]{2,5})$";
            using(SqlConnection connection = new SqlConnection(_conString)) {
                string updateEmployeeQuery;
                if(System.Text.RegularExpressions.Regex.Match(info, regex).Success) {
                    // info is an email
                    updateEmployeeQuery = "UPDATE Employee SET Email = @info WHERE EmployeeId = @Id";
                } else {
                    // info is a password
                    updateEmployeeQuery = "UPDATE Employee SET Password = @info WHERE EmployeeId = @Id";
                }
                SqlCommand command = new SqlCommand(updateEmployeeQuery, connection);
                command.Parameters.AddWithValue("@info", info); 
                command.Parameters.AddWithValue("@Id", id);

                return ExecuteUpdate(connection, command, id, info);
            }
        }

        // Add an employee to the system
        public Employee PostEmployee(string email, string password, int roleId) {
            using(SqlConnection connection = new SqlConnection(_conString)) {
                string insertEmployeeQuery = "INSERT INTO Employee (Email, Password, RoleId) VALUES (@email, @password, @RoleId);";
                SqlCommand command = new SqlCommand(insertEmployeeQuery, connection);
                command.Parameters.AddWithValue("@email", email);
                command.Parameters.AddWithValue("@password", password);
                command.Parameters.AddWithValue("@RoleId", roleId);
                try {
                    connection.Open();
                    int rowsAffected = command.ExecuteNonQuery();
                    if(rowsAffected == 1) {
                        _logger.LogSuccess("PostEmployee", "POST", new object[]{email, password, roleId});
                        return GetEmployee(email);
                    } else {
                        _logger.LogError("PostEmployee", "POST", new object[]{email, password, roleId}, "Insertion Failure");
                        return null!;
                    }
                } catch (Exception e) {
                    _logger.LogError("PostEmployee", "POST", new object[]{email, password, roleId}, e.Message);
                    return null!;
                }
            }
        }

        // Get Methods... retrieve unique employee by email, id, or email & password
        public Employee GetEmployee(string email) {
            using(SqlConnection connection = new SqlConnection(_conString)) {
                string queryEmployeeByEmail = "SELECT * FROM Employee WHERE Email = @email";
                SqlCommand command = new SqlCommand(queryEmployeeByEmail, connection);
                command.Parameters.AddWithValue("@Email", email);
                return ExecuteGet(connection, command, email);
            }
        }

        public Employee GetEmployee(int id) {
            using(SqlConnection connection = new SqlConnection(_conString)) {
                string queryEmployeeById = "SELECT * FROM Employee WHERE EmployeeId = @id";
                SqlCommand command = new SqlCommand(queryEmployeeById, connection);
                command.Parameters.AddWithValue("@id", id);
                return ExecuteGet(connection, command, id);
            }
        }

        public Employee LoginEmployee(string email, string password) {
            using(SqlConnection connection = new SqlConnection(_conString)) {
                string queryEmployeeByEmail = "SELECT * FROM Employee WHERE Email = @email AND Password = @password";
                SqlCommand command = new SqlCommand(queryEmployeeByEmail, connection);
                command.Parameters.AddWithValue("@Email", email);
                command.Parameters.AddWithValue("@Password", password);
                try {
                    connection.Open();
                    
                    using(SqlDataReader reader = command.ExecuteReader()) {
                        if(!reader.HasRows) {
                            
                            _logger.LogError("LoginEmployee", "GET", new object[]{email, password}, "Login Failure");
                            return null!;
                        }
                        else {
                            reader.Read();
                            _logger.LogSuccess("LoginEmployee", "GET", new object[]{email, password});
                            return GetEmployee(email);
                        }
                    }
                } catch(Exception e) {
                    _logger.LogError("LoginEmployee", "GET", new object[]{email, password}, e.Message);
                    return null!;
                }
            }
        }
        
        // Helper methods
        private Employee ExecuteUpdate(SqlConnection con, SqlCommand comm, int id, object logInfo) {
            // Steps for updating an employee
            try { 
                con.Open();
                int rowsAffected = comm.ExecuteNonQuery();
                if(rowsAffected == 1) {
                    _logger.LogSuccess("UpdateEmployee", "PUT", logInfo);
                    return GetEmployee(id);
                } else {    
                    _logger.LogError("UpdateEmployee", "PUT", logInfo, "Employee Update Error");
                    return null!;
                } 
            } catch(Exception e) {
                _logger.LogError("UpdateEmployee", "PUT", logInfo, e.Message);
                return null!;
            }
        }

        private Employee ExecuteGet(SqlConnection con, SqlCommand comm, object logInfo) {
            // Steps for getting an employee
            try {
                con.Open();
                
                using(SqlDataReader reader = comm.ExecuteReader()) {
                    if(!reader.HasRows) {
                        _logger.LogError("GetEmployee", "GET", logInfo, "Could not find employee matching the args.");
                        return null!;
                    } 
                    else {
                        reader.Read();
                        _logger.LogSuccess("GetEmployee", "GET", logInfo);
                        return new Employee(
                            (int)reader[0], 
                            (string)reader[1], 
                            (string)reader[2], 
                            (int)reader[3]
                        );
                    }
                }
            } catch(Exception e) {
                _logger.LogError("GetEmployee", "GET", logInfo, e.Message);
                return null!;
            }
        }
    }
}