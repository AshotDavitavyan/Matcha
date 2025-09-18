import { inject, Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { registrationModel } from '@core/auth/models/registration.model';
import { Observable } from 'rxjs';
import { loginModel } from '@core/auth/models/Login.model';

@Injectable({
    providedIn: 'root'
})
export class AuthProvider {
    private client: HttpClient = inject(HttpClient);


    login(model: loginModel): Observable<boolean> {
        return this.client.post<boolean>('/api/login', model);
    }

    register(model: registrationModel): Observable<boolean> {
        return this.client.post<boolean>('/api/register', model);
    }
}
