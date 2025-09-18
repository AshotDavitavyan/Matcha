import { Component, inject } from '@angular/core';
import { FormBuilder, FormGroup, Validators, ReactiveFormsModule } from '@angular/forms';
import { PasswordModule } from 'primeng/password';
import { MessageModule } from 'primeng/message';
import { Button } from 'primeng/button';
import { InputText } from 'primeng/inputtext';
import { RouterLink } from '@angular/router';

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
    fb: FormBuilder = inject(FormBuilder);
    loginForm: FormGroup = this.fb.group({
        username: ['', Validators.required],
        password: ['', [Validators.required, Validators.minLength(6)]],
    });

    onSubmit(): void {
        if (this.loginForm.valid) {
            console.log('Form value', this.loginForm.value);
            // Handle successful form submission (e.g., call an authentication service)
        } else {
            this.loginForm.markAllAsTouched();
        }
    }
}
