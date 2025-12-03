import { Component, inject, signal } from '@angular/core';
import { Router } from '@angular/router';
import { ButtonModule } from 'primeng/button';
import { CardModule } from 'primeng/card';
import { InputTextModule } from 'primeng/inputtext';

@Component({
  selector: 'app-home-page',
  imports: [ButtonModule, CardModule, InputTextModule],
  templateUrl: './home.page.html',
  styleUrl: './home.page.scss',
})
export class HomePageComponent {
  private readonly router = inject(Router);
  private readonly searchQuery = signal('');

  public onQueryChange(event: Event): void {
    const value = (event.target as HTMLInputElement).value;
    this.searchQuery.set(value);
  }

  public onSearchKeyup(event: KeyboardEvent): void {
    if (event.key === 'Enter' &&  this.searchQuery().trim() !== '') {
      this.redirectToSearch();
    }
  }
  
  public onSearch(): void {
    if (this.searchQuery().trim() !== '') {
      this.redirectToSearch();
    }
  }

  private redirectToSearch() {
    const query = this.searchQuery().trim();
    this.searchQuery.set('');
    
    this.router.navigate(['/movies'], {
      queryParams: {
        query: query,
        page: 1
      }
    });
  }
}
