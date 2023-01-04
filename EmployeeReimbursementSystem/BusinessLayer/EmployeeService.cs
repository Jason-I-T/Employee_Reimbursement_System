using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using ModelLayer;
using RepositoryLayer;

namespace BusinessLayer;

public interface IEmployeeService {
    public Task<Employee> PostEmployee(string email, string password, int roleid);
    public Task<string> LoginEmployee(string email, string password);
    public Task<Employee> EditEmployee(int id, string oldPassword, string newPassword);
    public Task<Employee> EditEmployee(int id, string email);
    public Task<Employee> EditEmployee(int managerId, int employeeId, int roleId, string sessionId);
    // TODO Make an authentication service class
    public Task<string> CloseSession(int employeeId);
}

public class EmployeeService : IEmployeeService {

    private readonly IEmployeeRepository _ier;
    private IDataLogger _logger;
    private IEmployeeValidationService _ievs;
    public EmployeeService(IEmployeeRepository ier, IEmployeeValidationService ievs, IDataLogger logger) { 
        this._ier = ier;
        this._ievs = ievs;
        this._logger = logger;
    }

    // Send sessionId to controller
    public async Task<string> LoginEmployee(string email, string password) { 
        if(!_ievs.ValidEmail(email) || !_ievs.ValidPassword(password)) {
            _logger.LogError("LoginEmployee", "POST", $"{email}, {password}", "Login Failure: Invalid input for email and/or password");
            return null!;
        }
        
        return await _ier.LoginEmployee(email, password); 
    }

    public async Task<Employee> PostEmployee(string email, string password, int roleid) {
        if(!_ievs.ValidRegistration(email, password, roleid)) {
            _logger.LogError("PostEmployee", "POST", $"{email}, {password}, {roleid}", "Invalid email, password, and/or roleId.");
            return null!;
        }
        
        return await _ier.PostEmployee(email, password, roleid);
    }

    #region // Edit Employee methods
    public async Task<Employee> EditEmployee(int id, string oldPassword, string newPassword) {
        if(!_ievs.ValidPassword(newPassword) || !_ievs.isPassword(id, oldPassword).Result) {
            _logger.LogError("EditEmail", "PUT", $"{id}, {oldPassword}, {newPassword}", "Invalid password(s)");
            return null!;
        }

        return await _ier.UpdateEmployee(id, newPassword);
    }

    public async Task<Employee> EditEmployee(int id, string email) {
        if(!_ievs.ValidEmail(email)) {
            _logger.LogError("EditEmail", "PUT", $"{id}, {email}", "Invalid email");
            return null!;
        }

        return await _ier.UpdateEmployee(id, email);
    }
    public async Task<Employee> EditEmployee(int managerId, int employeeId, int roleId, string sessionId) {
        if(managerId == employeeId) {
            _logger.LogError("EditEmail", "PUT", $"{managerId}, {employeeId}, {roleId}", $"Invalid targetId");
            return null!;
        } 

        if(!_ievs.isManager(managerId).Result || !_ievs.ValidRole(roleId)){
            _logger.LogError("EditEmail", "PUT", $"{managerId}, {employeeId}, {roleId}", $"Invalid managerId or roleId");
            return null!;
        }
        
        return await _ier.UpdateEmployee(employeeId, roleId, managerId, sessionId);
    }
    #endregion

    public async Task<string> CloseSession(int employeeId) {
        return await _ier.CloseSession(employeeId);
    }
}