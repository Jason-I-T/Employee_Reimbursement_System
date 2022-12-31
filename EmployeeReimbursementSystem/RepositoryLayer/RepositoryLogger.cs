using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RepositoryLayer
{
    public interface IRepositoryLogger {
        void LogSuccess(string caller, string type, object arg);
        void LogError(string caller, string type, object arg, string errMsg);
    }

    public class RepositoryLogger : IRepositoryLogger {
        public void LogSuccess(string caller, string type, object arg) => 
            Console.WriteLine($"{type} request from {caller} with argument {arg} successful");
        
        public void LogError(string caller, string type, object arg, string errMsg) =>
            Console.WriteLine($"{type} request from {caller} with argument {arg} unsuccessful\n{errMsg}");
    }
}