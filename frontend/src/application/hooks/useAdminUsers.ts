import { useState, useEffect, useCallback } from 'react';
import { AdminRepository } from '@infrastructure/repositories/AdminRepository';
import { UserDto } from '@domain/models/admin';

export const useAdminUsers = () => {
  const [users, setUsers] = useState<UserDto[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  const fetchUsers = useCallback(async (searchTerm?: string) => {
    setLoading(true);
    try {
      const repo = new AdminRepository();
      const res = await repo.getUsers(1, 50, searchTerm);
      setUsers(res.items || []);
    } catch (err: any) {
      console.error('Failed to fetch admin users', err);
      setError(err.message);
    } finally {
      setLoading(false);
    }
  }, []);

  const banUser = async (userId: string, reason: string) => {
    const repo = new AdminRepository();
    await repo.banUser(userId, reason);
  };

  const unbanUser = async (userId: string, reason: string) => {
    const repo = new AdminRepository();
    await repo.unbanUser(userId, reason);
  };

  const closeAccount = async (userId: string) => {
    const repo = new AdminRepository();
    await repo.closeAccount(userId);
  };

  const restoreAccount = async (userId: string) => {
    const repo = new AdminRepository();
    await repo.restoreAccount(userId);
  };

  return { users, loading, error, fetchUsers, banUser, unbanUser, closeAccount, restoreAccount };
};

