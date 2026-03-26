import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import {
  ApiResponse, Analytics, AnalyticsListItem, CreateAnalytics,
  UpdateAnalytics, ReviewAnalytics, PagedResult
} from '../../shared/models/models';

@Injectable({ providedIn: 'root' })
export class AnalyticsService {
  private readonly base = `${environment.apiBaseUrl}/api/analytics`;

  constructor(private http: HttpClient) {}

  // GET /api/analytics/manage/list
  list(page = 1, pageSize = 20): Observable<ApiResponse<PagedResult<AnalyticsListItem>>> {
    const params = new HttpParams().set('page', page).set('pageSize', pageSize);
    return this.http.get<ApiResponse<PagedResult<AnalyticsListItem>>>(`${this.base}/manage/list`, { params });
  }

  // GET /api/analytics/manage/{id}
  getById(id: string): Observable<ApiResponse<Analytics>> {
    return this.http.get<ApiResponse<Analytics>>(`${this.base}/manage/${id}`);
  }

  // POST /api/analytics/manage/create
  create(dto: CreateAnalytics): Observable<ApiResponse<Analytics>> {
    return this.http.post<ApiResponse<Analytics>>(`${this.base}/manage/create`, dto);
  }

  // PUT /api/analytics/manage/{id}
  update(id: string, dto: UpdateAnalytics): Observable<ApiResponse<Analytics>> {
    return this.http.put<ApiResponse<Analytics>>(`${this.base}/manage/${id}`, dto);
  }

  // DELETE /api/analytics/manage/{id}
  delete(id: string): Observable<ApiResponse<boolean>> {
    return this.http.delete<ApiResponse<boolean>>(`${this.base}/manage/${id}`);
  }

  // POST /api/analytics/manage/submit/{id}
  submit(id: string): Observable<ApiResponse<Analytics>> {
    return this.http.post<ApiResponse<Analytics>>(`${this.base}/manage/submit/${id}`, {});
  }

  // POST /api/analytics/manage/publish/{id}
  publish(id: string): Observable<ApiResponse<Analytics>> {
    return this.http.post<ApiResponse<Analytics>>(`${this.base}/manage/publish/${id}`, {});
  }

  // POST /api/analytics/review/approve/{id}
  approve(id: string, dto: ReviewAnalytics): Observable<ApiResponse<Analytics>> {
    return this.http.post<ApiResponse<Analytics>>(`${this.base}/review/approve/${id}`, dto);
  }

  // POST /api/analytics/review/reject/{id}
  reject(id: string, dto: ReviewAnalytics): Observable<ApiResponse<Analytics>> {
    return this.http.post<ApiResponse<Analytics>>(`${this.base}/review/reject/${id}`, dto);
  }
}
