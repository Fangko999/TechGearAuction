import * as signalR from '@microsoft/signalr';
import Cookies from 'js-cookie';
import { ChatMessageDto } from '@domain/models/chat';

export class ChatHubService {
  private connection: signalR.HubConnection | null = null;

  public async connect() {
    if (this.connection) return;

    const token = Cookies.get('token');
    const baseUrl = (process.env.NEXT_PUBLIC_API_URL || 'http://localhost:8888/api').replace('/api', '');

    this.connection = new signalR.HubConnectionBuilder()
      .withUrl(`${baseUrl}/hubs/chat`, {
        accessTokenFactory: () => token || '',
      })
      .withAutomaticReconnect()
      .build();

    try {
      await this.connection.start();
      console.log('Chat SignalR Connected.');
    } catch (err) {
      console.error('Chat SignalR Connection Error: ', err);
    }
  }

  public async joinRoom(roomId: string) {
    if (this.connection?.state === signalR.HubConnectionState.Connected) {
      await this.connection.invoke('JoinChatRoom', roomId);
    }
  }

  public async leaveRoom(roomId: string) {
    if (this.connection?.state === signalR.HubConnectionState.Connected) {
      await this.connection.invoke('LeaveChatRoom', roomId);
    }
  }

  public onReceiveMessage(callback: (message: ChatMessageDto) => void) {
    if (this.connection) {
      this.connection.on('ReceiveMessage', (msg: any) => {
        // Map any differences if necessary, but assuming signalr passes object similar to ChatMessageDto
        callback(msg);
      });
    }
  }

  public async disconnect() {
    if (this.connection) {
      await this.connection.stop();
      this.connection = null;
    }
  }
}
