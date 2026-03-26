import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterLink } from '@angular/router';
import { MatTableModule } from '@angular/material/table';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatChipsModule } from '@angular/material/chips';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatPaginatorModule, PageEvent } from '@angular/material/paginator';
import { MatSnackBar, MatSnackBarModule } from '@angular/material/snack-bar';
import { MatDialogModule, MatDialog } from '@angular/material/dialog';
import { AnalyticsService } from '../../../core/services/analytics.service';
import { AnalyticsListItem } from '../../../shared/models/models';

@Component({
  selector: 'app-analytics-list',
  standalone: true,
  imports: [
    CommonModule, RouterLink, MatTableModule, MatButtonModule, MatIconModule,
    MatChipsModule, MatProgressSpinnerModule, MatPaginatorModule,
    MatSnackBarModule, MatDialogModule
  ],
  template: `
    <div class="header-row">
      <h1>Analytics</h1>
      <a mat-raised-button color="primary" routerLink="/analytics/new">
        <mat-icon>add</mat-icon> New Analytics
      </a>
    </div>

    <div *ngIf="loading" class="center"><mat-spinner /></div>

    <mat-table *ngIf="!loading" [dataSource]="items" class="mat-elevation-z2">
      <ng-container matColumnDef="name">
        <mat-header-cell *matHeaderCellDef>Name</mat-header-cell>
        <mat-cell *matCellDef="let row">
          <a [routerLink]="['/analytics', row.id]">{{ row.name }}</a>
        </mat-cell>
      </ng-container>

      <ng-container matColumnDef="status">
        <mat-header-cell *matHeaderCellDef>Status</mat-header-cell>
        <mat-cell *matCellDef="let row">
          <mat-chip [class]="'status-' + row.status">{{ row.statusName }}</mat-chip>
        </mat-cell>
      </ng-container>

      <ng-container matColumnDef="actions">
        <mat-header-cell *matHeaderCellDef>Actions</mat-header-cell>
        <mat-cell *matCellDef="let row">
          <a mat-icon-button [routerLink]="['/analytics', row.id, 'edit']" title="Edit">
            <mat-icon>edit</mat-icon>
          </a>
          <a mat-icon-button [routerLink]="['/analytics', row.id, 'runs']" title="Runs">
            <mat-icon>play_circle</mat-icon>
          </a>
          <button mat-icon-button color="warn" (click)="delete(row)" title="Delete">
            <mat-icon>delete</mat-icon>
          </button>
        </mat-cell>
      </ng-container>

      <mat-header-row *matHeaderRowDef="columns" />
      <mat-row *matRowDef="let row; columns: columns;" />
    </mat-table>

    <mat-paginator
      [length]="totalCount"
      [pageSize]="pageSize"
      [pageSizeOptions]="[10, 20, 50]"
      (page)="onPage($event)" />
  `,
  styles: [`
    .header-row { display: flex; align-items: center; justify-content: space-between; margin-bottom: 16px; }
    .center { display: flex; justify-content: center; padding: 48px; }
    mat-table { width: 100%; }
    a { text-decoration: none; color: inherit; }
    .status-0 { background-color: #e0e0e0 !important; }
    .status-1 { background-color: #fff3e0 !important; }
    .status-2 { background-color: #e8f5e9 !important; }
    .status-3 { background-color: #ffebee !important; }
    .status-4 { background-color: #e3f2fd !important; }
  `]
})
export class AnalyticsListComponent implements OnInit {
  items: AnalyticsListItem[] = [];
  columns = ['name', 'status', 'actions'];
  loading = false;
  totalCount = 0;
  page = 1;
  pageSize = 20;

  constructor(
    private analyticsService: AnalyticsService,
    private snackBar: MatSnackBar
  ) {}

  ngOnInit(): void { this.load(); }

  load(): void {
    this.loading = true;
    this.analyticsService.list(this.page, this.pageSize).subscribe({
      next: res => {
        this.items = res.data?.items ?? [];
        this.totalCount = res.data?.totalCount ?? 0;
        this.loading = false;
      },
      error: () => {
        this.snackBar.open('Failed to load analytics', 'Dismiss', { duration: 3000 });
        this.loading = false;
      }
    });
  }

  onPage(event: PageEvent): void {
    this.page = event.pageIndex + 1;
    this.pageSize = event.pageSize;
    this.load();
  }

  delete(item: AnalyticsListItem): void {
    if (!confirm(`Delete "${item.name}"?`)) return;
    this.analyticsService.delete(item.id).subscribe({
      next: () => {
        this.snackBar.open('Deleted successfully', '', { duration: 2000 });
        this.load();
      },
      error: () => this.snackBar.open('Delete failed', 'Dismiss', { duration: 3000 })
    });
  }
}
