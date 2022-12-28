using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using ModelLayer;
using RepositoryLayer;

namespace BusinessLayer;

public interface IEmployeeService {
    public Employee PostEmployee(string email, string password);
    public Employee PostEmployee(string email, string password, int roleid);
    public Employee LoginEmployee(string email, string password);
    
    public Employee EditEmployee(int id, string oldPassword, string newPassword);
    public Employee EditEmployee(int id, string email);
    public Employee EditEmployee(int managerId, int employeeId, int roleId);
}

public class EmployeeService : IEmployeeService {

    private readonly IEmployeeRepository _ier;
    private IEmployeeValidationService _ievs;
    public EmployeeService(IEmployeeRepository ier) { 
        this._ier = ier;
        this._ievs = new EmployeeValidationService(_ier);
    }

    public Employee LoginEmployee(string email, string password) => _ier.LoginEmployee(email, password);

    #region //Registration methods
    public Employee PostEmployee(string email, string password) { 
        if(!_ievs.ValidRegistration(email, password)) {
            Console.WriteLine("Invalid email or password");
            return null!;
        }

        return _ier.PostEmployee(email, password, 0);
    }

    public Employee PostEmployee(string email, string password, int roleid) {
        if(!_ievs.ValidRegistration(email, password, roleid)) {
            Console.WriteLine("Invalid email, password, or roleId");
            return null!;
        }
        
        return _ier.PostEmployee(email, password, roleid);
    }
    #endregion

    #region // Edit Employee methods
    public Employee EditEmployee(int id, string oldPassword, string newPassword) {
        if(!_ievs.isEmployee(id) || !_ievs.ValidPassword(newPassword) || !_ievs.isPassword(id, oldPassword)) {
            Console.WriteLine("Invalid employeeId, invalid new password, or passwords don't match.");
            return null!;
        }

        return _ier.UpdateEmployee(id, newPassword);
    }

    public Employee EditEmployee(int id, string email) {
        if(!_ievs.isEmployee(id) || !_ievs.ValidEmail(email)) {
            Console.WriteLine("Invalid employeeId, or invalid email");
            return null!;
        }

        return _ier.UpdateEmployee(id, email);
    }
    public Employee EditEmployee(int managerId, int employeeId, int roleId) {
        if(managerId == employeeId) {
            Console.WriteLine("Invalid target id.");
            return null!;
        } 

        if(!_ievs.isManager(managerId) || !_ievs.ValidRole(roleId) || !_ievs.isEmployee(employeeId)){
            Console.WriteLine("Invalid managerId, roleId, or EmployeeId.");
            return null!;
        }
        
        return _ier.UpdateEmployee(employeeId, roleId);
    }
    #endregion
}