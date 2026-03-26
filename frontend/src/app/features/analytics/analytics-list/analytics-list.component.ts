import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterLink } from '@angular/router';
import { MatCardModule } from '@angular/material/card';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatChipsModule } from '@angular/material/chips';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatPaginatorModule, PageEvent } from '@angular/material/paginator';
import { MatSnackBar, MatSnackBarModule } from '@angular/material/snack-bar';
import { MatTooltipModule } from '@angular/material/tooltip';
import { BaseChartDirective } from 'ng2-charts';
import { ChartData, ChartOptions } from 'chart.js';
import { Chart, ArcElement, Tooltip, Legend, CategoryScale, LinearScale, BarElement, DoughnutController, BarController } from 'chart.js';
import { AnalyticsService } from '../../../core/services/analytics.service';
import { AnalyticsListItem, AnalyticsSummary } from '../../../shared/models/models';

Chart.register(ArcElement, Tooltip, Legend, CategoryScale, LinearScale, BarElement, DoughnutController, BarController);

const STATUS_LABELS = ['Draft', 'Submitted', 'Approved', 'Rejected', 'Published'];
const STATUS_COLORS = ['#bdbdbd', '#ffca28', '#8bc34a', '#ef5350', '#42a5f5'];
const STATUS_BORDER = ['#9e9e9e', '#ffa000', '#558b2f', '#c62828', '#1565c0'];

