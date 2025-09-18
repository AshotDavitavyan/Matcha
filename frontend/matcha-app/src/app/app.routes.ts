import { Routes } from '@angular/router';
import { UsersListComponent } from './features/user/components/users-list/users-list.component';
import { ChatComponent } from './features/chat/components/chat/chat.component';
import { UserSettingsComponent } from './features/settings/components/user-settings/user-settings.component';
import { PasswordEditComponent } from './features/settings/components/password-edit/password-edit.component';
import { UserProfileComponent } from './features/user/components/user-profile/user-profile.component';
import { BlackListComponent } from './features/settings/components/black-list/black-list.component';
import { AppShellComponent } from '@core/layout/app-shell/app-shell.component';
import { UserEditComponent } from './features/user/components/user-edit/user-edit.component';
import { LoginComponent } from '@core/auth/components/login/login.component';
import { RegistrationComponent } from '@core/auth/components/registration/registration.component';

export const routes: Routes = [
    { path: 'auth', children: [
            {path: 'login', component: LoginComponent},
            {path: 'register', component: RegistrationComponent},
        ] },
    {
        path: '',
        component: AppShellComponent,
        children: [
            {path: '', redirectTo: 'users', pathMatch: 'full'},
            {path: 'users', component: UsersListComponent},
            {path: 'users/:id', component: UserProfileComponent},
            {path:'chat', component: ChatComponent},
            {path: 'settings',
                component: UserSettingsComponent,
                children: [
                    { path: '', redirectTo: 'password', pathMatch: 'full' },
                    { path: 'password', component: PasswordEditComponent },
                    { path: 'black-list', component: BlackListComponent },
                ]},
            {path: 'profile', component: UserEditComponent},
        ]
    },
    { path: '**', redirectTo: 'auth/login' }
];
