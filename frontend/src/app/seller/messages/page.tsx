'use client';

import React from 'react';
import Link from 'next/link';

export default function SellerMessagesPage() {
  return (
    <div className="space-y-6">
      <div>
        <h1 className="text-2xl font-bold text-gray-900">Tin nhắn</h1>
        <p className="text-gray-500 text-sm mt-1">Quản lý tin nhắn với người mua.</p>
      </div>

      <div className="bg-white border border-gray-200 rounded-xl overflow-hidden shadow-sm p-12 text-center">
        <svg className="mx-auto h-12 w-12 text-gray-400 mb-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
          <path strokeLinecap="round" strokeLinejoin="round" strokeWidth="2" d="M8 10h.01M12 10h.01M16 10h.01M9 16H5a2 2 0 01-2-2V6a2 2 0 012-2h14a2 2 0 012 2v8a2 2 0 01-2 2h-5l-5 5v-5z" />
        </svg>
        <h3 className="text-lg font-medium text-gray-900">Tính năng tin nhắn đang được tích hợp</h3>
        <p className="text-gray-500 mt-2 mb-6">Bạn có thể truy cập trang tin nhắn chung để liên lạc.</p>
        
        <Link 
          href="/chat" 
          className="inline-flex px-4 py-2 bg-blue-600 hover:bg-blue-700 text-white rounded-lg font-medium transition-colors"
        >
          Đi tới hộp thư chung
        </Link>
      </div>
    </div>
  );
}

