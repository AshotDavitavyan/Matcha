import { inject, Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { RegistrationModel } from '@core/auth/models/registrationModel';
import { Observable } from 'rxjs';
import { LoginModel } from '@core/auth/models/loginModel';
import { environment } from '../../../../environments/environment';

@Injectable({
    providedIn: 'root'
})
export class AuthProvider {
    private client: HttpClient = inject(HttpClient);
    private apiUrl = environment.apiUrl;

    login(model: LoginModel): Observable<boolean> {
        console.log('AuthProvider login called with', model);
        return this.client.post<boolean>(`${this.apiUrl}/api/login`, model);
    }

    register(model: RegistrationModel): Observable<number> {
        console.log('AuthProvider register called with', model);
        return this.client.post<number>(`${this.apiUrl}/Users`, model);
    }
}
