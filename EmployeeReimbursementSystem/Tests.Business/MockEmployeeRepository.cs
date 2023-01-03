using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using RepositoryLayer;
using ModelLayer;

namespace Tests.Business
{
    public class MockEmployeeRepository : IEmployeeRepository
    {
        private List<Employee> mockDb = new List<Employee>();
        public MockEmployeeRepository() {
            Employee e1 = new Employee(1, "unique@email.com", "123Pass");
            Employee e2 = new Employee(2, "unique2@email.com", "321Pass");
            Employee e3 = new Employee(3, "manager@email.com", "123Pass", 1);
            mockDb.Add(e1);
            mockDb.Add(e2);
            mockDb.Add(e3);
        }

        public Task<Employee> GetEmployee(string email)
        {
            throw new NotImplementedException();
        }

        public Task<Employee> GetEmployee(int id)
        {
            foreach(Employee e in this.mockDb) {
                if(e.id == id)
                    return Task<Employee>.Factory.StartNew(() => {return e;});
            }
            return null!;
        }

        public Task<String> LoginEmployee(string email, string password)
        {
            throw new NotImplementedException();
        }

        public Task<Employee> PostEmployee(string email, string password)
        {
            throw new NotImplementedException();
        }

        public Task<Employee> PostEmployee(string email, string password, int roleId)
        {
            throw new NotImplementedException();
        }

        public Task<Employee> UpdateEmployee(int id, int roleId)
        {
            throw new NotImplementedException();
        }

        public Task<Employee> UpdateEmployee(int id, string info)
        {
            throw new NotImplementedException();
        }
    }
}