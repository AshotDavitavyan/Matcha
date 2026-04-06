import { Injectable, inject, signal, computed } from '@angular/core';
import { AuthProvider } from './auth.provider';
import { LoginModel } from '../models/loginModel';
import { RegistrationModel } from '../models/registrationModel';
import { Observable, tap, catchError, of } from 'rxjs';

export interface AuthState {
  isAuthenticated: boolean;
  user: UserInfo | null;
  token: string | null;
  isLoading: boolean;
}

export interface UserInfo {
  id: number;
  username: string;
  email?: string;
  profileComplete?: boolean;
}

@Injectable({
  providedIn: 'root'
})
export class AuthorizationService {
  private authProvider = inject(AuthProvider);

  // Private signals for internal state management
  private _isAuthenticated = signal<boolean>(false);
  private _user = signal<UserInfo | null>(null);
  private _token = signal<string | null>(null);
  private _isLoading = signal<boolean>(false);

  // Public computed signals for components to use
  public readonly isAuthenticated = this._isAuthenticated.asReadonly();
  public readonly user = this._user.asReadonly();
  public readonly token = this._token.asReadonly();
  public readonly isLoading = this._isLoading.asReadonly();

  // Computed signal that combines all auth state
  public readonly authState = computed<AuthState>(() => ({
    isAuthenticated: this._isAuthenticated(),
    user: this._user(),
    token: this._token(),
    isLoading: this._isLoading()
  }));

  constructor() {
    // Initialize from localStorage if available
    this.initializeFromStorage();
  }

  /**
   * Attempts to login with the provided credentials
   * @param loginData - The login credentials
   * @returns Observable<boolean> - Success status
   */
  login(loginData: LoginModel): Observable<boolean> {
    this._isLoading.set(true);

    return this.authProvider.login(loginData).pipe(
      tap((success: boolean) => {
        if (success) {
          // Create user info from login data (you might want to get this from the API response)
          const userInfo: UserInfo = {
            id: 0, // This should come from the API response
            username: loginData.username,
            profileComplete: false // This should come from the API response
          };

          // Generate a mock token (replace with actual token from API)
          const mockToken = this.generateMockToken(loginData.username);

          this._isAuthenticated.set(true);
          this._user.set(userInfo);
          this._token.set(mockToken);

          // Store in localStorage for persistence
          this.saveToStorage(userInfo, mockToken);
        }
        this._isLoading.set(false);
      }),
      catchError((error) => {
        this._isLoading.set(false);
        return of(false);
      })
    );
  }

  /**
   * Attempts to register a new user
   * @param registrationData - The registration data
   * @returns Observable<number> - User ID on success
   */
  register(registrationData: RegistrationModel): Observable<number> {
    this._isLoading.set(true);

    return this.authProvider.register(registrationData).pipe(
      tap((userId: number) => {
        // You might want to automatically log in after successful registration
        // For now, we'll just clear the loading state
        this._isLoading.set(false);
      }),
      catchError((error) => {
        this._isLoading.set(false);
        return of(0);
      })
    );
  }

  /**
   * Logs out the current user
   */
  logout(): void {
    this._isAuthenticated.set(false);
    this._user.set(null);
    this._token.set(null);
    
    // Clear from localStorage
    this.clearStorage();
  }


  /**
   * Updates user information
   * @param userInfo - Updated user information
   */
  updateUser(userInfo: Partial<UserInfo>): void {
    const currentUser = this._user();
    const token = this._token();
    if (currentUser && token) {
      const updatedUser = { ...currentUser, ...userInfo };
      this._user.set(updatedUser);
      this.saveToStorage(updatedUser, token);
    }
  }

  /**
   * Checks if the current token is valid (you can implement actual token validation here)
   */
  isTokenValid(): boolean {
    const token = this._token();
    if (!token) return false;
    
    // Add your token validation logic here
    // For now, we'll just check if token exists and is not expired
    try {
      const payload = JSON.parse(atob(token.split('.')[1]));
      return payload.exp > Date.now() / 1000;
    } catch {
      return false;
    }
  }

  /**
   * Refreshes the authentication token
   */
  refreshToken(): Observable<boolean> {
    // Implement token refresh logic here
    // For now, return a mock implementation
    return of(true);
  }

  // Private helper methods
  private initializeFromStorage(): void {
    try {
      const storedUser = localStorage.getItem('matcha_user');
      const storedToken = localStorage.getItem('matcha_token');
      
      if (storedUser && storedToken) {
        const userInfo = JSON.parse(storedUser);
        this._user.set(userInfo);
        this._token.set(storedToken);
        this._isAuthenticated.set(this.isTokenValid());
      }
    } catch (error) {
      console.error('Failed to initialize from storage:', error);
      this.clearStorage();
    }
  }

  private saveToStorage(userInfo: UserInfo, token: string): void {
    try {
      localStorage.setItem('matcha_user', JSON.stringify(userInfo));
      localStorage.setItem('matcha_token', token);
    } catch (error) {
      console.error('Failed to save to storage:', error);
    }
  }

  private clearStorage(): void {
    localStorage.removeItem('matcha_user');
    localStorage.removeItem('matcha_token');
  }

  private generateMockToken(username: string): string {
    // This is a mock implementation - replace with actual JWT generation
    const header = btoa(JSON.stringify({ alg: 'HS256', typ: 'JWT' }));
    const payload = btoa(JSON.stringify({
      sub: username,
      iat: Math.floor(Date.now() / 1000),
      exp: Math.floor(Date.now() / 1000) + (24 * 60 * 60) // 24 hours
    }));
    const signature = btoa('mock-signature');
    
    return `${header}.${payload}.${signature}`;
  }
}
