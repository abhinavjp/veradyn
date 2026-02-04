import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common'; // Import CommonModule
import { FormsModule } from '@angular/forms'; // Import FormsModule
import { ActivatedRoute, Router } from '@angular/router';
import { HttpClient, HttpClientModule } from '@angular/common/http'; // Import HttpClientModule

@Component({
  selector: 'app-login',
  standalone: true,
  imports: [CommonModule, FormsModule, HttpClientModule], // Add imports here
  templateUrl: './login.component.html',
  styleUrls: ['./login.component.css']
})
export class LoginComponent implements OnInit {
  username = '';
  password = '';
  returnUrl = '';
  error = '';

  constructor(private route: ActivatedRoute, private router: Router, private http: HttpClient) { }

  ngOnInit(): void {
    this.returnUrl = this.route.snapshot.queryParams['returnUrl'] || '/';
  }

  login() {
    this.http.post<any>('/api/account/login', { username: this.username, password: this.password })
      .subscribe({
        next: (res) => {
          if (res.success) {
            this.router.navigateByUrl(this.returnUrl);
          } else {
            this.error = res.message || 'Login failed';
          }
        },
        error: (err) => {
          console.error('Login error:', err);
          this.error = 'Invalid credentials or server error';
        }
      });
  }
}
