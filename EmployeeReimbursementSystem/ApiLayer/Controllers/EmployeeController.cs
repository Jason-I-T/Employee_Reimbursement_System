using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

using ModelLayer;
using BusinessLayer;

namespace ApiLayer.Controllers;
    
/**
 * TODO, Add descriptive status codes & refactor to be async
 * - Make methods async
 */

[ApiController]
[Route("api/[controller]")]
public class EmployeeController : ControllerBase {
    // Dependency Injection for Employee Service class and Ticket Service class
    private readonly IEmployeeService _ies;
    private readonly ITicketService _its;
    public EmployeeController(IEmployeeService ies, ITicketService its) {
        this._ies = ies;
        this._its = its;
    }
        
    [HttpPost("RegisterEmployee")]
    public ActionResult<Employee> PostEmployee(string email, string password) {
        Employee employee = new Employee();
        try {
            employee = _ies.PostEmployee(email, password);
        } catch(Exception e) {
            return StatusCode(500, e.Message);
        }
        if(employee is null) return StatusCode(400, "Unable to register, invalid input(s).");
        else return StatusCode(201, employee);
    }

    [HttpPost("RegisterManager")]
    public ActionResult<Employee> PostEmployee(string email, string password, int roleid) {
        Employee employee = new Employee();
        try {
            employee = _ies.PostEmployee(email, password, roleid);
        } catch(Exception e) {
            return StatusCode(500, e.Message);
        }
        if(employee is null) return StatusCode(400, "Unable to register, invalid input(s).");
        else return StatusCode(201, employee);
    }

    [HttpGet("LoginEmployee")]
    public ActionResult<Employee> LoginEmployee(string email, string password) {
        Employee employee = new Employee();
        try {
            employee = _ies.LoginEmployee(email, password);
        } catch(Exception e) {
            return StatusCode(500, e.Message);
        }
        if(employee is null) return StatusCode(400, "Unable to login, invalid input(s).");
        else return StatusCode(200, employee);
    }

    [HttpPut("ChangePassword")]
    public ActionResult<Employee> EditEmployee(int targetId, string oldPassword, string newPassword) {
        Employee employee = new Employee();
        try {
            employee = _ies.EditEmployee(targetId, oldPassword, newPassword);
        } catch(Exception e) {
            return StatusCode(500, e.Message);
        }
        if(employee is null) return StatusCode(400, "Unable to update password, invalid input(s).");
        else return StatusCode(200, employee);
    }

    [HttpPut("ChangeEmail")]
    public ActionResult<Employee> EditEmployee(int targetId, string newEmail) {
        Employee employee = new Employee();
        try {
            employee = _ies.EditEmployee(targetId, newEmail);
        } catch(Exception e) {
            return StatusCode(500, e.Message);
        }
        if(employee is null) return StatusCode(400, "Unable to update email, invalid input(s).");
        else return StatusCode(200, employee);
    }

    [HttpPut("ChangeRole")]
    public ActionResult<Employee> EditEmployee(int managerId, int targetId, int newRoleId) {
        Employee employee = new Employee();
        try {
            employee = _ies.EditEmployee(managerId, targetId, newRoleId);
        } catch(Exception e) {
            return StatusCode(500, e.Message);
        }
        if(employee is null) return StatusCode(400, "Unable to update role, invalid input(s).");
        else return StatusCode(200, employee);
    }

    [HttpGet("EmployeeTickets")]
    public ActionResult<List<ReimburseTicket>> EmployeeTickets(int empId) {
        List<ReimburseTicket> tickets = new List<ReimburseTicket>();
        try {
            tickets = _its.GetEmployeeTickets(empId);
        } catch(Exception e) {
            return StatusCode(500, e.Message);
        }
        if(tickets is null) return StatusCode(400, "Unable to retrieve tickets, invalid input.");
        else return StatusCode(200, tickets);
    }

    [HttpGet("EmployeeTicketsByStatus")]
    public ActionResult<List<ReimburseTicket>> EmployeeTickets(int empId, int status) {
        List<ReimburseTicket> tickets = new List<ReimburseTicket>();
        try {
            tickets = _its.GetEmployeeTickets(empId, status);
        } catch(Exception e) {
            return StatusCode(500, e.Message);
        }
        if(tickets is null) return StatusCode(400, "Unable to retrieve tickets, invalid input.");
        else return StatusCode(200, tickets);
    }
}