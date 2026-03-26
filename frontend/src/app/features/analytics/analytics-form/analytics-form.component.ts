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
      </mat-card-header>
      <mat-card-content>
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
    .actions { display: flex; gap: 12px; justify-content: flex-end; margin-top: 8px; }
  `]
})
export class AnalyticsFormComponent implements OnInit {
  form!: FormGroup;
  isEdit = false;
  saving = false;
  private id?: string;

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
      description: ['', [Validators.maxLength(2000)]]
    });

    this.id = this.route.snapshot.paramMap.get('id') ?? undefined;
    this.isEdit = !!this.id;

    if (this.isEdit && this.id) {
      this.analyticsService.getById(this.id).subscribe(res => {
        if (res.data) this.form.patchValue(res.data);
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
      next: () => {
        this.snackBar.open(this.isEdit ? 'Updated' : 'Created', '', { duration: 2000 });
        this.router.navigate(['/analytics']);
      },
      error: () => {
        this.snackBar.open('Save failed', 'Dismiss', { duration: 3000 });
        this.saving = false;
      }
    });
  }
}
