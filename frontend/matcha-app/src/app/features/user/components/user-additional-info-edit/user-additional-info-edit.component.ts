import { ChangeDetectionStrategy, Component, EventEmitter, Input, OnInit, Output, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ReactiveFormsModule, FormBuilder, FormGroup, Validators, FormArray, FormControl } from '@angular/forms';
import { InputText } from 'primeng/inputtext';
import { InputNumber } from 'primeng/inputnumber';
import { Button } from 'primeng/button';
import { Message } from 'primeng/message';
import { FileUpload } from 'primeng/fileupload';
import { UserProfileService } from '../../services/user-profile.service';
import { User, UserPhoto } from '../../models/user.model';
import { UpdateAdditionalInfoModel } from '../../models/update-additional-info.model';

@Component({
  selector: 'matcha-user-additional-info-edit',
  imports: [CommonModule, ReactiveFormsModule, InputText, InputNumber, Button, Message, FileUpload],
  templateUrl: './user-additional-info-edit.component.html',
  styleUrl: './user-additional-info-edit.component.css',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class UserAdditionalInfoEditComponent implements OnInit {
  @Input({ required: true }) user!: User;
  @Input({ required: true }) userId!: number;
  @Output() userUpdated = new EventEmitter<User>();

  private fb = inject(FormBuilder);
  private userProfileService = inject(UserProfileService);

  form = this.fb.group({
    bio: ['', [Validators.maxLength(500)]],
    age: [null as number | null, [Validators.min(18), Validators.max(100)]],
    location: [''],
    interests: this.fb.array([]),
    datingPreferences: this.fb.array([])
  });

  saving = signal(false);
  uploading = signal(false);
  error = signal<string | null>(null);
  success = signal<string | null>(null);
  photos = signal<UserPhoto[]>([]);

  ngOnInit(): void {
    this.form.patchValue({
      bio: this.user.bio || '',
      age: this.user.age || null,
      location: this.user.location || ''
    });

    // Initialize interests
    const interestsArray = this.form.get('interests') as FormArray;
    interestsArray.clear();
    if (this.user.interests) {
      this.user.interests.forEach(interest => {
        interestsArray.push(new FormControl(interest));
      });
    }

    // Initialize dating preferences
    const preferencesArray = this.form.get('datingPreferences') as FormArray;
    preferencesArray.clear();
    if (this.user.datingPreferences) {
      this.user.datingPreferences.forEach(preference => {
        preferencesArray.push(new FormControl(preference));
      });
    }

    // Initialize photos
    this.photos.set(this.user.photos || []);
  }

  onSubmit(): void {
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }

    this.saving.set(true);
    this.error.set(null);
    this.success.set(null);

    const formData: UpdateAdditionalInfoModel = {
      bio: this.form.get('bio')?.value || undefined,
      age: this.form.get('age')?.value || undefined,
      location: this.form.get('location')?.value || undefined,
      interests: (this.form.get('interests') as FormArray).value.filter((item: string) => item.trim() !== ''),
      datingPreferences: (this.form.get('datingPreferences') as FormArray).value.filter((item: string) => item.trim() !== '')
    };
    
    this.userProfileService.updateAdditionalInfo(this.userId, formData).subscribe({
      next: (updatedUser) => {
        this.success.set('Additional information updated successfully!');
        this.saving.set(false);
        this.userUpdated.emit(updatedUser);
      },
      error: (err) => {
        console.error('Failed to update additional info', err);
        this.error.set('Failed to update additional information.');
        this.saving.set(false);
      }
    });
  }

  onFileSelect(event: any): void {
    const file = event.files[0];
    if (!file) return;

    // Validate file type
    const allowedTypes = ['image/jpeg', 'image/png', 'image/webp'];
    if (!allowedTypes.includes(file.type)) {
      this.error.set('Please select a valid image file (JPEG, PNG, or WebP)');
      return;
    }

    // Validate file size (5MB max)
    if (file.size > 5 * 1024 * 1024) {
      this.error.set('File size must be less than 5MB');
      return;
    }

    this.uploading.set(true);
    this.error.set(null);

    this.userProfileService.uploadPhoto(this.userId, file).subscribe({
      next: (newPhoto) => {
        this.photos.update(photos => [...photos, newPhoto]);
        this.uploading.set(false);
      },
      error: (err) => {
        console.error('Failed to upload photo', err);
        this.error.set('Failed to upload photo.');
        this.uploading.set(false);
      }
    });
  }

  deletePhoto(photoId: number): void {
    this.userProfileService.deletePhoto(this.userId, photoId).subscribe({
      next: () => {
        this.photos.update(photos => photos.filter(p => p.id !== photoId));
      },
      error: (err) => {
        console.error('Failed to delete photo', err);
        this.error.set('Failed to delete photo.');
      }
    });
  }

  setMainPhoto(photoId: number): void {
    this.userProfileService.setMainPhoto(this.userId, photoId).subscribe({
      next: () => {
        this.photos.update(photos => 
          photos.map(p => ({ ...p, isMain: p.id === photoId }))
        );
      },
      error: (err) => {
        console.error('Failed to set main photo', err);
        this.error.set('Failed to set main photo.');
      }
    });
  }

  addInterest(): void {
    const interestsArray = this.form.get('interests') as FormArray;
    interestsArray.push(new FormControl(''));
  }

  removeInterest(index: number): void {
    const interestsArray = this.form.get('interests') as FormArray;
    interestsArray.removeAt(index);
  }

  addDatingPreference(): void {
    const preferencesArray = this.form.get('datingPreferences') as FormArray;
    preferencesArray.push(new FormControl(''));
  }

  removeDatingPreference(index: number): void {
    const preferencesArray = this.form.get('datingPreferences') as FormArray;
    preferencesArray.removeAt(index);
  }

  getFieldError(fieldName: string): string | null {
    const field = this.form.get(fieldName);
    if (field?.errors && field.touched) {
      if (field.errors['maxlength']) return `${fieldName} must be less than ${field.errors['maxlength'].requiredLength} characters`;
      if (field.errors['min']) return `${fieldName} must be at least ${field.errors['min'].min}`;
      if (field.errors['max']) return `${fieldName} must be at most ${field.errors['max'].max}`;
    }
    return null;
  }
}
