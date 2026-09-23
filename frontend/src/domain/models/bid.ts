export interface BidDto {
  bidderName: string;
  amount: number;
  timestamp: string;
}

export interface PlaceBidRequest {
  auctionId: string;
  bidAmount: number;
  ipAddress?: string; // Tùy chọn truyền từ client hoặc để backend tự lấy
  deviceHash?: string;
}

