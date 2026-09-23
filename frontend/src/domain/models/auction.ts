export interface CreateAuctionRequest {
  categoryId: string;
  title: string;
  description?: string;
  startPrice: number;
  bidIncrement: number;
  buyNowPrice?: number;
  startTime: string; // ISO 8601
  endTime: string; // ISO 8601
}

export interface AuctionDto {
  id: string;
  categoryId: string;
  categoryName: string;
  title: string;
  startPrice: number;
  currentPrice: number;
  buyNowPrice?: number;
  startTime: string;
  endTime: string;
  status: string;
  primaryImageUrl?: string;
  sellerName: string;
}

export interface AuctionImageDto {
  id: string;
  imageUrl: string;
  isPrimary: boolean;
}

export interface AuctionDetailDto extends AuctionDto {
  description?: string;
  bidIncrement: number;
  sellerId: string;
  sellerAvatar?: string;
  sellerAverageRating: number;
  sellerTotalReviews: number;
  images: AuctionImageDto[];
}

export interface PagedResult<T> {
  items: T[];
  totalCount: number;
  pageIndex: number;
  pageSize: number;
}

