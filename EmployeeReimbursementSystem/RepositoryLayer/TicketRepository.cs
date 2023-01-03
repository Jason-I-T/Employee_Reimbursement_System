using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Text.Json;
using Microsoft.Data.SqlClient;

using ModelLayer;

namespace RepositoryLayer;
public interface ITicketRepository {
    Task<ReimburseTicket> PostTicket(string guid, string r, double a, string d, DateTime t, int eId);
    Task<ReimburseTicket> GetTicket(string ticketId);
    Task<ReimburseTicket> UpdateTicket(string ticketId, int statusId, int managerId);
    Task<List<ReimburseTicket>> GetTickets(int employeeId);
    Task<List<ReimburseTicket>> GetTickets(int employeeId, int statusId);
    Task<Queue<ReimburseTicket>> GetPending(int managerId);
}

public class TicketRepository : ITicketRepository {
    // Injecting logger
    private readonly IDataLogger _logger;
    private readonly IEmployeeRepository _eRepo;
    private string _conString;
    public TicketRepository(IDataLogger logger, IEmployeeRepository eRepo) {
        this._logger = logger;
        this._eRepo = eRepo;
        this._conString = File.ReadAllText("../../ConString.txt");
    }
    
    public async Task<ReimburseTicket> UpdateTicket(string ticketId, int statusId, int managerId) {
        await _eRepo.UpdateLastRequest(managerId);
        using(SqlConnection connection = new SqlConnection(_conString)) {
            string updateTicketQuery = "UPDATE Ticket SET StatusId = @statusId WHERE TicketId = @ticketId";
            SqlCommand command = new SqlCommand(updateTicketQuery, connection);
            command.Parameters.AddWithValue("@statusId", statusId);
            command.Parameters.AddWithValue("@ticketId", ticketId);
            try {
                await connection.OpenAsync();
                int rowsAffected = await command.ExecuteNonQueryAsync();
                if(rowsAffected == 1) {
                    _logger.LogSuccess("UpdateTicket", "PUT", $"{ticketId}, {statusId}");
                    return await GetTicket(ticketId);
                } else {
                    _logger.LogError("UpdateTicket", "PUT", $"{ticketId}, {statusId}", "Upate failure");
                    return null!;
                }
            } catch(Exception e) {
                _logger.LogError("UpdateTicket", "PUT", $"{ticketId}, {statusId}", e.Message);
                return null!;
            }
        }
    }

    public async Task<ReimburseTicket> PostTicket(string guid, string r, double a, string d, DateTime t, int eId) {
        await _eRepo.UpdateLastRequest(eId);
        using(SqlConnection connection = new SqlConnection(_conString)) {
            string insertTicketQuery = "INSERT INTO Ticket (TicketId, Reason, Amount, Description, StatusId, RequestDate, EmployeeId) VALUES (@guid, @r, @a, @d, 0, @t, @eId);";
            SqlCommand command = new SqlCommand(insertTicketQuery, connection);
            command.Parameters.AddWithValue("@guid", guid);
            command.Parameters.AddWithValue("@r", r);
            command.Parameters.AddWithValue("@a", a);
            command.Parameters.AddWithValue("@d", d);
            command.Parameters.AddWithValue("@t", t);
            command.Parameters.AddWithValue("@eId", eId);

            try {
                await connection.OpenAsync();
                int rowsAffected = await command.ExecuteNonQueryAsync();
                if(rowsAffected == 1) {
                    _logger.LogSuccess("PostTicket", "POST", guid);
                    return await GetTicket(guid);
                } else {
                    _logger.LogError("PostTicket", "POST", guid, "Insertion failure.");
                    return null!;
                }
            } catch(Exception e) {
                _logger.LogError("PostTicket", "POST", guid, e.Message);
                return null!;
            }
        }
    }

