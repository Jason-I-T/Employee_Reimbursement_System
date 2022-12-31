using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

using BusinessLayer;

namespace Tests.Business
{
    public class XUnitEmployeeValidationService {
        [Theory]
        [InlineData("pass@email.com")]
        [InlineData("fail@email")]
        [InlineData("failemail.com")]
        [InlineData("@email.com")]
        public void ValidEmailFormatTest(string email) {
            string[] s = {"fail@email", "failemail.com", "@email.com"};
            var mer = new MockEmployeeRepository();
            var ivs = new EmployeeValidationService(mer);

            bool result = ivs.ValidEmail(email);

            if(Array.IndexOf(s, email) > -1) Assert.False(result);
            else Assert.True(result);
        }

        [Theory]
        [InlineData("GoodPass")]
        [InlineData("Good123")]
        [InlineData("Fail")]
        [InlineData("@NotOkay")]
        public void ValidPasswordFormatTest(string pass) {
            string[] s = {"Fail", "@NotOkay"};
            var mer = new MockEmployeeRepository();
            var ivs = new EmployeeValidationService(mer);

            bool result = ivs.ValidPassword(pass);

            if(Array.IndexOf(s, pass) > -1) Assert.False(result);
            else Assert.True(result);
        }

        [Theory]
        [InlineData(0)]
        [InlineData(1)]
        [InlineData(-1)]
        [InlineData(2)]
        public void ValidRoleTest(int roleId) {
            int[] i = {-1, 2};
            var mer = new MockEmployeeRepository();
            var ivs = new EmployeeValidationService(mer);

            bool result = ivs.ValidRole(roleId);

            if(Array.IndexOf(i, roleId) > -1) Assert.False(result);
            else Assert.True(result);
        }

        [Theory]
        [InlineData(3)]
        [InlineData(1)]
        [InlineData(2)]
        [InlineData(4)]
        public void IsManagerTest(int id) {
            int[] i = {1, 2, 4};
            var mer = new MockEmployeeRepository();
            var ivs = new EmployeeValidationService(mer);

            bool result = ivs.isManager(id);

            if(Array.IndexOf(i, id) > -1) Assert.False(result);
            else Assert.True(result);
        }

        [Theory]
        [InlineData(1, "123Pass")]
        [InlineData(3, "123Pass")]
        [InlineData(2, "321Pass")]
        [InlineData(1, "WrongOne")]
        [InlineData(1, "WrongAgain")]
        public void IsPasswordTest(int id, string pass) {
            string[] s = {"WrongOne", "WrongAgain"};
            var mer = new MockEmployeeRepository();
            var ivs = new EmployeeValidationService(mer);

            bool result = ivs.isPassword(id, pass);

            if(Array.IndexOf(s, pass) > -1) Assert.False(result);
            else Assert.True(result);
        }
    }
}