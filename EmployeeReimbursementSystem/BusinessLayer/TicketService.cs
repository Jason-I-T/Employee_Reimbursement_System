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

/**
 * TODO
 * - Add in loggers
 */
public class TicketService : ITicketService {
    // Dependency Injection
    private readonly ITicketRepository _itr;
    private readonly IEmployeeRepository _ier;
    private IEmployeeValidationService _ievs;
    private ITicketValidationService _itvs;
    public TicketService(ITicketRepository itr, IEmployeeRepository ier, IEmployeeValidationService ievs, ITicketValidationService itvs) {
        this._itr = itr;
        this._ier = ier;
        this._ievs = ievs;
        this._itvs = itvs;
    }
    
    public ReimburseTicket AddTicket(int empId, string reason, double amount, string desc) {
        if(!_itvs.ValidTicket(reason, amount, desc)) {
            // Console.WriteLine("Invalid employeeId, or your ticket was invalid.");
            return null!;
        }
        string guid = Guid.NewGuid().ToString();
        return _itr.PostTicket(guid, reason, Math.Round(amount, 2), desc, DateTime.Now, empId);
    }

    public Queue<ReimburseTicket> GetPendingTickets(int managerId) {
        if(!_ievs.isManager(managerId)) {
            // Console.WriteLine("Employee does not exist or have the righ permissions");
            return null!;
        } 
        
        return _itr.GetPending(managerId);
    }

    public ReimburseTicket ApproveTicket(int empId, string ticketId) {
        if(!_ievs.isManager(empId) || !_itvs.ValidStatusChange(empId, ticketId)){
            // Console.WriteLine("Invalid manager Id, manager is trying edit an invalid ticket, or ticket doesn't exist");
            return null!;
        } 

        return _itr.UpdateTicket(ticketId, 1);
    }

    public ReimburseTicket DenyTicket(int empId, string ticketId) {
        if(!_ievs.isManager(empId) || !_itvs.ValidStatusChange(empId, ticketId)){
            // Console.WriteLine("Invalid manager Id, manager is trying to edit an invalid ticket, or ticket doesn't exist");
            return null!;
        } 

        return _itr.UpdateTicket(ticketId, 2);
    }

    public List<ReimburseTicket> GetEmployeeTickets(int empId) => _itr.GetTickets(empId);
    public List<ReimburseTicket> GetEmployeeTickets(int empId, int status) => _itr.GetTickets(empId, status);
}