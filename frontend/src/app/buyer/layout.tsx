'use client';

import React from 'react';
import Link from 'next/link';
import { usePathname } from 'next/navigation';
import { Navbar } from '@presentation/components/Navbar';

export default function BuyerLayout({ children }: { children: React.ReactNode }) {
  const pathname = usePathname();

  const navItems = [
    { label: 'Tổng quan', href: '/buyer', icon: 'M4 6a2 2 0 012-2h2a2 2 0 012 2v2a2 2 0 01-2 2H6a2 2 0 01-2-2V6zM14 6a2 2 0 012-2h2a2 2 0 012 2v2a2 2 0 01-2 2h-2a2 2 0 01-2-2V6zM4 16a2 2 0 012-2h2a2 2 0 012 2v2a2 2 0 01-2 2H6a2 2 0 01-2-2v-2zM14 16a2 2 0 012-2h2a2 2 0 012 2v2a2 2 0 01-2 2h-2a2 2 0 01-2-2v-2z' },
    { label: 'Lịch sử đấu giá (Bids)', href: '/buyer/bids', icon: 'M9 5H7a2 2 0 00-2 2v12a2 2 0 002 2h10a2 2 0 002-2V7a2 2 0 00-2-2h-2M9 5a2 2 0 002 2h2a2 2 0 002-2M9 5a2 2 0 012-2h2a2 2 0 012 2m-6 9l2 2 4-4' },
    { label: 'Sản phẩm đã thắng', href: '/buyer/won', icon: 'M5 13l4 4L19 7' },
    { label: 'Danh sách theo dõi', href: '/buyer/watchlist', icon: 'M4.318 6.318a4.5 4.5 0 000 6.364L12 20.364l7.682-7.682a4.5 4.5 0 00-6.364-6.364L12 7.636l-1.318-1.318a4.5 4.5 0 00-6.364 0z' },
  ];

  return (
    <>
      <Navbar />
      
      <div className="flex flex-1 max-w-7xl w-full mx-auto px-4 sm:px-6 lg:px-8 py-8 gap-8">
        {/* Sidebar */}
        <aside className="w-64 flex-shrink-0">
          <div className="bg-white rounded-xl shadow-sm border border-gray-200 overflow-hidden">
            <div className="p-4 bg-blue-50 border-b border-gray-200">
              <h2 className="font-bold text-blue-800">Buyer Dashboard</h2>
              <p className="text-xs text-blue-600 mt-1">Quản lý hoạt động mua hàng</p>
            </div>
            <nav className="p-2 space-y-1">
              {navItems.map((item) => {
                const isActive = pathname === item.href || (item.href !== '/buyer' && pathname.startsWith(item.href));
                return (
                  <Link 
                    key={item.href} 
                    href={item.href}
                    className={`flex items-center space-x-3 px-3 py-2.5 rounded-lg transition-colors text-sm font-medium ${isActive ? 'bg-blue-600 text-white shadow-sm' : 'text-gray-700 hover:bg-gray-100'}`}
                  >
                    <svg className="w-5 h-5" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                      <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d={item.icon} />
                    </svg>
                    <span>{item.label}</span>
                  </Link>
                );
              })}
            </nav>
          </div>
        </aside>

        {/* Main Content */}
        <main className="flex-1 min-w-0">
          <div className="bg-white rounded-xl shadow-sm border border-gray-200 p-6 min-h-[500px]">
            {children}
          </div>
        </main>
      </div>
    </>
  );
}
