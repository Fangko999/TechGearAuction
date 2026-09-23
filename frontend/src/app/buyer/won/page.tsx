'use client';

import React, { useEffect, useState } from 'react';
import axiosClient from '@infrastructure/api/axiosClient';
import { AuctionDto } from '@domain/models/auction';
import { AuctionCard } from '@presentation/components/AuctionCard';

export default function BuyerWonPage() {
  const [auctions, setAuctions] = useState<AuctionDto[]>([]);
  const [loading, setLoading] = useState(true);

  useEffect(() => {
    const fetchWon = async () => {
      try {
        const res = await axiosClient.get('/users/me/auctions/won');
        setAuctions(res.data?.items || []);
      } catch (err) {
        console.error('Failed to fetch won auctions', err);
      } finally {
        setLoading(false);
      }
    };
    fetchWon();
  }, []);

  return (
    <div className="space-y-6">
      <div>
        <h1 className="text-2xl font-bold text-gray-900">Sáº£n pháº©m Ä‘Ã£ tháº¯ng</h1>
        <p className="text-gray-500 text-sm mt-1">CÃ¡c sáº£n pháº©m báº¡n Ä‘Ã£ Ä‘áº¥u giÃ¡ thÃ nh cÃ´ng.</p>
      </div>

      {loading ? (
        <div className="flex justify-center py-12">
          <div className="animate-spin rounded-full h-8 w-8 border-b-2 border-blue-600"></div>
        </div>
      ) : auctions.length === 0 ? (
        <div className="bg-white rounded-xl shadow-sm border border-gray-100 p-12 text-center">
          <p className="text-gray-500">Báº¡n chÆ°a tháº¯ng phiÃªn Ä‘áº¥u giÃ¡ nÃ o.</p>
        </div>
      ) : (
        <div className="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-3 gap-6">
          {auctions.map((auction) => (
            <AuctionCard key={auction.id} auction={auction} />
          ))}
        </div>
      )}
    </div>
  );
}

