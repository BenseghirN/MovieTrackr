import { Injectable, inject } from '@angular/core';
import { Title } from '@angular/platform-browser';
import { TitleStrategy, RouterStateSnapshot } from '@angular/router';

@Injectable({ providedIn: 'root' })
export class AppTitleStrategy extends TitleStrategy {
  private readonly title = inject(Title);

  override updateTitle(snapshot: RouterStateSnapshot): void {
    const routeTitle = this.buildTitle(snapshot);

    if (routeTitle) {
      this.title.setTitle(routeTitle);
    } else {
      this.title.setTitle('MovieTrackR');
    }
  }
}
