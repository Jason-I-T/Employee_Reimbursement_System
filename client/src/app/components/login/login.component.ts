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

    this._employeeService.login(employee)
      .subscribe(result => console.log(result));
  }

  // TODO put in a helper class
  goBack(): void { 
    this._location.back();
  }
}
