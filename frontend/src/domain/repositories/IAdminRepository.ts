import { ReportDto, ChatHistoryDto, AdminActionRequest, MetricsOverviewDto, UserDto, BlacklistDto } from '../models/admin';
import { PagedResult } from '../models/auction';

export interface IAdminRepository {
  getReports(pageIndex?: number, pageSize?: number, status?: string): Promise<PagedResult<ReportDto>>;
  getReportDetail(id: string): Promise<ReportDto>;
  getChatHistory(reportId: string): Promise<ChatHistoryDto[]>;
  resolveReport(request: AdminActionRequest): Promise<void>;
  
  getMetricsOverview(): Promise<MetricsOverviewDto>;
  getUsers(pageIndex?: number, pageSize?: number, searchTerm?: string): Promise<PagedResult<UserDto>>;
  getBlacklists(): Promise<BlacklistDto[]>;
  banUser(userId: string, reason: string): Promise<void>;
  unbanUser(userId: string, reason: string): Promise<void>;
  closeAccount(userId: string): Promise<void>;
  restoreAccount(userId: string): Promise<void>;
}

