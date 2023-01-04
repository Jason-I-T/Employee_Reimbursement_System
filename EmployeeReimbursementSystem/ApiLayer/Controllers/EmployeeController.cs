using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

using ModelLayer;
using BusinessLayer;

namespace ApiLayer.Controllers;

[ApiController]
[Route("api/[controller]")]
public class EmployeeController : ControllerBase {
    // Dependency Injection for Employee Service class and Ticket Service class
    private readonly IEmployeeService _ies;
    private readonly ITicketService _its;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private string _cookieName;
    public EmployeeController(IEmployeeService ies, ITicketService its, IHttpContextAccessor httpContextAccessor) {
        this._ies = ies;
        this._its = its;
        this._httpContextAccessor = httpContextAccessor;
        this._cookieName = "AuthCookie";
    }
        
    [HttpPost("Register")]
    public async Task<ActionResult<Employee>> PostEmployee(Employee e) {
        Employee employee = new Employee();
        try {
            employee = await _ies.PostEmployee(e.email!, e.password!, e.roleID);
        } catch(Exception ex) {
            return StatusCode(500, ex.Message);
        }
        if(employee is null) return StatusCode(400, "Unable to register, invalid input(s).");
        else return StatusCode(201, employee);
    }

    [HttpPost("LoginEmployee")]
    public async Task<ActionResult<Employee>> LoginEmployee(Employee e) {
        string sessionId = null!;
        try { 
            // TODO sessionId is a guid, look into System.Security.Cryptography to generate better ids
            sessionId = await _ies.LoginEmployee(e.email!, e.password!);
            if(sessionId is null) return StatusCode(400, "Unable to login, invalid input(s).");
            CookieOptions options = new CookieOptions();
            options.Expires = DateTime.Now.AddMinutes(15);
            options.Path = "/"; // ? what does this do
            options.Secure = true; // Ensure cookie is properly secured using SSL/TLS encryption(?)
            _httpContextAccessor.HttpContext!.Response.Cookies.Append(_cookieName, sessionId, options);
        } catch(Exception ex) {
            return StatusCode(500, ex.Message);
        }
        return StatusCode(200, sessionId);
    }

    [HttpDelete("LogoutEmployee")]
    public async Task<ActionResult<string>> LogoutEmployee(Employee e) {
        string logoutResult = null!;
        var cookie = Request.Cookies[_cookieName];
        try {
            if (cookie is null) {
                logoutResult = await _ies.CloseSession(e.id);
                return StatusCode(401, $"Error: Invalid cookies or session expired.\nCloseSession: {logoutResult}");
            }
            // Delete the session in session store
            logoutResult = await _ies.LogoutEmployee(e.id, cookie);
        } catch(Exception ex) {
            return StatusCode(500, ex.Message);
        }
        if(logoutResult is null) return StatusCode(400, "Unable to logout, invalid input(s).");
        else { // Destroy the cookie
            _httpContextAccessor.HttpContext!.Response.Cookies.Delete(cookie);
            CookieOptions options = new CookieOptions();
            options.Expires = DateTime.Now.AddMinutes(-1);
            _httpContextAccessor.HttpContext!.Response.Cookies.Append(_cookieName, cookie, options);
            return StatusCode(200, $"Logout result: {logoutResult}");
        }
    }

    [HttpPut("ChangePassword")]
    public async Task<ActionResult<Employee>> EditEmployee(Employee e, string oldPassword) {
        Employee employee = new Employee();
        var cookie = Request.Cookies[_cookieName];
        try {
            if(cookie is null) {
                string result = await _ies.CloseSession(e.id);
                return StatusCode(401, $"Error: Invalid cookies or session expired.\nCloseSession: {result}");
            }
            employee = await _ies.EditEmployee(e.id, oldPassword, e.password!, cookie);
        } catch(Exception ex) {
            return StatusCode(500, ex.Message);
        }
        if(employee is null) return StatusCode(400, "Unable to update password, invalid input(s).");
        else {
            // To ensure cookie doesn't expire automatically
            _httpContextAccessor.HttpContext!.Response.Cookies.Delete(cookie);
            CookieOptions options = new CookieOptions();
            options.Expires = DateTime.Now.AddMinutes(15); // Extend time on cookie
            options.Path = "/"; // Make cookie available to all parts of the system
            options.Secure = true; // Ensure cookie is properly secured using SSL
            _httpContextAccessor.HttpContext!.Response.Cookies.Append(_cookieName, cookie, options);
            return StatusCode(200, employee);
        }
    }

