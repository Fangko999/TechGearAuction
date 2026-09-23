import { IChatRepository } from '@domain/repositories/IChatRepository';
import { ChatRoomDto, ChatMessageDto, SendMessageRequest } from '@domain/models/chat';
import { PagedResult } from '@domain/models/auction';
import axiosClient from '../api/axiosClient';

export class ChatRepository implements IChatRepository {
  async getChatRooms(role: string = 'All', folder: string = 'Inbox'): Promise<ChatRoomDto[]> {
    const response = await axiosClient.get<any, ChatRoomDto[]>(`/chat?role=${role}&folder=${folder}`);
    return response;
  }

  async getMessages(roomId: string, pageIndex: number = 1, pageSize: number = 50): Promise<PagedResult<ChatMessageDto>> {
    const response = await axiosClient.get<any, PagedResult<ChatMessageDto>>(`/chat/${roomId}/messages?pageIndex=${pageIndex}&pageSize=${pageSize}`);
    return response;
  }

  async sendMessage(roomId: string, request: SendMessageRequest): Promise<string> {
    const response = await axiosClient.post<any, { MessageId: string }>(`/chat/${roomId}/messages`, request);
    return response.MessageId;
  }

  async uploadMedia(roomId: string, file: File): Promise<string> {
    const formData = new FormData();
    formData.append('file', file);
    const response = await axiosClient.post<any, { Url: string }>(`/chat/${roomId}/media`, formData, {
      headers: {
        'Content-Type': 'multipart/form-data',
      },
    });
    return response.Url;
  }

  async markAsRead(roomId: string): Promise<void> {
    await axiosClient.put(`/chat/${roomId}/messages/read`);
  }

  async archiveRoom(roomId: string): Promise<void> {
    await axiosClient.put(`/chat/${roomId}/archive`);
  }
}
