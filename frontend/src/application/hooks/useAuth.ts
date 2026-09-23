import { useState } from 'react';
import { useRouter } from 'next/navigation';
import Cookies from 'js-cookie';
import { AuthRepository } from '@infrastructure/repositories/AuthRepository';
import { LoginRequest } from '@domain/models/auth';
import { useAuthStore } from '../store/useAuthStore';

const authRepository = new AuthRepository();

export const useAuth = () => {
  const router = useRouter();
  const setAuth = useAuthStore((state) => state.setAuth);
  
  const [isLoading, setIsLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);

  const handleLogin = async (request: LoginRequest) => {
    setIsLoading(true);
    setError(null);
    try {
      const response = await authRepository.login(request);
      
      setAuth(response);

      if (typeof window !== 'undefined' && !localStorage.getItem('deviceHash')) {
        const mockDeviceHash = `DEV-${Math.random().toString(36).substring(2, 10).toUpperCase()}`;
        localStorage.setItem('deviceHash', mockDeviceHash);
      }

      // Set cookies for Next.js Middleware
      Cookies.set('token', response.token, { expires: 7, path: '/' });
      Cookies.set('user_data', JSON.stringify({ role: response.role }), { expires: 7, path: '/' });
      Cookies.remove('isBanned'); // Clear banned flag if login successful

      if (response.role === 'Admin') {
        router.push('/admin');
      } else {
        router.push('/');
      }
    } catch (err: any) {
      // Check if this is the BannedUserException (Status 403 with BanReason)
      if (err?.BanReason || err?.Message?.includes('ban')) {
        Cookies.set('isBanned', 'true', { expires: 7, path: '/' });
        Cookies.set('banReason', err.BanReason || err.Message, { expires: 7, path: '/' });
        Cookies.set('bannedEmail', request.email, { expires: 7, path: '/' });
        router.push('/appeal');
      } else {
        setError(err?.Message || err?.message || 'Đã xảy ra lỗi hệ thống.');
      }
    } finally {
      setIsLoading(false);
    }
  };

  const handleLogout = () => {
    const logout = useAuthStore.getState().logout;
    logout();
    Cookies.remove('token');
    Cookies.remove('user_data');
    Cookies.remove('isBanned');
    Cookies.remove('banReason');
    Cookies.remove('bannedEmail');
    router.push('/login');
  };

  return {
    handleLogin,
    handleLogout,
    isLoading,
    error,
  };
};

