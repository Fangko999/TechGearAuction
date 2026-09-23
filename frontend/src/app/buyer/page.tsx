'use client';

import React from 'react';
import Link from 'next/link';
import { useAuthStore } from '@application/store/useAuthStore';

export default function BuyerDashboardPage() {
  const { user } = useAuthStore();

  return (
    <div className="space-y-6">
      <div className="border-b border-gray-200 pb-5">
        <h3 className="text-2xl font-bold leading-6 text-gray-900">Tổng quan tài khoản Buyer</h3>
        <p className="mt-2 max-w-4xl text-sm text-gray-500">Xin chào, {user?.displayName}. Theo dõi và quản lý các phiên đấu giá bạn đang tham gia tại đây.</p>
      </div>

      <div className="grid grid-cols-1 md:grid-cols-3 gap-6">
        <div className="bg-blue-50 border border-blue-100 rounded-xl p-6">
          <div className="flex items-center">
            <div className="p-3 bg-blue-100 rounded-lg text-blue-600">
              <svg className="w-6 h-6" fill="none" viewBox="0 0 24 24" stroke="currentColor"><path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M9 5H7a2 2 0 00-2 2v12a2 2 0 002 2h10a2 2 0 002-2V7a2 2 0 00-2-2h-2M9 5a2 2 0 002 2h2a2 2 0 002-2M9 5a2 2 0 012-2h2a2 2 0 012 2m-6 9l2 2 4-4" /></svg>
            </div>
            <div className="ml-4">
              <p className="text-sm font-medium text-gray-500">Đang đấu giá</p>
              <p className="text-2xl font-bold text-gray-900">--</p>
            </div>
          </div>
          <div className="mt-4">
            <Link href="/buyer/bids" className="text-sm font-medium text-blue-600 hover:text-blue-500">Xem chi tiết &rarr;</Link>
          </div>
        </div>

        <div className="bg-green-50 border border-green-100 rounded-xl p-6">
          <div className="flex items-center">
            <div className="p-3 bg-green-100 rounded-lg text-green-600">
              <svg className="w-6 h-6" fill="none" viewBox="0 0 24 24" stroke="currentColor"><path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M5 13l4 4L19 7" /></svg>
            </div>
            <div className="ml-4">
              <p className="text-sm font-medium text-gray-500">Đã chiến thắng</p>
              <p className="text-2xl font-bold text-gray-900">--</p>
            </div>
          </div>
          <div className="mt-4">
            <Link href="/buyer/won" className="text-sm font-medium text-green-600 hover:text-green-500">Xem chi tiết &rarr;</Link>
          </div>
        </div>

        <div className="bg-yellow-50 border border-yellow-100 rounded-xl p-6">
          <div className="flex items-center">
            <div className="p-3 bg-yellow-100 rounded-lg text-yellow-600">
              <svg className="w-6 h-6" fill="none" viewBox="0 0 24 24" stroke="currentColor"><path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M4.318 6.318a4.5 4.5 0 000 6.364L12 20.364l7.682-7.682a4.5 4.5 0 00-6.364-6.364L12 7.636l-1.318-1.318a4.5 4.5 0 00-6.364 0z" /></svg>
            </div>
            <div className="ml-4">
              <p className="text-sm font-medium text-gray-500">Danh sách theo dõi</p>
              <p className="text-2xl font-bold text-gray-900">--</p>
            </div>
          </div>
          <div className="mt-4">
            <Link href="/buyer/watchlist" className="text-sm font-medium text-yellow-600 hover:text-yellow-500">Xem chi tiết &rarr;</Link>
          </div>
        </div>
      </div>
    </div>
  );
}

