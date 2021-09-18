import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { UserModel } from './model/user/user-model';

@Injectable({
  providedIn: 'root'
})
export class ApiService {

  constructor(private http:HttpClient) {}

  signIn(bodyData){
    const url = 'https://localhost:44331/api/Users/Login';
    return this.http.post<UserModel>(url, bodyData)
    .toPromise();
  }

  
}
