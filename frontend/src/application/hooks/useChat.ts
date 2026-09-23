import { useState, useEffect } from 'react';
import { ChatRepository } from '@infrastructure/repositories/ChatRepository';
import { ChatRoomDto } from '@domain/models/chat';

const chatRepository = new ChatRepository();

export const useChat = (role: string = 'All', folder: string = 'Inbox') => {
  const [rooms, setRooms] = useState<ChatRoomDto[]>([]);
  const [isLoading, setIsLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    let isMounted = true;
    
    const fetchRooms = async () => {
      setIsLoading(true);
      setError(null);
      try {
        const data = await chatRepository.getChatRooms(role, folder);
        if (isMounted) setRooms(data);
      } catch (err: any) {
        if (isMounted) setError(err?.Message || err?.message || 'Không thể tải danh sách chat.');
      } finally {
        if (isMounted) setIsLoading(false);
      }
    };

    fetchRooms();

    return () => {
      isMounted = false;
    };
  }, [role, folder]);

  const archiveRoom = async (roomId: string) => {
    try {
      await chatRepository.archiveRoom(roomId);
      setRooms((prev) => prev.filter(r => r.id !== roomId));
    } catch (err) {
      console.error('Archive failed', err);
    }
  };

  return {
    rooms,
    isLoading,
    error,
    archiveRoom,
  };
};

