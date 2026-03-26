import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import {
  ApiResponse, AnalyticsRun, StartAnalyticsRun,
  StopAnalyticsRun, HistoryItem
} from '../../shared/models/models';

@Injectable({ providedIn: 'root' })
export class AnalyticsRunService {
  private readonly base = `${environment.apiBaseUrl}/api/analytics/run`;

  constructor(private http: HttpClient) {}

  // POST /api/analytics/run/start/{analyticsId}
  start(analyticsId: string, dto: StartAnalyticsRun): Observable<ApiResponse<AnalyticsRun>> {
    return this.http.post<ApiResponse<AnalyticsRun>>(`${this.base}/start/${analyticsId}`, dto);
  }

  // POST /api/analytics/run/stop/{runId}
  stop(runId: string, dto: StopAnalyticsRun): Observable<ApiResponse<AnalyticsRun>> {
    return this.http.post<ApiResponse<AnalyticsRun>>(`${this.base}/stop/${runId}`, dto);
  }

  // GET /api/analytics/run/get/{runId}
  getById(runId: string): Observable<ApiResponse<AnalyticsRun>> {
    return this.http.get<ApiResponse<AnalyticsRun>>(`${this.base}/get/${runId}`);
  }

  // GET /api/analytics/run/history/{analyticsId}
  getHistory(analyticsId: string): Observable<ApiResponse<HistoryItem[]>> {
    return this.http.get<ApiResponse<HistoryItem[]>>(`${this.base}/history/${analyticsId}`);
  }
}
