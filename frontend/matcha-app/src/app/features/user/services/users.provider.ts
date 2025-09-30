import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../../environments/environment';
import { User } from '../models/user.model';
import { UpdateUserModel } from 'app/features/user/models/update-user.model';

@Injectable({ providedIn: 'root' })
export class UsersProvider {
  private http = inject(HttpClient);
  private apiUrl = environment.apiUrl;

  // Get all users
  getAll(): Observable<User[]> {
    return this.http.get<User[]>(`${this.apiUrl}/Users`);
  }

  // Get user by id
  getById(id: number | string): Observable<User> {
    return this.http.get<User>(`${this.apiUrl}/Users/${id}`);
  }

  // Update user by id (username, firstName, lastName)
  update(id: number | string, payload: UpdateUserModel): Observable<User> {
    return this.http.put<User>(`${this.apiUrl}/Users/${id}`, payload);
  }
}
