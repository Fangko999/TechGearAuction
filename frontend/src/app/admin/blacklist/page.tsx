'use client';

import React, { useEffect, useState } from 'react';
import { AdminRepository } from '@infrastructure/repositories/AdminRepository';
import { BlacklistDto } from '@domain/models/admin';

export default function BlacklistPage() {
  const [items, setItems] = useState<BlacklistDto[]>([]);
  const [isLoading, setIsLoading] = useState(true);

  useEffect(() => {
    const fetchBlacklists = async () => {
      setIsLoading(true);
      try {
        const repo = new AdminRepository();
        const res = await repo.getBlacklists();
        setItems(res || []);
      } catch (err) {
        console.error(err);
      } finally {
        setIsLoading(false);
      }
    };
    fetchBlacklists();
  }, []);

  return (
    <div className="space-y-6">
      <div className="flex justify-between items-center">
        <div>
          <h1 className="text-2xl font-bold text-white">Thiáº¿t bá»‹ Blacklist</h1>
          <p className="text-gray-400 text-sm mt-1">Quáº£n lÃ½ danh sÃ¡ch cÃ¡c thiáº¿t bá»‹ (Device Hash) bá»‹ cáº¥m truy cáº­p há»‡ thá»‘ng.</p>
        </div>
        <button className="px-4 py-2 bg-red-600 hover:bg-red-700 text-white rounded-lg font-medium transition-colors">
          + ThÃªm Thiáº¿t Bá»‹
        </button>
      </div>

      <div className="bg-gray-900 border border-gray-800 rounded-xl overflow-hidden">
        <table className="w-full text-left text-sm text-gray-300">
          <thead className="bg-gray-950 text-gray-400 text-xs uppercase border-b border-gray-800">
            <tr>
              <th className="px-6 py-4 font-medium">Device Hash</th>
              <th className="px-6 py-4 font-medium">LÃ½ do khÃ³a</th>
              <th className="px-6 py-4 font-medium">NgÃ y khÃ³a</th>
              <th className="px-6 py-4 font-medium">Thao tÃ¡c</th>
            </tr>
          </thead>
          <tbody className="divide-y divide-gray-800">
            {isLoading ? (
              <tr>
                <td colSpan={4} className="px-6 py-8 text-center text-gray-500">Äang táº£i...</td>
              </tr>
            ) : items.length === 0 ? (
              <tr>
                <td colSpan={4} className="px-6 py-8 text-center text-gray-500">ChÆ°a cÃ³ thiáº¿t bá»‹ nÃ o bá»‹ khÃ³a.</td>
              </tr>
            ) : (
              items.map((item) => (
                <tr key={item.deviceHash} className="hover:bg-gray-800/50 transition-colors">
                  <td className="px-6 py-4">
                    <div className="font-mono text-sm text-red-400 bg-red-900/20 px-2 py-1 rounded inline-block">
                      {item.deviceHash}
                    </div>
                  </td>
                  <td className="px-6 py-4 text-gray-300">
                    {item.reason || 'KhÃ´ng cÃ³ lÃ½ do'}
                  </td>
                  <td className="px-6 py-4 text-gray-400">
                    {new Date(item.createdAt).toLocaleDateString('vi-VN')}
                  </td>
                  <td className="px-6 py-4">
                    <button className="text-blue-400 hover:text-blue-300 transition-colors">Gá»¡ bá» (Unban)</button>
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
