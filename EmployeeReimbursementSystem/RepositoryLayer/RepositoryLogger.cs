using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

/** 
 * TODO, Make logger more simple & give more info on errors.
 * - Refactor logger to be one class and two methods. One for logging success,
 *   one for logging errors.
 * - Logger takes in method name, type of request, argument. If it is an error, also take
 *   in the error message. Log the status of the request to the CLI.
 */
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