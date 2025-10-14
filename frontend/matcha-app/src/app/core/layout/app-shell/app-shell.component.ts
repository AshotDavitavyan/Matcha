import { Component } from '@angular/core';
import { RouterOutlet } from '@angular/router';
import { NavBarComponent } from '@core/layout/nav-bar/nav-bar.component';
import { Toast } from 'primeng/toast';

@Component({
  selector: 'matcha-app-shell',
  imports: [
    RouterOutlet,
    NavBarComponent,
    Toast
  ],
  templateUrl: './app-shell.component.html',
  styleUrl: './app-shell.component.css'
})
export class AppShellComponent {

}
