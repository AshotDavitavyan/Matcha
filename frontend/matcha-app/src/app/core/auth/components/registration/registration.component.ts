import { Component, inject } from '@angular/core';
import { Button } from 'primeng/button';
import { FormBuilder, FormGroup, FormsModule, ReactiveFormsModule, Validators } from '@angular/forms';
import { InputText } from 'primeng/inputtext';
import { Password } from 'primeng/password';
import { RouterLink } from '@angular/router';
import { AuthProvider } from '@core/auth/services/auth.provider';
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
    private authProvider = inject(AuthProvider);
    fb: FormBuilder = inject(FormBuilder);
    registrationForm: FormGroup = this.fb.group({
        username: ['', Validators.required],
        firstName: ['', Validators.required],
        lastName: ['', Validators.required],
        email: ['', Validators.required],
        password: ['', [Validators.required, Validators.minLength(6)]],
    });

    onSubmit() {
        if (this.registrationForm.valid) {
            const registrationData: RegistrationModel = this.registrationForm.getRawValue();
            this.authProvider.register(registrationData).subscribe();
        } else {
            this.registrationForm.markAllAsTouched();
        }
    }
}
