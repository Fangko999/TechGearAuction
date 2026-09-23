import { CreateAuctionRequest, AuctionDto, AuctionDetailDto, PagedResult } from '../models/auction';
import { PlaceBidRequest } from '../models/bid';

export interface IAuctionRepository {
  createAuction(request: CreateAuctionRequest): Promise<string>;
  uploadAuctionImage(auctionId: string, file: File): Promise<string>;
  getActiveAuctions(pageIndex?: number, pageSize?: number): Promise<PagedResult<AuctionDto>>;
  getAuctionDetail(id: string): Promise<AuctionDetailDto>;
  placeBid(request: PlaceBidRequest): Promise<void>;
}

