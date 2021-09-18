import { Component } from '@angular/core';
import { Router } from '@angular/router';
import { ApiService } from "../api.service";

@Component({
  selector: 'app-login',
  templateUrl: './login.page.html',
  styleUrls: ['./login.page.scss'],
})
export class LoginPage {

  constructor(private apiService: ApiService, private router: Router) { }

  fnLogin(form) {
    const body = form.value;
    console.log(body);
    this.apiService.signIn(body)
    .then(res => {
      localStorage.token = res.token;
      console.log(res);
      this.router.navigate(['/']);
    })
    .catch(err => alert(err.error));
  }

}
