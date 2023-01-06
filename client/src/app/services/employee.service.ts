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
  httpOptions = {
    headers: new HttpHeaders({ 'Content-Type': 'application/json' })
  }
  
  constructor(
    private http: HttpClient,
  ) {  }

  login(employee: Employee): Observable<string> {
    // Entry point for logging in
    var loginUrl: string = this.employeeUrl + '/LoginEmployee';
    return this.http.post<string>(loginUrl, employee, this.httpOptions).pipe(
      tap((sessionId: string) => console.log(`Logged in with`)),
      catchError(this.handleError<string>('login'))
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
