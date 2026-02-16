import type { PlayerScore, HoleScore } from "../../types";
import {
  formatRelativeToPar,
  scoreColorClass,
  totalRelativeToPar,
} from "../../utils/scoring";

interface PlayerListProps {
  players: PlayerScore[];
  activeHoleIndex: number;
  currentUsername: string;
  onSelectPlayer: (username: string) => void;
}

export default function PlayerList({
  players,
  activeHoleIndex,
  currentUsername,
  onSelectPlayer,
}: PlayerListProps) {
  return (
    <div className="px-3 py-2 space-y-1">
      {players.map((p) => {
        const holeScore: HoleScore | undefined = p.scores[activeHoleIndex];
        const hasScored = holeScore ? holeScore.strokes > 0 : false;
        const holeStrokes = holeScore?.strokes ?? 0;
        const total = totalRelativeToPar(p.scores);
        const isCurrentUser = p.playerName === currentUsername;

          return (
            <button
              key={p.playerName}
              onClick={() => onSelectPlayer(p.playerName)}
              className={`w-full flex items-center justify-between px-4 py-3 rounded-xl shadow-sm transition-all active:scale-[0.99] ${
                isCurrentUser
                  ? "border-l-4 border-l-[var(--color-accent)] bg-[var(--color-surface)] shadow-[var(--color-shadow)]"
                  : "bg-[var(--color-bg)] border border-[var(--color-border)]/10"
              }`}
            >
              <div className="flex items-center gap-3">
                <span
                  className={`w-3 h-3 rounded-full transition-all ${
                    hasScored
                      ? "bg-[var(--color-birdie)] scale-110 shadow-sm"
                      : "border-2 border-[var(--color-text-muted)] opacity-40 animate-pulse-dot"
                  }`}
                />
                <span className={`text-sm font-bold ${isCurrentUser ? "text-[var(--color-text)]" : "text-[var(--color-text-muted)]"}`}>
                  {p.playerEmoji} {p.playerName}
                </span>
              </div>
              <div className="flex items-center gap-4">
                <span className={`text-base font-bold w-8 text-center ${hasScored ? "text-[var(--color-text)]" : "text-[var(--color-text-muted)]/30"}`}>
                  {hasScored ? holeStrokes : "-"}
                </span>
                <span
                  className={`text-sm font-bold min-w-[2.5rem] text-right ${scoreColorClass(
                    total,
                  )}`}
                >
                  {formatRelativeToPar(total)}
                </span>
              </div>
            </button>
          );
        })}
      </div>
    );
  }
