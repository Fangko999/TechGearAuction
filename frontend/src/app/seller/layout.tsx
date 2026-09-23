'use client';

import React from 'react';
import Link from 'next/link';
import { useAuthStore } from '@application/store/useAuthStore';
import { usePathname, useRouter } from 'next/navigation';

export default function SellerLayout({ children }: { children: React.ReactNode }) {
  const user = useAuthStore((state) => state.user);
  const logout = useAuthStore((state) => state.logout);
  const pathname = usePathname();
  const router = useRouter();

  const navigation = [
    { name: 'Tổng quan', href: '/seller' },
    { name: 'Kho hàng', href: '/seller/auctions' },
    { name: 'Đăng bán', href: '/seller/auctions/create' },
    { name: 'Tin nhắn', href: '/seller/messages' },
    { name: 'Cài đặt', href: '/seller/settings' },
  ];

  const handleLogout = () => {
    logout();
    router.push('/login');
  };

  return (
    <div className="flex h-screen bg-gray-50">
      {/* Sidebar */}
      <div className="flex flex-col w-64 bg-gray-900 border-r">
        <div className="flex items-center justify-center h-16 border-b border-gray-800">
          <span className="text-xl font-bold text-white">TechGear Seller</span>
        </div>
        <div className="flex-1 overflow-y-auto">
          <nav className="px-2 mt-5 space-y-1">
            {navigation.map((item) => {
              const isActive = pathname === item.href;
              return (
                <Link
                  key={item.name}
                  href={item.href}
                  className={`${
                    isActive ? 'bg-gray-800 text-white' : 'text-gray-300 hover:bg-gray-700 hover:text-white'
                  } group flex items-center px-2 py-2 text-sm font-medium rounded-md transition-colors`}
                >
                  {item.name}
                </Link>
              );
            })}
          </nav>
        </div>
        <div className="p-4 border-t border-gray-800">
          <button 
            onClick={handleLogout}
            className="w-full px-4 py-2 text-sm text-gray-300 transition-colors bg-gray-800 rounded-md hover:bg-gray-700"
          >
            Đăng xuất
          </button>
        </div>
      </div>

      {/* Main Content */}
      <div className="flex flex-col flex-1 overflow-hidden">
        {/* Header */}
        <header className="flex items-center justify-end h-16 px-6 bg-white border-b border-gray-200">
          <div className="flex items-center gap-3">
            <span className="text-sm font-medium text-gray-700">Xin chào, {user?.displayName || user?.email || 'Seller'}</span>
            <div className="w-8 h-8 bg-blue-500 rounded-full flex items-center justify-center text-white font-bold">
              {(user?.displayName || user?.email || 'S')[0].toUpperCase()}
            </div>
          </div>
        </header>

        {/* Main section */}
        <main className="flex-1 overflow-y-auto p-8">
          {children}
        </main>
      </div>
    </div>
  );
}

