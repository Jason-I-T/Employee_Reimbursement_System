using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Text.Json;
using Microsoft.Data.SqlClient;

using ModelLayer;

/**
 * TODO, Reduce the number of repeated lines of code and increase readability
 * - Identify where code keeps getting reused, seperate functionality into 
 *   seperate methods.
 * - Add in loggers.
 */
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
    private readonly ILoggerTicketRepository _loggerTR;
    public TicketRepository(ILoggerTicketRepository logger) => this._loggerTR = logger;
    
    
    public ReimburseTicket UpdateTicket(string ticketId, int statusId) {
        string conString = File.ReadAllText("../../ConString.txt");
        using(SqlConnection connection = new SqlConnection(conString)) {
            string updateTicketQuery = "UPDATE Ticket SET StatusId = @statusId WHERE TicketId = @ticketId";
            SqlCommand command = new SqlCommand(updateTicketQuery, connection);
            command.Parameters.AddWithValue("@statusId", statusId);
            command.Parameters.AddWithValue("@ticketId", ticketId);
            try {
                connection.Open();
                int rowsAffected = command.ExecuteNonQuery();
                if(rowsAffected == 1) {
                    _loggerTR.LogTicketPut(true, ticketId);
                    return GetTicket(ticketId);
                } else {
                    _loggerTR.LogTicketPut(false, ticketId);
                    return null!;
                }
            } catch(Exception e) {
                _loggerTR.LogTicketPut(false, ticketId);
                Console.WriteLine(e.Message);
                return null!;
            }
        }
    }

    public ReimburseTicket PostTicket(string guid, string r, double a, string d, DateTime t, int eId) {
        string conString = File.ReadAllText("../../ConString.txt");
        using(SqlConnection connection = new SqlConnection(conString)) {
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
                    _loggerTR.LogTicketPost(true, guid);
                    return GetTicket(guid);
                } else {
                    _loggerTR.LogTicketPost(false, guid);
                    return null!;
                }
            } catch(Exception e) {
                _loggerTR.LogTicketPost(false, guid);
                Console.WriteLine(e.Message);
                return null!;
            }
        }
    }

    public ReimburseTicket GetTicket(string ticketId) {
        string conString = File.ReadAllText("../../ConString.txt");
        using(SqlConnection connection = new SqlConnection(conString)) {
            string queryTicketById = "SELECT * FROM Ticket WHERE TicketId = @ticketId;";
            SqlCommand command = new SqlCommand(queryTicketById, connection);
            command.Parameters.AddWithValue("@ticketId", ticketId);
            try {
                connection.Open();

                using(SqlDataReader reader = command.ExecuteReader()) {
                    if(!reader.HasRows) {
                        _loggerTR.LogTicketGet(false, ticketId);
                        return null!;
                    } 
                    else {
                        reader.Read();
                        _loggerTR.LogTicketGet(true, ticketId);
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
                _loggerTR.LogTicketGet(false, ticketId);
                Console.WriteLine(e.Message);
                return null!;
            }
        }
    }

    public List<ReimburseTicket> GetTickets(int employeeId) {
        string conString = File.ReadAllText("../../ConString.txt");
        List<ReimburseTicket> employeeTickets = new List<ReimburseTicket>();
        using(SqlConnection connection = new SqlConnection(conString)) {
            string queryAllEmployeeTickets = "SELECT * FROM Ticket WHERE EmployeeId = @employeeId ORDER BY RequestDate;";
            SqlCommand command = new SqlCommand(queryAllEmployeeTickets, connection);
            command.Parameters.AddWithValue("@employeeId", employeeId);

            try {
                connection.Open();

                using(SqlDataReader reader = command.ExecuteReader()) {
                    if(!reader.HasRows) {
                        _loggerTR.LogTicketGet(false, employeeId);
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
                    _loggerTR.LogTicketGet(true, employeeId);
                    return employeeTickets;
                }
            } catch(Exception e) {
                _loggerTR.LogTicketGet(false, employeeId);
                Console.WriteLine(e.Message);
                return null!;
            }
        }
    }

    public List<ReimburseTicket> GetTickets(int employeeId, int statusId) {
        string conString = File.ReadAllText("../../ConString.txt");
        List<ReimburseTicket> employeeTickets = new List<ReimburseTicket>();
        using(SqlConnection connection = new SqlConnection(conString)) {
            string queryAllEmployeeTickets = "SELECT * FROM Ticket WHERE EmployeeId = @employeeId AND StatusId = @statusId ORDER BY RequestDate;";
            SqlCommand command = new SqlCommand(queryAllEmployeeTickets, connection);
            command.Parameters.AddWithValue("@employeeId", employeeId);
            command.Parameters.AddWithValue("@statusId", statusId);

            try {
                connection.Open();

                using(SqlDataReader reader = command.ExecuteReader()) {
                    if(!reader.HasRows) {
                        _loggerTR.LogTicketGet(false, employeeId);
                        return null!;
                    } 
                    while(reader.Read()) {
                        if((int)reader[4] == statusId) {
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
                    }
                    _loggerTR.LogTicketGet(true, employeeId);
                    return employeeTickets;
                }
            } catch(Exception e) {
                _loggerTR.LogTicketGet(false, employeeId);
                Console.WriteLine(e.Message);
                return null!;
            }
        }
    }

    public Queue<ReimburseTicket> GetPending(int managerId) {
                string conString = File.ReadAllText("../../ConString.txt");
        Queue<ReimburseTicket> employeeTickets = new Queue<ReimburseTicket>();
        using(SqlConnection connection = new SqlConnection(conString)) {
            string queryAllEmployeeTickets = "SELECT * FROM Ticket WHERE StatusId = @statusId ORDER BY RequestDate;";
            SqlCommand command = new SqlCommand(queryAllEmployeeTickets, connection);
            command.Parameters.AddWithValue("@statusId", 0);

            try {
                connection.Open();

                using(SqlDataReader reader = command.ExecuteReader()) {
                    if(!reader.HasRows) {
                        _loggerTR.LogTicketGet(false, managerId);
                        return null!;
                    } 
                    while(reader.Read()) {
                        if((int)reader[4] == 0) {
                            ReimburseTicket newTicket = new ReimburseTicket(
                                (string) reader[0],
                                (string) reader[1],
                                (double) reader[2],
                                (string) reader[3],
                                (int) reader[4],
                                (DateTime) reader[5],
                                (int) reader[6]
                            );
                            employeeTickets.Enqueue(newTicket);
                        }
                    }
                    _loggerTR.LogTicketGet(true, managerId);
                    return employeeTickets;
                }
            } catch(Exception e) {
                _loggerTR.LogTicketGet(false, managerId);
                Console.WriteLine(e.Message);
                return null!;
            }
        }
    }
}