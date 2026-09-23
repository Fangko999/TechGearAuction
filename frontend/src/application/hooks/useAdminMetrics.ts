import { useState, useEffect } from 'react';
import { AdminRepository } from '@infrastructure/repositories/AdminRepository';
import { MetricsOverviewDto } from '@domain/models/admin';

export const useAdminMetrics = () => {
  const [metrics, setMetrics] = useState<MetricsOverviewDto | null>(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    const fetchMetrics = async () => {
      try {
        const repo = new AdminRepository();
        const res = await repo.getMetricsOverview();
        setMetrics(res);
      } catch (err: any) {
        console.error('Failed to fetch admin metrics', err);
        setError(err.message);
      } finally {
        setLoading(false);
      }
    };
    fetchMetrics();
  }, []);

  return { metrics, loading, error };
};

