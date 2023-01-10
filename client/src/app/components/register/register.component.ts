import { Component } from '@angular/core';
import { Employee } from 'src/app/models/employee';
import { EmployeeService } from 'src/app/services/employee.service';
import { Location } from '@angular/common';

@Component({
  selector: 'app-register',
  templateUrl: './register.component.html',
  styleUrls: ['./register.component.css']
})
export class RegisterComponent {
/**
 * TODO Send a registration request to ERS api
 * Create registration form
 * Get input from the form to instantiate employee to be sent in req
 * Register an employee from the frontend
 */
  constructor(
    private _employeeService: EmployeeService,
    private _location: Location
    ) { }
  
  registerEmployee(emailInput: string, passwordInput: string): void {
    var newEmployee: Employee = {
      id: -1,
      email: emailInput,
      password: passwordInput,
      roleID: 0
    };

    this._employeeService.register(newEmployee)
      .subscribe(result => console.log(result));
  }

  goBack(): void {
    this._location.back();
  }
}
