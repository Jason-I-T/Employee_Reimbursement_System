using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using ModelLayer;
using RepositoryLayer;

namespace BusinessLayer;

public interface ITicketService { 
    public ReimburseTicket AddTicket(int empId, string reason, double amount, string description);
    public Queue<ReimburseTicket> GetPendingTickets(int managerId);
    public ReimburseTicket ApproveTicket(int managerId, string tickId);
    public ReimburseTicket DenyTicket(int managerId, string ticketId);
    public List<ReimburseTicket> GetEmployeeTickets(int empId);
    public List<ReimburseTicket> GetEmployeeTickets(int empId, int status);
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
    
    public ReimburseTicket AddTicket(int empId, string reason, double amount, string desc) {
        if(!_itvs.ValidTicket(reason, amount, desc)) {
            _logger.LogError("AddTicket", "POST", $"{empId}, {reason}, {amount}, {desc}", "Ticket input is invalid");
            return null!;
        }
        string guid = Guid.NewGuid().ToString();
        return _itr.PostTicket(guid, reason, Math.Round(amount, 2), desc, DateTime.Now, empId);
    }

    public Queue<ReimburseTicket> GetPendingTickets(int managerId) {
        if(!_ievs.isManager(managerId)) {
            _logger.LogError("GetPending", "GET", managerId, "Invalid managerId");
            return null!;
        } 
        return _itr.GetPending(managerId);
    }

    public ReimburseTicket ApproveTicket(int empId, string ticketId) {
        if(!_ievs.isManager(empId) || !_itvs.ValidStatusChange(empId, ticketId)){
            _logger.LogError("ApproveTicket", "PUT", $"{empId}, {ticketId}", "Invalid managerId or invalid change requested");
            return null!;
        } 
        return _itr.UpdateTicket(ticketId, 1);
    }

    public ReimburseTicket DenyTicket(int empId, string ticketId) {
        if(!_ievs.isManager(empId) || !_itvs.ValidStatusChange(empId, ticketId)){
            _logger.LogError("DenyTicket", "PUT", $"{empId}, {ticketId}", "Invalid managerId or invalid change requested");
            return null!;
        } 
        return _itr.UpdateTicket(ticketId, 2);
    }

    public List<ReimburseTicket> GetEmployeeTickets(int empId) => _itr.GetTickets(empId);
    public List<ReimburseTicket> GetEmployeeTickets(int empId, int status) => _itr.GetTickets(empId, status);
}