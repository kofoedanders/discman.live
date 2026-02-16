import type { ReactNode } from "react";
import AppHeader from "./AppHeader";
import BottomNav from "./BottomNav";

export default function DashboardLayout({
  title,
  children,
}: {
  title: string;
  children: ReactNode;
}) {
  return (
    <div className="flex-1 flex flex-col bg-[var(--color-bg)] h-full">
      <AppHeader title={title} />
      <main className="flex-1 overflow-y-auto px-4 py-6 pb-24">
        {children}
      </main>
      <BottomNav />
    </div>
  );
}
