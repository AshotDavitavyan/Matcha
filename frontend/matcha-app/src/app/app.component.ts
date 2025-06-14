import { Component } from '@angular/core';
import { RouterOutlet } from '@angular/router';
import { TestComponentComponent } from '@core/test-component/test-component.component';

@Component({
  selector: 'matcha-root',
    imports: [RouterOutlet, TestComponentComponent],
  templateUrl: './app.component.html',
  styleUrl: './app.component.css'
})
export class AppComponent {
  title = 'matcha-app';
}
