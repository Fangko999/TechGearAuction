import { useState } from 'react';
import { AuctionRepository } from '@infrastructure/repositories/AuctionRepository';
import { CreateAuctionRequest } from '@domain/models/auction';

const auctionRepository = new AuctionRepository();

export const useCreateAuction = () => {
  const [isLoading, setIsLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const [success, setSuccess] = useState<boolean>(false);

  const handleCreateAuction = async (request: CreateAuctionRequest, files: File[]) => {
    setIsLoading(true);
    setError(null);
    setSuccess(false);

    try {
      // 1. Validate cục bộ
      if (request.startPrice <= 0) {
        throw new Error('Giá khởi điểm phải lớn hơn 0.');
      }
      
      const start = new Date(request.startTime);
      const end = new Date(request.endTime);
      
      if (end <= start) {
        throw new Error('Thời gian kết thúc phải sau thời gian bắt đầu.');
      }
      
      const diffHours = (end.getTime() - start.getTime()) / (1000 * 60 * 60);
      if (diffHours < 3) {
        throw new Error('Thời gian đấu giá tối thiểu là 3 giờ.');
      }

      // 2. Gửi request tạo thông tin Auction
      const auctionId = await auctionRepository.createAuction(request);

      // 3. Loop qua danh sách file và upload lần lượt (có thể dùng Promise.all nếu muốn tải song song)
      if (files && files.length > 0) {
        const uploadPromises = files.map(file => auctionRepository.uploadAuctionImage(auctionId, file));
        await Promise.all(uploadPromises);
      }

      setSuccess(true);
      return auctionId;

    } catch (err: any) {
      setError(err?.Message || err?.message || 'Không thể tạo phiên đấu giá. Vui lòng thử lại.');
      return null;
    } finally {
      setIsLoading(false);
    }
  };

  return {
    handleCreateAuction,
    isLoading,
    error,
    success
  };
};

