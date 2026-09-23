export enum ResolutionAction {
  Reject = 0,
  IssueStrike = 1,
  DirectBan = 2
}

export interface ReportDto {
  id: string;
  reporterId: string;
  reporterName: string;
  reportedUserId: string;
  reportedUserName: string;
  auctionId?: string;
  chatRoomId?: string;
  reason: string;
  description: string;
  status: string;
  createdAt: string;
  evidences: ReportEvidenceDto[];
}

export interface ReportEvidenceDto {
  id: string;
  mediaUrl: string;
  type: string;
}

export interface ChatHistoryDto {
  id: string;
  senderName: string;
  content: string;
  createdAt: string;
  messageType: string;
  mediaUrl?: string;
}

export interface AdminActionRequest {
  reportId: string;
  action: ResolutionAction;
  adminNote?: string;
}

export interface MetricsOverviewDto {
  totalUsers: number;
  activeAuctions: number;
  totalRevenue: number;
  pendingReports: number;
}

export interface UserDto {
  id: string;
  email: string;
  displayName: string;
  role: string;
  status: string;
  createdAt: string;
  violationCount: number;
}

export interface BlacklistDto {
  deviceHash: string;
  reason: string;
  createdAt: string;
}
