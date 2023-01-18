import { Component } from '@angular/core';
import { CookieService } from 'ngx-cookie-service';
import { Observable, of } from 'rxjs';

@Component({
  selector: 'app-root',
  templateUrl: './app.component.html',
  styleUrls: ['./app.component.css']
})
export class AppComponent {
  title = 'Employee Reimbursement Frontend';
  isLoggedIn: boolean;

  constructor(
    private _cookieService: CookieService,
  ) { 
    this.isLoggedIn = false;
  }
  
  // When we have a cookie... set isLoggedIn to true...
  loginStatus() : boolean {
    if(this._cookieService.get('AuthCookie')) {
      this.isLoggedIn = true;
    } else {
      this.isLoggedIn = false;
    }
    return this.isLoggedIn;
  }
}
