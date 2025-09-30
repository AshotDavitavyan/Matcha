import { Component, inject } from '@angular/core';
import { FormBuilder, FormGroup, Validators, ReactiveFormsModule } from '@angular/forms';
import { PasswordModule } from 'primeng/password';
import { MessageModule } from 'primeng/message';
import { Button } from 'primeng/button';
import { InputText } from 'primeng/inputtext';
import { RouterLink } from '@angular/router';
import { AuthProvider } from '@core/auth/services/auth.provider';
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
    private authProvider = inject(AuthProvider);
    fb: FormBuilder = inject(FormBuilder);
    loginForm: FormGroup = this.fb.group({
        username: ['', Validators.required],
        password: ['', [Validators.required, Validators.minLength(6)]],
    });

    onSubmit(): void {
        if (this.loginForm.valid) {
            const loginData: LoginModel = this.loginForm.getRawValue();
            this.authProvider.login(loginData).subscribe();
        } else {
            this.loginForm.markAllAsTouched();
        }
    }
}
