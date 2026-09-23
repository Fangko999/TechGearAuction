import { Navbar } from '@presentation/components/Navbar';

export default function PublicLayout({ children }: { children: React.ReactNode }) {
  return (
    <>
      <Navbar />
      <div className="flex-1">
        {children}
      </div>
    </>
  );
}
