using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using ModelLayer;
using RepositoryLayer;

namespace BusinessLayer;

public interface ITicketService { 
    public Task<ReimburseTicket> AddTicket(int empId, string reason, double amount, string description, string sessionId);
    public Task<Queue<ReimburseTicket>> GetPendingTickets(int managerId, string sessionId);
    public Task<ReimburseTicket> ApproveTicket(int managerId, string tickId, string sessionId);
    public Task<ReimburseTicket> DenyTicket(int managerId, string ticketId, string sessionId);
    public Task<List<ReimburseTicket>> GetEmployeeTickets(int empId, string sessionId);
    public Task<List<ReimburseTicket>> GetEmployeeTickets(int empId, int status, string sessionId);
}

public class TicketService : ITicketService {
    // Dependency Injection
    private readonly ITicketRepository _itr;
    private readonly IEmployeeRepository _ier;
    private IEmployeeValidationService _ievs;
    private ITicketValidationService _itvs;
    private IDataLogger _logger;
    public TicketService(ITicketRepository itr, IEmployeeRepository ier, IEmployeeValidationService ievs, ITicketValidationService itvs, IDataLogger logger) {
        this._itr = itr;
        this._ier = ier;
        this._ievs = ievs;
        this._itvs = itvs;
        this._logger = logger;
    }
    
    public async Task<ReimburseTicket> AddTicket(int empId, string reason, double amount, string desc, string sessionId) {
        if(!_itvs.ValidTicket(reason, amount, desc)) {
            _logger.LogError("AddTicket", "POST", $"{empId}, {reason}, {amount}, {desc}", "Ticket input is invalid");
            return null!;
        }
        string guid = Guid.NewGuid().ToString();
        return await _itr.PostTicket(guid, reason, Math.Round(amount, 2), desc, DateTime.Now, empId, sessionId);
    }

    public async Task<Queue<ReimburseTicket>> GetPendingTickets(int managerId, string sessionId) {
        if(!_ievs.isManager(managerId).Result) {
            _logger.LogError("GetPending", "GET", managerId, "Invalid managerId");
            return null!;
        } 
        return await _itr.GetPending(managerId, sessionId);
    }

    public async Task<ReimburseTicket> ApproveTicket(int empId, string ticketId, string sessionId) {
        if(!_ievs.isManager(empId).Result || !_itvs.ValidStatusChange(empId, ticketId)){
            _logger.LogError("ApproveTicket", "PUT", $"{empId}, {ticketId}", "Invalid managerId or invalid change requested");
            return null!;
        } 
        return await _itr.UpdateTicket(ticketId, 1, empId, sessionId);
    }

    public async Task<ReimburseTicket> DenyTicket(int empId, string ticketId, string sessionId) {
        if(!_ievs.isManager(empId).Result || !_itvs.ValidStatusChange(empId, ticketId)){
            _logger.LogError("DenyTicket", "PUT", $"{empId}, {ticketId}", "Invalid managerId or invalid change requested");
            return null!;
        } 
        return await _itr.UpdateTicket(ticketId, 2, empId, sessionId);
    }

    public async Task<List<ReimburseTicket>> GetEmployeeTickets(int empId, string sessionId) => await _itr.GetTickets(empId, sessionId);
    public async Task<List<ReimburseTicket>> GetEmployeeTickets(int empId, int status, string sessionId) => await _itr.GetTickets(empId, status, sessionId);
}