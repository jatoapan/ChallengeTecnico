import { Component, signal } from '@angular/core';
import { RouterOutlet } from '@angular/router';
import { Toasts } from './shared/toasts/toasts';

@Component({
  imports: [RouterOutlet, Toasts],
  selector: 'app-root',
  styleUrl: './app.scss',
  templateUrl: './app.html',
})
export class App {
  protected readonly title = signal('frontend');
}
