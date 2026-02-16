interface BottomBarProps {
  onScorecard: () => void;
  onStats: () => void;
}

export default function BottomBar({ onScorecard, onStats }: BottomBarProps) {
  return (
    <div className="flex border-t-2 border-[var(--color-border)] bg-[var(--color-navbar)]">
      <button
        onClick={onScorecard}
        className="flex-1 py-3 text-center text-sm font-bold text-[var(--color-text)] active:opacity-60"
      >
        📋 Scorecard
      </button>
      <div className="w-px bg-[var(--color-border)]" />
      <button
        onClick={onStats}
        className="flex-1 py-3 text-center text-sm font-bold text-[var(--color-text)] active:opacity-60"
      >
        📊 Stats
      </button>
    </div>
  );
}
