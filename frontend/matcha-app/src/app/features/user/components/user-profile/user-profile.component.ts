import { ChangeDetectionStrategy, Component, OnInit, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ActivatedRoute } from '@angular/router';
import { UsersProvider } from '../../services/users.provider';
import { User } from '../../models/user.model';

@Component({
  selector: 'matcha-user-profile',
  imports: [CommonModule],
  templateUrl: './user-profile.component.html',
  styleUrl: './user-profile.component.css',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class UserProfileComponent implements OnInit {
  private route = inject(ActivatedRoute);
  private usersProvider = inject(UsersProvider);

  loading = signal(false);
  error = signal<string | null>(null);
  user = signal<User | null>(null);

  userId = signal<number | null>(null);

  ngOnInit(): void {
    const idFromPath = this.route.snapshot.paramMap.get('id');
    const idFromQuery = this.route.snapshot.queryParamMap.get('id');
    const id = idFromPath ?? idFromQuery;
    if (!id) {
      this.error.set('User id not provided.');
      return;
    }
    const parsed = Number(id);
    this.userId.set(Number.isNaN(parsed) ? null : parsed);
    this.fetchUser(id);
  }

  private fetchUser(id: string): void {
    this.loading.set(true);
    this.error.set(null);
    this.usersProvider.getById(id).subscribe({
      next: (u: User) => {
        this.user.set(u);
        this.loading.set(false);
      },
      error: (err: unknown) => {
        console.error('Failed to load user', err);
        this.error.set('Failed to load user.');
        this.loading.set(false);
      }
    });
  }
}
