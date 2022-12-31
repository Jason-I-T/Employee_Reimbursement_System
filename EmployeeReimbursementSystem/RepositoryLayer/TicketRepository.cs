using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Text.Json;
using Microsoft.Data.SqlClient;

using ModelLayer;

namespace RepositoryLayer;
public interface ITicketRepository {
    ReimburseTicket PostTicket(string guid, string r, double a, string d, DateTime t, int eId);
    ReimburseTicket GetTicket(string ticketId);
    ReimburseTicket UpdateTicket(string ticketId, int statusId);
    List<ReimburseTicket> GetTickets(int employeeId);
    List<ReimburseTicket> GetTickets(int employeeId, int statusId);
    Queue<ReimburseTicket> GetPending(int managerId);
}

public class TicketRepository : ITicketRepository {
    // Injecting logger
    private readonly IRepositoryLogger _logger;
    private string _conString;
    public TicketRepository(IRepositoryLogger logger) {
        this._logger = logger;
        this._conString = File.ReadAllText("../../ConString.txt");
    }
    
    public ReimburseTicket UpdateTicket(string ticketId, int statusId) {
        using(SqlConnection connection = new SqlConnection(_conString)) {
            string updateTicketQuery = "UPDATE Ticket SET StatusId = @statusId WHERE TicketId = @ticketId";
            SqlCommand command = new SqlCommand(updateTicketQuery, connection);
            command.Parameters.AddWithValue("@statusId", statusId);
            command.Parameters.AddWithValue("@ticketId", ticketId);
            try {
                connection.Open();
                int rowsAffected = command.ExecuteNonQuery();
                if(rowsAffected == 1) {
                    _logger.LogSuccess("UpdateTicket", "PUT", $"{ticketId}, {statusId}");
                    return GetTicket(ticketId);
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

    public ReimburseTicket PostTicket(string guid, string r, double a, string d, DateTime t, int eId) {
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
                connection.Open();
                int rowsAffected = command.ExecuteNonQuery();
                if(rowsAffected == 1) {
                    _logger.LogSuccess("PostTicket", "POST", guid);
                    return GetTicket(guid);
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

    public ReimburseTicket GetTicket(string ticketId) {
        using(SqlConnection connection = new SqlConnection(_conString)) {
            string queryTicketById = "SELECT * FROM Ticket WHERE TicketId = @ticketId;";
            SqlCommand command = new SqlCommand(queryTicketById, connection);
            command.Parameters.AddWithValue("@ticketId", ticketId);
            try {
                connection.Open();

                using(SqlDataReader reader = command.ExecuteReader()) {
                    if(!reader.HasRows) {
                        _logger.LogError("GetTicket", "GET", ticketId, "No result for given input");
                        return null!;
                    } 
                    else {
                        reader.Read();
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

    public List<ReimburseTicket> GetTickets(int employeeId) {
        List<ReimburseTicket> employeeTickets = new List<ReimburseTicket>();
        using(SqlConnection connection = new SqlConnection(_conString)) {
            string queryAllEmployeeTickets = "SELECT * FROM Ticket WHERE EmployeeId = @employeeId ORDER BY RequestDate;";
            SqlCommand command = new SqlCommand(queryAllEmployeeTickets, connection);
            command.Parameters.AddWithValue("@employeeId", employeeId);
            return ExecuteGetTickets(connection, command, employeeId);
        }
    }

    public List<ReimburseTicket> GetTickets(int employeeId, int statusId) {
        List<ReimburseTicket> employeeTickets = new List<ReimburseTicket>();
        using(SqlConnection connection = new SqlConnection(_conString)) {
            string queryAllEmployeeTickets = "SELECT * FROM Ticket WHERE EmployeeId = @employeeId AND StatusId = @statusId ORDER BY RequestDate;";
            SqlCommand command = new SqlCommand(queryAllEmployeeTickets, connection);
            command.Parameters.AddWithValue("@employeeId", employeeId);
            command.Parameters.AddWithValue("@statusId", statusId);
            return ExecuteGetTickets(connection, command, $"{employeeId}, {statusId}");
        }
    }

    public Queue<ReimburseTicket> GetPending(int managerId) {
        using(SqlConnection connection = new SqlConnection(_conString)) {
            string queryAllEmployeeTickets = "SELECT * FROM Ticket WHERE StatusId = @statusId ORDER BY RequestDate;";
            SqlCommand command = new SqlCommand(queryAllEmployeeTickets, connection);
            command.Parameters.AddWithValue("@statusId", 0);
            return new Queue<ReimburseTicket>(ExecuteGetTickets(connection, command, managerId));
        }
    }

    private List<ReimburseTicket> ExecuteGetTickets(SqlConnection con, SqlCommand comm, object logInfo) {
        List<ReimburseTicket> employeeTickets = new List<ReimburseTicket>();
        try {
            con.Open();
            using(SqlDataReader reader = comm.ExecuteReader()) {
                if(!reader.HasRows) {
                    _logger.LogError("GetTickets", "GET", logInfo, "No results matching the input.");
                    return null!;
                } 
                while(reader.Read()) {
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