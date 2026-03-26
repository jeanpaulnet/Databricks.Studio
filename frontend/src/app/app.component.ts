import { Component } from '@angular/core';
import { RouterOutlet, RouterLink } from '@angular/router';
import { MatToolbarModule } from '@angular/material/toolbar';
import { MatButtonModule } from '@angular/material/button';

@Component({
  selector: 'app-root',
  standalone: true,
  imports: [RouterOutlet, RouterLink, MatToolbarModule, MatButtonModule],
  template: `
    <mat-toolbar color="primary">
      <span>Databricks Studio</span>
      <span class="spacer"></span>
      <a mat-button routerLink="/analytics">Analytics</a>
      <a mat-button routerLink="/review">Review</a>
    </mat-toolbar>
    <main class="content">
      <router-outlet />
    </main>
  `,
  styles: [`
    .spacer { flex: 1 1 auto; }
    .content { padding: 24px; max-width: 1280px; margin: 0 auto; }
  `]
})
export class AppComponent {}
