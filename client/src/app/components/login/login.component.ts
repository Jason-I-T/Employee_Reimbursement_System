import { Component, Input } from '@angular/core';
import { Employee } from 'src/app/models/employee';
import { EmployeeService } from 'src/app/services/employee.service';
import { Location } from '@angular/common'; // Used in the helper

@Component({
  selector: 'app-login',
  templateUrl: './login.component.html',
  styleUrls: ['./login.component.css']
})
export class LoginComponent {
  /** 
    * TODO Send a login request to ERS api
    * x Create the login form
    * x Get input from login form to instantiate employee to be sent to api
    * - Make a login request using the frontend
    */
  constructor
  ( // Dependency injections: EmployeeService, Location (Helper)
    private _employeeService: EmployeeService, 
    private _location: Location
  ) { }

  loginEmployee(emailInput: string, passwordInput: string): void {
    var employee : Employee = {
      id: -1,
      email: emailInput,
      password: passwordInput,
      roleID: 0
    };
    var result: string='SessionId: ';
    this._employeeService.login(employee)
      .subscribe(sessionId => result += sessionId);

    // TODO Move to logger
    console.log(result);
  }

  // TODO put in a helper class
  goBack(): void { 
    this._location.back();
  }
}
