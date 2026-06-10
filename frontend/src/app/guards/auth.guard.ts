import { inject } from '@angular/core';
import { CanActivateFn, Router } from '@angular/router';

export const authGuard: CanActivateFn = (route, state) => {
  const router = inject(Router);
  
  // 💡 Check if the backend JWT token is sitting in the browser's session memory
  const token = sessionStorage.getItem('cached_jwt');

  if (token) {
    return true; // ✅ Token exists! Allow the user to see the dashboard
  }

  // ❌ No token found! Block the URL routing change and kick them back to login
  router.navigate(['/']);
  return false;
};
