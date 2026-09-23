import { ChatRoomDto, ChatMessageDto, SendMessageRequest } from '../models/chat';
import { PagedResult } from '../models/auction'; // Reusing PagedResult

export interface IChatRepository {
  getChatRooms(role?: string, folder?: string): Promise<ChatRoomDto[]>;
  getMessages(roomId: string, pageIndex?: number, pageSize?: number): Promise<PagedResult<ChatMessageDto>>;
  sendMessage(roomId: string, request: SendMessageRequest): Promise<string>; // Returns MessageId
  uploadMedia(roomId: string, file: File): Promise<string>; // Returns MediaUrl
  markAsRead(roomId: string): Promise<void>;
  archiveRoom(roomId: string): Promise<void>;
}

