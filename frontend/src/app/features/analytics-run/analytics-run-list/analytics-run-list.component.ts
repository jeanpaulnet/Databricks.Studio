import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ActivatedRoute, RouterLink } from '@angular/router';
import { ReactiveFormsModule, FormBuilder, Validators } from '@angular/forms';
import { MatTableModule } from '@angular/material/table';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatChipsModule } from '@angular/material/chips';
import { MatCardModule } from '@angular/material/card';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatSnackBar, MatSnackBarModule } from '@angular/material/snack-bar';
import { MatTabsModule } from '@angular/material/tabs';
import { MatSelectModule } from '@angular/material/select';
import { AnalyticsRunService } from '../../../core/services/analytics-run.service';
import { AnalyticsService } from '../../../core/services/analytics.service';
import { AnalyticsRun, HistoryItem } from '../../../shared/models/models';

@Component({
  selector: 'app-analytics-run-list',
  standalone: true,
  imports: [
    CommonModule, RouterLink, ReactiveFormsModule,
    MatTableModule, MatButtonModule, MatIconModule, MatChipsModule,
    MatCardModule, MatFormFieldModule, MatInputModule, MatSelectModule,
    MatProgressSpinnerModule, MatSnackBarModule, MatTabsModule
  ],
  template: `
    <div class="header-row">
      <h1>Runs</h1>
      <a mat-button routerLink="/analytics"><mat-icon>arrow_back</mat-icon> Back</a>
    </div>

    <mat-tab-group>
      <!-- ── Start New Run ── -->
      <mat-tab label="Start Run">
        <mat-card class="tab-card">
          <mat-card-content>
            <form [formGroup]="startForm" (ngSubmit)="startRun()">
              <div class="run-top-row">
                <mat-form-field appearance="outline" class="job-id-field">
                  <mat-label>Databricks Job ID</mat-label>
                  <input matInput formControlName="jobId" readonly />
                  <mat-hint>Auto-assigned on start</mat-hint>
                </mat-form-field>

                <mat-form-field appearance="outline" class="version-field">
                  <mat-label>Major Version</mat-label>
                  <mat-select formControlName="majorVersion">
                    <mat-option *ngFor="let v of majorVersionOptions" [value]="v">
                      v{{ v }}.x
                    </mat-option>
                  </mat-select>
                  <mat-hint>Latest auto-selected</mat-hint>
                </mat-form-field>
              </div>

              <mat-form-field appearance="outline" class="full-width">
                <mat-label>Input</mat-label>
                <textarea matInput formControlName="inputJson" rows="3"
                  placeholder="Describe or paste the input data for this run"></textarea>
              </mat-form-field>

              <mat-form-field appearance="outline" class="full-width">
                <mat-label>Output</mat-label>
                <textarea matInput formControlName="outputJson" rows="3"
                  placeholder="Describe or paste the expected output data for this run"></textarea>
              </mat-form-field>

              <button mat-raised-button color="primary" type="submit" [disabled]="starting">
                <mat-spinner *ngIf="starting" diameter="20" />
                <mat-icon *ngIf="!starting">play_arrow</mat-icon> Start Run
              </button>
            </form>
          </mat-card-content>
        </mat-card>
      </mat-tab>

      <!-- ── Current Run ── -->
      <mat-tab label="Current Run">
        <mat-card class="tab-card">
          <mat-card-content *ngIf="currentRun; else noRun">
            <p><strong>Run ID:</strong> {{ currentRun.id }}</p>
            <p><strong>Job ID:</strong> {{ currentRun.jobId }}</p>
            <p><strong>Version:</strong> <span class="version-chip">v{{ currentRun.majorVersion }}.x</span></p>
            <p><strong>Status:</strong>
              <mat-chip [class]="'run-status-' + currentRun.status">{{ currentRun.statusName }}</mat-chip>
            </p>
            <p><strong>Started:</strong> {{ currentRun.startedOn | date:'medium' }}</p>
            <p *ngIf="currentRun.completedOn"><strong>Completed:</strong> {{ currentRun.completedOn | date:'medium' }}</p>
            <p *ngIf="currentRun.terminatedOn"><strong>Terminated:</strong> {{ currentRun.terminatedOn | date:'medium' }}</p>
            <p *ngIf="currentRun.inputJson"><strong>Input:</strong> {{ currentRun.inputJson }}</p>
            <p *ngIf="currentRun.outputJson"><strong>Output:</strong> {{ currentRun.outputJson }}</p>
          </mat-card-content>
          <ng-template #noRun><mat-card-content>No active run.</mat-card-content></ng-template>
          <mat-card-actions *ngIf="currentRun && (currentRun.status === 0 || currentRun.status === 1)">
            <button mat-raised-button color="warn" (click)="stopRun()" [disabled]="stopping">
              <mat-spinner *ngIf="stopping" diameter="20" />
              <mat-icon *ngIf="!stopping">stop</mat-icon> Stop Run
            </button>
          </mat-card-actions>
        </mat-card>
      </mat-tab>

      <!-- ── History ── -->
      <mat-tab label="History">
        <mat-card class="tab-card">
          <mat-card-content>
            <div *ngIf="loadingHistory" class="center"><mat-spinner /></div>
            <mat-table *ngIf="!loadingHistory" [dataSource]="history" class="mat-elevation-z1">
              <ng-container matColumnDef="actionType">
                <mat-header-cell *matHeaderCellDef>Action</mat-header-cell>
                <mat-cell *matCellDef="let row">{{ row.actionType }}</mat-cell>
              </ng-container>
              <ng-container matColumnDef="actionBy">
                <mat-header-cell *matHeaderCellDef>By</mat-header-cell>
                <mat-cell *matCellDef="let row">{{ row.actionBy }}</mat-cell>
              </ng-container>
              <ng-container matColumnDef="actionOn">
                <mat-header-cell *matHeaderCellDef>When</mat-header-cell>
                <mat-cell *matCellDef="let row">{{ row.actionOn | date:'medium' }}</mat-cell>
              </ng-container>
              <mat-header-row *matHeaderRowDef="historyColumns" />
              <mat-row *matRowDef="let row; columns: historyColumns;" />
            </mat-table>
          </mat-card-content>
        </mat-card>
      </mat-tab>
    </mat-tab-group>
  `,
  styles: [`
    .header-row { display: flex; align-items: center; justify-content: space-between; margin-bottom: 16px; }
    .tab-card { margin-top: 16px; }
    .full-width { width: 100%; margin-bottom: 16px; }
    .center { display: flex; justify-content: center; padding: 32px; }
    .run-top-row { display: flex; gap: 16px; margin-bottom: 4px; }
    .job-id-field { flex: 1; }
    .version-field { width: 160px; }
    .version-chip {
      display: inline-block;
      background: #1976d2;
      color: white;
      font-size: 12px;
      font-weight: 600;
      padding: 2px 10px;
      border-radius: 12px;
    }
    .run-status-0 { background: #fff3e0 !important; }
    .run-status-1 { background: #e3f2fd !important; }
    .run-status-2 { background: #e8f5e9 !important; }
    .run-status-3 { background: #ffebee !important; }
  `]
})
export class AnalyticsRunListComponent implements OnInit {
  analyticsId!: string;
  currentRun?: AnalyticsRun;
  history: HistoryItem[] = [];
  historyColumns = ['actionType', 'actionBy', 'actionOn'];
  starting = false;
  stopping = false;
  loadingHistory = false;
  majorVersionOptions: number[] = [1];

