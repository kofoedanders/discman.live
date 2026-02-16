import { useAuthStore } from "../../stores/authStore";

export default function AppHeader({ title }: { title: string }) {
  const logout = useAuthStore((s) => s.logout);

  return (
    <header className="flex items-center justify-between px-4 py-4 bg-[var(--color-navbar)] shadow-[0_1px_3px_var(--color-shadow)] z-10">
      <span className="text-xl font-bold text-[var(--color-text)]">
        {title}
      </span>
      <button
        onClick={logout}
        className="text-sm font-semibold text-[var(--color-text-muted)] active:opacity-60"
      >
        Log Out
      </button>
    </header>
  );
}
