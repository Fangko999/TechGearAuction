import { create } from 'zustand';
import { persist, createJSONStorage } from 'zustand/middleware';
import { AuthResponse } from '@domain/models/auth';

interface AuthState {
  token: string | null;
  user: {
    userId: string;
    email: string;
    displayName: string;
    role: string;
  } | null;
  isAuthenticated: boolean;
  setAuth: (data: AuthResponse) => void;
  logout: () => void;
}

export const useAuthStore = create<AuthState>()(
  persist(
    (set) => ({
      token: null,
      user: null,
      isAuthenticated: false,
      setAuth: (data: AuthResponse) => {
        // Đồng bộ token riêng ra localStorage để axiosClient có thể đọc độc lập (nếu cần)
        if (typeof window !== 'undefined') {
          localStorage.setItem('token', data.token);
        }
        set({
          token: data.token,
          user: {
            userId: data.userId,
            email: data.email,
            displayName: data.displayName,
            role: data.role,
          },
          isAuthenticated: true,
        });
      },
      logout: () => {
        if (typeof window !== 'undefined') {
          localStorage.removeItem('token');
        }
        set({ token: null, user: null, isAuthenticated: false });
      },
    }),
    {
      name: 'auth-storage',
      storage: createJSONStorage(() => localStorage),
    }
  )
);

