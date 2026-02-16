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
            className={`w-full flex items-center justify-between px-3 py-2 rounded-lg border-2 active:opacity-80 ${
              isCurrentUser
                ? "border-[var(--color-accent)] bg-[var(--color-surface)]"
                : "border-transparent bg-[var(--color-bg)]"
            }`}
          >
            <div className="flex items-center gap-2">
              <span
                className={`w-2.5 h-2.5 rounded-full ${
                  hasScored
                    ? "bg-[var(--color-accent)]"
                    : "border-2 border-[var(--color-text-muted)]"
                }`}
              />
              <span className="text-sm font-bold text-[var(--color-text)]">
                {p.playerEmoji} {p.playerName}
              </span>
            </div>
            <div className="flex items-center gap-3">
              <span className="text-sm font-medium text-[var(--color-text)]">
                {hasScored ? holeStrokes : "-"}
              </span>
              <span
                className={`text-sm font-bold min-w-[2.5rem] text-right ${scoreColorClass(
                  total,
                )}`}
              >
                ({formatRelativeToPar(total)})
              </span>
            </div>
          </button>
        );
      })}
    </div>
  );
}
