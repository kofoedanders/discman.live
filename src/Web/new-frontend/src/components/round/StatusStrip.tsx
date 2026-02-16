import type { HoleStats, CurrentPace, HoleScore } from "../../types";
import { formatTime } from "../../utils/scoring";

interface StatusStripProps {
  holeNumber: number;
  holePar: number;
  holeDistance: number;
  currentPace: CurrentPace | null;
  holeStats: HoleStats | undefined;
  previousScores: HoleScore[];
  activeHoleIndex: number;
  totalHoles: number;
  setActiveHole: (index: number) => void;
  onMenuOpen: () => void;
}

export default function StatusStrip({
  holeNumber,
  holePar,
  holeDistance,
  currentPace,
  holeStats,
  previousScores,
  activeHoleIndex,
  totalHoles,
  setActiveHole,
  onMenuOpen,
}: StatusStripProps) {
  const finishTimeStr =
    currentPace?.estimatedFinishTime
      ? formatTime(new Date(currentPace.estimatedFinishTime))
      : "--:--";

  const hasBirdie = holeStats?.birdie ?? false;

  return (
    <div className="px-4 py-3 bg-gradient-to-r from-[var(--color-navbar)] to-[var(--color-gradient-end)] shadow-[0_1px_3px_var(--color-shadow)] z-20">
      <div className="flex items-center justify-between">
        <div className="flex items-center gap-3">
          <button
            onClick={() => setActiveHole(activeHoleIndex - 1)}
            disabled={activeHoleIndex === 0}
            className="text-[var(--color-text-muted)] disabled:opacity-20 p-2 -ml-2 active:scale-90 transition-transform"
          >
            <svg width="24" height="24" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2.5" strokeLinecap="round" strokeLinejoin="round">
              <path d="M15 18l-6-6 6-6" />
            </svg>
          </button>

          <div className="w-12 h-12 rounded-full bg-[var(--color-accent)] text-white flex items-center justify-center text-xl font-bold shadow-md">
            {holeNumber}
          </div>

          <button
            onClick={() => setActiveHole(activeHoleIndex + 1)}
            disabled={activeHoleIndex === totalHoles - 1}
            className="text-[var(--color-text-muted)] disabled:opacity-20 p-2 active:scale-90 transition-transform"
          >
            <svg width="24" height="24" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2.5" strokeLinecap="round" strokeLinejoin="round">
              <path d="M9 18l6-6-6-6" />
            </svg>
          </button>

          <div className="flex flex-col gap-1 ml-1">
            <div className="flex gap-2">
              <span className="px-2.5 py-0.5 rounded-full bg-[var(--color-accent-light)] text-xs font-bold text-[var(--color-accent)] border border-[var(--color-accent)]/10">
                Par {holePar}
              </span>
              {holeDistance > 0 && (
                <span className="px-2.5 py-0.5 rounded-full bg-[var(--color-surface)] text-xs font-semibold text-[var(--color-text-muted)] border border-[var(--color-border)]/10">
                  {holeDistance}m
                </span>
              )}
            </div>
          </div>
        </div>

        <button 
          onClick={onMenuOpen}
          className="p-2 -mr-2 text-[var(--color-text-muted)] active:scale-90 transition-transform"
        >
          <svg width="24" height="24" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2" strokeLinecap="round" strokeLinejoin="round">
            <circle cx="12" cy="12" r="1" />
            <circle cx="12" cy="5" r="1" />
            <circle cx="12" cy="19" r="1" />
          </svg>
        </button>
      </div>

      <div className="flex items-center justify-between mt-3 pl-1">
        <div className="flex items-center gap-2">
          {hasBirdie && <span className="text-base animate-pulse-dot">🕊</span>}
          {previousScores.length > 0 && (
            <div className="flex items-center gap-1.5">
              <span className="text-xs font-medium text-[var(--color-text-muted)] mr-1">
                Last:
              </span>
              {previousScores.map((score, i) => {
                 let dotColor = "bg-[var(--color-par)]";
                 if (score.relativeToPar < 0) dotColor = "bg-[var(--color-birdie)]";
                 if (score.relativeToPar > 0) dotColor = "bg-[var(--color-bogey)]";
                 
                 return (
                  <div 
                    key={i} 
                    className={`w-2.5 h-2.5 rounded-full ${dotColor} shadow-sm`} 
                    title={`${score.strokes}`}
                  />
                 );
              })}
            </div>
          )}
        </div>
        
        <span className="text-xs font-semibold text-[var(--color-text-muted)] bg-[var(--color-surface)]/50 px-2 py-1 rounded-lg">
          ⏱ ~{finishTimeStr}
        </span>
      </div>
    </div>
  );
}
