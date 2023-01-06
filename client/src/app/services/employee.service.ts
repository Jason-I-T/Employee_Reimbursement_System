import { Injectable } from '@angular/core';
import { Employee } from '../models/employee';

@Injectable({
  providedIn: 'root'
})
export class EmployeeService {

  constructor() {  }

  // Method parameters and return values should be the same/similar to API action methods

  /** 
   * TODO Employee login 
   * - Parameter is an employee object, returns a string. 
   * - Do the login request, get the sessionId.
   */
  login(employee: Employee): string {
    return `${employee.email}, ${employee.password}`;
  }
}
