import { CommonModule } from '@angular/common';
import { Component, effect, inject, OnInit, signal } from '@angular/core';
import { Router, RouterLink, RouterLinkActive } from '@angular/router';
import { AvatarModule } from 'primeng/avatar';
import { ButtonModule } from 'primeng/button';
import { MenubarModule } from 'primeng/menubar';
import { AuthService } from '../../../../core/services/auth.service';
import { TooltipModule } from 'primeng/tooltip';

@Component({
  selector: 'app-header',
  standalone: true,
  imports: [RouterLink, RouterLinkActive, ButtonModule, MenubarModule, AvatarModule, CommonModule, TooltipModule],
  templateUrl: './header.html',
  styleUrl: './header.scss',
})
export class Header implements OnInit {
  readonly authService = inject(AuthService);
  readonly router = inject(Router);

  readonly mobileMenuOpen = signal(false);

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

  onAvatarClick(): void {
    const currentUserId = this.authService.currentUser()?.id;
    if (!currentUserId) return;

    this.router.navigate(['/me']);
  }

  toggleMobileMenu(): void {
    this.mobileMenuOpen.update(v => !v);
  }

  closeMobileMenu(): void {
    this.mobileMenuOpen.set(false);
  }

  login(): void {    
    this.authService.login(window.location.pathname);
  }

  logout(): void {
    this.authService.logout(window.location.pathname);
  }
}
