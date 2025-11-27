import { CommonModule } from '@angular/common';
import { Component, effect, inject, OnInit, signal } from '@angular/core';
import { RouterLink, RouterLinkActive } from '@angular/router';
import { AvatarModule } from 'primeng/avatar';
import { ButtonModule } from 'primeng/button';
import { MenubarModule } from 'primeng/menubar';
import { AuthService } from '../../../../core/auth/auth-service';

@Component({
  selector: 'app-header',
  standalone: true,
  imports: [RouterLink, RouterLinkActive, ButtonModule, MenubarModule, AvatarModule, CommonModule],
  templateUrl: './header.html',
  styleUrl: './header.scss',
})
export class Header implements OnInit {
  readonly authService = inject(AuthService);

  constructor() {    
    effect(() => {
      if (this.authService.isAuthenticated() && !this.authService.currentUser()) {
        this.authService.getUserInfo().subscribe();
      }
    });
  }

  ngOnInit(): void {
    if (this.authService.isAuthenticated() && !this.authService.currentUser()) {
      this.authService.getUserInfo().subscribe();
    }
  }

  login(): void {
    const currentUrl =
    window.location.origin +
    window.location.pathname +
    window.location.search +
    window.location.hash;
    this.authService.login(currentUrl);
  }

  logout(): void {
    this.authService.logout('/');
  }
}
