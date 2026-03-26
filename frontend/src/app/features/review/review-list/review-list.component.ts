import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ReactiveFormsModule, FormBuilder } from '@angular/forms';
import { MatTableModule } from '@angular/material/table';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatChipsModule } from '@angular/material/chips';
import { MatCardModule } from '@angular/material/card';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatSnackBar, MatSnackBarModule } from '@angular/material/snack-bar';
import { AnalyticsService } from '../../../core/services/analytics.service';
import { AnalyticsListItem } from '../../../shared/models/models';

@Component({
  selector: 'app-review-list',
  standalone: true,
  imports: [
    CommonModule, ReactiveFormsModule, MatTableModule, MatButtonModule, MatIconModule,
    MatChipsModule, MatCardModule, MatFormFieldModule, MatInputModule,
    MatProgressSpinnerModule, MatSnackBarModule
  ],
  template: `
    <h1>Review Queue</h1>
    <p class="subtitle">Analytics awaiting approval or rejection.</p>

    <div *ngIf="loading" class="center"><mat-spinner /></div>

    <mat-table *ngIf="!loading" [dataSource]="items" class="mat-elevation-z2">
      <ng-container matColumnDef="name">
        <mat-header-cell *matHeaderCellDef>Name</mat-header-cell>
        <mat-cell *matCellDef="let row">{{ row.name }}</mat-cell>
      </ng-container>

      <ng-container matColumnDef="status">
        <mat-header-cell *matHeaderCellDef>Status</mat-header-cell>
        <mat-cell *matCellDef="let row">
          <mat-chip class="status-1">{{ row.statusName }}</mat-chip>
        </mat-cell>
      </ng-container>

      <ng-container matColumnDef="actions">
        <mat-header-cell *matHeaderCellDef>Actions</mat-header-cell>
        <mat-cell *matCellDef="let row">
          <button mat-raised-button color="primary" (click)="openReview(row, 'approve')"
                  [disabled]="processing === row.id" class="action-btn">
            <mat-icon>check</mat-icon> Approve
          </button>
          <button mat-raised-button color="warn" (click)="openReview(row, 'reject')"
                  [disabled]="processing === row.id" class="action-btn">
            <mat-icon>close</mat-icon> Reject
          </button>
        </mat-cell>
      </ng-container>

      <mat-header-row *matHeaderRowDef="columns" />
      <mat-row *matRowDef="let row; columns: columns;" />
    </mat-table>

    <p *ngIf="!loading && items.length === 0" class="empty">No items pending review.</p>

    <mat-card *ngIf="selectedItem" class="review-panel">
      <mat-card-header>
        <mat-card-title>
          {{ reviewAction === 'approve' ? 'Approve' : 'Reject' }}: {{ selectedItem.name }}
        </mat-card-title>
      </mat-card-header>
      <mat-card-content>
        <form [formGroup]="reviewForm" (ngSubmit)="submitReview()">
          <mat-form-field appearance="outline" class="full-width">
            <mat-label>Comments (optional)</mat-label>
            <textarea matInput formControlName="comments" rows="3"></textarea>
          </mat-form-field>
          <div class="review-actions">
            <button mat-button type="button" (click)="cancelReview()">Cancel</button>
            <button mat-raised-button [color]="reviewAction === 'approve' ? 'primary' : 'warn'"
                    type="submit" [disabled]="processing === selectedItem.id">
              Confirm {{ reviewAction === 'approve' ? 'Approval' : 'Rejection' }}
            </button>
          </div>
        </form>
      </mat-card-content>
    </mat-card>
  `,
  styles: [`
    .subtitle { color: #666; margin-bottom: 16px; }
    .center { display: flex; justify-content: center; padding: 48px; }
    .empty { text-align: center; color: #888; padding: 32px; }
    .action-btn { margin-right: 8px; }
    .status-1 { background-color: #fff3e0 !important; }
    .review-panel { margin-top: 24px; max-width: 600px; }
    .full-width { width: 100%; margin-top: 12px; }
    .review-actions { display: flex; gap: 12px; justify-content: flex-end; }
  `]
})
export class ReviewListComponent implements OnInit {
  items: AnalyticsListItem[] = [];
  columns = ['name', 'status', 'actions'];
  loading = false;
  processing: string | null = null;
  selectedItem?: AnalyticsListItem;
  reviewAction: 'approve' | 'reject' = 'approve';

  reviewForm = this.fb.group({ comments: [''] });

  constructor(
    private analyticsService: AnalyticsService,
    private snackBar: MatSnackBar,
    private fb: FormBuilder
  ) {}

  ngOnInit(): void { this.load(); }

  load(): void {
    this.loading = true;
    this.analyticsService.list(1, 100).subscribe({
      next: res => {
        this.items = (res.data?.items ?? []).filter(i => i.status === 1);
        this.loading = false;
      },
      error: () => { this.snackBar.open('Failed to load', 'Dismiss', { duration: 3000 }); this.loading = false; }
    });
  }

  openReview(item: AnalyticsListItem, action: 'approve' | 'reject'): void {
    this.selectedItem = item;
    this.reviewAction = action;
    this.reviewForm.reset();
  }

  cancelReview(): void { this.selectedItem = undefined; }

  submitReview(): void {
    if (!this.selectedItem) return;
    this.processing = this.selectedItem.id;
    const dto = {
      reviewedBy: 'anonymous',
      comments: this.reviewForm.value.comments ?? undefined
    };

    const call = this.reviewAction === 'approve'
      ? this.analyticsService.approve(this.selectedItem.id, dto)
      : this.analyticsService.reject(this.selectedItem.id, dto);

    call.subscribe({
      next: () => {
        this.snackBar.open(`${this.reviewAction === 'approve' ? 'Approved' : 'Rejected'} successfully`, '', { duration: 2000 });
        this.processing = null;
        this.selectedItem = undefined;
        this.load();
      },
      error: err => {
        this.snackBar.open(err?.error?.message ?? 'Action failed', 'Dismiss', { duration: 3000 });
        this.processing = null;
      }
    });
  }
}
