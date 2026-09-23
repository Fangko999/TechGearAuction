'use client';

import React, { useState } from 'react';
import { useDisputes } from '@application/hooks/useDisputes';
import { ResolutionAction } from '@domain/models/admin';
import { ChatMessageType } from '@domain/models/chat';

export default function AdminReportsPage() {
  const [filter, setFilter] = useState('Pending');
  const { reports, isLoading, selectedReport, chatHistory, isDetailLoading, viewReportDetail, closeDetail, handleResolveDispute } = useDisputes(filter);
  
  const [adminNote, setAdminNote] = useState('');

  const getStatusBadge = (status: string) => {
    switch(status) {
      case 'Pending': return <span className="px-2.5 py-1 text-xs font-medium bg-yellow-900/50 text-yellow-500 rounded-full border border-yellow-800">Đang chờ</span>;
      case 'Processing': return <span className="px-2.5 py-1 text-xs font-medium bg-blue-900/50 text-blue-400 rounded-full border border-blue-800">Đang xử lý</span>;
      case 'Resolved': return <span className="px-2.5 py-1 text-xs font-medium bg-green-900/50 text-green-400 rounded-full border border-green-800">Đã giải quyết</span>;
      case 'Rejected': return <span className="px-2.5 py-1 text-xs font-medium bg-gray-800 text-gray-400 rounded-full border border-gray-700">Bị từ chối</span>;
      default: return <span className="px-2.5 py-1 text-xs font-medium bg-gray-800 text-gray-400 rounded-full">{status}</span>;
    }
  };

  const onResolve = (action: ResolutionAction) => {
    if (!selectedReport) return;
    
    if (action === ResolutionAction.DirectBan && !adminNote) {
      alert('Vui lòng nhập Admin Note khi khóa tài khoản!');
      return;
    }

    const actionText = action === ResolutionAction.Reject ? 'Bác bỏ' : 
                       action === ResolutionAction.IssueStrike ? 'Ghi lỗi' : 'Khóa (Ban)';

    if (confirm(`Bạn có chắc chắn muốn thực hiện hành động: ${actionText}?`)) {
      handleResolveDispute(selectedReport.id, action, adminNote);
    }
  };

  return (
    <div className="space-y-6">
      <div className="flex justify-between items-center">
        <h1 className="text-2xl font-bold text-white">Quản lý Đơn Tố Cáo</h1>
        <select 
          value={filter}
          onChange={(e) => setFilter(e.target.value)}
          className="bg-gray-800 border border-gray-700 text-gray-200 text-sm rounded-lg focus:ring-red-500 focus:border-red-500 block p-2.5"
        >
          <option value="All">Tất cả</option>
          <option value="Pending">Đang chờ (Pending)</option>
          <option value="Processing">Đang xử lý (Processing)</option>
          <option value="Resolved">Đã giải quyết (Resolved)</option>
        </select>
      </div>

      {/* Data Grid */}
      <div className="bg-gray-950 border border-gray-800 rounded-xl overflow-hidden shadow-lg">
        <div className="overflow-x-auto">
          <table className="w-full text-sm text-left text-gray-400">
            <thead className="text-xs text-gray-300 uppercase bg-gray-900 border-b border-gray-800">
              <tr>
                <th className="px-6 py-4">ID</th>
                <th className="px-6 py-4">Người kiện</th>
                <th className="px-6 py-4">Bị kiện</th>
                <th className="px-6 py-4">Lý do</th>
                <th className="px-6 py-4">Trạng thái</th>
                <th className="px-6 py-4">Ngày tạo</th>
                <th className="px-6 py-4 text-right">Hành động</th>
              </tr>
            </thead>
            <tbody>
              {isLoading ? (
                <tr>
                  <td colSpan={7} className="px-6 py-8 text-center">Đang tải dữ liệu...</td>
                </tr>
              ) : reports.length === 0 ? (
                <tr>
                  <td colSpan={7} className="px-6 py-8 text-center text-gray-500">Không có đơn tố cáo nào phù hợp.</td>
                </tr>
              ) : (
                reports.map((report) => (
                  <tr key={report.id} className="bg-gray-950 border-b border-gray-800 hover:bg-gray-900 transition-colors">
                    <td className="px-6 py-4 font-mono text-xs">{report.id.substring(0, 8)}</td>
                    <td className="px-6 py-4 text-white font-medium">{report.reporterName}</td>
                    <td className="px-6 py-4 text-red-400">{report.reportedUserName}</td>
                    <td className="px-6 py-4 max-w-xs truncate" title={report.reason}>{report.reason}</td>
                    <td className="px-6 py-4">{getStatusBadge(report.status)}</td>
                    <td className="px-6 py-4">{new Date(report.createdAt).toLocaleDateString('vi-VN')}</td>
                    <td className="px-6 py-4 text-right">
                      <button 
                        onClick={() => viewReportDetail(report.id)}
                        className="font-medium text-blue-400 hover:text-blue-300 hover:underline"
                      >
                        Phân xử
                      </button>
                    </td>
                  </tr>
                ))
              )}
            </tbody>
          </table>
        </div>
      </div>

      {/* Detail Modal */}
      {selectedReport && (
        <div className="fixed inset-0 z-50 flex items-center justify-center bg-black/80 backdrop-blur-sm p-4 sm:p-8">
          <div className="bg-gray-900 w-full max-w-6xl rounded-2xl shadow-2xl border border-gray-700 flex flex-col h-[90vh] overflow-hidden">
            
            {/* Modal Header */}
            <div className="flex justify-between items-center px-6 py-4 border-b border-gray-800 bg-gray-950">
              <div>
                <h3 className="text-xl font-bold text-white">Chi tiết Tố cáo #{selectedReport.id.substring(0, 8)}</h3>
                <p className="text-sm text-gray-400 mt-1">
                  Người kiện: <span className="text-white">{selectedReport.reporterName}</span> 
                  {'  '}➔{'  '} 
                  Bị kiện: <span className="text-red-400">{selectedReport.reportedUserName}</span>
                </p>
              </div>
              <button onClick={closeDetail} className="text-gray-400 hover:text-white p-2 rounded-lg hover:bg-gray-800 transition-colors">
                <svg className="w-6 h-6" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M6 18L18 6M6 6l12 12" /></svg>
              </button>
            </div>

            {/* Modal Body: Split view */}
            {isDetailLoading ? (
              <div className="flex-1 flex items-center justify-center">
                <div className="w-10 h-10 border-4 border-red-500 border-t-transparent rounded-full animate-spin"></div>
              </div>
            ) : (
              <div className="flex-1 flex overflow-hidden">
                
                {/* Left Column: Evidence */}
                <div className="w-1/2 p-6 border-r border-gray-800 overflow-y-auto bg-gray-900">
                  <h4 className="text-lg font-semibold text-white mb-4 border-b border-gray-800 pb-2">Bằng chứng (Media)</h4>
                  
                  <div className="bg-gray-800/50 p-4 rounded-lg mb-6 border border-gray-700">
                    <p className="text-sm font-medium text-gray-300 mb-2">Lý do: {selectedReport.reason}</p>
                    <p className="text-sm text-gray-400 whitespace-pre-wrap">{selectedReport.description}</p>
                  </div>

                  <div className="grid grid-cols-2 gap-4">
                    {selectedReport.evidences?.map((ev) => (
                      <div key={ev.id} className="bg-gray-950 rounded-lg overflow-hidden border border-gray-800">
                        {ev.type === 'Image' ? (
                          <img src={ev.mediaUrl} alt="Bằng chứng" className="w-full h-48 object-cover hover:scale-105 transition-transform" />
                        ) : ev.type === 'Video' ? (
                          <video src={ev.mediaUrl} controls className="w-full h-48 bg-black object-contain" />
                        ) : (
                          <div className="w-full h-48 flex items-center justify-center text-gray-500">
                            Không hỗ trợ định dạng
                          </div>
                        )}
                        <div className="p-2 text-xs text-center text-gray-400 border-t border-gray-800 bg-gray-900">
                          {ev.type}
                        </div>
                      </div>
                    ))}
                    {(!selectedReport.evidences || selectedReport.evidences.length === 0) && (
                      <div className="col-span-2 py-8 text-center text-gray-500 border border-dashed border-gray-700 rounded-lg">
                        Người dùng không cung cấp file bằng chứng.
                      </div>
                    )}
                  </div>
                </div>

                {/* Right Column: Raw Chat History */}
                <div className="w-1/2 p-6 flex flex-col bg-gray-950">
                  <h4 className="text-lg font-semibold text-white mb-4 border-b border-gray-800 pb-2 flex justify-between items-center">
                    <span>Lịch sử Chat Thô (Bypass Privacy)</span>
                    <span className="text-xs font-normal px-2 py-1 bg-red-900/30 text-red-400 border border-red-900/50 rounded">Confidential</span>
                  </h4>
                  
                  <div className="flex-1 overflow-y-auto space-y-4 pr-2 custom-scrollbar">
                    {chatHistory.length === 0 ? (
                      <p className="text-center text-gray-600 mt-10">Không có dữ liệu chat hoặc không tìm thấy phòng.</p>
                    ) : (
                      chatHistory.map((msg) => (
                        <div key={msg.id} className="bg-gray-900 p-3 rounded-lg border border-gray-800">
                          <div className="flex justify-between items-baseline mb-1">
                            <span className={`text-xs font-bold ${msg.senderName === selectedReport.reportedUserName ? 'text-red-400' : 'text-blue-400'}`}>
                              {msg.senderName}
                            </span>
                            <span className="text-[10px] text-gray-500">{new Date(msg.createdAt).toLocaleString()}</span>
                          </div>
                          
                          {msg.messageType === ChatMessageType.Image && msg.mediaUrl && (
                            <img src={msg.mediaUrl} alt="Media" className="max-w-[200px] rounded mb-2 border border-gray-700" />
                          )}
                          
                          <p className="text-sm text-gray-300 font-mono leading-relaxed">{msg.content}</p>
                        </div>
                      ))
                    )}
                  </div>
                </div>
              </div>
            )}

            {/* Modal Footer / Actions */}
            <div className="p-4 border-t border-gray-800 bg-gray-900">
              <input 
                type="text"
                value={adminNote}
                onChange={(e) => setAdminNote(e.target.value)}
                placeholder="Ghi chú của Admin (Bắt buộc nếu Ban)..."
                className="w-full bg-gray-950 border border-gray-700 text-gray-300 text-sm rounded-lg focus:ring-red-500 focus:border-red-500 block p-2.5 mb-4"
              />
              <div className="flex justify-end space-x-4">
                <button 
                  onClick={() => onResolve(ResolutionAction.Reject)}
                  className="px-5 py-2.5 text-sm font-medium text-gray-300 bg-gray-800 border border-gray-700 rounded-lg hover:bg-gray-700 transition-colors"
                >
                  Bác bỏ (Reject)
                </button>
                <button 
                  onClick={() => onResolve(ResolutionAction.IssueStrike)}
                  className="px-5 py-2.5 text-sm font-medium text-yellow-500 bg-yellow-900/20 border border-yellow-900/50 rounded-lg hover:bg-yellow-900/40 transition-colors"
                >
                  Ghi lỗi (Issue Strike)
                </button>
                <button 
                  onClick={() => onResolve(ResolutionAction.DirectBan)}
                  className="px-5 py-2.5 text-sm font-bold text-white bg-red-600 border border-red-700 rounded-lg hover:bg-red-700 shadow-[0_0_15px_rgba(220,38,38,0.3)] transition-all"
                >
                  Khóa Tài Khoản/Thiết Bị (Ban)
                </button>
              </div>
            </div>

          </div>
        </div>
      )}
    </div>
  );
}
