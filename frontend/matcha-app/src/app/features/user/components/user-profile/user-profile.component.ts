import { ChangeDetectionStrategy, Component, OnInit, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { AccordionModule } from 'primeng/accordion';
import { UserProfileService } from '../../services/user-profile.service';
import { AuthorizationService } from '@core/auth/services/authorization.service';
import { User } from '../../models/user.model';
import { UserMainInfoEditComponent } from '../user-main-info-edit/user-main-info-edit.component';
import { UserAdditionalInfoEditComponent } from '../user-additional-info-edit/user-additional-info-edit.component';
import { UserPasswordEditComponent } from '../user-password-edit/user-password-edit.component';

@Component({
  selector: 'matcha-user-profile',
  imports: [
    CommonModule,
    AccordionModule,
    UserMainInfoEditComponent,
    UserAdditionalInfoEditComponent,
    UserPasswordEditComponent
  ],
  templateUrl: './user-profile.component.html',
  styleUrl: './user-profile.component.css',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class UserProfileComponent implements OnInit {
  private userProfileService = inject(UserProfileService);
  private authService = inject(AuthorizationService);

  loading = signal(false);
  error = signal<string | null>(null);
  user = signal<User | null>(null);
  userId = signal<number | null>(null);

  ngOnInit(): void {
    this.loadCurrentUser();
  }

  private loadCurrentUser(): void {
    this.loading.set(true);
    this.error.set(null);

    // Get current user ID from authentication service
    // For now, we'll use a mock user ID since the auth service might not be fully implemented
    const currentUserId = 1; // This should come from authService.getCurrentUserId()
    
    this.userId.set(currentUserId);
    this.loadUser(currentUserId);
  }

  private loadUser(id: number): void {
    this.userProfileService.getById(id).subscribe({
      next: (user: User) => {
        this.user.set(user);
        this.loading.set(false);
      },
      error: (err: unknown) => {
        console.error('Failed to load user', err);
        this.error.set('Failed to load user profile.');
        this.loading.set(false);
      }
    });
  }

  onUserUpdated(updatedUser: User): void {
    this.user.set(updatedUser);
  }
}
