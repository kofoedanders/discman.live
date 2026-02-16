import type { UserStats } from "../../types";

interface StatsDialogProps {
  stats: UserStats[];
  onClose: () => void;
}

export default function StatsDialog({ stats, onClose }: StatsDialogProps) {
  return (
    <div className="fixed inset-0 z-50 flex flex-col bg-[var(--color-bg)] animate-slide-up">
      <header className="flex items-center justify-between px-4 py-4 bg-[var(--color-navbar)] shadow-[0_1px_3px_var(--color-shadow)] z-10">
        <span className="text-lg font-bold text-[var(--color-text)]">
          Round Stats
        </span>
        <button
          onClick={onClose}
          className="px-4 py-2 rounded-lg bg-[var(--color-surface)] text-sm font-bold text-[var(--color-text)] shadow-sm active:scale-95 transition-transform"
        >
          Close
        </button>
      </header>

      <div className="flex-1 overflow-y-auto p-4 space-y-4 pb-safe">
        {stats.length === 0 && (
          <div className="flex flex-col items-center justify-center h-64 text-center p-8">
            <span className="text-4xl mb-4">📊</span>
            <p className="text-base font-bold text-[var(--color-text)] mb-2">
              No stats available yet
            </p>
            <p className="text-sm text-[var(--color-text-muted)]">
              Complete the round to see full statistics.
            </p>
          </div>
        )}

        {stats.map((s) => (
          <div
            key={s.username}
            className="p-5 rounded-2xl bg-gradient-to-br from-[var(--color-surface)] to-[var(--color-bg)] shadow-md border border-[var(--color-border)]/10"
          >
            <p className="text-base font-bold text-[var(--color-text)] mb-4 flex items-center gap-2 border-b border-[var(--color-border)]/10 pb-2">
              <span className="w-2 h-2 rounded-full bg-[var(--color-accent)]"></span>
              {s.username}
            </p>
            <div className="grid grid-cols-2 gap-x-6 gap-y-6">
              <StatItem label="Fairway" value={s.fairwayHitRate} color="bg-[var(--color-fairway)]" />
              <StatItem label="C1 in Reg" value={s.circle1Rate} color="bg-[var(--color-circle1)]" />
              <StatItem label="C2 in Reg" value={s.circle2Rate} color="bg-[var(--color-circle2)]" />
              <StatItem label="Scramble" value={s.scrambleRate} color="bg-[var(--color-par)]" />
              <StatItem label="Birdie Rate" value={s.birdieRate} color="bg-[var(--color-birdie)]" />
              <StatItem label="OB Rate" value={s.obRate} color="bg-[var(--color-ob)]" invert />
            </div>
          </div>
        ))}
      </div>
    </div>
  );
}

function StatItem({ label, value, color }: { label: string; value: number; color: string; invert?: boolean }) {
  const percentage = Math.round(value * 100);
  const displayValue = `${percentage}%`;
  
  return (
    <div className="flex flex-col gap-1.5">
      <div className="flex justify-between items-end">
        <span className="text-xs font-bold text-[var(--color-text-muted)] uppercase tracking-wide">{label}</span>
        <span className="text-lg font-black text-[var(--color-text)]">{displayValue}</span>
      </div>
      <div className="w-full h-2 rounded-full bg-[var(--color-bg)] shadow-inner overflow-hidden">
        <div 
          className={`h-full rounded-full ${color} transition-all duration-1000 ease-out`} 
          style={{ width: displayValue }} 
        />
      </div>
    </div>
  );
}