@Component({
  selector: 'app-analytics-list',
  standalone: true,
  imports: [
    CommonModule, RouterLink, MatCardModule, MatButtonModule, MatIconModule,
    MatChipsModule, MatProgressSpinnerModule, MatPaginatorModule,
    MatSnackBarModule, MatTooltipModule, BaseChartDirective
  ],
  template: `
    <div class="header-row">
      <h1>Analytics Dashboard</h1>
      <a mat-raised-button color="primary" routerLink="/analytics/new">
        <mat-icon>add</mat-icon> New Analytics
      </a>
    </div>

    <!-- KPI strip -->
    <div class="kpi-strip" *ngIf="summary">
      <div class="kpi-card kpi-total">
        <span class="kpi-value">{{ summary.totalCount }}</span>
        <span class="kpi-label">Total</span>
      </div>
      <div *ngFor="let s of summary.countByStatus" class="kpi-card" [ngClass]="'kpi-status-' + s.status.toLowerCase()">
        <span class="kpi-value">{{ s.count }}</span>
        <span class="kpi-label">{{ s.status }}</span>
      </div>
    </div>

    <!-- Charts row -->
    <div class="charts-row" *ngIf="summary && summary.totalCount > 0">
      <mat-card class="chart-card mat-elevation-z2">
        <mat-card-header><mat-card-title>By Status</mat-card-title></mat-card-header>
        <mat-card-content>
          <canvas baseChart
            [data]="doughnutData"
            [options]="doughnutOptions"
            type="doughnut">
          </canvas>
        </mat-card-content>
      </mat-card>

      <mat-card class="chart-card mat-elevation-z2">
        <mat-card-header><mat-card-title>Count per Status</mat-card-title></mat-card-header>
        <mat-card-content>
          <canvas baseChart
            [data]="barData"
            [options]="barOptions"
            type="bar">
          </canvas>
        </mat-card-content>
      </mat-card>
    </div>

    <!-- List section -->
    <h2 class="section-title">All Analytics</h2>

    <div *ngIf="loading" class="center"><mat-spinner /></div>

    <div *ngIf="!loading && items.length === 0" class="empty-state">
      <mat-icon>bar_chart</mat-icon>
      <p>No analytics yet. Create your first one.</p>
    </div>

    <div *ngIf="!loading" class="card-grid">
      <mat-card *ngFor="let row of items" class="analytics-card mat-elevation-z3" [ngClass]="'card-status-' + row.status">

        <!-- Title row -->
        <mat-card-header>
          <mat-card-title>
            <a [routerLink]="['/analytics', row.id]">{{ row.name }}</a>
          </mat-card-title>
          <mat-chip class="status-chip" [ngClass]="'status-' + row.status">{{ row.statusName }}</mat-chip>
        </mat-card-header>

        <!-- Body: mini chart + info -->
        <mat-card-content>
          <div class="card-body">

            <!-- Mini SVG donut chart -->
            <div class="mini-chart-wrap">
              <svg viewBox="0 0 80 80" width="80" height="80">
                <!-- Background track -->
                <circle cx="40" cy="40" r="30"
                  fill="none" stroke="#e0e0e0" stroke-width="8"/>
                <!-- Colored arc -->
                <circle cx="40" cy="40" r="30"
                  fill="none"
                  [attr.stroke]="statusColor(row.status)"
                  stroke-width="8"
                  stroke-linecap="round"
                  [attr.stroke-dasharray]="188.5"
                  [attr.stroke-dashoffset]="dashOffset(row.status)"
                  transform="rotate(-90 40 40)"/>
                <!-- Center label -->
                <text x="40" y="44" text-anchor="middle"
                  font-size="13" font-weight="700"
                  [attr.fill]="statusColor(row.status)">
                  {{ progressLabel(row.status) }}
                </text>
              </svg>
              <span class="chart-caption">Workflow</span>
            </div>

            <!-- Description + value -->
            <div class="card-info">
              <p class="description">{{ row.description || 'No description provided.' }}</p>
              <div class="value-row">
                <mat-icon class="value-icon">trending_up</mat-icon>
                <span class="value-label">Value:</span>
                <span class="value-num">{{ row.value | number:'1.0-2' }}</span>
              </div>
            </div>
          </div>
        </mat-card-content>

        <mat-card-actions align="end">
          <a mat-icon-button [routerLink]="['/analytics', row.id]" matTooltip="View">
            <mat-icon>visibility</mat-icon>
          </a>
          <a mat-icon-button [routerLink]="['/analytics', row.id, 'edit']" matTooltip="Edit">
            <mat-icon>edit</mat-icon>
          </a>
          <a mat-icon-button [routerLink]="['/analytics', row.id, 'runs']" matTooltip="Runs">
            <mat-icon>play_circle</mat-icon>
          </a>
          <button mat-icon-button color="warn" (click)="delete(row)" matTooltip="Delete">
            <mat-icon>delete</mat-icon>
          </button>
        </mat-card-actions>
      </mat-card>
    </div>

    <mat-paginator
      [length]="totalCount"
      [pageSize]="pageSize"
      [pageSizeOptions]="[10, 20, 50]"
      (page)="onPage($event)" />
  `,
  styles: [`
    .header-row { display: flex; align-items: center; justify-content: space-between; margin-bottom: 20px; }
    h1 { margin: 0; }
    .section-title { margin: 28px 0 16px; font-size: 1.1rem; color: #555; font-weight: 600; }
    .center { display: flex; justify-content: center; padding: 48px; }
    .empty-state { display: flex; flex-direction: column; align-items: center; padding: 64px; color: #9e9e9e; }
    .empty-state mat-icon { font-size: 64px; width: 64px; height: 64px; margin-bottom: 12px; }

    /* KPI strip */
    .kpi-strip {
      display: flex; gap: 16px; flex-wrap: wrap; margin-bottom: 24px;
    }
    .kpi-card {
      flex: 1; min-width: 100px; border-radius: 12px; padding: 16px 20px;
      display: flex; flex-direction: column; align-items: center;
      background: #f5f5f5; border-top: 4px solid #9e9e9e;
    }
    .kpi-total { background: #e8eaf6 !important; border-top-color: #3949ab !important; }
    .kpi-status-draft { background: #f5f5f5 !important; border-top-color: #9e9e9e !important; }
    .kpi-status-submitted { background: #fff8e1 !important; border-top-color: #ffa000 !important; }
    .kpi-status-approved { background: #f1f8e9 !important; border-top-color: #558b2f !important; }
    .kpi-status-rejected { background: #fce4ec !important; border-top-color: #c62828 !important; }
    .kpi-status-published { background: #e3f2fd !important; border-top-color: #1565c0 !important; }
    .kpi-value { font-size: 2rem; font-weight: 700; line-height: 1; }
    .kpi-label { font-size: 0.8rem; color: #666; margin-top: 4px; text-transform: uppercase; letter-spacing: 0.5px; }

    /* Charts */
    .charts-row {
      display: grid;
      grid-template-columns: 300px 1fr;
      gap: 24px;
      margin-bottom: 8px;
    }
    .chart-card { border-radius: 14px; }
    .chart-card mat-card-content { display: flex; justify-content: center; align-items: center; padding: 16px; max-height: 260px; }
    canvas { max-height: 220px !important; }

    /* Analytics cards */
    .card-grid {
      display: grid;
      grid-template-columns: repeat(auto-fill, minmax(380px, 1fr));
      gap: 24px;
      margin-bottom: 20px;
    }
    .analytics-card {
      border-radius: 14px; transition: box-shadow 0.2s, transform 0.15s;
      display: flex; flex-direction: column;
    }
    .analytics-card:hover { box-shadow: 0 8px 28px rgba(0,0,0,0.2) !important; transform: translateY(-3px); }
    mat-card-header { display: flex; justify-content: space-between; align-items: center; padding-bottom: 4px; }
    mat-card-title { margin: 0 !important; }
    mat-card-title a { text-decoration: none; color: inherit; font-size: 1.15rem; font-weight: 700; }
    mat-card-title a:hover { color: #1565c0; }
    .status-chip { font-size: 0.72rem; font-weight: 600; margin-left: 8px; }
    mat-card-content { flex: 1; padding-top: 12px !important; }

    /* Card body: chart left, info right */
    .card-body { display: flex; gap: 16px; align-items: flex-start; }
    .mini-chart-wrap { display: flex; flex-direction: column; align-items: center; flex-shrink: 0; }
    .chart-caption { font-size: 0.68rem; color: #888; margin-top: 2px; text-transform: uppercase; letter-spacing: 0.5px; }
    .card-info { flex: 1; min-width: 0; }
    .description {
      margin: 0 0 12px; font-size: 0.88rem; color: #555; line-height: 1.55;
      display: -webkit-box; -webkit-line-clamp: 3; -webkit-box-orient: vertical; overflow: hidden;
    }
    .value-row { display: flex; align-items: center; gap: 4px; font-size: 0.85rem; color: #444; }
    .value-icon { font-size: 16px; width: 16px; height: 16px; color: #7e57c2; }
    .value-label { font-weight: 600; }
    .value-num { color: #7e57c2; font-weight: 700; font-size: 1rem; }

    /* Status colours */
    .card-status-0 { background-color: #f5f5f5 !important; border-left: 5px solid #9e9e9e; }
    .card-status-1 { background-color: #fff8e1 !important; border-left: 5px solid #ffa000; }
    .card-status-2 { background-color: #f1f8e9 !important; border-left: 5px solid #558b2f; }
    .card-status-3 { background-color: #fce4ec !important; border-left: 5px solid #c62828; }
    .card-status-4 { background-color: #e3f2fd !important; border-left: 5px solid #1565c0; }
    .status-0 { background-color: #e0e0e0 !important; }
    .status-1 { background-color: #ffe082 !important; }
    .status-2 { background-color: #aed581 !important; }
    .status-3 { background-color: #ef9a9a !important; }
    .status-4 { background-color: #90caf9 !important; }
  `]
})
export class AnalyticsListComponent implements OnInit {
  items: AnalyticsListItem[] = [];
  summary: AnalyticsSummary | null = null;
  loading = false;
  totalCount = 0;
  page = 1;
  pageSize = 20;

