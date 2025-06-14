import { Component } from '@angular/core';
import { Menubar } from 'primeng/menubar';
import { MenuItem, PrimeTemplate } from 'primeng/api';
import { Button } from 'primeng/button';
import { RouterLink } from '@angular/router';

@Component({
  selector: 'matcha-navbar',
  imports: [
    Menubar,
    Button,
    PrimeTemplate,
    RouterLink,
  ],
  templateUrl: './nav-bar.component.html',
  styleUrl: './nav-bar.component.css'
})
export class NavBarComponent {
  menuItems: MenuItem[] = [
    { label: 'Edit Profile', icon: 'pi pi-user', routerLink: '/profile' },
    { label: 'Chat', icon: 'pi pi-comments', routerLink: '/chat' },
    { label: 'Settings', icon: 'pi pi-pencil', routerLink: '/settings' },
  ];

  logout() {
    console.log('Logged Out');
  }

}
