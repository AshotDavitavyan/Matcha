import { Component, inject } from '@angular/core';
import { Button } from 'primeng/button';
import { FormBuilder, FormGroup, FormsModule, ReactiveFormsModule, Validators } from '@angular/forms';
import { InputText } from 'primeng/inputtext';
import { Password } from 'primeng/password';
import { RouterLink, Router } from '@angular/router';
import { AuthorizationService } from '@core/auth/services/authorization.service';
import { RegistrationModel } from '@core/auth/models/registrationModel';

@Component({
    selector: 'matcha-registration',
    imports: [
        Button,
        FormsModule,
        InputText,
        Password,
        ReactiveFormsModule,
        RouterLink
    ],
    templateUrl: './registration.component.html',
    styleUrl: './registration.component.css'
})
export class RegistrationComponent {
    private authService = inject(AuthorizationService);
    private router = inject(Router);
    fb: FormBuilder = inject(FormBuilder);
    
    registrationForm: FormGroup = this.fb.group({
        username: ['', Validators.required],
        firstName: ['', Validators.required],
        lastName: ['', Validators.required],
        email: ['', [Validators.required, Validators.email]],
        password: ['', [Validators.required, Validators.minLength(6)]],
    });

    // Access to auth state signals
    readonly isLoading = this.authService.isLoading;

    onSubmit() {
        if (this.registrationForm.valid) {
            const registrationData: RegistrationModel = this.registrationForm.getRawValue();
            
            this.authService.register(registrationData).subscribe({
                next: (userId: number) => {
                    if (userId > 0) {
                        // Navigate to login page or auto-login after successful registration
                        this.router.navigate(['/auth/login']);
                    }
                }
                // Error handling is now done by the HTTP interceptor
            });
        } else {
            this.registrationForm.markAllAsTouched();
        }
    }

}
