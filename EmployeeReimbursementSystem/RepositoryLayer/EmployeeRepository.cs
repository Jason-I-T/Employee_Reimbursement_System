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

        // TODO Move to Auth repo class...
        Task<string> LoginEmployee(string email, string password);
        Task<string> LogoutEmployee(int employeeId, string sessionId);
        Task UpdateLastRequest(int employeeId);
        Task<string> CloseSession(int employeeId);
        Task<string> AuthorizeUser(int employeeId, string sessionId);
    }

    public class EmployeeRepository : IEmployeeRepository {
        // Injecting a logger
        private readonly IDataLogger _logger;
        private string _conString;
        public EmployeeRepository(IDataLogger logger) {
            this._logger = logger;
            this._conString = File.ReadAllText("../../ConString.txt");
        } 

        // Update an employee's role, email, or password
        public async Task<Employee> UpdateEmployee(int id, int roleId, int managerId, string sessionId) {
            await UpdateLastRequest(managerId);
            if(await AuthorizeUser(managerId, sessionId) is null) return null!;
            using(SqlConnection connection = new SqlConnection(_conString)) {
                string updateEmployeeQuery = "UPDATE E SET E.RoleId = @RoleId FROM Employee E INNER JOIN Session S ON E.EmployeeId = S.EmployeeId WHERE E.EmployeeId = @Id;";
                SqlCommand command = new SqlCommand(updateEmployeeQuery, connection);
                command.Parameters.AddWithValue("@RoleId", roleId);
                command.Parameters.AddWithValue("@Id", id);
                command.Parameters.AddWithValue("@SessionId", sessionId);

                return await ExecuteUpdate(connection, command, id, roleId);
            } 
        }

        public async Task<Employee> UpdateEmployee(int id, string info, string sessionId) {
            await UpdateLastRequest(id);
            if(await AuthorizeUser(id, sessionId) is null) return null!;
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

                return await ExecuteUpdate(connection, command, id, info);
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
        
        /** 
          * TODO Move to an auth repository class
          * Login an employee. First verify credentials, then 
          * store the session data in the database 
          */
        public async Task<string> LoginEmployee(string email, string password) {
            string sessionId = Guid.NewGuid().ToString();
            using(SqlConnection connection = new SqlConnection(_conString)) {
                // Verify credentials against database
                string queryEmployeeByEmail = "SELECT * FROM Employee WHERE Email = @email AND Password = @password";
                SqlCommand command = new SqlCommand(queryEmployeeByEmail, connection);
                command.Parameters.AddWithValue("@Email", email);
                command.Parameters.AddWithValue("@Password", password);
                Employee verifiedEmployee = await VerifyLoginCredentials(connection, command, $"{email}, {password}");
                if(verifiedEmployee is null) return null!;

                // Insert employeeId, sessionId, timestamp into LoginSession table. Return the created sessionId from the database.
                DateTime timeStamp = DateTime.Now;
                string insertLoginSession = "INSERT INTO Session VALUES (@SessionId, @EmployeeId, @LastRequest); SELECT SessionId FROM Session WHERE EmployeeId = @EmployeeId;";
                command = new SqlCommand(insertLoginSession, connection);
                command.Parameters.AddWithValue("@SessionId", sessionId);
                command.Parameters.AddWithValue("@EmployeeId", verifiedEmployee.id);
                command.Parameters.AddWithValue("@LastRequest", timeStamp);
                return await ExecuteLoginSessionInsert(connection, command, $"{sessionId}, {verifiedEmployee.id}, {timeStamp}", email);
            }
        }

        /** 
          * TODO Move to an auth repository class
          * Delete the login session of an existing logged-in employee 
          */
        public async Task<string> LogoutEmployee(int employeeId, string sessionId) {
            await UpdateLastRequest(employeeId);
            if(await AuthorizeUser(employeeId, sessionId) is null) return null!;
            using(SqlConnection connection = new SqlConnection(_conString)) {
                string deleteSessionQuery = "DELETE FROM Session WHERE EmployeeId = @id AND SessionId = @sId";
                SqlCommand command = new SqlCommand(deleteSessionQuery, connection);
                command.Parameters.AddWithValue("@id", employeeId);
                command.Parameters.AddWithValue("@sId", sessionId);
                try {
                    await connection.OpenAsync();
                    int rowsAffected = await command.ExecuteNonQueryAsync();
                    if(rowsAffected == 1) {
                        _logger.LogSuccess("LogoutEmployee", "DELETE", $"{employeeId}, {sessionId}");
                        return "Success";
                    } else {
                        _logger.LogError("LogoutEmployee", "DELETE", $"{employeeId}, {sessionId}", "Delete Error, check database");
                        return null!;
                    }
                } catch(Exception ex) {
                    _logger.LogError("LogoutEmployee", "DELETE", $"{employeeId}, {sessionId}", ex.Message);
                    return null!;
                }
            }
        }

        /** 
          * TODO Move to an auth repository class
          * Close a session based on an employee. Checks if session has expired first. 
          */
        public async Task<string> CloseSession(int employeeId) {
            using(SqlConnection connection = new SqlConnection(_conString)) {
                string deleteSession = "DELETE FROM Session WHERE EmployeeId = @employeeId AND DATEDIFF(minute, LastRequest, @now) >= 15;"; //
                SqlCommand command = new SqlCommand(deleteSession, connection);
                command.Parameters.AddWithValue("@employeeId", employeeId);
                command.Parameters.AddWithValue("@now", DateTime.Now);
                try {
                    await connection.OpenAsync();
                    int rowsAffected = await command.ExecuteNonQueryAsync();
                    if(rowsAffected == 1) {
                        _logger.LogSuccess("CloseSession", "DELETE", employeeId);
                        return "Success";
                    } else {
                        _logger.LogError("CloseSession", "DELETE", employeeId, "Error: invalid input or session still valid");
                        return null!;
                    }
                } catch(Exception ex) {
                        _logger.LogError("CloseSession", "DELETE", employeeId, ex.Message);
                        return null!;
                }
            }
        }

        /** 
          * TODO Move to an auth repository class
          * Close session based on email. For logging in if a session already exists. 
          */
        private async Task<string> CloseSession(string email) {
            using(SqlConnection connection = new SqlConnection(_conString)) {
                string deleteSession = "DELETE S FROM Employee E LEFT JOIN Session S ON E.EmployeeId = S.EmployeeId WHERE Email = @email"; //AND DATEDIFF(minute, LastRequest, @now) >= 15;
                SqlCommand command = new SqlCommand(deleteSession, connection);
                command.Parameters.AddWithValue("@email", email);
                try {
                    await connection.OpenAsync();
                    int rowsAffected = await command.ExecuteNonQueryAsync();
                    if(rowsAffected == 1) {
                        _logger.LogSuccess("CloseSession", "DELETE", email);
                        return "Success";
                    } else {
                        _logger.LogError("CloseSession", "DELETE", email, "Error: invalid input or session still valid");
                        return null!;
                    }
                } catch(Exception ex) {
                        _logger.LogError("CloseSession", "DELETE", email, ex.Message);
                        return null!;
                }
            }
        }
        
        /******************************************* Helper methods *******************************************/
        private async Task<Employee> ExecuteUpdate(SqlConnection con, SqlCommand comm, int id, object logInfo) {
            // Steps for updating an employee
            try { 
                await con.OpenAsync();
                int rowsAffected = await comm.ExecuteNonQueryAsync();
                if(rowsAffected == 1) {
                    _logger.LogSuccess("UpdateEmployee", "PUT", logInfo);
                    return await GetEmployee(id);
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

        /** 
          * TODO Move to an auth repository class
          * Verify login credentials (email, pass) against the database 
          */
        private async Task<Employee> VerifyLoginCredentials(SqlConnection con, SqlCommand comm, object logInfo) {
            try {
                await con.OpenAsync();
                
                using(SqlDataReader reader = await comm.ExecuteReaderAsync()) {
                    if(!reader.HasRows) {
                        
                        _logger.LogError("LoginEmployee", "POST", logInfo, "Login Failure: Credentials do not match any record");
                        return null!;
                    }
                    else {
                        await reader.ReadAsync();
                        _logger.LogSuccess("LoginEmployee", "POST", logInfo);
                        return new Employee(
                            (int)reader[0], 
                            (string)reader[1], 
                            (string)reader[2], 
                            (int)reader[3]
                        );
                    }
                }
            } catch(Exception e) {
                _logger.LogError("LoginEmployee", "POST", logInfo, e.Message);
                return null!;
            } finally {
                await con.CloseAsync();
            }
        }

        /** 
          * TODO Move to an auth repository class
          * Create the login session for the employee in database 
          */
        private async Task<string> ExecuteLoginSessionInsert(SqlConnection con, SqlCommand comm, object logInfo, string email) {
            try { 
                await con.OpenAsync();
                using(SqlDataReader reader = await comm.ExecuteReaderAsync()) {
                    if(!reader.HasRows) {
                        _logger.LogError("LoginEmployee", "POST", logInfo, "Login Error: Insertion error");
                        return null!;
                    } else { 
                        await reader.ReadAsync();   
                        _logger.LogSuccess("LoginEmployee", "POST", logInfo);
                        return (string) reader[0];
                    }
                }
            } catch(SqlException sex) {  
                await con.CloseAsync();  
                if(sex.Number == 2627) { // Unique key constraint error. There's a user logged in with the creds
                    await CloseSession(email); // Delete existing login session from database, try logging in again
                    return await ExecuteLoginSessionInsert(con, comm, logInfo, email);
                }
                _logger.LogError("LoginEmployee", "POST", logInfo, sex.Message);
                return null!; 
            }catch(Exception ex) {
                _logger.LogError("LoginEmployee", "POST", logInfo, ex.Message);
                return null!;
            } finally {
                await con.CloseAsync();
            }
        }

        /** 
          * TODO Move to an auth repository class
          * Update LastRequest column in session table with updated datetime. 
          */
        public async Task UpdateLastRequest(int employeeId) {
            DateTime lastRequest = DateTime.Now;
            using(SqlConnection connection = new SqlConnection(_conString)) {
                string updateQuery = "UPDATE Session SET LastRequest = @LastRequest WHERE EmployeeId = @employeeId";
                SqlCommand command = new SqlCommand(updateQuery, connection);
                command.Parameters.AddWithValue("@LastRequest", lastRequest);
                command.Parameters.AddWithValue("@EmployeeId", employeeId);
                try { 
                    await connection.OpenAsync();
                    int rowsAffected = await command.ExecuteNonQueryAsync();
                    if(rowsAffected == 1) {
                        _logger.LogSuccess("UpdateLastRequest", "PUT", employeeId);
                    } else {    
                        _logger.LogError("UpdateLastRequest", "PUT", employeeId, "Session Update Error");
                    } 
                } catch(Exception e) {
                    _logger.LogError("UpdateLastRequest", "PUT", employeeId, e.Message);
                } finally {
                    await connection.CloseAsync();
                }
            }            
        }

        /** 
          * TODO Move to an auth repository class
          * Authorize user using id and sessionId 
          */
        public async Task<string> AuthorizeUser(int employeeId, string sessionId) {
            using(SqlConnection connection = new SqlConnection(_conString)) {
                string queryVerifyAuth = "SELECT E.* FROM Employee E INNER JOIN Session S ON E.EmployeeId = S.EmployeeId WHERE E.EmployeeId = @Id AND SessionId = @SessionId;";
                SqlCommand command = new SqlCommand(queryVerifyAuth, connection);
                command.Parameters.AddWithValue("@Id", employeeId);
                command.Parameters.AddWithValue("@SessionId", sessionId);    
                try {
                    await connection.OpenAsync();
                    
                    using(SqlDataReader reader = await command.ExecuteReaderAsync()) {
                        if(!reader.HasRows) {
                            _logger.LogError("Authorize", "GET", $"{employeeId}, {sessionId}", "Auth Failure: Credentials invalid");
                            return null!;
                        }
                        else {
                            await reader.ReadAsync();
                            _logger.LogSuccess("Authorize", "GET", $"{employeeId}, {sessionId}");
                            return "Success";
                        }
                    }
                } catch(Exception e) {
                    _logger.LogError("Authorize", "GET", $"{employeeId}, {sessionId}", e.Message);
                    return null!;
                } finally {
                    await connection.CloseAsync();
                }
            }
        }
    }
}