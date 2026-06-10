import { Component, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { HttpClient } from '@angular/common/http';
import { Router } from '@angular/router';

@Component({
  selector: 'app-dashboard',
  standalone: true,
  imports: [CommonModule],
  template: `
    <div class="dashboard-wrapper" style="padding: 3rem; font-family: sans-serif; max-width: 600px; margin: 0 auto;">
      <div style="display: flex; justify-content: space-between; align-items: center; margin-bottom: 2rem;">
        <h2>FinSight Control Room Dashboard</h2>
        <button (click)="onLogout()" style="padding: 0.5rem 1rem; background: #ef4444; color: white; border: none; border-radius: 4px; cursor: pointer;">Logout</button>
      </div>
      
      <p style="color: #64748b; margin-bottom: 2rem;">Trigger the endpoints below to test your backend token authorization claims policy rules engine.</p>

      <!-- Status Logs Output Panel -->
      <div *ngIf="logMessage" [style.background-color]="isSuccess ? '#f0fdf4' : '#fef2f2'" 
           [style.color]="isSuccess ? '#166534' : '#991b1b'"
           [style.border-color]="isSuccess ? '#bbf7d0' : '#fecaca'"
           style="padding: 1rem; border: 1px solid; border-radius: 8px; margin-bottom: 2rem; font-weight: 600;">
        {{ logMessage }}
      </div>

      <div style="display: flex; gap: 1rem;">
        <!-- Button 1: Will trigger a 403 Forbidden failure because Admin is 26, not 30+ -->
        <button (click)="triggerAgeRestrictionCheck()" 
                style="flex: 1; padding: 1rem; background: #0f172a; color: white; font-weight: 600; border: none; border-radius: 6px; cursor: pointer;">
          Execute Age Verification (Fails)
        </button>

        <!-- Button 2: Will return 200 OK successfully -->
        <button (click)="triggerStandardAuthorizationCheck()" 
                style="flex: 1; padding: 1rem; background: #2563eb; color: white; font-weight: 600; border: none; border-radius: 6px; cursor: pointer;">
          Execute Standard Access (Succeeds)
        </button>
      </div>
    </div>
  `
})
export class DashboardComponent {
  logMessage = '';
  isSuccess = true;

  private http = inject(HttpClient);
  private router = inject(Router);

  triggerAgeRestrictionCheck() {
    this.logMessage = '';
    
    // Calls the endpoint guarded by [Authorize(Policy = "Above30Policy")]
    this.http.post<any>('https://localhost:7156/api/identity/test-restricted-age', {})
      .subscribe({
        next: (res) => {
          this.isSuccess = true;
          this.logMessage = res.message;
        },
        error: (err) => {
          this.isSuccess = false;
          // Catches the 403 Forbidden action natively
          this.logMessage = `[HTTP ${err.status} Forbidden] Authorization Claim Failed: Your account does not meet the Above30Policy ruleset parameters.`;
        }
      });
  }

  triggerStandardAuthorizationCheck() {
    this.logMessage = '';

    // Calls the endpoint guarded by [Authorize(Policy = "Above21Policy")]
    this.http.post<any>('https://localhost:7156/api/identity/test-standard-access', {})
      .subscribe({
        next: (res) => {
          this.isSuccess = true;
          this.logMessage = res.message;
        },
        error: (err) => {
          this.isSuccess = false;
          this.logMessage = `Error: ${err.message}`;
        }
      });
  }

  onLogout() {
    sessionStorage.clear();
    this.router.navigate(['/']);
  }
}
