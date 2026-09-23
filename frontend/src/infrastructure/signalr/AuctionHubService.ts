import * as signalR from '@microsoft/signalr';
import Cookies from 'js-cookie';

export class AuctionHubService {
  private connection: signalR.HubConnection | null = null;

  public async connect() {
    if (this.connection) return;

    // Get token to pass for Authorization if needed
    const token = Cookies.get('token');

    const baseUrl = (process.env.NEXT_PUBLIC_API_URL || 'http://localhost:8888/api').replace('/api', '');
    
    this.connection = new signalR.HubConnectionBuilder()
      .withUrl(`${baseUrl}/hubs/auction`, {
        accessTokenFactory: () => token || '',
      })
      .withAutomaticReconnect()
      .build();

    try {
      await this.connection.start();
      console.log('SignalR Connected.');
    } catch (err) {
      console.error('SignalR Connection Error: ', err);
    }
  }

  public async joinAuction(auctionId: string) {
    if (this.connection?.state === signalR.HubConnectionState.Connected) {
      await this.connection.invoke('JoinAuctionGroup', auctionId);
    }
  }

  public async leaveAuction(auctionId: string) {
    if (this.connection?.state === signalR.HubConnectionState.Connected) {
      await this.connection.invoke('LeaveAuctionGroup', auctionId);
    }
  }

  public onPriceUpdated(callback: (price: number) => void) {
    if (this.connection) {
      this.connection.on('ReceivePriceUpdate', (price: number) => {
        callback(price);
      });
    }
  }

  public onNewBid(callback: (bidderName: string, amount: number, timestamp: string) => void) {
    if (this.connection) {
      this.connection.on('ReceiveNewBid', (bidderName: string, amount: number, timestamp: string) => {
        callback(bidderName, amount, timestamp);
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
