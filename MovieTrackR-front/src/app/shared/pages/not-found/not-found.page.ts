import { CommonModule, Location } from '@angular/common';
import { Component, inject, signal } from '@angular/core';
import { RouterLink } from '@angular/router';
import { ButtonModule } from 'primeng/button';
import { CardModule } from 'primeng/card';

interface NotFoundGif {
  url: string;
  movie: string;
  quote: string | null;
}

@Component({
  selector: 'app-not-found.page',
  standalone: true,
  imports: [CommonModule, RouterLink, CardModule, ButtonModule],
  templateUrl: './not-found.page.html',
  styleUrl: './not-found.page.scss',
})
export class NotFoundPageComponent {
  readonly location = inject(Location);
  readonly notFoundGifs: NotFoundGif[] = [
    {
      url: 'https://media3.giphy.com/media/v1.Y2lkPTc5MGI3NjExc3dyaWNjOWw3OHZoMHdiaGNyZHo4ZTMzNXQzNTBmcXV2cnp4MjNtdyZlcD12MV9pbnRlcm5hbF9naWZfYnlfaWQmY3Q9Zw/6uGhT1O4sxpi8/giphy.gif',
      movie: 'Pulp Fiction',
      quote: 'Où...Où il est l\'intercom?'
    },
    {
      url: 'https://media3.giphy.com/media/v1.Y2lkPTc5MGI3NjExbXlyNXk5cHFycWMxM3QybDBlM2llNGo1YzBpaXk5dXRrcXFmcXN4NCZlcD12MV9pbnRlcm5hbF9naWZfYnlfaWQmY3Q9Zw/oY0KOHdmCkiJi/giphy.gif',
      movie: 'Pirates des Caraïbes',
      quote: 'Il faut se perdre pour trouver ce qui est introuvable'
    },
    {
      url: 'https://media2.giphy.com/media/v1.Y2lkPTc5MGI3NjExYXZibDZsZ2RrZXZ6dXE1Z3hsdWc2a2ZyNTQzY2FkeHZ4NTYxeXJsNyZlcD12MV9pbnRlcm5hbF9naWZfYnlfaWQmY3Q9Zw/ZDlb9n3XirDy0/giphy.gif',
      movie: 'Le magicien d\'Oz',
      quote: 'Nous ne devons plus être au Kansas'
    },
    {
      url: 'https://media4.giphy.com/media/v1.Y2lkPTc5MGI3NjExbjRkazJ5Y3gyMHI1Y3V6NHB3NXBkYzR0ZTA5ZjlzdWZvbmtiN2YxdSZlcD12MV9pbnRlcm5hbF9naWZfYnlfaWQmY3Q9Zw/Y01J4qwVlWVvsja2L9/giphy.gif',
      movie: 'Le Seigneur des Anneaux',
      quote: 'On est déja passé par ici. On tourne en rond'
    }
  ] as const;

  readonly selectedGif = signal<NotFoundGif>(
    this.notFoundGifs[Math.floor(Math.random() * this.notFoundGifs.length)]
  );

    goBack(): void {
    this.location.back();
  }
}
