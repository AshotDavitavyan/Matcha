import { Component, inject } from '@angular/core';
import { FormBuilder, FormGroup, Validators, ReactiveFormsModule } from '@angular/forms';
import { PasswordModule } from 'primeng/password';
import { MessageModule } from 'primeng/message';
import { Button } from 'primeng/button';
import { InputText } from 'primeng/inputtext';
import { RouterLink, Router } from '@angular/router';
import { AuthorizationService } from '@core/auth/services/authorization.service';
import { LoginModel } from '@core/auth/models/loginModel';

@Component({
    selector: 'matcha-login',
    standalone: true,
    imports: [
        ReactiveFormsModule,
        PasswordModule,
        MessageModule,
        InputText,
        RouterLink,
        Button
    ],
    templateUrl: './login.component.html',
    styleUrls: ['./login.component.css'],
})
export class LoginComponent {
    private authService = inject(AuthorizationService);
    private router = inject(Router);
    fb: FormBuilder = inject(FormBuilder);
    
    loginForm: FormGroup = this.fb.group({
        username: ['', Validators.required],
        password: ['', [Validators.required, Validators.minLength(6)]],
    });

    // Access to auth state signals
    readonly isLoading = this.authService.isLoading;
    readonly isAuthenticated = this.authService.isAuthenticated;

    onSubmit(): void {
        if (this.loginForm.valid) {
            const loginData: LoginModel = this.loginForm.getRawValue();
            
            this.authService.login(loginData).subscribe({
                next: (success: boolean) => {
                    if (success) {
                        // Navigate to dashboard or main app area after successful login
                        this.router.navigate(['/dashboard']); // Adjust route as needed
                    }
                }
                // Error handling is now done by the HTTP interceptor
            });
        } else {
            this.loginForm.markAllAsTouched();
        }
    }

}
