export interface ApiResponse<T> {
  success: boolean;
  data: T | null;
  message: string | null;
  errors: string[] | null;
}

export interface PagedResult<T> {
  items: T[];
  totalCount: number;
  page: number;
  pageSize: number;
}

export interface Analytics {
  id: string;
  name: string;
  description: string;
  status: number;
  statusName: string;
}

export interface AnalyticsListItem {
  id: string;
  name: string;
  description?: string;
  value: number;
  status: number;
  statusName: string;
}

export interface CreateAnalytics {
  name: string;
  description: string;
}

export interface UpdateAnalytics {
  name: string;
  description: string;
}

export interface ReviewAnalytics {
  reviewedBy: string;
  comments?: string;
}

export interface AnalyticsRun {
  id: string;
  analyticsId: string;
  jobId: string;
  status: number;
  statusName: string;
  startedOn: string;
  completedOn?: string;
  terminatedOn?: string;
  inputJson?: string;
  outputJson?: string;
}

export interface StartAnalyticsRun {
  jobId: string;
  startedBy: string;
  inputJson?: string;
  outputJson?: string;
}

export interface StopAnalyticsRun {
  stoppedBy: string;
}

export interface HistoryItem {
  id: string;
  entityType: string;
  entityJson: string;
  actionType: string;
  actionBy: string;
  actionOn: string;
}

export const AnalyticsStatusLabels: Record<number, string> = {
  0: 'Draft', 1: 'Submitted', 2: 'Approved', 3: 'Rejected', 4: 'Published'
};

export const AnalyticsRunStatusLabels: Record<number, string> = {
  0: 'Queued', 1: 'Started', 2: 'Completed', 3: 'Terminated'
};

export interface StatusCount {
  status: string;
  count: number;
  totalValue: number;
}

export interface AnalyticsSummary {
  totalCount: number;
  totalValue: number;
  averageValue: number;
  countByStatus: StatusCount[];
}

export interface ChatMessage {
  role: 'user' | 'assistant';
  content: string;
}

export interface ChatRequest {
  message: string;
  history: ChatMessage[];
}

export interface ChatResponse {
  reply: string;
}
