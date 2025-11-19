import { Component } from '@angular/core';
import { CardModule } from 'primeng/card';

@Component({
  selector: 'app-movies-page',
  standalone: true,
  imports: [CardModule],
  templateUrl: './movies-page.html',
  styleUrl: './movies-page.scss',
})
export class MoviesPage {

}
