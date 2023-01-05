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
        public Task<string> CloseSession(string sessionId);
    }
    
    public class EmployeeAuthService : IEmployeeAuthService {
        private readonly IAuthRepository _iar;
        private readonly IEmployeeValidationService _ievs;
        private readonly IDataLogger _logger;
        public EmployeeAuthService(IAuthRepository iar, IEmployeeValidationService ievs, IDataLogger logger) {
            this._iar = iar;
            this._ievs = ievs;
            this._logger = logger;
        }

        public async Task<string> LoginEmployee(string email, string password) {
            if(!_ievs.ValidEmail(email) || !_ievs.ValidPassword(password)) {
            _logger.LogError("LoginEmployee", "POST", $"{email}, {password}", "Login Failure: Invalid input for email and/or password");
            return null!;
        }
        
            return await _iar.LoginEmployee(email, password);
        }

        public async Task<string> LogoutEmployee(int employeeId, string sessionId)
            => await _iar.LogoutEmployee(employeeId, sessionId);

        public async Task<string> CloseSession(int employeeId) 
            => await _iar.CloseSession(employeeId);

        public async Task<string> CloseSession(string sessionId)
            => await _iar.CloseSession(sessionId);
    }
}