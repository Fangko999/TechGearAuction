'use client';

import React, { useEffect, useState } from 'react';
import { AdminRepository } from '@infrastructure/repositories/AdminRepository';
import { MetricsOverviewDto } from '@domain/models/admin';

export default function AdminDashboardOverview() {
  const [data, setData] = useState<MetricsOverviewDto | null>(null);
  const [isLoading, setIsLoading] = useState(true);

  useEffect(() => {
    const fetchData = async () => {
      try {
        const repo = new AdminRepository();
        const res = await repo.getMetricsOverview();
        setData(res);
      } catch (err) {
        console.error('Failed to load metrics:', err);
      } finally {
        setIsLoading(false);
      }
    };
    fetchData();
  }, []);

  return (
    <div className="space-y-6">
      <h1 className="text-2xl font-bold text-white">Tá»•ng quan Há»‡ thá»‘ng</h1>
      
      {isLoading ? (
        <div className="text-gray-400">Äang táº£i dá»¯ liá»‡u...</div>
      ) : (
        <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-4 gap-6">
          <div className="bg-gray-800 border border-gray-700 p-6 rounded-xl">
            <h3 className="text-gray-400 text-sm font-medium">NgÆ°á»i dÃ¹ng</h3>
            <p className="text-3xl font-bold text-white mt-2">{data?.totalUsers || 0}</p>
          </div>
          
          <div className="bg-gray-800 border border-gray-700 p-6 rounded-xl">
            <h3 className="text-gray-400 text-sm font-medium">Äáº¥u giÃ¡ Ä‘ang diá»…n ra</h3>
            <p className="text-3xl font-bold text-white mt-2">{data?.activeAuctions || 0}</p>
          </div>

          <div className="bg-gray-800 border border-gray-700 p-6 rounded-xl">
            <h3 className="text-gray-400 text-sm font-medium">ÄÆ¡n tá»‘ cÃ¡o (Pending)</h3>
            <p className="text-3xl font-bold text-red-400 mt-2">{data?.pendingReports || 0}</p>
          </div>

          <div className="bg-gray-800 border border-gray-700 p-6 rounded-xl">
            <h3 className="text-gray-400 text-sm font-medium">Tá»•ng giao dá»‹ch</h3>
            <p className="text-3xl font-bold text-green-400 mt-2">
              {new Intl.NumberFormat('vi-VN', { style: 'currency', currency: 'VND' }).format(data?.totalRevenue || 0)}
            </p>
          </div>
        </div>
      )}

      <div className="bg-gray-800 border border-gray-700 p-8 rounded-xl flex flex-col items-center justify-center min-h-[400px]">
        <svg className="w-16 h-16 text-gray-600 mb-4" fill="none" viewBox="0 0 24 24" stroke="currentColor">
          <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={1} d="M9 19v-6a2 2 0 00-2-2H5a2 2 0 00-2 2v6a2 2 0 002 2h2a2 2 0 002-2zm0 0V9a2 2 0 012-2h2a2 2 0 012 2v10m-6 0a2 2 0 002 2h2a2 2 0 002-2m0 0V5a2 2 0 012-2h2a2 2 0 012 2v14a2 2 0 01-2 2h-2a2 2 0 01-2-2z" />
        </svg>
        <h2 className="text-xl font-bold text-gray-300">Biá»ƒu Ä‘á»“ Doanh thu</h2>
        <p className="text-gray-500 mt-2 text-center max-w-md">Chá»©c nÄƒng váº½ biá»ƒu Ä‘á»“ trá»±c quan sáº½ sá»›m Ä‘Æ°á»£c hoÃ n thiá»‡n.</p>
      </div>
    </div>
  );
}
