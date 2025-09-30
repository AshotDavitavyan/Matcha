import { Component, inject, OnInit, ChangeDetectionStrategy, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterLink } from '@angular/router';
import { UsersProvider } from '../../services/users.provider';
import { User } from '../../models/user.model';

@Component({
  selector: 'matcha-users-list',
  imports: [CommonModule, RouterLink],
  templateUrl: './users-list.component.html',
  styleUrl: './users-list.component.css',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class UsersListComponent implements OnInit {
  private usersProvider = inject(UsersProvider);

  users = signal<User[]>([]);
  loading = signal(false);
  error = signal<string | null>(null);

  ngOnInit(): void {
    this.fetchUsers();
  }

  private fetchUsers(): void {
    this.loading.set(true);
    this.error.set(null);
    this.usersProvider.getAll().subscribe({
      next: (users: User[]) => {
        this.users.set(users ?? []);
        this.loading.set(false);
      },
      error: (err: unknown) => {
        console.error('Failed to load users', err);
        this.error.set('Failed to load users.');
        this.loading.set(false);
      }
    });
  }
}
