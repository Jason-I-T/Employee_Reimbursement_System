using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

using ModelLayer;
using BusinessLayer;

namespace ApiLayer.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ReimburseTicketController : ControllerBase
    {
        // Dependency injection for ticket service class
        private readonly ITicketService _its;
        // TODO Refactor when auth classes are made
        private readonly IEmployeeAuthService _ieas;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private string _cookieName;
        public ReimburseTicketController(ITicketService its, IHttpContextAccessor httpContextAccessor, IEmployeeAuthService ieas) {
            this._its = its;
            this._ieas = ieas;
            this._httpContextAccessor = httpContextAccessor;
            this._cookieName = "AuthCookie";
        } 

        [HttpPost("Ticket")]
        public async Task<ActionResult<ReimburseTicket>> Ticket(int employeeId, ReimburseTicket t) {
            ReimburseTicket ticket = new ReimburseTicket();
            var cookie = Request.Cookies[_cookieName];
            try {
                if (cookie is null) {
                    string result = await _ieas.CloseSession(employeeId);
                    return StatusCode(401, $"Error: Invalid cookies or session expired.\nCloseSession: {result}");
                }
                ticket = await _its.AddTicket(employeeId, t.reason!, t.amount, t.description!, cookie);
            } catch(Exception e) {
                return StatusCode(500, e.Message);
            }
            if(ticket is null) return StatusCode(400, "Unable to add a new ticket, invalid input(s).");
            else {
                _httpContextAccessor.HttpContext!.Response.Cookies.Delete(cookie);
                CookieOptions options = new CookieOptions();
                options.Expires = DateTime.Now.AddMinutes(15); // Extend time on cookie
                options.Path = "/"; // Make cookie available to all parts of the system
                options.Secure = true; // Ensure cookie is properly secured using SSL
                _httpContextAccessor.HttpContext!.Response.Cookies.Append(_cookieName, cookie, options);
                return StatusCode(201, ticket);
            } 
        }

        [HttpGet("PendingTickets")]
        public async Task<ActionResult<Queue<ReimburseTicket>>> PendingTickets(int managerId) {
            Queue<ReimburseTicket> tickets = new Queue<ReimburseTicket>();
            var cookie = Request.Cookies[_cookieName];
            try {
                if (cookie is null) {
                    string result = await _ieas.CloseSession(managerId);
                    return StatusCode(401, $"Error: Invalid cookies or session expired.\nCloseSession: {result}");
                }
                tickets = await _its.GetPendingTickets(managerId, cookie);
            } catch(Exception e) {
                return StatusCode(500, e.Message);
            }
            if(tickets is null) return StatusCode(400, "Unable to get pending tickets, invalid input(s).");
            else {
                _httpContextAccessor.HttpContext!.Response.Cookies.Delete(cookie);
                CookieOptions options = new CookieOptions();
                options.Expires = DateTime.Now.AddMinutes(15); // Extend time on cookie
                options.Path = "/"; // Make cookie available to all parts of the system
                options.Secure = true; // Ensure cookie is properly secured using SSL
                _httpContextAccessor.HttpContext!.Response.Cookies.Append(_cookieName, cookie, options);
                return StatusCode(201, tickets);
            }
        }

        [HttpPut("Approve")]
        public async Task<ActionResult<ReimburseTicket>> Approve(int managerId, string ticketId) {
            ReimburseTicket ticket = new ReimburseTicket();
            var cookie = Request.Cookies[_cookieName];
            try {
                if (cookie is null) {
                    string result = await _ieas.CloseSession(managerId);
                    return StatusCode(401, $"Error: Invalid cookies or session expired.\nCloseSession: {result}");
                }
                ticket = await _its.ApproveTicket(managerId, ticketId, cookie);
            } catch(Exception e) {
                return StatusCode(500, e.Message);
            }
            if(ticket is null) return StatusCode(400, "Unable to approve ticket, invalid input(s).");
            else {
                _httpContextAccessor.HttpContext!.Response.Cookies.Delete(cookie);
                CookieOptions options = new CookieOptions();
                options.Expires = DateTime.Now.AddMinutes(15); // Extend time on cookie
                options.Path = "/"; // Make cookie available to all parts of the system
                options.Secure = true; // Ensure cookie is properly secured using SSL
                _httpContextAccessor.HttpContext!.Response.Cookies.Append(_cookieName, cookie, options);
                return StatusCode(201, ticket);
            }
        }

        [HttpPut("Deny")]
        public async Task<ActionResult<ReimburseTicket>> Deny(int managerId, string ticketId) {
            ReimburseTicket ticket = new ReimburseTicket();
            var cookie = Request.Cookies[_cookieName];
            try {
                if (cookie is null) {
                    string result = await _ieas.CloseSession(managerId);
                    return StatusCode(401, $"Error: Invalid cookies or session expired.\nCloseSession: {result}");
                }
                ticket = await _its.DenyTicket(managerId, ticketId, cookie);
            } catch(Exception e) {
                return StatusCode(500, e.Message);
            }
            if(ticket is null) return StatusCode(400, "Unable to deny ticket, invalid input(s).");
            else {
                _httpContextAccessor.HttpContext!.Response.Cookies.Delete(cookie);
                CookieOptions options = new CookieOptions();
                options.Expires = DateTime.Now.AddMinutes(15); // Extend time on cookie
                options.Path = "/"; // Make cookie available to all parts of the system
                options.Secure = true; // Ensure cookie is properly secured using SSL
                _httpContextAccessor.HttpContext!.Response.Cookies.Append(_cookieName, cookie, options);
                return StatusCode(201, ticket);
            }
        }
    }
}