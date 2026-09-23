export enum ChatMessageType {
  Text = 'Text',
  Image = 'Image',
  Video = 'Video',
  SystemText = 'SystemText'
}

export interface ChatRoomDto {
  id: string;
  auctionId: string;
  auctionTitle: string;
  status: string;
  expiresAt: string;
  auctionThumbnailUrl: string;
  opponentId: string;
  opponentName: string;
  opponentAvatarUrl?: string;
  unreadCount: number;
  createdAt: string;
}

export interface ChatMessageDto {
  id: string;
  chatRoomId: string;
  senderId?: string;
  senderName?: string;
  content: string;
  messageType: string; // Map to ChatMessageType
  mediaUrl?: string;
  isRead: boolean;
  createdAt: string;
}

export interface SendMessageRequest {
  content: string;
  messageType: string;
  mediaUrl?: string;
}

