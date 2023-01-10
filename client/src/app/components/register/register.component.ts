import { Component } from '@angular/core';
import { Employee } from 'src/app/models/employee';
import { EmployeeService } from 'src/app/services/employee.service';
import { Location } from '@angular/common'; // Used in helper

@Component({
  selector: 'app-register',
  templateUrl: './register.component.html',
  styleUrls: ['./register.component.css']
})

export class RegisterComponent {
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

  // TODO put in a helper class
  goBack(): void {
    this._location.back();
  }
}
