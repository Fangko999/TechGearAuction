import { useState, useEffect, useRef } from 'react';
import { ChatRepository } from '@infrastructure/repositories/ChatRepository';
import { ChatHubService } from '@infrastructure/signalr/ChatHubService';
import { ChatMessageDto, SendMessageRequest } from '@domain/models/chat';

const chatRepository = new ChatRepository();

export const useChatRoom = (roomId: string | null) => {
  const [messages, setMessages] = useState<ChatMessageDto[]>([]);
  const [isLoading, setIsLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);
  
  const hubServiceRef = useRef<ChatHubService | null>(null);

  useEffect(() => {
    if (!roomId) return;
    
    let isMounted = true;
    hubServiceRef.current = new ChatHubService();

    const fetchMessages = async () => {
      setIsLoading(true);
      try {
        const pagedData = await chatRepository.getMessages(roomId, 1, 100);
        if (isMounted) {
          setMessages(pagedData.items);
          // Auto mark as read when fetching
          await chatRepository.markAsRead(roomId);
        }
      } catch (err: any) {
        if (isMounted) setError(err?.Message || err?.message || 'Không thể tải tin nhắn.');
      } finally {
        if (isMounted) setIsLoading(false);
      }
    };

    const setupSignalR = async () => {
      const hub = hubServiceRef.current;
      if (hub) {
        await hub.connect();
        await hub.joinRoom(roomId);

        hub.onReceiveMessage((newMessage) => {
          setMessages((prev) => [...prev, newMessage]);
          // Optional: mark as read if room is focused
          chatRepository.markAsRead(roomId).catch(console.error);
        });
      }
    };

    fetchMessages();
    setupSignalR();

    return () => {
      isMounted = false;
      const hub = hubServiceRef.current;
      if (hub) {
        hub.leaveRoom(roomId);
        hub.disconnect();
      }
    };
  }, [roomId]);

  const handleSendMessage = async (request: SendMessageRequest) => {
    if (!roomId) return;
    try {
      await chatRepository.sendMessage(roomId, request);
      // Not pushing to messages here, waiting for SignalR to echo it back to ensure synchronization
    } catch (err) {
      console.error('Send message error:', err);
      throw err;
    }
  };

  const handleUploadMedia = async (file: File) => {
    if (!roomId) throw new Error('No room selected');
    return await chatRepository.uploadMedia(roomId, file);
  };

  return {
    messages,
    isLoading,
    error,
    handleSendMessage,
    handleUploadMedia
  };
};

