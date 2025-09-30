import { ChangeDetectionStrategy, Component, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ReactiveFormsModule, FormBuilder, FormGroup, Validators, FormControl } from '@angular/forms';
import { InputText } from 'primeng/inputtext';
import { Button } from 'primeng/button';
import { UsersProvider } from '../../services/users.provider';
import { User } from '../../models/user.model';
import { UpdateUserModel } from '../../models/update-user.model';

@Component({
  selector: 'matcha-user-edit',
  imports: [CommonModule, ReactiveFormsModule, InputText, Button],
  templateUrl: './user-edit.component.html',
  styleUrl: './user-edit.component.css',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class UserEditComponent {
  private fb = inject(FormBuilder);
  private usersProvider = inject(UsersProvider);

  // Temporary control to mock reading the user id from cookies/session
  userIdControl = new FormControl<number | null>(null, { nonNullable: false, validators: [Validators.required] });

  // Edit form: username, firstName, lastName, email
  form: FormGroup = this.fb.group({
    username: ['', Validators.required],
    firstName: ['', Validators.required],
    lastName: ['', Validators.required],
    email: ['', [Validators.required, Validators.email]],
  });

  loading = false;
  saving = false;
  error: string | null = null;
  success: string | null = null;

  // Load user by id and populate the form
  loadUser(): void {
    this.error = null;
    this.success = null;
    const id = this.userIdControl.value;
    if (id == null) {
      this.error = 'Please enter a valid user id.';
      return;
    }
    this.loading = true;
    this.usersProvider.getById(id).subscribe({
      next: (user: User) => {
        this.form.reset({
          username: user.username ?? '',
          firstName: user.firstName ?? '',
          lastName: user.lastName ?? '',
          email: user.email ?? '',
        });
        this.loading = false;
      },
      error: (err: unknown) => {
        console.error('Failed to load user', err);
        this.error = 'Failed to load user.';
        this.loading = false;
      }
    });
  }

  // Submit updated data
  onSubmit(): void {
    this.error = null;
    this.success = null;
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }
    const id = this.userIdControl.value;
    if (id == null) {
      this.error = 'User id is required to update profile.';
      return;
    }

    const payload: UpdateUserModel = this.form.getRawValue() as UpdateUserModel;

    this.saving = true;
    this.usersProvider.update(id, payload).subscribe({
      next: (updated: User) => {
        this.success = 'Profile updated successfully.';
        this.saving = false;
        // Optionally sync form with server response
        this.form.patchValue({
          username: updated.username ?? payload.username,
          firstName: updated.firstName ?? payload.firstName,
          lastName: updated.lastName ?? payload.lastName,
          email: updated.email ?? payload.email,
        });
      },
      error: (err: unknown) => {
        console.error('Failed to update user', err);
        this.error = 'Failed to update user.';
        this.saving = false;
      }
    });
  }
}