    [HttpPut("ChangeEmail")]
    public async Task<ActionResult<Employee>> EditEmployee(Employee e) {
        Employee employee = new Employee();
        var cookie = Request.Cookies[_cookieName];
        try {
            if(cookie is null) {
                string result = await _ies.CloseSession(e.id);
                return StatusCode(401, $"Error: Invalid cookies or session expired.\nCloseSession: {result}");
            }
            employee = await _ies.EditEmployee(e.id, e.email!, cookie);
        } catch(Exception ex) {
            return StatusCode(500, ex.Message);
        }
        if(employee is null) return StatusCode(400, "Unable to update email, invalid input(s).");
        else {
            // To ensure cookie doesn't expire automatically
            _httpContextAccessor.HttpContext!.Response.Cookies.Delete(cookie);
            CookieOptions options = new CookieOptions();
            options.Expires = DateTime.Now.AddMinutes(15); // Extend time on cookie
            options.Path = "/"; // Make cookie available to all parts of the system
            options.Secure = true; // Ensure cookie is properly secured using SSL
            _httpContextAccessor.HttpContext!.Response.Cookies.Append(_cookieName, cookie, options);
            return StatusCode(200, employee);
        } 
    }

    [HttpPut("ChangeRole")]
    public async Task<ActionResult<Employee>> EditEmployee(int managerId, int targetId, int newRoleId) {
        Employee employee = new Employee();
        var cookie = Request.Cookies[_cookieName];
        try {
            if(cookie is null) {
                string result = await _ies.CloseSession(managerId);
                return StatusCode(401, $"Error: Invalid cookies or session expired.\nCloseSession: {result}");
            }
            employee = await _ies.EditEmployee(managerId, targetId, newRoleId, cookie); // Sending cookie for authorization
        } catch(Exception ex) {
            return StatusCode(500, ex.Message);
        }
        if(employee is null) return StatusCode(400, "Unable to update role, invalid input(s).");
        else { 
            // To ensure cookie doesn't expire automatically
            _httpContextAccessor.HttpContext!.Response.Cookies.Delete(cookie);
            CookieOptions options = new CookieOptions();
            options.Expires = DateTime.Now.AddMinutes(15); // Extend time on cookie
            options.Path = "/"; // Make cookie available to all parts of the system
            options.Secure = true; // Ensure cookie is properly secured using SSL
            _httpContextAccessor.HttpContext!.Response.Cookies.Append(_cookieName, cookie, options);

            return StatusCode(200, employee); 
        }
    }

    [HttpGet("EmployeeTickets")]
    public async Task<ActionResult<List<ReimburseTicket>>> EmployeeTickets(int employeeId) {
        List<ReimburseTicket> tickets = new List<ReimburseTicket>();
        var cookie = Request.Cookies[_cookieName];
        try {
            if(cookie is null) { // If cookie is invalid, close the session. Return unauthenticated status code
                string result = await _ies.CloseSession(employeeId);
                return StatusCode(401, $"Error: Invalid cookies or session expired.\nCloseSession: {result}"); 
            }
            tickets = await _its.GetEmployeeTickets(employeeId, cookie); // Sending cookie for authorization against session store
        } catch(Exception ex) {
            return StatusCode(500, ex.Message);
        }
        if(tickets is null) return StatusCode(400, "Unable to retrieve tickets, invalid input.");
        else { 
            // To ensure cookie doesn't expire automatically
            _httpContextAccessor.HttpContext!.Response.Cookies.Delete(cookie);
            CookieOptions options = new CookieOptions();
            options.Expires = DateTime.Now.AddMinutes(15); // Extend time on cookie
            options.Path = "/"; // Make cookie available to all parts of the system
            options.Secure = true; // Ensure cookie is properly secured using SSL
            _httpContextAccessor.HttpContext!.Response.Cookies.Append(_cookieName, cookie, options);

            return StatusCode(200, tickets);
        }
    }

    [HttpGet("EmployeeTicketsByStatus")]
    public async Task<ActionResult<List<ReimburseTicket>>> EmployeeTickets(int employeeId, int status) {
        List<ReimburseTicket> tickets = new List<ReimburseTicket>();
        var cookie = Request.Cookies[_cookieName];
        try {
            if(cookie is null) { // If cookie is invalid, close the session. Return unauthenticated status code
                string result = await _ies.CloseSession(employeeId);
                return StatusCode(401, $"Error: Invalid cookies or session expired.\nCloseSession: {result}"); 
            }
            tickets = await _its.GetEmployeeTickets(employeeId, status, cookie);
        } catch(Exception ex) {
            return StatusCode(500, ex.Message);
        }
        if(tickets is null) return StatusCode(400, "Unable to retrieve tickets, invalid input.");
        else { 
            // To ensure cookie doesn't expire automatically
            _httpContextAccessor.HttpContext!.Response.Cookies.Delete(cookie);
            CookieOptions options = new CookieOptions();
            options.Expires = DateTime.Now.AddMinutes(15); // Extend time on cookie
            options.Path = "/"; // Make cookie available to all parts of the system
            options.Secure = true; // Ensure cookie is properly secured using SSL
            _httpContextAccessor.HttpContext!.Response.Cookies.Append(_cookieName, cookie, options);

            return StatusCode(200, tickets);
        }
    }
}