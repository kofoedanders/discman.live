import type { UserStats } from "../../types";

interface StatsDialogProps {
  stats: UserStats[];
  onClose: () => void;
}

export default function StatsDialog({ stats, onClose }: StatsDialogProps) {
  return (
    <div className="fixed inset-0 z-40 flex flex-col bg-[var(--color-bg)]">
      <header className="flex items-center justify-between px-4 py-3 bg-[var(--color-navbar)] border-b-2 border-[var(--color-border)]">
        <span className="text-base font-bold text-[var(--color-text)]">
          Round Stats
        </span>
        <button
          onClick={onClose}
          className="text-sm font-bold text-[var(--color-accent)] active:opacity-60"
        >
          Close
        </button>
      </header>

      <div className="flex-1 overflow-y-auto p-4 space-y-4">
        {stats.length === 0 && (
          <p className="text-sm text-[var(--color-text-muted)] text-center py-8">
            Stats will be available after the round is completed.
          </p>
        )}

        {stats.map((s) => (
          <div
            key={s.username}
            className="p-3 rounded-xl bg-[var(--color-surface)] border-2 border-[var(--color-border)]"
          >
            <p className="text-sm font-bold text-[var(--color-text)] mb-2">
              {s.username}
            </p>
            <div className="grid grid-cols-2 gap-y-1 gap-x-4 text-xs">
              <StatRow label="Fairway %" value={`${(s.fairwayHitRate * 100).toFixed(0)}%`} />
              <StatRow label="C1 %" value={`${(s.circle1Rate * 100).toFixed(0)}%`} />
              <StatRow label="C2 %" value={`${(s.circle2Rate * 100).toFixed(0)}%`} />
              <StatRow label="Scramble %" value={`${(s.scrambleRate * 100).toFixed(0)}%`} />
              <StatRow label="OB %" value={`${(s.obRate * 100).toFixed(0)}%`} />
              <StatRow label="Birdie %" value={`${(s.birdieRate * 100).toFixed(0)}%`} />
            </div>
          </div>
        ))}
      </div>
    </div>
  );
}

function StatRow({ label, value }: { label: string; value: string }) {
  return (
    <div className="flex justify-between">
      <span className="text-[var(--color-text-muted)]">{label}</span>
      <span className="font-bold text-[var(--color-text)]">{value}</span>
    </div>
  );
}