  doughnutData: ChartData<'doughnut'> = { labels: [], datasets: [] };
  doughnutOptions: ChartOptions<'doughnut'> = {
    responsive: true,
    plugins: { legend: { position: 'bottom' } }
  };

  barData: ChartData<'bar'> = { labels: [], datasets: [] };
  barOptions: ChartOptions<'bar'> = {
    responsive: true,
    plugins: { legend: { display: false } },
    scales: { y: { beginAtZero: true, ticks: { stepSize: 1 } } }
  };

  constructor(
    private analyticsService: AnalyticsService,
    private snackBar: MatSnackBar
  ) {}

  ngOnInit(): void {
    this.loadSummary();
    this.load();
  }

  loadSummary(): void {
    this.analyticsService.getSummary().subscribe({
      next: res => {
        if (!res.data) return;
        this.summary = res.data;
        this.buildCharts(res.data);
      }
    });
  }

  buildCharts(s: AnalyticsSummary): void {
    const ordered = STATUS_LABELS.map(label => {
      const found = s.countByStatus.find(x => x.status === label);
      return found?.count ?? 0;
    });

    const nonZeroLabels: string[] = [];
    const nonZeroCounts: number[] = [];
    const nonZeroColors: string[] = [];
    const nonZeroBorders: string[] = [];
    STATUS_LABELS.forEach((label, i) => {
      if (ordered[i] > 0) {
        nonZeroLabels.push(label);
        nonZeroCounts.push(ordered[i]);
        nonZeroColors.push(STATUS_COLORS[i]);
        nonZeroBorders.push(STATUS_BORDER[i]);
      }
    });

    this.doughnutData = {
      labels: nonZeroLabels,
      datasets: [{
        data: nonZeroCounts,
        backgroundColor: nonZeroColors,
        borderColor: nonZeroBorders,
        borderWidth: 2
      }]
    };

    this.barData = {
      labels: STATUS_LABELS,
      datasets: [{
        data: ordered,
        backgroundColor: STATUS_COLORS,
        borderColor: STATUS_BORDER,
        borderWidth: 2,
        borderRadius: 6
      }]
    };
  }

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

  onPage(e: PageEvent): void {
    this.page = e.pageIndex + 1;
    this.pageSize = e.pageSize;
    this.load();
  }

  delete(item: AnalyticsListItem): void {
    if (!confirm(`Delete "${item.name}"?`)) return;
    this.analyticsService.delete(item.id).subscribe({
      next: () => {
        this.snackBar.open('Deleted successfully', '', { duration: 2000 });
        this.loadSummary();
        this.load();
      },
      error: () => this.snackBar.open('Delete failed', 'Dismiss', { duration: 3000 })
    });
  }

  // ── Mini donut helpers ────────────────────────────────────────────────────

  private static readonly STATUS_COLORS_SVG = ['#9e9e9e', '#ffa000', '#558b2f', '#c62828', '#1565c0'];
  // Workflow progress per status: Draft=8%, Submitted=30%, Approved=60%, Rejected=15%, Published=100%
  private static readonly STATUS_PROGRESS = [0.08, 0.30, 0.60, 0.15, 1.0];

  statusColor(status: number): string {
    return AnalyticsListComponent.STATUS_COLORS_SVG[status] ?? '#9e9e9e';
  }

  dashOffset(status: number): number {
    const circumference = 188.5;
    const progress = AnalyticsListComponent.STATUS_PROGRESS[status] ?? 0;
    return circumference * (1 - progress);
  }

  progressLabel(status: number): string {
    if (status === 3) return '✕';
    return `${Math.round((AnalyticsListComponent.STATUS_PROGRESS[status] ?? 0) * 100)}%`;
  }
}
