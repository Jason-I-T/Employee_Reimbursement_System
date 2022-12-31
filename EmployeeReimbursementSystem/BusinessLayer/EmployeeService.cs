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
    private IRepositoryLogger _logger;
    private IEmployeeValidationService _ievs;
    public EmployeeService(IEmployeeRepository ier, IEmployeeValidationService ievs, IRepositoryLogger logger) { 
        this._ier = ier;
        this._ievs = ievs;
        this._logger = logger;
    }

    public Employee LoginEmployee(string email, string password) => _ier.LoginEmployee(email, password);

    #region //Registration methods
    public Employee PostEmployee(string email, string password) { 
        if(!_ievs.ValidRegistration(email, password)) {
            _logger.LogError("PostEmployee", "POST", $"{email}, {password}", "Invalid email and/or password");
            return null!;
        }

        return _ier.PostEmployee(email, password, 0);
    }

    public Employee PostEmployee(string email, string password, int roleid) {
        if(!_ievs.ValidRegistration(email, password, roleid)) {
            _logger.LogError("PostEmployee", "POST", $"{email}, {password}, {roleid}", "Invalid email, password, and/or roleId.");
            return null!;
        }
        
        return _ier.PostEmployee(email, password, roleid);
    }
    #endregion

    #region // Edit Employee methods
    public Employee EditEmployee(int id, string oldPassword, string newPassword) {
        if(!_ievs.ValidPassword(newPassword) || !_ievs.isPassword(id, oldPassword)) {
            _logger.LogError("EditEmail", "PUT", $"{id}, {oldPassword}, {newPassword}", "Invalid password(s)");
            return null!;
        }

        return _ier.UpdateEmployee(id, newPassword);
    }

    public Employee EditEmployee(int id, string email) {
        if(!_ievs.ValidEmail(email)) {
            _logger.LogError("EditEmail", "PUT", $"{id}, {email}", "Invalid email");
            return null!;
        }

        return _ier.UpdateEmployee(id, email);
    }
    public Employee EditEmployee(int managerId, int employeeId, int roleId) {
        if(managerId == employeeId) {
            _logger.LogError("EditEmail", "PUT", $"{managerId}, {employeeId}, {roleId}", $"Invalid targetId");
            return null!;
        } 

        if(!_ievs.isManager(managerId) || !_ievs.ValidRole(roleId)){
            _logger.LogError("EditEmail", "PUT", $"{managerId}, {employeeId}, {roleId}", $"Invalid managerId or roleId");
            return null!;
        }
        
        return _ier.UpdateEmployee(employeeId, roleId);
    }
    #endregion
}