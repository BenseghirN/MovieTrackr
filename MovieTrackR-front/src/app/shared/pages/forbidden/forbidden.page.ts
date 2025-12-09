import { CommonModule, Location } from '@angular/common';
import { Component, inject, signal } from '@angular/core';
import { RouterLink } from '@angular/router';
import { ButtonModule } from 'primeng/button';
import { CardModule } from 'primeng/card';

interface ForbiddenGif {
  url: string;
  movie: string;
  quote: string | null;
}

@Component({
  selector: 'app-forbidden-page',
  standalone: true,
  imports: [CommonModule, RouterLink, CardModule, ButtonModule],
  templateUrl: './forbidden.page.html',
  styleUrl: './forbidden.page.scss',
})
export class ForbiddenPageComponent {  
  readonly location = inject(Location);
  readonly forbiddenGifs: ForbiddenGif[] = [
    {
      url: 'https://media2.giphy.com/media/v1.Y2lkPTc5MGI3NjExZTN2dDlqdXg5OHY5aXpta25rZ25ibGlmdno3b3J3aGExNmVlZmtsdCZlcD12MV9pbnRlcm5hbF9naWZfYnlfaWQmY3Q9Zw/njYrp176NQsHS/giphy.gif',
      movie: 'Le Seigneur des Anneaux',
      quote: 'Vous ne passerez pas !'
    },
    {
      url: 'https://media2.giphy.com/media/v1.Y2lkPTc5MGI3NjExNWhhMDRpY2FuZ3hjZDE3MGl6d3hra3h0b3o3dzFoZWp0ZTEydGhhbiZlcD12MV9pbnRlcm5hbF9naWZfYnlfaWQmY3Q9Zw/l2JJKs3I69qfaQleE/giphy.gif',
      movie: 'Star Wars',
      quote: 'Ce ne sont pas les droïdes que vous cherchez'
    },
    {
      url: 'https://media4.giphy.com/media/v1.Y2lkPTc5MGI3NjExcDFoaWJrZHNhMDBidXloN2J1MDRyNHliY2s1M2h2cXBkN284Y3hnaCZlcD12MV9pbnRlcm5hbF9naWZfYnlfaWQmY3Q9Zw/ip5L71rU6sjcc/giphy.gif',
      movie: 'Matrix',
      quote: null
    },
    {
      url: 'https://media3.giphy.com/media/v1.Y2lkPTc5MGI3NjExaHpnbnppc3huZnNuYjQxZ2dyemRuamxpNG9pOWRmNG1oZmt0MGZlbSZlcD12MV9pbnRlcm5hbF9naWZfYnlfaWQmY3Q9Zw/owRSsSHHoVYFa/giphy.gif',
      movie: 'Jurassic Park',
      quote: 'Ah ah ah, vous n\'avez pas dit le mot magique !'
    }
  ] as const;

  readonly selectedGif = signal<ForbiddenGif>(
    this.forbiddenGifs[Math.floor(Math.random() * this.forbiddenGifs.length)]
  );

    goBack(): void {
    this.location.back();
  }
}
