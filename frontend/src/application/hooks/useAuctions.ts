import { useState, useEffect } from 'react';
import { AuctionRepository } from '@infrastructure/repositories/AuctionRepository';
import { AuctionDto, PagedResult } from '@domain/models/auction';

const auctionRepository = new AuctionRepository();

export const useAuctions = (pageIndex: number = 1, pageSize: number = 12) => {
  const [data, setData] = useState<PagedResult<AuctionDto>>({
    items: [],
    totalCount: 0,
    pageIndex,
    pageSize,
  });
  const [isLoading, setIsLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    let isMounted = true;

    const fetchAuctions = async () => {
      setIsLoading(true);
      setError(null);
      try {
        const response = await auctionRepository.getActiveAuctions(pageIndex, pageSize);
        if (isMounted) {
          setData(response);
        }
      } catch (err: any) {
        if (isMounted) {
          setError(err?.Message || err?.message || 'Không thể tải dữ liệu đấu giá.');
        }
      } finally {
        if (isMounted) {
          setIsLoading(false);
        }
      }
    };

    fetchAuctions();

    return () => {
      isMounted = false;
    };
  }, [pageIndex, pageSize]);

  return {
    data,
    isLoading,
    error,
  };
};

