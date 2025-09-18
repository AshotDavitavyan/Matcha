import { Component, inject } from '@angular/core';
import { Button } from 'primeng/button';
import { FormBuilder, FormGroup, FormsModule, ReactiveFormsModule, Validators } from '@angular/forms';
import { InputText } from 'primeng/inputtext';
import { Password } from 'primeng/password';
import { RouterLink } from '@angular/router';

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
            console.log('Form value', this.registrationForm.value);
            // Handle successful form submission (e.g., call a registration service)
        } else {
            this.registrationForm.markAllAsTouched();
        }
    }
}
