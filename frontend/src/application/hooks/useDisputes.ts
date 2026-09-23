import { useState, useEffect } from 'react';
import { AdminRepository } from '@infrastructure/repositories/AdminRepository';
import { ReportDto, ChatHistoryDto, ResolutionAction } from '@domain/models/admin';

const adminRepository = new AdminRepository();

export const useDisputes = (statusFilter: string = 'Pending') => {
  const [reports, setReports] = useState<ReportDto[]>([]);
  const [isLoading, setIsLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  const [selectedReport, setSelectedReport] = useState<ReportDto | null>(null);
  const [chatHistory, setChatHistory] = useState<ChatHistoryDto[]>([]);
  const [isDetailLoading, setIsDetailLoading] = useState(false);

  useEffect(() => {
    let isMounted = true;
    
    const fetchReports = async () => {
      setIsLoading(true);
      setError(null);
      try {
        const data = await adminRepository.getReports(1, 50, statusFilter);
        if (isMounted) setReports(data.items);
      } catch (err: any) {
        if (isMounted) setError(err?.Message || err?.message || 'Không thể tải danh sách Tố cáo.');
      } finally {
        if (isMounted) setIsLoading(false);
      }
    };

    fetchReports();

    return () => { isMounted = false; };
  }, [statusFilter]);

  const viewReportDetail = async (reportId: string) => {
    setIsDetailLoading(true);
    setChatHistory([]);
    try {
      const reportDetail = await adminRepository.getReportDetail(reportId);
      setSelectedReport(reportDetail);
      
      if (reportDetail.chatRoomId) {
        const chat = await adminRepository.getChatHistory(reportId);
        setChatHistory(chat);
      }
    } catch (err) {
      console.error('Failed to load detail', err);
      alert('Lỗi tải chi tiết đơn tố cáo.');
    } finally {
      setIsDetailLoading(false);
    }
  };

  const closeDetail = () => {
    setSelectedReport(null);
    setChatHistory([]);
  };

  const handleResolveDispute = async (reportId: string, action: ResolutionAction, adminNote: string = '') => {
    try {
      await adminRepository.resolveReport({ reportId, action, adminNote });
      
      // Update local state to remove the resolved report from the list
      setReports((prev) => prev.filter(r => r.id !== reportId));
      closeDetail();
      alert('Đã xử lý phán quyết thành công!');
    } catch (err: any) {
      alert(err?.Message || err?.message || 'Có lỗi xảy ra khi xử lý.');
    }
  };

  return {
    reports,
    isLoading,
    error,
    selectedReport,
    chatHistory,
    isDetailLoading,
    viewReportDetail,
    closeDetail,
    handleResolveDispute,
  };
};

