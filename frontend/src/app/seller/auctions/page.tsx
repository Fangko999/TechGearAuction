'use client';

import React, { useEffect, useState } from 'react';
import axiosClient from '@infrastructure/api/axiosClient';
import { AuctionDto } from '@domain/models/auction';
import Link from 'next/link';

export default function SellerAuctionsPage() {
  const [auctions, setAuctions] = useState<AuctionDto[]>([]);
  const [loading, setLoading] = useState(true);

  useEffect(() => {
    const fetchMyAuctions = async () => {
      try {
        const res = await axiosClient.get('/auctions/me');
        setAuctions(res.data?.items || []);
      } catch (err) {
        console.error('Failed to fetch my auctions', err);
      } finally {
        setLoading(false);
      }
    };
    fetchMyAuctions();
  }, []);

  const getStatusBadge = (status: string | number) => {
    // 0: Draft, 1: Scheduled, 2: Active, 3: Completed, 4: Cancelled
    if (status === 'Active' || status === 2) return <span className="px-2 py-1 bg-green-100 text-green-800 rounded-full text-xs font-medium">Đang diễn ra</span>;
    if (status === 'Draft' || status === 0) return <span className="px-2 py-1 bg-gray-100 text-gray-800 rounded-full text-xs font-medium">Bản nháp</span>;
    if (status === 'Completed' || status === 3) return <span className="px-2 py-1 bg-blue-100 text-blue-800 rounded-full text-xs font-medium">Đã kết thúc</span>;
    if (status === 'Cancelled' || status === 4) return <span className="px-2 py-1 bg-red-100 text-red-800 rounded-full text-xs font-medium">Đã hủy</span>;
    return <span className="px-2 py-1 bg-yellow-100 text-yellow-800 rounded-full text-xs font-medium">Đã lên lịch</span>;
  };

  return (
    <div className="space-y-6">
      <div className="flex justify-between items-center">
        <div>
          <h1 className="text-2xl font-bold text-gray-900">Kho hàng của tôi</h1>
          <p className="text-gray-500 text-sm mt-1">Quản lý các sản phẩm bạn đang bán.</p>
        </div>
        <Link 
          href="/seller/auctions/create" 
          className="px-4 py-2 bg-blue-600 hover:bg-blue-700 text-white rounded-lg font-medium transition-colors"
        >
          + Đăng sản phẩm mới
        </Link>
      </div>

      <div className="bg-white border border-gray-200 rounded-xl overflow-hidden shadow-sm">
        <table className="w-full text-left text-sm text-gray-600">
          <thead className="bg-gray-50 text-gray-500 text-xs uppercase border-b border-gray-200">
            <tr>
              <th className="px-6 py-4 font-medium">Sản phẩm</th>
              <th className="px-6 py-4 font-medium">Trạng thái</th>
              <th className="px-6 py-4 font-medium">Giá hiện tại</th>
              <th className="px-6 py-4 font-medium">Ngày kết thúc</th>
              <th className="px-6 py-4 font-medium">Thao tác</th>
            </tr>
          </thead>
          <tbody className="divide-y divide-gray-100">
            {loading ? (
              <tr>
                <td colSpan={5} className="px-6 py-8 text-center text-gray-500">Đang tải...</td>
              </tr>
            ) : auctions.length === 0 ? (
              <tr>
                <td colSpan={5} className="px-6 py-8 text-center text-gray-500">Bạn chưa đăng sản phẩm nào.</td>
              </tr>
            ) : (
              auctions.map((auction) => (
                <tr key={auction.id} className="hover:bg-gray-50 transition-colors">
                  <td className="px-6 py-4">
                    <div className="font-medium text-gray-900 line-clamp-1">{auction.title}</div>
                  </td>
                  <td className="px-6 py-4">
                    {getStatusBadge(auction.status)}
                  </td>
                  <td className="px-6 py-4 font-medium text-blue-600">
                    {auction.currentPrice.toLocaleString('vi-VN')} đ
                  </td>
                  <td className="px-6 py-4">
                    {new Date(auction.endTime).toLocaleDateString('vi-VN')}
                  </td>
                  <td className="px-6 py-4">
                    <Link href={`/auctions/${auction.id}`} className="text-blue-600 hover:text-blue-800 transition-colors font-medium">
                      Xem chi tiết
                    </Link>
                  </td>
                </tr>
              ))
            )}
          </tbody>
        </table>
      </div>
    </div>
  );
}

