import type { HoleStats, CurrentPace } from "../../types";
import { formatTime } from "../../utils/scoring";

interface StatusStripProps {
  holeNumber: number;
  holePar: number;
  holeDistance: number;
  currentPace: CurrentPace | null;
  holeStats: HoleStats | undefined;
  previousScores: number[];
}

export default function StatusStrip({
  holeNumber,
  holePar,
  holeDistance,
  currentPace,
  holeStats,
  previousScores,
}: StatusStripProps) {
  const finishTimeStr =
    currentPace?.estimatedFinishTime
      ? formatTime(new Date(currentPace.estimatedFinishTime))
      : "--:--";

  const hasBirdie = holeStats?.birdie ?? false;

  return (
    <div className="px-3 py-2 bg-[var(--color-navbar)] border-b-2 border-[var(--color-border)]">
      <div className="flex items-center justify-between">
        <div className="flex items-center gap-2">
          <span className="text-lg font-bold text-[var(--color-text)]">
            H{holeNumber}
          </span>
          <span className="text-sm font-semibold text-[var(--color-text-muted)]">
            Par {holePar}
          </span>
          {holeDistance > 0 && (
            <span className="text-sm text-[var(--color-text-muted)]">
              {holeDistance}m
            </span>
          )}
        </div>
        <span className="text-sm font-semibold text-[var(--color-text-muted)]">
          ⏱ ~{finishTimeStr}
        </span>
      </div>

      <div className="flex items-center gap-2 mt-0.5">
        {hasBirdie && <span className="text-sm">🕊</span>}
        {previousScores.length > 0 && (
          <span className="text-xs font-medium text-[var(--color-text-muted)]">
            prev:{" "}
            {previousScores.map((s, i) => (
              <span key={i} className="ml-1">
                {s}
              </span>
            ))}
          </span>
        )}
      </div>
    </div>
  );
}