  startForm = this.fb.group({
    jobId: [{ value: '', disabled: true }, Validators.required],
    majorVersion: [1, Validators.required],
    inputJson: [''],
    outputJson: ['']
  });

  constructor(
    private route: ActivatedRoute,
    private runService: AnalyticsRunService,
    private analyticsService: AnalyticsService,
    private snackBar: MatSnackBar,
    private fb: FormBuilder
  ) {}

  ngOnInit(): void {
    this.analyticsId = this.route.snapshot.paramMap.get('id')!;
    this.startForm.patchValue({ jobId: 'TBD' });
    this.loadAnalyticsVersions();
    this.loadHistory();
  }

  loadAnalyticsVersions(): void {
    this.analyticsService.getById(this.analyticsId).subscribe({
      next: res => {
        if (res.data) {
          const max = res.data.majorVersion;
          this.majorVersionOptions = Array.from({ length: max }, (_, i) => i + 1);
          this.startForm.patchValue({ majorVersion: max });
        }
      }
    });
  }

  startRun(): void {
    this.starting = true;
    const assignedJobId = this.generateJobId();
    this.startForm.patchValue({ jobId: assignedJobId });
    this.runService.start(this.analyticsId, {
      jobId: assignedJobId,
      startedBy: 'anonymous',
      inputJson: this.startForm.value.inputJson ?? undefined,
      outputJson: this.startForm.value.outputJson ?? undefined,
      majorVersion: this.startForm.value.majorVersion ?? this.majorVersionOptions.at(-1) ?? 1
    }).subscribe({
      next: res => {
        this.currentRun = res.data ?? undefined;
        this.snackBar.open('Run started — completes automatically in 5 minutes', '', { duration: 4000 });
        this.startForm.patchValue({ jobId: 'TBD', inputJson: '', outputJson: '' });
        this.starting = false;
        this.loadHistory();
      },
      error: err => {
        this.snackBar.open(err?.error?.message ?? 'Start failed', 'Dismiss', { duration: 3000 });
        this.starting = false;
      }
    });
  }

  stopRun(): void {
    if (!this.currentRun) return;
    this.stopping = true;
    this.runService.stop(this.currentRun.id, { stoppedBy: 'anonymous' }).subscribe({
      next: res => {
        this.currentRun = res.data ?? undefined;
        this.snackBar.open('Run stopped', '', { duration: 2000 });
        this.stopping = false;
        this.loadHistory();
      },
      error: () => {
        this.snackBar.open('Stop failed', 'Dismiss', { duration: 3000 });
        this.stopping = false;
      }
    });
  }

  loadHistory(): void {
    this.loadingHistory = true;
    this.runService.getHistory(this.analyticsId).subscribe({
      next: res => { this.history = res.data ?? []; this.loadingHistory = false; },
      error: () => this.loadingHistory = false
    });
  }

  private generateJobId(): string {
    return `JOB-${Math.random().toString(36).substring(2, 10).toUpperCase()}`;
  }
}
