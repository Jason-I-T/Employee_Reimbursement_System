import { Component } from '@angular/core';
import { CookieService } from 'ngx-cookie-service';


@Component({
  selector: 'app-navbar',
  templateUrl: './navbar.component.html',
  styleUrls: ['./navbar.component.css']
})
export class NavbarComponent {
  isLoggedIn: boolean;

  constructor(
    private _cookieService: CookieService,
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
}