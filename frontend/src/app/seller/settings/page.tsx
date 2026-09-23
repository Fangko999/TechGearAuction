'use client';

import React, { useState } from 'react';
import { useAuthStore } from '@application/store/useAuthStore';
import axiosClient from '@infrastructure/api/axiosClient';

export default function SellerSettingsPage() {
  const user = useAuthStore(state => state.user);
  
  const [currentPassword, setCurrentPassword] = useState('');
  const [newPassword, setNewPassword] = useState('');
  const [confirmPassword, setConfirmPassword] = useState('');
  
  const [message, setMessage] = useState('');
  const [error, setError] = useState('');
  const [isLoading, setIsLoading] = useState(false);

  const handleUpdatePassword = async (e: React.FormEvent) => {
    e.preventDefault();
    if (newPassword !== confirmPassword) {
      setError('Mật khẩu xác nhận không khớp.');
      return;
    }

    setIsLoading(true);
    setError('');
    setMessage('');

    try {
      await axiosClient.put('/users/me/change-password', {
        currentPassword,
        newPassword
      });
      setMessage('Đổi mật khẩu thành công!');
      setCurrentPassword('');
      setNewPassword('');
      setConfirmPassword('');
    } catch (err: any) {
      setError(err?.response?.data?.Message || 'Đã có lỗi xảy ra.');
    } finally {
      setIsLoading(false);
    }
  };

  return (
    <div className="space-y-6 max-w-3xl">
      <div>
        <h1 className="text-2xl font-bold text-gray-900">Cài đặt tài khoản</h1>
        <p className="text-gray-500 text-sm mt-1">Cập nhật thông tin cá nhân và bảo mật.</p>
      </div>

      <div className="bg-white border border-gray-200 rounded-xl shadow-sm overflow-hidden">
        <div className="p-6 border-b border-gray-200">
          <h2 className="text-lg font-medium text-gray-900">Thông tin hồ sơ</h2>
          <div className="mt-4 grid grid-cols-1 gap-6 sm:grid-cols-2">
            <div>
              <label className="block text-sm font-medium text-gray-700">Tên hiển thị</label>
              <input type="text" readOnly value={user?.displayName || ''} className="mt-1 block w-full px-3 py-2 bg-gray-50 border border-gray-300 rounded-md shadow-sm text-gray-500 sm:text-sm" />
            </div>
            <div>
              <label className="block text-sm font-medium text-gray-700">Email</label>
              <input type="email" readOnly value={user?.email || ''} className="mt-1 block w-full px-3 py-2 bg-gray-50 border border-gray-300 rounded-md shadow-sm text-gray-500 sm:text-sm" />
            </div>
          </div>
        </div>

        <div className="p-6">
          <h2 className="text-lg font-medium text-gray-900">Đổi mật khẩu</h2>
          
          {message && (
            <div className="mt-4 p-3 bg-green-50 text-green-700 rounded-md text-sm">
              {message}
            </div>
          )}
          
          {error && (
            <div className="mt-4 p-3 bg-red-50 text-red-700 rounded-md text-sm">
              {error}
            </div>
          )}

          <form className="mt-4 space-y-4" onSubmit={handleUpdatePassword}>
            <div>
              <label className="block text-sm font-medium text-gray-700">Mật khẩu hiện tại</label>
              <input 
                type="password" 
                required 
                value={currentPassword}
                onChange={e => setCurrentPassword(e.target.value)}
                className="mt-1 block w-full px-3 py-2 border border-gray-300 rounded-md shadow-sm focus:ring-blue-500 focus:border-blue-500 sm:text-sm" 
              />
            </div>
            <div>
              <label className="block text-sm font-medium text-gray-700">Mật khẩu mới</label>
              <input 
                type="password" 
                required 
                value={newPassword}
                onChange={e => setNewPassword(e.target.value)}
                className="mt-1 block w-full px-3 py-2 border border-gray-300 rounded-md shadow-sm focus:ring-blue-500 focus:border-blue-500 sm:text-sm" 
              />
            </div>
            <div>
              <label className="block text-sm font-medium text-gray-700">Xác nhận mật khẩu mới</label>
              <input 
                type="password" 
                required 
                value={confirmPassword}
                onChange={e => setConfirmPassword(e.target.value)}
                className="mt-1 block w-full px-3 py-2 border border-gray-300 rounded-md shadow-sm focus:ring-blue-500 focus:border-blue-500 sm:text-sm" 
              />
            </div>
            
            <div className="pt-2">
              <button 
                type="submit" 
                disabled={isLoading}
                className="px-4 py-2 bg-blue-600 hover:bg-blue-700 text-white rounded-lg font-medium transition-colors disabled:opacity-50"
              >
                {isLoading ? 'Đang lưu...' : 'Lưu mật khẩu'}
              </button>
            </div>
          </form>
        </div>
      </div>
    </div>
  );
}

