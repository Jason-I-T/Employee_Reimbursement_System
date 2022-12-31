using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

using BusinessLayer;

namespace Tests.Business
{
    public class XUnitTicketValidationService
    {
        [Theory]
        [InlineData("")]
        [InlineData("Valid")]
        public void ValidReasonTest(string reason) {
            string[] s = {""};
            var mtr = new MockTicketRepository();
            var ivs = new TicketValidationService(mtr);

            bool result = ivs.ValidReason(reason);

            if(Array.IndexOf(s, reason) > -1) Assert.False(result);
            else Assert.True(result);

        }

        [Theory]
        [InlineData(1000)]
        [InlineData(0)]
        [InlineData(-1)]
        [InlineData(10001)]
        public void ValidAmountTest(int amount) {
            int[] i = {0, -1, 10001};
            var mtr = new MockTicketRepository();
            var ivs = new TicketValidationService(mtr);

            bool result = ivs.ValidAmount(amount);

            if(Array.IndexOf(i, amount) > -1) Assert.False(result);
            else Assert.True(result);
        }

        [Theory]
        [InlineData("")]
        [InlineData("A Valid description.")]
        public void ValidDescription(string description) {
            string[] s = {""};
            var mtr = new MockTicketRepository();
            var ivs = new TicketValidationService(mtr);

            bool result = ivs.ValidDescription(description);

            if(Array.IndexOf(s, description) > -1) Assert.False(result);
            else Assert.True(result);
        }

        [Theory]
        [InlineData(2, "t1")]
        [InlineData(1, "t1")]
        [InlineData(2, "t3")]
        public void ValidStatusChangeTest(int managerId, string ticketId) {
            // Let an employee with id of 1 be an employee, id of 2 be manager.
            string[] s = {"t0", "t3"};
            int[] i = {1};
            var mtr = new MockTicketRepository();
            var ivs = new TicketValidationService(mtr);

            bool result = ivs.ValidStatusChange(managerId, ticketId);

            if(Array.IndexOf(s, ticketId) > -1 || Array.IndexOf(i, managerId) > -1) Assert.False(result);
            else Assert.True(result);
        }
    }
}