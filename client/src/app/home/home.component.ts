import { Component, OnInit, inject } from '@angular/core';
import { RegisterComponent } from "../register/register.component";
import { HttpClient } from '@angular/common/http';
import { AccountService } from '../_services/account.service';

@Component({
  selector: 'app-home',
  standalone: true,
  imports: [RegisterComponent],
  templateUrl: './home.component.html',
  styleUrl: './home.component.css'
})
export class HomeComponent implements OnInit{
  registerMode = false;
  http = inject(HttpClient);
  private readonly accountService = inject(AccountService);
  users: any;
  
  ngOnInit(): void {
    this.setCurrentUser();
    this.users = this.getUsers();
  }
  registerToggle(){
    this.registerMode = !this.registerMode;
  }

  cancelRegisterMode(event: boolean){
    this.registerMode = event;
  }

  getUsers(){
    this.http.get("http://localhost:5000/api/users").subscribe({
      next : (response) =>{
        this.users = response
      },
      error : (error) => {
        console.log(error);
      },
      complete: () =>{
        console.log('Request has completed');
      }
     });
  }

  setCurrentUser(){
    const userString = localStorage.getItem('user');
    if(!userString) return;
    const user = JSON.parse(userString);
    this.accountService.currentUser.set(user);
  }
}