    public async Task<ReimburseTicket> GetTicket(string ticketId) {
        using(SqlConnection connection = new SqlConnection(_conString)) {
            string queryTicketById = "SELECT * FROM Ticket WHERE TicketId = @ticketId;";
            SqlCommand command = new SqlCommand(queryTicketById, connection);
            command.Parameters.AddWithValue("@ticketId", ticketId);
            try {
                await connection.OpenAsync();

                using(SqlDataReader reader = await command.ExecuteReaderAsync()) {
                    if(!reader.HasRows) {
                        _logger.LogError("GetTicket", "GET", ticketId, "No result for given input");
                        return null!;
                    } 
                    else {
                        await reader.ReadAsync();
                        _logger.LogSuccess("GetTicket", "GET", ticketId);
                        return new ReimburseTicket(
                            (string) reader[0],
                            (string) reader[1],
                            (double) reader[2],
                            (string) reader[3],
                            (int) reader[4],
                            (DateTime) reader[5],
                            (int) reader[6]
                        );
                    }
                }
            } catch(Exception e) {
                _logger.LogError("GetTicket", "GET", ticketId, e.Message);
                return null!;
            }
        }
    }

    public async Task<List<ReimburseTicket>> GetTickets(int employeeId) {
        List<ReimburseTicket> employeeTickets = new List<ReimburseTicket>();
        using(SqlConnection connection = new SqlConnection(_conString)) {
            string queryAllEmployeeTickets = "SELECT T.* FROM Ticket T INNER JOIN Session S ON T.EmployeeId = S.EmployeeId WHERE T.EmployeeId = @employeeId ORDER BY RequestDate;";
            SqlCommand command = new SqlCommand(queryAllEmployeeTickets, connection);
            command.Parameters.AddWithValue("@employeeId", employeeId);
            return await ExecuteGetTickets(connection, command, employeeId, employeeId);
        }
    }

    public async Task<List<ReimburseTicket>> GetTickets(int employeeId, int statusId) {
        List<ReimburseTicket> employeeTickets = new List<ReimburseTicket>();
        using(SqlConnection connection = new SqlConnection(_conString)) {
            string queryAllEmployeeTickets = "SELECT T.* FROM Ticket T INNER JOIN Session S ON T.EmployeeId = S.EmployeeId WHERE T.EmployeeId = @employeeId AND StatusId = @statusId ORDER BY RequestDate;";
            SqlCommand command = new SqlCommand(queryAllEmployeeTickets, connection);
            command.Parameters.AddWithValue("@employeeId", employeeId);
            command.Parameters.AddWithValue("@statusId", statusId);
            return await ExecuteGetTickets(connection, command, $"{employeeId}, {statusId}", employeeId);
        }
    }

    public async Task<Queue<ReimburseTicket>> GetPending(int managerId) {
        using(SqlConnection connection = new SqlConnection(_conString)) {
            string queryAllEmployeeTickets = "SELECT * FROM Ticket WHERE StatusId = @statusId ORDER BY RequestDate;";
            SqlCommand command = new SqlCommand(queryAllEmployeeTickets, connection);
            command.Parameters.AddWithValue("@statusId", 0);
            return new Queue<ReimburseTicket>(await ExecuteGetTickets(connection, command, managerId, managerId));
        }
    }

    private async Task<List<ReimburseTicket>> ExecuteGetTickets(SqlConnection con, SqlCommand comm, object logInfo, int callerId) {
        await _eRepo.UpdateLastRequest(callerId);
        List<ReimburseTicket> employeeTickets = new List<ReimburseTicket>();
        try {
            await con.OpenAsync();
            using(SqlDataReader reader = await comm.ExecuteReaderAsync()) {
                if(!reader.HasRows) {
                    _logger.LogError("GetTickets", "GET", logInfo, "No results matching the input and/or unauthorized.");
                    return null!;
                } 
                while(await reader.ReadAsync()) {
                    ReimburseTicket newTicket = new ReimburseTicket(
                        (string) reader[0],
                        (string) reader[1],
                        (double) reader[2],
                        (string) reader[3],
                        (int) reader[4],
                        (DateTime) reader[5],
                        (int) reader[6]
                    );
                    employeeTickets.Add(newTicket);
                }
                _logger.LogSuccess("GetTickets", "GET", logInfo);
                return employeeTickets;
            }
        } catch(Exception e) {
            _logger.LogError("GetTickets", "GET", logInfo, e.Message);
            return null!;
        }    
    }
}