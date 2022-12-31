using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

using ModelLayer;
using BusinessLayer;

/**
 * TODO, Add descriptive status codes & refactor to be async
 * - Make methods async
 */
namespace ApiLayer.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ReimburseTicketController : ControllerBase
    {
        // Dependency injection for ticket service class
        private readonly ITicketService _its;
        public ReimburseTicketController(ITicketService its) => this._its = its;

        [HttpPost("Ticket")]
        public ActionResult<ReimburseTicket> Ticket(int employeeId, string reason, double amount, string description) {
            ReimburseTicket ticket = new ReimburseTicket();
            try {
                ticket = _its.AddTicket(employeeId, reason, amount, description);
            } catch(Exception e) {
                return StatusCode(500, e.Message);
            }
            if(ticket is null) return StatusCode(400, "Unable to add a new ticket, invalid input(s).");
            else return StatusCode(201, ticket);
        }

        [HttpGet("PendingTickets")]
        public ActionResult<Queue<ReimburseTicket>> PendingTickets(int managerId) {
            Queue<ReimburseTicket> tickets = new Queue<ReimburseTicket>();
            try {
                tickets = _its.GetPendingTickets(managerId);
            } catch(Exception e) {
                return StatusCode(500, e.Message);
            }
            if(tickets is null) return StatusCode(400, "Unable to get pending tickets, invalid input(s).");
            else return StatusCode(200, tickets);
        }

        [HttpPut("Approve")]
        public ActionResult<ReimburseTicket> Approve(int managerId, string ticketId) {
            ReimburseTicket ticket = new ReimburseTicket();
            try {
                ticket = _its.ApproveTicket(managerId, ticketId);
            } catch(Exception e) {
                return StatusCode(500, e.Message);
            }
            if(ticket is null) return StatusCode(400, "Unable to approve ticket, invalid input(s).");
            else return StatusCode(200, ticket);
        }

        [HttpPut("Deny")]
        public ActionResult<ReimburseTicket> Deny(int managerId, string ticketId) {
            ReimburseTicket ticket = new ReimburseTicket();
            try {
                ticket = _its.DenyTicket(managerId, ticketId);
            } catch(Exception e) {
                return StatusCode(500, e.Message);
            }
            if(ticket is null) return StatusCode(400, "Unable to deny ticket, invalid input(s).");
            else return StatusCode(200, ticket);
        }
    }
}