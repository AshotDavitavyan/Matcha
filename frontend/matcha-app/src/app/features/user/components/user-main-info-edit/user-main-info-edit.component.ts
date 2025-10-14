import { ChangeDetectionStrategy, Component, EventEmitter, Input, OnInit, Output, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ReactiveFormsModule, FormBuilder, FormGroup, Validators } from '@angular/forms';
import { InputText } from 'primeng/inputtext';
import { Button } from 'primeng/button';
import { Message } from 'primeng/message';
import { UserProfileService } from '../../services/user-profile.service';
import { User } from '../../models/user.model';
import { UpdateUserModel } from '../../models/update-user.model';

@Component({
  selector: 'matcha-user-main-info-edit',
  imports: [CommonModule, ReactiveFormsModule, InputText, Button, Message],
  templateUrl: './user-main-info-edit.component.html',
  styleUrl: './user-main-info-edit.component.css',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class UserMainInfoEditComponent implements OnInit {
  @Input({ required: true }) user!: User;
  @Input({ required: true }) userId!: number;
  @Output() userUpdated = new EventEmitter<User>();

  private fb = inject(FormBuilder);
  private userProfileService = inject(UserProfileService);

  form = this.fb.group({
    username: ['', [Validators.required, Validators.minLength(3)]],
    firstName: ['', [Validators.required]],
    lastName: ['', [Validators.required]],
    email: ['', [Validators.required, Validators.email]]
  });

  saving = signal(false);
  error = signal<string | null>(null);
  success = signal<string | null>(null);

  ngOnInit(): void {
    this.form.patchValue({
      username: this.user.username,
      firstName: this.user.firstName,
      lastName: this.user.lastName,
      email: this.user.email
    });
  }

  onSubmit(): void {
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }

    this.saving.set(true);
    this.error.set(null);
    this.success.set(null);

    const formData = this.form.getRawValue() as UpdateUserModel;
    
    this.userProfileService.updateMainInfo(this.userId, formData).subscribe({
      next: (updatedUser) => {
        this.success.set('Main information updated successfully!');
        this.saving.set(false);
        this.userUpdated.emit(updatedUser);
      },
      error: (err) => {
        console.error('Failed to update main info', err);
        this.error.set('Failed to update main information.');
        this.saving.set(false);
      }
    });
  }

  getFieldError(fieldName: string): string | null {
    const field = this.form.get(fieldName);
    if (field?.errors && field.touched) {
      if (field.errors['required']) return `${fieldName} is required`;
      if (field.errors['minlength']) return `${fieldName} must be at least ${field.errors['minlength'].requiredLength} characters`;
      if (field.errors['email']) return 'Please enter a valid email address';
    }
    return null;
  }
}
