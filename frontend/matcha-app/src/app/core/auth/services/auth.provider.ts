import { inject, Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { RegistrationModel } from '@core/auth/models/registrationModel';
import { Observable } from 'rxjs';
import { LoginModel } from '@core/auth/models/loginModel';

@Injectable({
    providedIn: 'root'
})
export class AuthProvider {
    private client: HttpClient = inject(HttpClient);


    login(model: LoginModel): Observable<boolean> {
        console.log('AuthProvider login called with', model);
        return this.client.post<boolean>('/api/login', model);
    }

    register(model: RegistrationModel): Observable<boolean> {
        console.log('AuthProvider register called with', model);
        return this.client.post<boolean>('/api/register', model);
    }
}
