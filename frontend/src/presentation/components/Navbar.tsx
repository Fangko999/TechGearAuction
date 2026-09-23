"use client";

import React, { useState, useEffect } from 'react';
import Link from 'next/link';
import { useRouter } from 'next/navigation';
import { useAuthStore } from '@application/store/useAuthStore';
import axiosClient from '@infrastructure/api/axiosClient';

export const Navbar: React.FC = () => {
  const { user, isAuthenticated, logout } = useAuthStore();
  const router = useRouter();
  const [unreadCount, setUnreadCount] = useState(0);
  const [isDropdownOpen, setIsDropdownOpen] = useState(false);

  useEffect(() => {
    if (isAuthenticated) {
      const fetchUnread = async () => {
        try {
          const res = await axiosClient.get<any, any[]>('/notifications/unread');
          setUnreadCount(res.length);
        } catch (error) {
          console.error(error);
        }
      };
      
      fetchUnread();
      const interval = setInterval(fetchUnread, 30000); // poll every 30s
      return () => clearInterval(interval);
    }
  }, [isAuthenticated]);

  const handleLogout = () => {
    logout();
    router.push('/login');
  };

  return (
    <nav className="bg-white border-b border-gray-200 sticky top-0 z-50">
      <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8">
        <div className="flex justify-between h-16">
          <div className="flex">
            <div className="flex-shrink-0 flex items-center">
              <Link href="/" className="text-xl font-black tracking-tighter text-blue-600 uppercase">
                TechGear<span className="text-gray-900">Auction</span>
              </Link>
            </div>
            <div className="hidden sm:ml-8 sm:flex sm:space-x-8">
              <Link href="/" className="border-transparent text-gray-500 hover:border-gray-300 hover:text-gray-700 inline-flex items-center px-1 pt-1 border-b-2 text-sm font-medium">
                Trang chủ
              </Link>
              <Link href="/auctions" className="border-transparent text-gray-500 hover:border-gray-300 hover:text-gray-700 inline-flex items-center px-1 pt-1 border-b-2 text-sm font-medium">
                Sản phẩm
              </Link>
            </div>
          </div>
          
          <div className="flex items-center space-x-4">
            {isAuthenticated ? (
              <>
                <Link href="/chat" className="text-gray-500 hover:text-gray-700 relative p-2">
                  <svg className="w-6 h-6" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M8 10h.01M12 10h.01M16 10h.01M9 16H5a2 2 0 01-2-2V6a2 2 0 012-2h14a2 2 0 012 2v8a2 2 0 01-2 2h-5l-5 5v-5z" /></svg>
                  {unreadCount > 0 && (
                    <span className="absolute top-1 right-1 block h-2.5 w-2.5 rounded-full ring-2 ring-white bg-red-400"></span>
                  )}
                </Link>

                <div className="relative">
                  <button 
                    onClick={() => setIsDropdownOpen(!isDropdownOpen)}
                    className="flex items-center space-x-2 focus:outline-none"
                  >
                    <div className="w-8 h-8 rounded-full bg-blue-600 text-white flex items-center justify-center font-bold">
                      {user?.displayName?.[0] || 'U'}
                    </div>
                  </button>

                  {isDropdownOpen && (
                    <>
                      <div className="fixed inset-0 z-40" onClick={() => setIsDropdownOpen(false)}></div>
                      <div className="origin-top-right absolute right-0 mt-2 w-48 rounded-md shadow-lg py-1 bg-white ring-1 ring-black ring-opacity-5 z-50">
                        <div className="px-4 py-2 border-b border-gray-100">
                          <p className="text-sm font-medium text-gray-900 truncate">{user?.displayName}</p>
                          <p className="text-xs text-gray-500 truncate">{user?.role}</p>
                        </div>
                        
                        <Link href="/buyer" className="block px-4 py-2 text-sm text-gray-700 hover:bg-gray-100" onClick={() => setIsDropdownOpen(false)}>
                          Mua hàng (Buyer)
                        </Link>
                        <Link href="/seller" className="block px-4 py-2 text-sm text-gray-700 hover:bg-gray-100" onClick={() => setIsDropdownOpen(false)}>
                          Bán hàng (Seller)
                        </Link>
                        
                        {user?.role === 'Admin' && (
                          <Link href="/admin" className="block px-4 py-2 text-sm text-red-600 hover:bg-gray-100" onClick={() => setIsDropdownOpen(false)}>
                            Admin Control Panel
                          </Link>
                        )}
                        
                        <div className="border-t border-gray-100"></div>
                        <button onClick={() => { setIsDropdownOpen(false); handleLogout(); }} className="block w-full text-left px-4 py-2 text-sm text-gray-700 hover:bg-gray-100">
                          Đăng xuất
                        </button>
                      </div>
                    </>
                  )}
                </div>
              </>
            ) : (
              <div className="flex items-center space-x-4">
                <Link href="/login" className="text-gray-500 hover:text-gray-900 font-medium text-sm">Đăng nhập</Link>
                <Link href="/register" className="bg-blue-600 hover:bg-blue-700 text-white px-4 py-2 rounded-md font-medium text-sm transition-colors">Đăng ký</Link>
              </div>
            )}
          </div>
        </div>
      </div>
    </nav>
  );
};

