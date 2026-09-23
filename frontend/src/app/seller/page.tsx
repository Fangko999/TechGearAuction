import { redirect } from 'next/navigation';

export default function SellerDashboardPage() {
  // Redirect to create auction page by default for now
  redirect('/seller/auctions/create');
}

