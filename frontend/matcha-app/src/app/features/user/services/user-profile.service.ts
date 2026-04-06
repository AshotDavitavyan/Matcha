import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable, of, delay, throwError } from 'rxjs';
import { environment } from '../../../../environments/environment';
import { User, UserPhoto } from '../models/user.model';
import { UpdateUserModel } from '../models/update-user.model';
import { UpdateAdditionalInfoModel } from '../models/update-additional-info.model';
import { ChangePasswordModel } from '../models/change-password.model';

@Injectable({ providedIn: 'root' })
export class UserProfileService {
  private http = inject(HttpClient);
  private apiUrl = environment.apiUrl;

  // Mock data storage
  private mockUsers: User[] = [
    {
      id: 1,
      username: 'john_doe',
      firstName: 'John',
      lastName: 'Doe',
      email: 'john@example.com',
      avatarUrl: 'https://via.placeholder.com/150',
      bio: 'Love hiking and photography',
      age: 28,
      location: 'New York',
      interests: ['Photography', 'Hiking', 'Travel'],
      datingPreferences: ['Long-term relationship', 'Adventure'],
      photos: [
        { id: 1, url: 'https://via.placeholder.com/300x300', isMain: true, order: 1 },
        { id: 2, url: 'https://via.placeholder.com/300x300', isMain: false, order: 2 }
      ]
    }
  ];

  // Get user by id
  getById(id: number): Observable<User> {
    return of(this.mockUsers.find(u => u.id === id)!).pipe(delay(500));
  }

  // Update main info (username, firstName, lastName, email)
  updateMainInfo(userId: number, data: UpdateUserModel): Observable<User> {
    return new Observable(observer => {
      setTimeout(() => {
        const userIndex = this.mockUsers.findIndex(u => u.id === userId);
        if (userIndex === -1) {
          observer.error('User not found');
          return;
        }

        this.mockUsers[userIndex] = {
          ...this.mockUsers[userIndex],
          username: data.username,
          firstName: data.firstName,
          lastName: data.lastName,
          email: data.email
        };

        observer.next(this.mockUsers[userIndex]);
        observer.complete();
      }, 800);
    });
  }

  // Update additional info (bio, age, location, interests, dating preferences)
  updateAdditionalInfo(userId: number, data: UpdateAdditionalInfoModel): Observable<User> {
    return new Observable(observer => {
      setTimeout(() => {
        const userIndex = this.mockUsers.findIndex(u => u.id === userId);
        if (userIndex === -1) {
          observer.error('User not found');
          return;
        }

        this.mockUsers[userIndex] = {
          ...this.mockUsers[userIndex],
          bio: data.bio,
          age: data.age,
          location: data.location,
          interests: data.interests,
          datingPreferences: data.datingPreferences
        };

        observer.next(this.mockUsers[userIndex]);
        observer.complete();
      }, 800);
    });
  }

  // Change password
  changePassword(userId: number, data: ChangePasswordModel): Observable<void> {
    return new Observable(observer => {
      setTimeout(() => {
        // Mock password validation
        if (data.currentPassword !== 'current123') {
          observer.error('Current password is incorrect');
          return;
        }

        if (data.newPassword !== data.confirmPassword) {
          observer.error('New passwords do not match');
          return;
        }

        if (data.newPassword.length < 8) {
          observer.error('Password must be at least 8 characters long');
          return;
        }

        if (!/[!@#$%^&*(),.?":{}|<>]/.test(data.newPassword)) {
          observer.error('Password must contain at least one special character');
          return;
        }

        observer.next();
        observer.complete();
      }, 1000);
    });
  }

  // Upload photo
  uploadPhoto(userId: number, file: File): Observable<UserPhoto> {
    return new Observable(observer => {
      setTimeout(() => {
        const userIndex = this.mockUsers.findIndex(u => u.id === userId);
        if (userIndex === -1) {
          observer.error('User not found');
          return;
        }

        const newPhoto: UserPhoto = {
          id: Date.now(),
          url: URL.createObjectURL(file),
          isMain: false,
          order: (this.mockUsers[userIndex].photos?.length || 0) + 1
        };

        if (!this.mockUsers[userIndex].photos) {
          this.mockUsers[userIndex].photos = [];
        }

        this.mockUsers[userIndex].photos!.push(newPhoto);
        observer.next(newPhoto);
        observer.complete();
      }, 1200);
    });
  }

  // Delete photo
  deletePhoto(userId: number, photoId: number): Observable<void> {
    return new Observable(observer => {
      setTimeout(() => {
        const userIndex = this.mockUsers.findIndex(u => u.id === userId);
        if (userIndex === -1) {
          observer.error('User not found');
          return;
        }

        const photoIndex = this.mockUsers[userIndex].photos?.findIndex(p => p.id === photoId);
        if (photoIndex === -1 || photoIndex === undefined) {
          observer.error('Photo not found');
          return;
        }

        this.mockUsers[userIndex].photos!.splice(photoIndex, 1);
        observer.next();
        observer.complete();
      }, 600);
    });
  }

  // Set main photo
  setMainPhoto(userId: number, photoId: number): Observable<void> {
    return new Observable(observer => {
      setTimeout(() => {
        const userIndex = this.mockUsers.findIndex(u => u.id === userId);
        if (userIndex === -1) {
          observer.error('User not found');
          return;
        }

        const photos = this.mockUsers[userIndex].photos;
        if (!photos) {
          observer.error('No photos found');
          return;
        }

        // Reset all photos to not main
        photos.forEach(photo => photo.isMain = false);
        
        // Set the selected photo as main
        const targetPhoto = photos.find(p => p.id === photoId);
        if (targetPhoto) {
          targetPhoto.isMain = true;
          this.mockUsers[userIndex].avatarUrl = targetPhoto.url;
        }

        observer.next();
        observer.complete();
      }, 600);
    });
  }
}
