import { useState, useEffect, useRef } from 'react';
import { AuctionRepository } from '@infrastructure/repositories/AuctionRepository';
import { AuctionHubService } from '@infrastructure/signalr/AuctionHubService';
import { AuctionDetailDto } from '@domain/models/auction';
import { BidDto, PlaceBidRequest } from '@domain/models/bid';

const auctionRepository = new AuctionRepository();

export const useAuctionDetail = (auctionId: string) => {
  const [data, setData] = useState<AuctionDetailDto | null>(null);
  const [bidHistory, setBidHistory] = useState<BidDto[]>([]);
  const [isLoading, setIsLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [isBidding, setIsBidding] = useState(false);
  const [bidError, setBidError] = useState<string | null>(null);
  
  const hubServiceRef = useRef<AuctionHubService | null>(null);

  useEffect(() => {
    let isMounted = true;
    hubServiceRef.current = new AuctionHubService();

    const fetchDetail = async () => {
      try {
        const detail = await auctionRepository.getAuctionDetail(auctionId);
        if (isMounted) {
          setData(detail);
        }
      } catch (err: any) {
        if (isMounted) {
          setError(err?.Message || err?.message || 'Không thể tải thông tin phiên đấu giá.');
        }
      } finally {
        if (isMounted) {
          setIsLoading(false);
        }
      }
    };

    const setupSignalR = async () => {
      const hub = hubServiceRef.current;
      if (hub) {
        await hub.connect();
        await hub.joinAuction(auctionId);

        // Update current price
        hub.onPriceUpdated((newPrice: number) => {
          setData((prev) => prev ? { ...prev, currentPrice: newPrice } : prev);
        });

        // Add new bid to history (prepend and keep only top 5)
        hub.onNewBid((bidderName: string, amount: number, timestamp: string) => {
          setBidHistory((prev) => {
            const newBid = { bidderName, amount, timestamp };
            const updatedHistory = [newBid, ...prev];
            return updatedHistory.slice(0, 5);
          });
        });
      }
    };

    fetchDetail();
    setupSignalR();

    return () => {
      isMounted = false;
      const hub = hubServiceRef.current;
      if (hub) {
        hub.leaveAuction(auctionId);
        hub.disconnect();
      }
    };
  }, [auctionId]);

  const handlePlaceBid = async (bidAmount: number) => {
    setIsBidding(true);
    setBidError(null);
    try {
      const request: PlaceBidRequest = { auctionId, bidAmount };
      await auctionRepository.placeBid(request);
      // We don't update state here manually, SignalR will push the new price and bid history
    } catch (err: any) {
      setBidError(err?.Message || err?.message || 'Không thể đặt giá.');
    } finally {
      setIsBidding(false);
    }
  };

  return {
    data,
    bidHistory,
    isLoading,
    error,
    isBidding,
    bidError,
    handlePlaceBid,
  };
};

