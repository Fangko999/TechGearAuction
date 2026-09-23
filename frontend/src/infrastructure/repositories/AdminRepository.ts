import { IAdminRepository } from '@domain/repositories/IAdminRepository';
import { ReportDto, ChatHistoryDto, AdminActionRequest, MetricsOverviewDto, UserDto, BlacklistDto } from '@domain/models/admin';
import { PagedResult } from '@domain/models/auction';
import axiosClient from '../api/axiosClient';

export class AdminRepository implements IAdminRepository {
  async getReports(pageIndex: number = 1, pageSize: number = 10, status?: string): Promise<PagedResult<ReportDto>> {
    let url = `/admin/reports?pageIndex=${pageIndex}&pageSize=${pageSize}`;
    if (status && status !== 'All') url += `&status=${status}`;
    const response = await axiosClient.get<any, PagedResult<ReportDto>>(url);
    return response;
  }

  async getReportDetail(id: string): Promise<ReportDto> {
    const response = await axiosClient.get<any, ReportDto>(`/admin/reports/${id}`);
    return response;
  }

  async getChatHistory(reportId: string): Promise<ChatHistoryDto[]> {
    const response = await axiosClient.get<any, ChatHistoryDto[]>(`/admin/reports/${reportId}/chat-history`);
    return response;
  }

  async resolveReport(request: AdminActionRequest): Promise<void> {
    await axiosClient.put(`/admin/reports/${request.reportId}/resolve`, request);
  }

  async getMetricsOverview(): Promise<MetricsOverviewDto> {
    const response = await axiosClient.get<any, MetricsOverviewDto>('/admin/metrics/overview');
    return response;
  }

  async getUsers(pageIndex: number = 1, pageSize: number = 10, searchTerm?: string): Promise<PagedResult<UserDto>> {
    let url = `/admin/users?pageIndex=${pageIndex}&pageSize=${pageSize}`;
    if (searchTerm) url += `&searchTerm=${searchTerm}`;
    const response = await axiosClient.get<any, PagedResult<UserDto>>(url);
    return response;
  }

  async getBlacklists(): Promise<BlacklistDto[]> {
    const response = await axiosClient.get<any, PagedResult<BlacklistDto>>('/admin/blacklists');
    return response.items || response as any;
  }

  async banUser(userId: string, reason: string): Promise<void> {
    await axiosClient.put(`/admin/users/${userId}/ban`, { targetUserId: userId, reason });
  }

  async unbanUser(userId: string, reason: string): Promise<void> {
    await axiosClient.put(`/admin/users/${userId}/unban`, { targetUserId: userId, reason });
  }

  async closeAccount(userId: string): Promise<void> {
    await axiosClient.put(`/admin/users/${userId}/close`);
  }

  async restoreAccount(userId: string): Promise<void> {
    await axiosClient.put(`/admin/users/${userId}/restore`);
  }
}

