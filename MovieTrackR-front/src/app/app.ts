import { Component, inject, OnInit, signal } from '@angular/core';
import { RouterOutlet } from '@angular/router';
import { Header } from './shared/components/layout/header/header';
import { Footer } from './shared/components/layout/footer/footer';
import { ToastComponent } from './shared/components/toast/toast.component';
import { AuthService } from './core/services/auth.service';

@Component({
  selector: 'app-root',
  standalone: true,
  imports: [RouterOutlet, Header, Footer, ToastComponent],
  templateUrl: './app.html',
  styleUrl: './app.scss',
})
export class App implements OnInit {
  private readonly authService = inject(AuthService);

  ngOnInit(): void {
    this.authService.init().subscribe();
  }
}
