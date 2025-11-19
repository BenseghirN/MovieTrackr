import { Component, signal } from '@angular/core';
import { RouterLink, RouterLinkActive } from '@angular/router';
import { AvatarModule } from 'primeng/avatar';
import { ButtonModule } from 'primeng/button';
import { MenubarModule } from 'primeng/menubar';

@Component({
  selector: 'app-header',
  standalone: true,
  imports: [RouterLink, RouterLinkActive, ButtonModule, MenubarModule, AvatarModule],
  templateUrl: './header.html',
  styleUrl: './header.scss',
})
export class Header {
  protected readonly isLoggedIn = signal(false);

  login(): void {
    const returnUrl = encodeURIComponent(window.location.pathname + window.location.search);
    window.location.href = `/api/v1/connect?returnUrl=${returnUrl}`;
  }

  logout(): void {
    const returnUrl = encodeURIComponent(window.location.pathname + window.location.search);
    window.location.href = `/api/v1/connect?returnUrl=${returnUrl}`;
  }
}
