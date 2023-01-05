using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Data.SqlClient;

using ModelLayer;

namespace RepositoryLayer
{
    public interface IEmployeeRepository {
        Task<Employee> UpdateEmployee(int id, int roleId, int managerId, string sessionId);
        Task<Employee> UpdateEmployee(int id, string info, string sessionId);
        Task<Employee> PostEmployee(string email, string password, int roleId);
        Task<Employee> GetEmployee(string email);
        Task<Employee> GetEmployee(int id);
    }

    public class EmployeeRepository : IEmployeeRepository {
        // Injecting a logger and Auth repo class
        private readonly IDataLogger _logger;
        private readonly IAuthRepository _iar;
        private string _conString;
        public EmployeeRepository(IAuthRepository iar, IDataLogger logger) {
            this._iar = iar;
            this._logger = logger;
            this._conString = File.ReadAllText("../../ConString.txt");
        } 

        // Update an employee's role, email, or password
        public async Task<Employee> UpdateEmployee(int id, int roleId, int managerId, string sessionId) {
            //await _iar.UpdateLastRequest(managerId);
            if(await _iar.AuthorizeUser(managerId, sessionId) is null) return null!;
            using(SqlConnection connection = new SqlConnection(_conString)) {
                string updateEmployeeQuery = "UPDATE Employee SET RoleId = @RoleId FROM Employee WHERE EmployeeId = @Id;";
                SqlCommand command = new SqlCommand(updateEmployeeQuery, connection);
                command.Parameters.AddWithValue("@RoleId", roleId);
                command.Parameters.AddWithValue("@Id", id);
                command.Parameters.AddWithValue("@SessionId", sessionId);

                return await ExecuteUpdate(connection, command, id, managerId, roleId);
            } 
        }

        public async Task<Employee> UpdateEmployee(int id, string info, string sessionId) {
            //await _iar.UpdateLastRequest(id);
            if(await _iar.AuthorizeUser(id, sessionId) is null) return null!;
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

                return await ExecuteUpdate(connection, command, id, id, info);
            }
        }

        // Add an employee to the system
        public async Task<Employee> PostEmployee(string email, string password, int roleId) {
            using(SqlConnection connection = new SqlConnection(_conString)) {
                string insertEmployeeQuery = "INSERT INTO Employee (Email, Password, RoleId) VALUES (@email, @password, @RoleId);";
                SqlCommand command = new SqlCommand(insertEmployeeQuery, connection);
                command.Parameters.AddWithValue("@email", email);
                command.Parameters.AddWithValue("@password", password);
                command.Parameters.AddWithValue("@RoleId", roleId);
                try {
                    await connection.OpenAsync();
                    int rowsAffected = await command.ExecuteNonQueryAsync();
                    if(rowsAffected == 1) {
                        _logger.LogSuccess("PostEmployee", "POST", $"{email}, {password}, {roleId}");
                        return await GetEmployee(email);
                    } else {
                        _logger.LogError("PostEmployee", "POST", $"{email}, {password}, {roleId}", "Insertion Failure");
                        return null!;
                    }
                } catch (Exception e) {
                    _logger.LogError("PostEmployee", "POST", $"{email}, {password}, {roleId}", e.Message);
                    return null!;
                }
            }
        }

        // Get Methods... retrieve unique employee by email, id, or email & password
        public async Task<Employee> GetEmployee(string email) {
            using(SqlConnection connection = new SqlConnection(_conString)) {
                string queryEmployeeByEmail = "SELECT * FROM Employee WHERE Email = @email";
                SqlCommand command = new SqlCommand(queryEmployeeByEmail, connection);
                command.Parameters.AddWithValue("@Email", email);
                return await ExecuteGet(connection, command, email);
            }
        }

        public async Task<Employee> GetEmployee(int id) {
            using(SqlConnection connection = new SqlConnection(_conString)) {
                string queryEmployeeById = "SELECT * FROM Employee WHERE EmployeeId = @id";
                SqlCommand command = new SqlCommand(queryEmployeeById, connection);
                command.Parameters.AddWithValue("@id", id);
                return await ExecuteGet(connection, command, id);
            }
        }
        
        /******************************************* Helper methods *******************************************/
        private async Task<Employee> ExecuteUpdate(SqlConnection con, SqlCommand comm, int targetId, int callerId, object logInfo) {
            // Steps for updating an employee
            try { 
                await con.OpenAsync();
                int rowsAffected = await comm.ExecuteNonQueryAsync();
                if(rowsAffected == 1) {
                    _logger.LogSuccess("UpdateEmployee", "PUT", logInfo);
                    await _iar.UpdateLastRequest(callerId);
                    return await GetEmployee(targetId);
                } else {    
                    _logger.LogError("UpdateEmployee", "PUT", logInfo, "Employee Update Error");
                    return null!;
                } 
            } catch(Exception e) {
                _logger.LogError("UpdateEmployee", "PUT", logInfo, e.Message);
                return null!;
            }
        }

        private async Task<Employee> ExecuteGet(SqlConnection con, SqlCommand comm, object logInfo) {
            // Steps for getting an employee
            try {
                await con.OpenAsync();
                
                using(SqlDataReader reader = await comm.ExecuteReaderAsync()) {
                    if(!reader.HasRows) {
                        _logger.LogError("GetEmployee", "GET", logInfo, "Could not find employee matching the args.");
                        return null!;
                    } 
                    else {
                        await reader.ReadAsync();
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