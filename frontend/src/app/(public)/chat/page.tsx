'use client';

import React, { useState, useRef, useEffect } from 'react';
import { useChat } from '@application/hooks/useChat';
import { useChatRoom } from '@application/hooks/useChatRoom';
import { useAuthStore } from '@application/store/useAuthStore';
import { ChatRoomDto, ChatMessageType } from '@domain/models/chat';

export default function ChatPage() {
  const currentUser = useAuthStore((state) => state.user);
  
  const [roleFilter, setRoleFilter] = useState<'All' | 'Buyer' | 'Seller'>('All');
  const [folderFilter, setFolderFilter] = useState<'Inbox' | 'Archived'>('Inbox');
  
  const { rooms, isLoading: isRoomsLoading, archiveRoom } = useChat(roleFilter, folderFilter);
  const [selectedRoom, setSelectedRoom] = useState<ChatRoomDto | null>(null);

  const { messages, isLoading: isMessagesLoading, handleSendMessage, handleUploadMedia } = useChatRoom(selectedRoom?.id || null);

  const [inputText, setInputText] = useState('');
  const [isSending, setIsSending] = useState(false);
  const fileInputRef = useRef<HTMLInputElement>(null);
  const messagesEndRef = useRef<HTMLDivElement>(null);

  useEffect(() => {
    messagesEndRef.current?.scrollIntoView({ behavior: 'smooth' });
  }, [messages]);

  const onSendText = async (e: React.FormEvent) => {
    e.preventDefault();
    if (!inputText.trim() || !selectedRoom) return;
    
    setIsSending(true);
    try {
      await handleSendMessage({
        content: inputText,
        messageType: ChatMessageType.Text
      });
      setInputText('');
    } catch (err) {
      alert('Lỗi gửi tin nhắn');
    } finally {
      setIsSending(false);
    }
  };

  const onFileSelect = async (e: React.ChangeEvent<HTMLInputElement>) => {
    const file = e.target.files?.[0];
    if (!file || !selectedRoom) return;

    setIsSending(true);
    try {
      const mediaUrl = await handleUploadMedia(file);
      const isVideo = file.type.startsWith('video/');
      
      await handleSendMessage({
        content: isVideo ? 'Đã gửi một video' : 'Đã gửi một hình ảnh',
        messageType: isVideo ? ChatMessageType.Video : ChatMessageType.Image,
        mediaUrl
      });
    } catch (err) {
      alert('Lỗi tải file');
    } finally {
      setIsSending(false);
      if (fileInputRef.current) fileInputRef.current.value = '';
    }
  };

  const onArchive = async () => {
    if (selectedRoom) {
      await archiveRoom(selectedRoom.id);
      setSelectedRoom(null);
    }
  };

  return (
    <div className="flex h-[calc(100vh-64px)] bg-gray-50 overflow-hidden">
      
      {/* Sidebar: Chat Rooms List */}
      <div className="w-1/3 max-w-md bg-white border-r border-gray-200 flex flex-col">
        <div className="p-4 border-b border-gray-200">
          <h2 className="text-xl font-bold text-gray-800 mb-4">Tin nhắn</h2>
          
          <div className="flex gap-2 mb-4">
            <select 
              value={roleFilter} 
              onChange={(e) => setRoleFilter(e.target.value as any)}
              className="flex-1 text-sm border-gray-300 rounded-md shadow-sm focus:ring-blue-500 focus:border-blue-500"
            >
              <option value="All">Tất cả vai trò</option>
              <option value="Buyer">Tôi là Người mua</option>
              <option value="Seller">Tôi là Người bán</option>
            </select>

            <select 
              value={folderFilter} 
              onChange={(e) => setFolderFilter(e.target.value as any)}
              className="flex-1 text-sm border-gray-300 rounded-md shadow-sm focus:ring-blue-500 focus:border-blue-500"
            >
              <option value="Inbox">Hộp thư đến</option>
              <option value="Archived">Đã lưu trữ</option>
            </select>
          </div>
        </div>

        <div className="flex-1 overflow-y-auto">
          {isRoomsLoading ? (
            <div className="p-8 text-center text-gray-500">Đang tải...</div>
          ) : rooms.length === 0 ? (
            <div className="p-8 text-center text-gray-500 text-sm">Không có cuộc hội thoại nào.</div>
          ) : (
            <ul className="divide-y divide-gray-100">
              {rooms.map(room => (
                <li 
                  key={room.id}
                  onClick={() => setSelectedRoom(room)}
                  className={`p-4 cursor-pointer hover:bg-blue-50 transition-colors ${selectedRoom?.id === room.id ? 'bg-blue-50' : ''}`}
                >
                  <div className="flex items-center space-x-4">
                    <div className="relative">
                      {room.opponentAvatarUrl ? (
                        <img src={room.opponentAvatarUrl} alt="" className="w-12 h-12 rounded-full object-cover" />
                      ) : (
                        <div className="w-12 h-12 bg-gray-200 rounded-full flex items-center justify-center font-bold text-gray-500">
                          {room.opponentName[0]}
                        </div>
                      )}
                      {room.unreadCount > 0 && (
                        <span className="absolute -top-1 -right-1 flex items-center justify-center w-5 h-5 text-xs font-bold text-white bg-red-500 rounded-full border-2 border-white">
                          {room.unreadCount}
                        </span>
                      )}
                    </div>
                    
                    <div className="flex-1 min-w-0">
                      <p className="text-sm font-medium text-gray-900 truncate">{room.opponentName}</p>
                      <p className="text-xs text-gray-500 truncate mt-0.5">Sản phẩm: {room.auctionTitle}</p>
                    </div>
                    
                    {room.auctionThumbnailUrl && (
                      <img src={room.auctionThumbnailUrl} alt="" className="w-10 h-10 rounded border border-gray-200 object-cover" />
                    )}
                  </div>
                </li>
              ))}
            </ul>
          )}
        </div>
      </div>

      {/* Main Area: Chat Messages */}
      <div className="flex-1 flex flex-col bg-gray-50">
        {selectedRoom ? (
          <>
            {/* Chat Header */}
            <div className="h-16 flex items-center justify-between px-6 bg-white border-b border-gray-200">
              <div className="flex items-center space-x-3">
                <span className="font-semibold text-gray-800">{selectedRoom.opponentName}</span>
                <span className="text-sm text-gray-500">|</span>
                <span className="text-sm text-blue-600 truncate max-w-xs" title={selectedRoom.auctionTitle}>{selectedRoom.auctionTitle}</span>
              </div>
              <button 
                onClick={onArchive}
                className="px-3 py-1.5 text-xs font-medium text-gray-600 bg-gray-100 rounded-md hover:bg-gray-200 transition-colors"
              >
                Lưu trữ / Ẩn
              </button>
            </div>

            {/* Chat Messages */}
            <div className="flex-1 p-6 overflow-y-auto">
              {isMessagesLoading ? (
                <div className="flex justify-center items-center h-full">
                  <div className="w-8 h-8 border-4 border-blue-500 border-t-transparent rounded-full animate-spin"></div>
                </div>
              ) : (
                <div className="space-y-6">
                  {messages.map((msg) => {
                    if (msg.messageType === ChatMessageType.SystemText) {
                      return (
                        <div key={msg.id} className="flex justify-center">
                          <span className="px-4 py-1.5 bg-gray-200 text-gray-600 text-xs rounded-full shadow-sm">
                            {msg.content}
                          </span>
                        </div>
                      );
                    }

                    const isMine = msg.senderId === currentUser?.userId;

                    return (
                      <div key={msg.id} className={`flex ${isMine ? 'justify-end' : 'justify-start'}`}>
                        <div className={`max-w-[70%] rounded-2xl px-4 py-2 ${isMine ? 'bg-blue-600 text-white rounded-br-none shadow-md' : 'bg-white text-gray-800 border border-gray-200 rounded-bl-none shadow-sm'}`}>
                          
                          {/* Media Handling */}
                          {msg.messageType === ChatMessageType.Image && msg.mediaUrl && (
                            <img src={msg.mediaUrl} alt="chat media" className="max-w-full max-h-64 rounded-lg mb-2 object-cover" />
                          )}
                          
                          {msg.messageType === ChatMessageType.Video && msg.mediaUrl && (
                            <video src={msg.mediaUrl} controls className="max-w-full max-h-64 rounded-lg mb-2 bg-black" />
                          )}

                          <p className="text-sm whitespace-pre-wrap leading-relaxed">{msg.content}</p>
                          
                          <p className={`text-[10px] mt-1 text-right ${isMine ? 'text-blue-200' : 'text-gray-400'}`}>
                            {new Date(msg.createdAt).toLocaleTimeString([], { hour: '2-digit', minute: '2-digit' })}
                          </p>
                        </div>
                      </div>
                    );
                  })}
                  <div ref={messagesEndRef} />
                </div>
              )}
            </div>

            {/* Chat Input */}
            <div className="p-4 bg-white border-t border-gray-200">
              <form onSubmit={onSendText} className="flex items-end space-x-2">
                <button 
                  type="button"
                  onClick={() => fileInputRef.current?.click()}
                  disabled={isSending}
                  className="p-3 text-gray-500 hover:text-blue-600 bg-gray-100 hover:bg-blue-50 rounded-full transition-colors flex-shrink-0"
                >
                  <svg className="w-5 h-5" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M15.172 7l-6.586 6.586a2 2 0 102.828 2.828l6.414-6.586a4 4 0 00-5.656-5.656l-6.415 6.585a6 6 0 108.486 8.486L20.5 13" /></svg>
                </button>
                <input 
                  type="file" 
                  ref={fileInputRef} 
                  onChange={onFileSelect} 
                  accept="image/*,video/*" 
                  className="hidden" 
                />
                
                <input
                  type="text"
                  value={inputText}
                  onChange={(e) => setInputText(e.target.value)}
                  placeholder="Nhập tin nhắn..."
                  disabled={isSending}
                  className="flex-1 bg-gray-100 border-transparent focus:bg-white focus:ring-2 focus:ring-blue-500 focus:border-transparent rounded-full px-5 py-3 text-sm transition-all"
                />
                
                <button 
                  type="submit"
                  disabled={isSending || !inputText.trim()}
                  className="p-3 text-white bg-blue-600 hover:bg-blue-700 rounded-full transition-colors disabled:opacity-50 flex-shrink-0"
                >
                  {isSending ? (
                    <div className="w-5 h-5 border-2 border-white border-t-transparent rounded-full animate-spin"></div>
                  ) : (
                    <svg className="w-5 h-5" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M12 19l9 2-9-18-9 18 9-2zm0 0v-8" /></svg>
                  )}
                </button>
              </form>
            </div>
          </>
        ) : (
          <div className="flex flex-col items-center justify-center h-full text-gray-500">
            <svg className="w-16 h-16 mb-4 text-gray-300" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path strokeLinecap="round" strokeLinejoin="round" strokeWidth={1} d="M8 12h.01M12 12h.01M16 12h.01M21 12c0 4.418-4.03 8-9 8a9.863 9.863 0 01-4.255-.949L3 20l1.395-3.72C3.512 15.042 3 13.574 3 12c0-4.418 4.03-8 9-8s9 3.582 9 8z" /></svg>
            <p>Chọn một cuộc hội thoại để bắt đầu nhắn tin</p>
          </div>
        )}
      </div>
    </div>
  );
}

