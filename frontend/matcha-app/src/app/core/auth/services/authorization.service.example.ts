// Example usage of AuthorizationService in components
// This file shows how to use the authorization service in different scenarios

import { Component, inject } from '@angular/core';
import { JsonPipe } from '@angular/common';
import { AuthorizationService } from './authorization.service';

@Component({
  selector: 'app-example',
  imports: [JsonPipe],
  template: `
    <!-- Example template showing how to use auth signals -->
    <div>
      <!-- Show loading state -->
      @if (authService.isLoading()) {
        <div>Loading...</div>
      }

      <!-- Error messages are now handled globally by the HTTP interceptor -->

      <!-- Show different content based on auth state -->
      @if (authService.isAuthenticated()) {
        <div>
          Welcome, {{ authService.user()?.username }}!
          <button (click)="logout()">Logout</button>
        </div>
      } @else {
        <div>
          <a routerLink="/auth/login">Login</a>
          <a routerLink="/auth/register">Register</a>
        </div>
      }

      <!-- Access the complete auth state -->
      <div>
        Auth State: {{ authService.authState() | json }}
      </div>
    </div>
  `
})
export class ExampleComponent {
  // Inject the authorization service
  authService = inject(AuthorizationService);

  logout() {
    this.authService.logout();
  }

  // Example of updating user information
  updateUser() {
    this.authService.updateUser({
      profileComplete: true
    });
  }

  // Example of checking token validity
  checkToken() {
    const isValid = this.authService.isTokenValid();
    console.log('Token is valid:', isValid);
  }
}

// Example of using the service in a guard
export class AuthGuard {
  authService = inject(AuthorizationService);

  canActivate(): boolean {
    return this.authService.isAuthenticated() && this.authService.isTokenValid();
  }
}

// Example of using the service in an HTTP interceptor
export class AuthInterceptor {
  authService = inject(AuthorizationService);

  intercept(req: any, next: any) {
    const token = this.authService.token();
    
    if (token) {
      req = req.clone({
        setHeaders: {
          Authorization: `Bearer ${token}`
        }
      });
    }
    
    return next.handle(req);
  }
}
