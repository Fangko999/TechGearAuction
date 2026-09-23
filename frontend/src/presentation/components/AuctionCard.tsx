"use client";

import React, { useEffect, useState } from 'react';
import Link from 'next/link';
import { AuctionDto } from '@domain/models/auction';

interface AuctionCardProps {
  auction: AuctionDto;
}

export const AuctionCard: React.FC<AuctionCardProps> = ({ auction }) => {
  const [timeLeftStr, setTimeLeftStr] = useState<string>('');
  const [isEndingSoon, setIsEndingSoon] = useState<boolean>(false);

  useEffect(() => {
    const updateCountdown = () => {
      const end = new Date(auction.endTime).getTime();
      const now = new Date().getTime();
      const distance = end - now;

      if (distance < 0) {
        setTimeLeftStr('Đã kết thúc');
        setIsEndingSoon(false);
        return;
      }

      const days = Math.floor(distance / (1000 * 60 * 60 * 24));
      const hours = Math.floor((distance % (1000 * 60 * 60 * 24)) / (1000 * 60 * 60));
      const minutes = Math.floor((distance % (1000 * 60 * 60)) / (1000 * 60));
      const seconds = Math.floor((distance % (1000 * 60)) / 1000);

      // Effect FOMO: Red color if less than 24 hours left
      setIsEndingSoon(days === 0);

      if (days > 0) {
        setTimeLeftStr(`Còn ${days} ngày ${hours} giờ`);
      } else {
        setTimeLeftStr(`Còn ${hours}h ${minutes}p ${seconds}s`);
      }
    };

    updateCountdown();
    const intervalId = setInterval(updateCountdown, 1000);

    return () => clearInterval(intervalId);
  }, [auction.endTime]);

  const formattedPrice = new Intl.NumberFormat('vi-VN', {
    style: 'currency',
    currency: 'VND',
  }).format(auction.currentPrice);

  return (
    <Link href={`/auctions/${auction.id}`} className="group flex flex-col bg-white border border-gray-200 rounded-xl overflow-hidden hover:shadow-lg transition-all duration-300">
      {/* Thumbnail */}
      <div className="relative w-full aspect-square bg-gray-100 overflow-hidden">
        {auction.primaryImageUrl ? (
          <img 
            src={auction.primaryImageUrl} 
            alt={auction.title} 
            className="w-full h-full object-cover group-hover:scale-105 transition-transform duration-500" 
          />
        ) : (
          <div className="w-full h-full flex items-center justify-center text-gray-400">
            <svg className="w-12 h-12" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path strokeLinecap="round" strokeLinejoin="round" strokeWidth={1.5} d="M4 16l4.586-4.586a2 2 0 012.828 0L16 16m-2-2l1.586-1.586a2 2 0 012.828 0L20 14m-6-6h.01M6 20h12a2 2 0 002-2V6a2 2 0 00-2-2H6a2 2 0 00-2 2v12a2 2 0 002 2z" /></svg>
          </div>
        )}
        
        {/* Category Badge */}
        <div className="absolute top-2 left-2 px-2 py-1 bg-white/90 backdrop-blur-sm text-xs font-medium text-gray-700 rounded-md shadow-sm">
          {auction.categoryName}
        </div>
      </div>

      {/* Content */}
      <div className="flex flex-col flex-1 p-4">
        <h3 className="text-sm font-medium text-gray-900 line-clamp-2 min-h-[40px] group-hover:text-blue-600 transition-colors">
          {auction.title}
        </h3>
        
        <div className="mt-3 flex items-end justify-between">
          <div>
            <p className="text-xs text-gray-500 mb-1">Giá hiện tại</p>
            <p className="text-lg font-bold text-gray-900">{formattedPrice}</p>
          </div>
        </div>

        {/* Footer info */}
        <div className="mt-4 pt-3 border-t border-gray-100 flex items-center justify-between text-xs">
          <div className="flex items-center text-gray-500">
            <svg className="w-4 h-4 mr-1" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M16 7a4 4 0 11-8 0 4 4 0 018 0zM12 14a7 7 0 00-7 7h14a7 7 0 00-7-7z" /></svg>
            <span className="truncate max-w-[80px]">{auction.sellerName}</span>
          </div>
          
          <div className={`flex items-center font-medium ${isEndingSoon ? 'text-red-600 animate-pulse' : 'text-gray-500'}`}>
            <svg className="w-4 h-4 mr-1" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M12 8v4l3 3m6-3a9 9 0 11-18 0 9 9 0 0118 0z" /></svg>
            {timeLeftStr}
          </div>
        </div>
      </div>
    </Link>
  );
};

