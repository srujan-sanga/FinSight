// login.component.ts
import { Component, inject } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { HttpClient } from '@angular/common/http';
import { Router } from '@angular/router';

@Component({
  selector: 'app-login',
  standalone: true,
  imports: [FormsModule],
   templateUrl: './login.component.html',
  styleUrls: ['./login.component.css']
})
export class LoginComponent {
  // 💡 Data bindings matching your abstract base tracking fields
  internalSystemId: string | null = null;
  username = '';
  password = '';

  private http = inject(HttpClient);
  private router = inject(Router);

  onSubmit() {
  if (!this.username || !this.password || !this.internalSystemId) {
    alert('Please fill out all credentials including the System ID.');
    return;
  }

  // 💡 FIXED: No more Number() conversion! Pass it cleanly as a raw string text format.
  const payload = {
    internalSystemId: this.internalSystemId.trim(), // 👈 Pure string ('tenant_alpha' or '1024')
    correlationId: crypto.randomUUID(),
    requestId: crypto.randomUUID(),
    timestamp: Date.now(),
    username: this.username.trim(),
    password: this.password
  };

  this.http.post<{ token: string }>('https://localhost:7156/api/identity/login', payload)
    .subscribe({
      next: (res) => {
        sessionStorage.setItem('cached_jwt', res.token);
        this.router.navigate(['/dashboard']);
      },
      error: () => alert('Invalid credentials or tenant database connection failure.')
    });
}

  // Helper utility to instantiate clean tracking UUIDs for your base contracts
  private generateGuid(): string {
    return 'xxxxxxxx-xxxx-4xxx-yxxx-xxxxxxxxxxxx'.replace(/[xy]/g, (c) => {
      const r = (Math.random() * 16) | 0;
      const v = c === 'x' ? r : (r & 0x3) | 0x8;
      return v.toString(16);
    });
  }
}
