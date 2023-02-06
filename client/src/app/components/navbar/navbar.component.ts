import { Component } from '@angular/core';
import { Router } from '@angular/router';
import { CookieService } from 'ngx-cookie-service';
import { EmployeeService } from 'src/app/services/employee.service';


@Component({
  selector: 'app-navbar',
  templateUrl: './navbar.component.html',
  styleUrls: ['./navbar.component.css']
})
export class NavbarComponent {
  isLoggedIn: boolean;

  constructor(
    private _cookieService: CookieService,
    private _employeeService: EmployeeService,
    private _router: Router,
  ) { 
    this.isLoggedIn = false;
  }
  
  /** 
   * TODO Put in helper class or something
   * When we have a cookie... set isLoggedIn to true...
   */
  // 
  loginStatus() : boolean {
    if(this._cookieService.get('AuthCookie')) {
      this.isLoggedIn = true;
    } else {
      this.isLoggedIn = false;
    }
    return this.isLoggedIn;
  }

  logoutWithSession() {
    var cookie: string = this._cookieService.get('AuthCookie');
    this._employeeService.logout(cookie).subscribe(result => {
      console.log(result);
      if(result != null) {
        this._router.navigate(['/']);
      }
    });
  }
}