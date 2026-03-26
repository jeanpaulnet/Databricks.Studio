import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ActivatedRoute, RouterLink } from '@angular/router';
import { MatCardModule } from '@angular/material/card';
import { MatButtonModule } from '@angular/material/button';
import { MatChipsModule } from '@angular/material/chips';
import { MatIconModule } from '@angular/material/icon';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatSnackBar, MatSnackBarModule } from '@angular/material/snack-bar';
import { AnalyticsService } from '../../../core/services/analytics.service';
import { Analytics } from '../../../shared/models/models';

@Component({
  selector: 'app-analytics-detail',
  standalone: true,
  imports: [
    CommonModule, RouterLink, MatCardModule, MatButtonModule,
    MatChipsModule, MatIconModule, MatProgressSpinnerModule, MatSnackBarModule
  ],
  template: `
    <div *ngIf="loading" class="center"><mat-spinner /></div>

    <ng-container *ngIf="!loading && item">
      <div class="header-row">
        <h1>{{ item.name }}</h1>
        <mat-chip [class]="'status-' + item.status">{{ item.statusName }}</mat-chip>
      </div>

      <mat-card>
        <mat-card-content>
          <p>{{ item.description || 'No description provided.' }}</p>
        </mat-card-content>
        <mat-card-actions>
          <a mat-button [routerLink]="['/analytics', item.id, 'edit']">
            <mat-icon>edit</mat-icon> Edit
          </a>
          <a mat-button [routerLink]="['/analytics', item.id, 'runs']">
            <mat-icon>play_circle</mat-icon> View Runs
          </a>
          <button mat-button color="accent" *ngIf="item.status === 0" (click)="submit()">
            <mat-icon>send</mat-icon> Submit for Review
          </button>
          <a mat-button routerLink="/analytics">
            <mat-icon>arrow_back</mat-icon> Back
          </a>
        </mat-card-actions>
      </mat-card>
    </ng-container>
  `,
  styles: [`
    .header-row { display: flex; align-items: center; gap: 16px; margin-bottom: 16px; }
    .center { display: flex; justify-content: center; padding: 48px; }
    .status-0 { background-color: #e0e0e0 !important; }
    .status-1 { background-color: #fff3e0 !important; }
    .status-2 { background-color: #e8f5e9 !important; }
    .status-3 { background-color: #ffebee !important; }
    .status-4 { background-color: #e3f2fd !important; }
  `]
})
export class AnalyticsDetailComponent implements OnInit {
  item?: Analytics;
  loading = false;

  constructor(
    private route: ActivatedRoute,
    private analyticsService: AnalyticsService,
    private snackBar: MatSnackBar
  ) {}

  ngOnInit(): void {
    const id = this.route.snapshot.paramMap.get('id')!;
    this.loading = true;
    this.analyticsService.getById(id).subscribe({
      next: res => { this.item = res.data ?? undefined; this.loading = false; },
      error: () => { this.snackBar.open('Failed to load', 'Dismiss', { duration: 3000 }); this.loading = false; }
    });
  }

  submit(): void {
    // Placeholder — wire up a "submit for review" endpoint if desired
    this.snackBar.open('Submitted for review', '', { duration: 2000 });
  }
}
