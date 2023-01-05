using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Data.SqlClient;
using ModelLayer;

namespace RepositoryLayer
{
    public interface IAuthRepository {
        Task<string> LoginEmployee(string email, string password);
        Task<string> LogoutEmployee(int employeeId, string sessionId);
        Task UpdateLastRequest(int employeeId);
        Task<string> CloseSession(int employeeId);
        Task<string> CloseSession(string sessionId);
        Task<string> AuthorizeUser(int employeeId, string sessionId);
    }

    public class AuthRepository : IAuthRepository
    {
        private readonly IDataLogger _logger;
        private string _conString;
        public AuthRepository(IDataLogger logger) {
            this._logger = logger;
            this._conString = File.ReadAllText("../../ConString.txt");
        }
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

                // Create the session in the database
                DateTime timeStamp = DateTime.Now;
                string insertLoginSession = "INSERT INTO Session VALUES (@SessionId, @EmployeeId, @LastRequest); SELECT SessionId FROM Session WHERE EmployeeId = @EmployeeId;";
                command = new SqlCommand(insertLoginSession, connection);
                command.Parameters.AddWithValue("@SessionId", sessionId);
                command.Parameters.AddWithValue("@EmployeeId", verifiedEmployee.id);
                command.Parameters.AddWithValue("@LastRequest", timeStamp);
                return await ExecuteLoginSessionInsert(connection, command, $"{sessionId}, {verifiedEmployee.id}, {timeStamp}", email);
            }
        }

        public async Task<string> LogoutEmployee(int employeeId, string sessionId) {
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

        public async Task<string> CloseSession(string sessionId) {
            using(SqlConnection connection = new SqlConnection(_conString)) {
                string deleteSession = "DELETE FROM Session WHERE SessionId = @sessionId;";
                SqlCommand command = new SqlCommand(deleteSession, connection);
                command.Parameters.AddWithValue("@sessionId", sessionId);
                try {
                    await connection.OpenAsync();
                    int rowsAffected = await command.ExecuteNonQueryAsync();
                    if(rowsAffected == 1) {
                        _logger.LogSuccess("DestroySession", "DELETE", sessionId);
                        return "Successfully terminated session";
                    } else {
                        _logger.LogError("DestroySession", "DELETE", sessionId, "Error: invalid input");
                        return "Session does not exist";
                    }
                } catch(Exception ex) {
                        _logger.LogError("DestroySession", "DELETE", sessionId, ex.Message);
                        return null!;
                }
            }
        }

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

        /*********************************************** HELPERS ***********************************************/
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
                    await DestroySession(email); // Delete existing login session from database, try logging in again
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

        private async Task<string> DestroySession(string email) {
            using(SqlConnection connection = new SqlConnection(_conString)) {
                string deleteSession = "DELETE S FROM Employee E LEFT JOIN Session S ON E.EmployeeId = S.EmployeeId WHERE Email = @email";
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
    }
}