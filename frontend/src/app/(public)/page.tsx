'use client';

import React from 'react';
import { useAuctions } from '@application/hooks/useAuctions';
import { AuctionCard } from '@presentation/components/AuctionCard';

export default function HomePage() {
  // Hardcode page 1, size 12 for the demo. In a real app we'd have pagination controls.
  const { data, isLoading, error } = useAuctions(1, 12);

  return (
    <main className="min-h-screen bg-gray-50">
      {/* Hero Banner */}
      <div className="bg-gray-900 py-16 sm:py-24">
        <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8 text-center">
          <h1 className="text-4xl font-extrabold tracking-tight text-white sm:text-5xl lg:text-6xl">
            Săn đồ Gear chuẩn
          </h1>
          <p className="mt-4 max-w-2xl mx-auto text-xl text-gray-300">
            Khám phá và tham gia đấu giá các món đồ công nghệ, EDC, phụ kiện dã ngoại độc đáo nhất.
          </p>
        </div>
      </div>

      {/* Main Content */}
      <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8 py-12">
        <div className="flex items-center justify-between mb-8">
          <h2 className="text-2xl font-bold text-gray-900">Phiên đấu giá nổi bật</h2>
        </div>

        {error && (
          <div className="p-4 mb-8 bg-red-50 text-red-700 rounded-lg">
            {error}
          </div>
        )}

        {isLoading ? (
          // Skeleton Loading Grid
          <div className="grid grid-cols-2 md:grid-cols-4 gap-6">
            {[1, 2, 3, 4, 5, 6, 7, 8].map((n) => (
              <div key={n} className="flex flex-col bg-white border border-gray-200 rounded-xl overflow-hidden animate-pulse">
                <div className="w-full aspect-square bg-gray-200"></div>
                <div className="p-4 space-y-3">
                  <div className="h-4 bg-gray-200 rounded w-3/4"></div>
                  <div className="h-4 bg-gray-200 rounded w-1/2"></div>
                  <div className="h-6 bg-gray-200 rounded w-1/3 mt-4"></div>
                </div>
              </div>
            ))}
          </div>
        ) : (
          <>
            {data.items.length === 0 ? (
              <div className="text-center py-20 bg-white rounded-xl border border-dashed border-gray-300">
                <svg className="mx-auto h-12 w-12 text-gray-400" fill="none" viewBox="0 0 24 24" stroke="currentColor" aria-hidden="true">
                  <path strokeLinecap="round" strokeLinejoin="round" strokeWidth="1" d="M19 11H5m14 0a2 2 0 012 2v6a2 2 0 01-2 2H5a2 2 0 01-2-2v-6a2 2 0 012-2m14 0V9a2 2 0 00-2-2M5 11V9a2 2 0 012-2m0 0V5a2 2 0 012-2h6a2 2 0 012 2v2M7 7h10" />
                </svg>
                <h3 className="mt-2 text-sm font-medium text-gray-900">Không có dữ liệu</h3>
                <p className="mt-1 text-sm text-gray-500">Chưa có phiên đấu giá nào đang diễn ra lúc này.</p>
              </div>
            ) : (
              <div className="grid grid-cols-2 md:grid-cols-4 gap-6">
                {data.items.map((auction) => (
                  <AuctionCard key={auction.id} auction={auction} />
                ))}
              </div>
            )}
          </>
        )}
      </div>
    </main>
  );
}
