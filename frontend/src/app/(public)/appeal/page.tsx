'use client';

import React, { useState, useEffect } from 'react';
import Cookies from 'js-cookie';
import axiosClient from '@infrastructure/api/axiosClient';
import { useAuth } from '@application/hooks/useAuth';

export default function AppealPage() {
  const { handleLogout } = useAuth();
  const [banReason, setBanReason] = useState<string>('Bạn đã vi phạm quy chế của nền tảng.');
  const [bannedEmail, setBannedEmail] = useState<string>('');
  
  const [description, setDescription] = useState('');
  const [files, setFiles] = useState<FileList | null>(null);
  const [isSubmitting, setIsSubmitting] = useState(false);
  const [submitSuccess, setSubmitSuccess] = useState(false);
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    const reason = Cookies.get('banReason');
    const email = Cookies.get('bannedEmail');
    if (reason) setBanReason(reason);
    if (email) setBannedEmail(email);
  }, []);

  const onSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    if (!description.trim()) {
      setError('Vui lòng nhập lý do kháng cáo.');
      return;
    }

    setIsSubmitting(true);
    setError(null);

    try {
      // Gửi FormData tới API Bypass
      const formData = new FormData();
      formData.append('Email', bannedEmail);
      formData.append('Description', description);
      
      if (files) {
        for (let i = 0; i < files.length; i++) {
          formData.append('Evidences', files[i]);
        }
      }

      await axiosClient.post('/appeals/banned-users', formData, {
        headers: {
          'Content-Type': 'multipart/form-data',
        },
      });

      setSubmitSuccess(true);
    } catch (err: any) {
      setError(err?.Message || err?.message || 'Không thể gửi đơn kháng cáo. Thử lại sau.');
    } finally {
      setIsSubmitting(false);
    }
  };

  return (
    <div className="flex flex-col items-center justify-center min-h-screen p-4 bg-gray-900">
      <div className="w-full max-w-lg p-8 bg-white rounded-lg shadow-2xl">
        <div className="flex flex-col items-center mb-6">
          <div className="flex items-center justify-center w-16 h-16 bg-red-100 rounded-full">
            <svg className="w-8 h-8 text-red-600" fill="none" stroke="currentColor" viewBox="0 0 24 24">
              <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M12 9v2m0 4h.01m-6.938 4h13.856c1.54 0 2.502-1.667 1.732-3L13.732 4c-.77-1.333-2.694-1.333-3.464 0L3.34 16c-.77 1.333.192 3 1.732 3z" />
            </svg>
          </div>
          <h1 className="mt-4 text-2xl font-bold text-gray-900 uppercase">Tài khoản bị vô hiệu hóa</h1>
        </div>

        <div className="p-4 mb-6 border-l-4 border-red-500 bg-red-50">
          <p className="font-semibold text-red-800">Lý do:</p>
          <p className="text-red-700">{banReason}</p>
        </div>

        {submitSuccess ? (
          <div className="p-6 text-center bg-green-50 rounded-xl">
            <h2 className="text-lg font-bold text-green-800">Đã gửi đơn kháng cáo!</h2>
            <p className="mt-2 text-green-700">Ban quản trị sẽ xem xét bằng chứng của bạn và phản hồi qua Email. Vui lòng chờ đợi.</p>
            <button 
              onClick={handleLogout}
              className="px-6 py-2 mt-6 text-white bg-gray-800 rounded-lg hover:bg-gray-700"
            >
              Thoát ra ngoài
            </button>
          </div>
        ) : (
          <form onSubmit={onSubmit} className="space-y-4">
            <p className="text-sm text-gray-600">
              Nếu bạn tin rằng đây là sự nhầm lẫn, vui lòng điền form dưới đây để yêu cầu xem xét lại (kèm hình ảnh/video bằng chứng nếu có).
            </p>

            {error && (
              <p className="text-sm text-red-600">{error}</p>
            )}

            <div>
              <label className="block text-sm font-medium text-gray-700">Lý do giải oan</label>
              <textarea
                required
                rows={4}
                className="w-full px-3 py-2 mt-1 border border-gray-300 rounded-md focus:outline-none focus:ring-2 focus:ring-red-500"
                placeholder="Trình bày chi tiết vấn đề..."
                value={description}
                onChange={(e) => setDescription(e.target.value)}
                disabled={isSubmitting}
              />
            </div>

            <div>
              <label className="block text-sm font-medium text-gray-700">Bằng chứng đính kèm (Tùy chọn)</label>
              <input
                type="file"
                multiple
                accept="image/*,video/*"
                className="w-full px-3 py-2 mt-1 border border-gray-300 rounded-md focus:outline-none focus:ring-2 focus:ring-red-500"
                onChange={(e) => setFiles(e.target.files)}
                disabled={isSubmitting}
              />
            </div>

            <div className="flex gap-4 pt-4">
              <button
                type="button"
                onClick={handleLogout}
                disabled={isSubmitting}
                className="flex-1 px-4 py-2 text-gray-700 bg-gray-200 rounded-md hover:bg-gray-300 focus:outline-none"
              >
                Đăng xuất
              </button>
              
              <button
                type="submit"
                disabled={isSubmitting}
                className="flex-1 px-4 py-2 font-medium text-white bg-red-600 rounded-md hover:bg-red-700 focus:outline-none focus:ring-2 focus:ring-red-500 disabled:opacity-50"
              >
                {isSubmitting ? 'Đang gửi...' : 'Gửi kháng cáo'}
              </button>
            </div>
          </form>
        )}
      </div>
    </div>
  );
}

