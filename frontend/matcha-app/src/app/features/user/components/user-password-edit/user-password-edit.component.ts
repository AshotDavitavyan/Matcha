import { ChangeDetectionStrategy, Component, Input, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ReactiveFormsModule, FormBuilder, FormGroup, Validators, AbstractControl } from '@angular/forms';
import { PasswordModule } from 'primeng/password';
import { Button } from 'primeng/button';
import { Message } from 'primeng/message';
import { UserProfileService } from '../../services/user-profile.service';
import { ChangePasswordModel } from '../../models/change-password.model';

@Component({
  selector: 'matcha-user-password-edit',
  imports: [CommonModule, ReactiveFormsModule, PasswordModule, Button, Message],
  templateUrl: './user-password-edit.component.html',
  styleUrl: './user-password-edit.component.css',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class UserPasswordEditComponent {
  @Input({ required: true }) userId!: number;

  private fb = inject(FormBuilder);
  private userProfileService = inject(UserProfileService);

  form = this.fb.group({
    currentPassword: ['', [Validators.required]],
    newPassword: ['', [Validators.required, Validators.minLength(8), this.specialCharacterValidator]],
    confirmPassword: ['', [Validators.required]]
  }, { validators: this.passwordMatchValidator });

  changing = signal(false);
  error = signal<string | null>(null);
  success = signal<string | null>(null);

  onSubmit(): void {
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }

    this.changing.set(true);
    this.error.set(null);
    this.success.set(null);

    const formData: ChangePasswordModel = this.form.getRawValue() as ChangePasswordModel;
    
    this.userProfileService.changePassword(this.userId, formData).subscribe({
      next: () => {
        this.success.set('Password changed successfully!');
        this.changing.set(false);
        this.form.reset();
      },
      error: (err) => {
        console.error('Failed to change password', err);
        this.error.set(err.message || 'Failed to change password.');
        this.changing.set(false);
      }
    });
  }

  private specialCharacterValidator(control: AbstractControl): { [key: string]: any } | null {
    const value = control.value;
    if (!value) return null;
    
    const hasSpecialChar = /[!@#$%^&*(),.?":{}|<>]/.test(value);
    return hasSpecialChar ? null : { specialCharacter: true };
  }

  private passwordMatchValidator(control: AbstractControl): { [key: string]: any } | null {
    const newPassword = control.get('newPassword');
    const confirmPassword = control.get('confirmPassword');
    
    if (!newPassword || !confirmPassword) return null;
    
    return newPassword.value === confirmPassword.value ? null : { passwordMismatch: true };
  }

  getFieldError(fieldName: string): string | null {
    const field = this.form.get(fieldName);
    if (field?.errors && field.touched) {
      if (field.errors['required']) return `${fieldName} is required`;
      if (field.errors['minlength']) return `${fieldName} must be at least ${field.errors['minlength'].requiredLength} characters`;
      if (field.errors['specialCharacter']) return 'Password must contain at least one special character';
    }
    return null;
  }

  getFormError(): string | null {
    if (this.form.errors?.['passwordMismatch'] && this.form.get('confirmPassword')?.touched) {
      return 'Passwords do not match';
    }
    return null;
  }
}
