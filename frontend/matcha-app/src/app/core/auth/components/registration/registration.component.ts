import { Component, inject } from '@angular/core';
import { Button } from 'primeng/button';
import { FormBuilder, FormGroup, FormsModule, ReactiveFormsModule, Validators } from '@angular/forms';
import { InputText } from 'primeng/inputtext';
import { Password } from 'primeng/password';
import { RouterLink } from '@angular/router';
import { AuthProvider } from '@core/auth/services/auth.provider';
import { LoginModel } from '@core/auth/models/loginModel';

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
    private authProvider = inject(AuthProvider);
    fb: FormBuilder = inject(FormBuilder);
    registrationForm: FormGroup = this.fb.group({
        username: ['', Validators.required],
        name: ['', Validators.required],
        surname: ['', Validators.required],
        email: ['', Validators.required],
        password: ['', [Validators.required, Validators.minLength(6)]],
    });

    onSubmit() {
        if (this.registrationForm.valid) {
            const registrationData: LoginModel = this.registrationForm.getRawValue();
            this.authProvider.login(registrationData).subscribe();
        } else {
            this.registrationForm.markAllAsTouched();
        }
    }
}
