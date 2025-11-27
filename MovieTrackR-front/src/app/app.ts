import { Component, inject, OnInit, Renderer2, signal } from '@angular/core';
import { RouterOutlet } from '@angular/router';
import { Header } from './shared/components/layout/header/header';
import { Footer } from './shared/components/layout/footer/footer';
import { ToastComponent } from './shared/components/toast/toast.component';

@Component({
  selector: 'app-root',
  standalone: true,
  imports: [RouterOutlet, Header, Footer, ToastComponent],
  templateUrl: './app.html',
  styleUrl: './app.scss',
})
export class App implements OnInit {
  protected readonly title = signal('MovieTrackR');
  private readonly renderer = inject(Renderer2);
  
  ngOnInit(): void {
    this.applyTheme();
  }

   private applyTheme(): void {
    const prefersDark = window.matchMedia('(prefers-color-scheme: dark)').matches;
    
    if (prefersDark) {
      this.renderer.addClass(document.documentElement, 'dark-mode');
    } else {
      this.renderer.removeClass(document.documentElement, 'dark-mode');
    }
  }
}
