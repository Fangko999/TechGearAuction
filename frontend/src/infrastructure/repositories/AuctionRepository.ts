import { IAuctionRepository } from '@domain/repositories/IAuctionRepository';
import { CreateAuctionRequest, AuctionDto, AuctionDetailDto, PagedResult } from '@domain/models/auction';
import { PlaceBidRequest } from '@domain/models/bid';
import axiosClient from '../api/axiosClient';

export class AuctionRepository implements IAuctionRepository {
  async createAuction(request: CreateAuctionRequest): Promise<string> {
    const response = await axiosClient.post<any, { Message: string; Id: string }>('/auctions', request);
    return response.Id;
  }

  async uploadAuctionImage(auctionId: string, file: File): Promise<string> {
    const formData = new FormData();
    formData.append('file', file);
    
    const response = await axiosClient.post<any, { Message: string; ImageUrl: string }>(`/auctions/${auctionId}/images`, formData, {
      headers: {
        'Content-Type': 'multipart/form-data',
      },
    });
    
    return response.ImageUrl;
  }

  async getActiveAuctions(pageIndex: number = 1, pageSize: number = 12): Promise<PagedResult<AuctionDto>> {
    const response = await axiosClient.get<any, PagedResult<AuctionDto>>(`/auctions?pageIndex=${pageIndex}&pageSize=${pageSize}`);
    return response;
  }

  async getAuctionDetail(id: string): Promise<AuctionDetailDto> {
    const response = await axiosClient.get<any, AuctionDetailDto>(`/auctions/${id}`);
    return response;
  }

  async placeBid(request: PlaceBidRequest): Promise<void> {
    // API expecting { bidAmount } in body, auctionId in URL
    await axiosClient.post(`/auctions/${request.auctionId}/bids`, { bidAmount: request.bidAmount });
  }
}
