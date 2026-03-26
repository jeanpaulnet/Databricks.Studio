import { Component } from '@angular/core';
import { RouterOutlet, RouterLink } from '@angular/router';
import { MatToolbarModule } from '@angular/material/toolbar';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';

@Component({
  selector: 'app-root',
  standalone: true,
  imports: [RouterOutlet, RouterLink, MatToolbarModule, MatButtonModule, MatIconModule],
  template: `
    <mat-toolbar color="primary">
      <span>Databricks Studio</span>
      <span class="spacer"></span>
      <a mat-button routerLink="/analytics">Analytics</a>
      <a mat-button routerLink="/review">Review</a>
      <a mat-button routerLink="/chat">
        <mat-icon>smart_toy</mat-icon>
        Chat
      </a>
    </mat-toolbar>
    <main class="content">
      <router-outlet />
    </main>
  `,
  styles: [`
    .spacer { flex: 1 1 auto; }
    .content { padding: 24px; max-width: 1280px; margin: 0 auto; }
    a[routerLink="/chat"] { display: flex; align-items: center; gap: 4px; }
  `]
})
export class AppComponent {}
