import { Injectable } from '@angular/core';
import { HttpClient, HttpHeaders } from '@angular/common/http';
import { Observable, of } from 'rxjs';
import { catchError, map, tap } from 'rxjs';

import { Employee } from '../models/employee';

@Injectable({
  providedIn: 'root'
})
export class EmployeeService {
  // Method parameters and return values should be the same/similar to API action methods

  /** 
   * TODO Employee login 
   * - Parameter is an employee object, returns a string. 
   * - Do the login request, get the sessionId.
   */
  private employeeUrl = 'https://localhost:7240/api/Employee';
  constructor(
    private http: HttpClient,
  ) {  }

  login(employee: Employee): Observable<string> {
    // Entry point for logging in
    var result: string = "";
    var loginUrl: string = this.employeeUrl + '/LoginEmployee';
    // responseType & witCredentials are necessary for the cookie to appear in angular. Why... 
    return this.http.post(loginUrl, employee, {responseType: 'text', withCredentials: true}).pipe(
      tap((sessionId: any) => console.log(`Login success with sessionId ${sessionId}`)),
      catchError(this.handleError<string>('login'))
    );
  }

  register(newEmployee: Employee): Observable<Employee> {
    var registerUrl: string = this.employeeUrl + '/Register';
    return this.http.post<Employee>(registerUrl, newEmployee, {headers: new HttpHeaders({ 'Content-Type': 'application/json' }), withCredentials: true}).pipe(
      tap((employee: Employee) => console.log(`Registration success: ${employee}`)),
      catchError(this.handleError<Employee>('register'))
    );
  }

  // Helper
  private handleError<T>(operation='operation', result?: T) {
    return(error: any): Observable<T> => {
      console.error(error);
      console.log(`${operation} failed: ${error.message}`);
      return of(result as T);
    };
  }
}
