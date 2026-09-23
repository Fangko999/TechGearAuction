'use client';

import React from 'react';
import Link from 'next/link';
import { useAuctions } from '@application/hooks/useAuctions';
import { AuctionCard } from '@presentation/components/AuctionCard';

export default function AuctionsListPage() {
  const { data, isLoading: loading, error } = useAuctions();
  const auctions = data?.items || [];
  const hasMore = false;
  const loadMore = () => {};

  return (
    <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8 py-8">
      <div className="flex justify-between items-center mb-8">
        <h1 className="text-3xl font-bold text-gray-900">Sáº£n pháº©m Ä‘ang Ä‘áº¥u giÃ¡</h1>
      </div>

      {error && (
        <div className="bg-red-50 text-red-700 p-4 rounded-lg mb-8">
          ÄÃ£ cÃ³ lá»—i xáº£y ra: {error}
        </div>
      )}

      {auctions.length === 0 && !loading && !error ? (
        <div className="text-center py-12 bg-white rounded-lg shadow-sm border border-gray-100">
          <svg className="mx-auto h-12 w-12 text-gray-400" fill="none" stroke="currentColor" viewBox="0 0 24 24">
            <path strokeLinecap="round" strokeLinejoin="round" strokeWidth="2" d="M20 13V6a2 2 0 00-2-2H6a2 2 0 00-2 2v7m16 0v5a2 2 0 01-2 2H6a2 2 0 01-2-2v-5m16 0h-2.586a1 1 0 00-.707.293l-2.414 2.414a1 1 0 01-.707.293h-3.172a1 1 0 01-.707-.293l-2.414-2.414A1 1 0 006.586 13H4" />
          </svg>
          <h3 className="mt-2 text-sm font-medium text-gray-900">KhÃ´ng cÃ³ sáº£n pháº©m</h3>
          <p className="mt-1 text-sm text-gray-500">Hiá»‡n táº¡i khÃ´ng cÃ³ phiÃªn Ä‘áº¥u giÃ¡ nÃ o Ä‘ang diá»…n ra.</p>
        </div>
      ) : (
        <>
          <div className="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-4 gap-6">
            {auctions.map((auction: any) => (
              <AuctionCard key={auction.id} auction={auction} />
            ))}
          </div>
          
          {loading && (
            <div className="flex justify-center mt-8">
              <div className="animate-spin rounded-full h-8 w-8 border-b-2 border-blue-600"></div>
            </div>
          )}

          {hasMore && !loading && (
            <div className="flex justify-center mt-8">
              <button
                onClick={loadMore}
                className="px-6 py-2 bg-white border border-gray-300 text-gray-700 rounded-lg hover:bg-gray-50 font-medium transition-colors"
              >
                Táº£i thÃªm
              </button>
            </div>
          )}
        </>
      )}
    </div>
  );
}

