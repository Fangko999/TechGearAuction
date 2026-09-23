import { useState, useEffect, useCallback } from 'react';
import { AdminRepository } from '@infrastructure/repositories/AdminRepository';
import { BlacklistDto } from '@domain/models/admin';

export const useBlacklist = () => {
  const [items, setItems] = useState<BlacklistDto[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  const fetchBlacklists = useCallback(async () => {
    setLoading(true);
    try {
      const repo = new AdminRepository();
      const res = await repo.getBlacklists();
      setItems(res || []);
    } catch (err: any) {
      console.error('Failed to fetch blacklists', err);
      setError(err.message);
    } finally {
      setLoading(false);
    }
  }, []);

  useEffect(() => {
    fetchBlacklists();
  }, [fetchBlacklists]);

  return { items, loading, error, fetchBlacklists };
};

