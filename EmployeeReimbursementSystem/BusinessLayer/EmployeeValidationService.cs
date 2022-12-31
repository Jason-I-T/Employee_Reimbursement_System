using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Text.RegularExpressions;

using ModelLayer;
using RepositoryLayer;

namespace BusinessLayer
{
    public interface IEmployeeValidationService {
        public bool ValidEmail(string email);
        public bool ValidPassword(string pass);
        public bool ValidRole(int roleId);
        public bool ValidRegistration(string email, string pass);
        public bool ValidRegistration(string email, string pass, int roleId);
        public bool isManager(int id);
        public bool isPassword(int id, string oldPass);
    }

    public class EmployeeValidationService : IEmployeeValidationService {
        private readonly IEmployeeRepository _ier;
        public EmployeeValidationService(IEmployeeRepository ier) => this._ier = ier;

        public bool ValidRegistration(string email, string pass) => ValidEmail(email) && ValidPassword(pass);
        public bool ValidRegistration(string email, string pass, int roleId) => ValidEmail(email) && ValidPassword(pass) && ValidRole(roleId);

        #region // Functions for validating registration
        public bool ValidEmail(string email) {
            string regex = @"^([a-zA-Z0-9_\-\.]+)@([a-zA-Z0-9_\-\.]+)\.([a-zA-Z]{2,5})$";
            return Regex.Match(email, regex).Success;
        }            
        public bool ValidPassword(string pass) => Regex.Match(pass, @"^([0-9a-zA-Z]{6,})$").Success;
        public bool ValidRole(int roleId) => (roleId >= 0 && roleId <= 1);
        #endregion

        public bool isManager(int id) {
            Employee tmp = _ier.GetEmployee(id);
            if(tmp is null || tmp.roleID == 0) return false;
            else return true;
        }

        public bool isPassword(int id, string oldPass) {
            Employee tmp = _ier.GetEmployee(id);
            if(!((tmp.password!).Equals(oldPass))) return false;
            else return true;
        }
    }
}