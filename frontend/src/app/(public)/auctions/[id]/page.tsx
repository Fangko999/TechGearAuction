'use client';

import React, { useState, useEffect } from 'react';
import { useParams } from 'next/navigation';
import { useAuctionDetail } from '@application/hooks/useAuctionDetail';
import { useAuthStore } from '@application/store/useAuthStore';

export default function AuctionDetailPage() {
  const { id } = useParams() as { id: string };
  const { data, bidHistory, isLoading, error, isBidding, bidError, handlePlaceBid } = useAuctionDetail(id);
  const isAuthenticated = useAuthStore((state) => state.isAuthenticated);

  const [bidAmount, setBidAmount] = useState<number | ''>('');
  const [timeLeftStr, setTimeLeftStr] = useState<string>('');

  useEffect(() => {
    if (!data) return;

    const updateCountdown = () => {
      const end = new Date(data.endTime).getTime();
      const now = new Date().getTime();
      const distance = end - now;

      if (distance < 0) {
        setTimeLeftStr('Đã kết thúc');
        return;
      }

      const days = Math.floor(distance / (1000 * 60 * 60 * 24));
      const hours = Math.floor((distance % (1000 * 60 * 60 * 24)) / (1000 * 60 * 60));
      const minutes = Math.floor((distance % (1000 * 60 * 60)) / (1000 * 60));
      const seconds = Math.floor((distance % (1000 * 60)) / 1000);

      if (days > 0) {
        setTimeLeftStr(`${days} ngày ${hours} giờ ${minutes} phút`);
      } else {
        setTimeLeftStr(`${hours}h ${minutes}p ${seconds}s`);
      }
    };

    updateCountdown();
    const intervalId = setInterval(updateCountdown, 1000);

    return () => clearInterval(intervalId);
  }, [data?.endTime]);

  if (isLoading) {
    return (
      <div className="flex items-center justify-center min-h-screen bg-gray-50">
        <div className="w-10 h-10 border-4 border-blue-600 border-t-transparent rounded-full animate-spin"></div>
      </div>
    );
  }

  if (error || !data) {
    return (
      <div className="flex flex-col items-center justify-center min-h-screen bg-gray-50">
        <h2 className="text-2xl font-bold text-gray-900 mb-4">Lỗi</h2>
        <p className="text-gray-600">{error || 'Không tìm thấy phiên đấu giá.'}</p>
      </div>
    );
  }

  const formatPrice = (price: number) => new Intl.NumberFormat('vi-VN', { style: 'currency', currency: 'VND' }).format(price);
  
  const minBidAmount = data.currentPrice + data.bidIncrement;
  const isBidDisabled = isBidding || !isAuthenticated || (Number(bidAmount) < minBidAmount);

  const onSubmitBid = async (e: React.FormEvent) => {
    e.preventDefault();
    if (!bidAmount) return;
    await handlePlaceBid(Number(bidAmount));
    setBidAmount(''); // Reset input
  };

  return (
    <div className="min-h-screen bg-gray-50 py-12 px-4 sm:px-6 lg:px-8">
      <div className="max-w-7xl mx-auto">
        <div className="bg-white rounded-2xl shadow-sm overflow-hidden border border-gray-100">
          <div className="grid grid-cols-1 md:grid-cols-2">
            
            {/* Left Column: Images */}
            <div className="bg-gray-100 p-8 flex items-center justify-center relative min-h-[400px]">
              {data.primaryImageUrl ? (
                <img src={data.primaryImageUrl} alt={data.title} className="max-w-full max-h-[500px] object-contain drop-shadow-xl" />
              ) : (
                <div className="text-gray-400 flex flex-col items-center">
                  <svg className="w-20 h-20 mb-4" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                    <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={1} d="M4 16l4.586-4.586a2 2 0 012.828 0L16 16m-2-2l1.586-1.586a2 2 0 012.828 0L20 14m-6-6h.01M6 20h12a2 2 0 002-2V6a2 2 0 00-2-2H6a2 2 0 00-2 2v12a2 2 0 002 2z" />
                  </svg>
                  <span>Chưa có hình ảnh</span>
                </div>
              )}
            </div>

            {/* Right Column: Details & Bidding */}
            <div className="p-8 lg:p-12 flex flex-col">
              <div className="mb-2 flex items-center justify-between">
                <span className="inline-flex items-center px-3 py-1 rounded-full text-sm font-medium bg-blue-100 text-blue-800">
                  {data.categoryName}
                </span>
                <span className="text-sm text-gray-500">ID: {data.id.substring(0, 8)}...</span>
              </div>
              
              <h1 className="text-3xl font-bold text-gray-900 mb-4">{data.title}</h1>
              
              <div className="flex items-center space-x-4 mb-8 pb-8 border-b border-gray-100">
                <div className="w-10 h-10 rounded-full bg-gray-200 overflow-hidden flex items-center justify-center text-gray-500 font-bold">
                  {data.sellerAvatar ? <img src={data.sellerAvatar} alt="Seller" /> : data.sellerName[0].toUpperCase()}
                </div>
                <div>
                  <p className="text-sm font-medium text-gray-900">Bởi {data.sellerName}</p>
                  <p className="text-xs text-gray-500">Đánh giá: {data.sellerAverageRating} ({data.sellerTotalReviews} nhận xét)</p>
                </div>
              </div>

              {/* Price & Timer */}
              <div className="grid grid-cols-2 gap-6 mb-8">
                <div className="bg-blue-50 rounded-xl p-4 border border-blue-100">
                  <p className="text-sm text-blue-800 font-medium mb-1">Giá hiện tại</p>
                  {/* SignalR will update this value, adding animation class to highlight change could be an enhancement */}
                  <p className="text-3xl font-extrabold text-blue-900 transition-all duration-300 ease-in-out">
                    {formatPrice(data.currentPrice)}
                  </p>
                </div>
                <div className="bg-gray-50 rounded-xl p-4 border border-gray-100">
                  <p className="text-sm text-gray-500 font-medium mb-1">Thời gian còn lại</p>
                  <p className="text-2xl font-bold text-gray-900 text-red-600 font-mono tracking-tight">
                    {timeLeftStr}
                  </p>
                </div>
              </div>

              <div className="text-sm text-gray-600 mb-8 whitespace-pre-wrap flex-1">
                {data.description || 'Không có mô tả chi tiết.'}
              </div>

              {/* Bidding Form */}
              <div className="mt-auto pt-6 border-t border-gray-100">
                <form onSubmit={onSubmitBid} className="space-y-4">
                  {bidError && (
                    <div className="p-3 text-sm text-red-700 bg-red-100 rounded-lg">{bidError}</div>
                  )}
                  
                  <div>
                    <label className="block text-sm font-medium text-gray-700 mb-1">
                      Bước giá tối thiểu: {formatPrice(data.bidIncrement)}
                    </label>
                    <div className="flex rounded-md shadow-sm">
                      <span className="inline-flex items-center px-4 rounded-l-md border border-r-0 border-gray-300 bg-gray-50 text-gray-500 font-medium">
                        VNĐ
                      </span>
                      <input
                        type="number"
                        required
                        min={minBidAmount}
                        step={data.bidIncrement}
                        value={bidAmount}
                        onChange={(e) => setBidAmount(Number(e.target.value))}
                        disabled={isBidding || !isAuthenticated}
                        placeholder={`Tối thiểu ${formatPrice(minBidAmount)}`}
                        className="flex-1 min-w-0 block w-full px-4 py-3 rounded-none rounded-r-md focus:ring-blue-500 focus:border-blue-500 border-gray-300 font-bold text-gray-900 text-lg"
                      />
                    </div>
                  </div>
                  
                  <button
                    type="submit"
                    disabled={isBidDisabled}
                    className="w-full flex justify-center py-3 px-4 border border-transparent rounded-md shadow-sm text-base font-bold text-white bg-blue-600 hover:bg-blue-700 focus:outline-none focus:ring-2 focus:ring-offset-2 focus:ring-blue-500 disabled:opacity-50 disabled:cursor-not-allowed transition-colors"
                  >
                    {!isAuthenticated ? 'Đăng nhập để đặt giá' : isBidding ? 'Đang xử lý...' : 'Đặt giá ngay'}
                  </button>
                </form>

                {/* Bid History */}
                {bidHistory.length > 0 && (
                  <div className="mt-8">
                    <h3 className="text-sm font-semibold text-gray-900 mb-4 uppercase tracking-wider">Lịch sử đặt giá mới nhất</h3>
                    <ul className="space-y-3">
                      {bidHistory.map((bid, index) => (
                        <li key={index} className={`flex justify-between items-center p-3 rounded-lg ${index === 0 ? 'bg-green-50 border border-green-100' : 'bg-gray-50 border border-gray-100'}`}>
                          <div className="flex items-center">
                            {index === 0 && <span className="flex w-2 h-2 rounded-full bg-green-500 mr-2 animate-pulse"></span>}
                            <span className="text-sm font-medium text-gray-900">{bid.bidderName}</span>
                            <span className="text-xs text-gray-500 ml-2">{new Date(bid.timestamp).toLocaleTimeString()}</span>
                          </div>
                          <span className={`font-bold ${index === 0 ? 'text-green-700' : 'text-gray-700'}`}>
                            {formatPrice(bid.amount)}
                          </span>
                        </li>
                      ))}
                    </ul>
                  </div>
                )}
              </div>
            </div>
          </div>
        </div>
      </div>
    </div>
  );
}

