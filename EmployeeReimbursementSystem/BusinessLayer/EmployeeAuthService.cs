using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using RepositoryLayer;
using ModelLayer;

namespace BusinessLayer
{
    public interface IEmployeeAuthService {
        public Task<string> LoginEmployee(string email, string password);
        public Task<string> LogoutEmployee(int employeeId, string sessionId);
        public Task<string> CloseSession(int employeeId);
    }
    
    public class EmployeeAuthService : IEmployeeAuthService {
        private readonly IEmployeeRepository _ier;
        private readonly IEmployeeValidationService _ievs;
        private readonly IDataLogger _logger;
        public EmployeeAuthService(IEmployeeRepository ier, IEmployeeValidationService ievs, IDataLogger logger) {
            this._ier = ier;
            this._ievs = ievs;
            this._logger = logger;
        }

        public async Task<string> LoginEmployee(string email, string password) {
            if(!_ievs.ValidEmail(email) || !_ievs.ValidPassword(password)) {
            _logger.LogError("LoginEmployee", "POST", $"{email}, {password}", "Login Failure: Invalid input for email and/or password");
            return null!;
        }
        
            return await _ier.LoginEmployee(email, password);
        }

        public async Task<string> LogoutEmployee(int employeeId, string sessionId)
            => await _ier.LogoutEmployee(employeeId, sessionId);

        public async Task<string> CloseSession(int employeeId) 
            => await _ier.CloseSession(employeeId);
    }
}