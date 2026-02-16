import { Link, useLocation } from "react-router-dom";

type Tab = {
  to: string;
  label: string;
  icon: string;
};

const tabs: Tab[] = [
  { to: "/", label: "Home", icon: "🏠" },
  { to: "/feed", label: "Feed", icon: "📰" },
  { to: "/leaders", label: "Leaders", icon: "🏆" },
];

function isActiveTab(pathname: string, tabTo: string): boolean {
  if (tabTo === "/") return pathname === "/";
  return pathname === tabTo || pathname.startsWith(`${tabTo}/`);
}

export default function BottomNav() {
  const { pathname } = useLocation();

  return (
    <nav
      className="fixed bottom-0 left-0 right-0 z-40 bg-[var(--color-surface)] shadow-[0_-1px_3px_var(--color-shadow)] pb-safe"
      aria-label="Primary"
    >
      <div className="grid grid-cols-3">
        {tabs.map((t) => {
          const active = isActiveTab(pathname, t.to);
          const textClass = active
            ? "text-[var(--color-accent)]"
            : "text-[var(--color-text-muted)]";

          return (
            <Link
              key={t.to}
              to={t.to}
              className={`py-3 active:opacity-70 ${textClass}`}
            >
              <div className="flex flex-col items-center justify-center gap-1">
                <span className="text-lg" aria-hidden="true">
                  {t.icon}
                </span>
                <span className="text-xs font-bold">{t.label}</span>
              </div>
            </Link>
          );
        })}
      </div>
    </nav>
  );
}
