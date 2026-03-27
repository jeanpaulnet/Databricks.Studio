import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ReactiveFormsModule, FormBuilder, FormGroup, Validators } from '@angular/forms';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatButtonModule } from '@angular/material/button';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatSnackBar, MatSnackBarModule } from '@angular/material/snack-bar';
import { MatCardModule } from '@angular/material/card';
import { AnalyticsService } from '../../../core/services/analytics.service';

@Component({
  selector: 'app-analytics-form',
  standalone: true,
  imports: [
    CommonModule, ReactiveFormsModule, RouterLink,
    MatFormFieldModule, MatInputModule, MatButtonModule,
    MatProgressSpinnerModule, MatSnackBarModule, MatCardModule
  ],
  template: `
    <mat-card class="form-card">
      <mat-card-header>
        <mat-card-title>{{ isEdit ? 'Edit' : 'New' }} Analytics</mat-card-title>
        <mat-card-subtitle *ngIf="isEdit && version">
          <span class="version-badge">v{{ version }}</span>
          <span *ngIf="isNonDraft" class="version-hint draft-warn">
            ⚠ This is a published/submitted record — saving will create a new Draft v{{ nextVersion }}
          </span>
          <span *ngIf="!isNonDraft" class="version-hint">
            Draft — saving updates in place to v{{ nextVersion }}
          </span>
        </mat-card-subtitle>
      </mat-card-header>
      <mat-card-content>
        <div *ngIf="loading" class="loading-overlay">
          <mat-spinner diameter="40" />
        </div>
        <form [formGroup]="form" (ngSubmit)="submit()">
          <mat-form-field appearance="outline" class="full-width">
            <mat-label>Name</mat-label>
            <input matInput formControlName="name" placeholder="Analytics name" />
            <mat-error *ngIf="form.get('name')?.hasError('required')">Name is required</mat-error>
            <mat-error *ngIf="form.get('name')?.hasError('maxlength')">Max 256 characters</mat-error>
          </mat-form-field>

          <mat-form-field appearance="outline" class="full-width">
            <mat-label>Description</mat-label>
            <textarea matInput formControlName="description" rows="4" placeholder="Describe this analytics"></textarea>
            <mat-error *ngIf="form.get('description')?.hasError('maxlength')">Max 2000 characters</mat-error>
          </mat-form-field>

          <mat-form-field appearance="outline" class="value-field">
            <mat-label>Value</mat-label>
            <input matInput type="number" formControlName="value" placeholder="0" />
            <mat-error *ngIf="form.get('value')?.hasError('min')">Value must be 0 or greater</mat-error>
          </mat-form-field>

          <div class="actions">
            <a mat-button routerLink="/analytics">Cancel</a>
            <button mat-raised-button color="primary" type="submit" [disabled]="form.invalid || saving">
              <mat-spinner *ngIf="saving" diameter="20" />
              {{ isEdit ? 'Save Changes' : 'Create' }}
            </button>
          </div>
        </form>
      </mat-card-content>
    </mat-card>
  `,
  styles: [`
    .form-card { max-width: 600px; margin: 0 auto; }
    .full-width { width: 100%; margin-bottom: 16px; }
    .value-field { width: 200px; margin-bottom: 16px; }
    .actions { display: flex; gap: 12px; justify-content: flex-end; margin-top: 8px; }
    .loading-overlay { display: flex; justify-content: center; padding: 24px; }
    .version-badge {
      display: inline-block;
      background: #1976d2;
      color: white;
      font-size: 12px;
      font-weight: 600;
      padding: 2px 10px;
      border-radius: 12px;
      margin-right: 8px;
    }
    .version-hint { font-size: 11px; color: #888; }
    .draft-warn { color: #e65100 !important; font-weight: 500; }
  `]
})
export class AnalyticsFormComponent implements OnInit {
  form!: FormGroup;
  isEdit = false;
  saving = false;
  loading = false;
  version = '';
  nextVersion = '';
  isNonDraft = false;
  private id?: string;
  private currentStatus = 0;

  constructor(
    private fb: FormBuilder,
    private route: ActivatedRoute,
    private router: Router,
    private analyticsService: AnalyticsService,
    private snackBar: MatSnackBar
  ) {}

  ngOnInit(): void {
    this.form = this.fb.group({
      name: ['', [Validators.required, Validators.maxLength(256)]],
      description: ['', [Validators.maxLength(2000)]],
      value: [0, [Validators.required, Validators.min(0)]]
    });

    this.id = this.route.snapshot.paramMap.get('id') ?? undefined;
    this.isEdit = !!this.id;

    if (this.isEdit && this.id) {
      this.loading = true;
      this.analyticsService.getById(this.id).subscribe({
        next: res => {
          if (res.data) {
            this.form.patchValue({ name: res.data.name, description: res.data.description, value: res.data.value });
            this.currentStatus = res.data.status;
            this.isNonDraft = res.data.status !== 0;
            this.version = `${res.data.majorVersion}.${res.data.minorVersion}`;
            this.nextVersion = `${res.data.majorVersion}.${res.data.minorVersion + 1}`;
          } else {
            this.snackBar.open('Analytics not found', 'Dismiss', { duration: 3000 });
            this.router.navigate(['/analytics']);
          }
          this.loading = false;
        },
        error: () => {
          this.snackBar.open('Failed to load analytics', 'Dismiss', { duration: 3000 });
          this.loading = false;
          this.router.navigate(['/analytics']);
        }
      });
    }
  }

  submit(): void {
    if (this.form.invalid) return;
    this.saving = true;
    const dto = this.form.value;

    const call = this.isEdit && this.id
      ? this.analyticsService.update(this.id, dto)
      : this.analyticsService.create(dto);

    call.subscribe({
      next: res => {
        const saved = res.data;
        if (this.isEdit && saved && saved.id !== this.id) {
          // A new draft was created from a non-draft original
          this.snackBar.open(`New Draft v${saved.majorVersion}.${saved.minorVersion} created`, '', { duration: 3000 });
          this.router.navigate(['/analytics', saved.id, 'edit']);
        } else {
          this.snackBar.open(this.isEdit ? 'Saved' : 'Created', '', { duration: 2000 });
          this.router.navigate(['/analytics']);
        }
      },
      error: () => {
        this.snackBar.open('Save failed', 'Dismiss', { duration: 3000 });
        this.saving = false;
      }
    });
  }
}
