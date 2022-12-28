using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

/**
 * TODO, Deprecate this class
 * - Merging the logger class into one.
 */
namespace RepositoryLayer
{
    public interface ILoggerTicketRepository {
        // Logger for GET requests
        void LogTicketGet(bool success, object o);
        
        // Logger for POST requests
        void LogTicketPost(bool success, object o);
        
        // Logger for PUT requests
        void LogTicketPut(bool success, object o);
    }

    public class LoggerTicketRepository : ILoggerTicketRepository {
        public void LogTicketGet(bool success, object o) {
            if(success)
                Console.WriteLine($"GET request for ReimburseTicket was successful using {o} argument");
            else
                Console.WriteLine($"GET request for ReimburseTicket FAILED using {o} argument");
        }

        public void LogTicketPost(bool success, object o) {
            if(success)
                Console.WriteLine($"POST request for ReimburseTicket was successful using {o} argument");
            else
                Console.WriteLine($"POST request for ReimburseTicket FAILED using {o} argument");
        }

        public void LogTicketPut(bool success, object o) {
            if(success)
                Console.WriteLine($"PUT request for ReimburseTicket was successful using {o} argument");
            else
                Console.WriteLine($"PUT request for ReimburseTicket FAILED using {o} argument");
        }
    }
}