interface BottomBarProps {
  onScorecard: () => void;
  onStats: () => void;
}

export default function BottomBar({ onScorecard, onStats }: BottomBarProps) {
  return (
    <div className="flex bg-gradient-to-r from-[var(--color-navbar)] to-[var(--color-gradient-end)] shadow-[0_-1px_3px_var(--color-shadow)] relative z-20 pb-safe">
      <button
        onClick={onScorecard}
        className="flex-1 py-4 text-center text-sm font-bold text-[var(--color-text)] active:opacity-60 flex items-center justify-center gap-2"
      >
        <span className="text-lg">📋</span> Scorecard
      </button>
      <div className="w-px bg-[var(--color-border)]/10 my-3" />
      <button
        onClick={onStats}
        className="flex-1 py-4 text-center text-sm font-bold text-[var(--color-text)] active:opacity-60 flex items-center justify-center gap-2"
      >
        <span className="text-lg">📊</span> Stats
      </button>
    </div>
  );
}
