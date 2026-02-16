import type { Round } from "../../types";
import {
  formatRelativeToPar,
  scoreColorClass,
  totalRelativeToPar,
} from "../../utils/scoring";

interface ScorecardOverlayProps {
  round: Round;
  onClose: () => void;
}

export default function ScorecardOverlay({
  round,
  onClose,
}: ScorecardOverlayProps) {
  const holes = round.playerScores[0]?.scores ?? [];

  return (
    <div className="fixed inset-0 z-40 flex flex-col bg-[var(--color-bg)] animate-slide-up">
      <header className="flex items-center justify-between px-4 py-4 bg-[var(--color-navbar)] shadow-[0_1px_3px_var(--color-shadow)] z-20">
        <span className="text-lg font-bold text-[var(--color-text)]">
          Scorecard
        </span>
        <button
          onClick={onClose}
          className="px-4 py-2 rounded-lg bg-[var(--color-surface)] text-sm font-bold text-[var(--color-text)] shadow-sm active:scale-95 transition-transform"
        >
          Close
        </button>
      </header>

      <div className="flex-1 overflow-auto p-2 pb-safe">
        <div className="rounded-xl border border-[var(--color-border)]/10 shadow-sm overflow-hidden bg-[var(--color-surface)]/30">
          <table className="w-full text-xs border-collapse">
            <thead className="bg-[var(--color-surface)] sticky top-0 z-10 shadow-sm">
              <tr>
                <th className="text-left py-3 px-2 font-bold text-[var(--color-text)] sticky left-0 bg-[var(--color-surface)] z-20 border-b border-[var(--color-border)]/10 min-w-[100px]">
                  Hole
                </th>
                {holes.map((h) => (
                  <th
                    key={h.hole.number}
                    className="py-3 px-1 font-bold text-[var(--color-text)] text-center min-w-[32px] border-b border-[var(--color-border)]/10"
                  >
                    {h.hole.number}
                  </th>
                ))}
                <th className="py-3 px-2 font-bold text-[var(--color-text)] text-center border-b border-[var(--color-border)]/10 min-w-[40px]">
                  Tot
                </th>
              </tr>
              <tr>
                <td className="py-2 px-2 text-[var(--color-text-muted)] font-medium sticky left-0 bg-[var(--color-surface)] z-20 border-b border-[var(--color-border)]/10">
                  Par
                </td>
                {holes.map((h) => (
                  <td
                    key={h.hole.number}
                    className="py-2 px-1 text-center text-[var(--color-text-muted)] font-medium border-b border-[var(--color-border)]/10"
                  >
                    {h.hole.par}
                  </td>
                ))}
                <td className="py-2 px-2 text-center text-[var(--color-text-muted)] font-medium border-b border-[var(--color-border)]/10">
                  {holes.reduce((acc, h) => acc + h.hole.par, 0)}
                </td>
              </tr>
            </thead>
            <tbody>
              {round.playerScores.map((p, i) => {
                const total = totalRelativeToPar(p.scores);
                return (
                  <tr
                    key={p.playerName}
                    className={`border-b border-[var(--color-border)]/5 ${
                      i % 2 === 0 ? "bg-[var(--color-bg)]" : "bg-[var(--color-accent-light)]/30"
                    }`}
                  >
                    <td className={`py-3 px-2 font-bold text-[var(--color-text)] whitespace-nowrap sticky left-0 z-10 border-r border-[var(--color-border)]/5 ${
                      i % 2 === 0 ? "bg-[var(--color-bg)]" : "bg-[#f4f6e6]"
                    }`}>
                      {p.playerEmoji} {p.playerName}
                    </td>
                    {p.scores.map((s) => {
                      let cellClass = "";
                      if (s.strokes > 0) {
                        if (s.relativeToPar < 0) cellClass = "bg-[var(--color-birdie)] text-white";
                        else if (s.relativeToPar > 0) cellClass = "bg-[var(--color-bogey)]/10 text-[var(--color-bogey)]";
                        else cellClass = "text-[var(--color-text)]";
                      } else {
                         cellClass = "text-[var(--color-text-muted)]/30";
                      }
                      
                      return (
                        <td
                          key={s.hole.number}
                          className="p-1"
                        >
                          <div className={`w-7 h-7 mx-auto flex items-center justify-center rounded-md font-bold ${cellClass} ${s.strokes > 0 && s.relativeToPar < 0 ? "shadow-sm" : ""}`}>
                            {s.strokes === 0 ? "·" : s.strokes}
                          </div>
                        </td>
                      );
                    })}
                    <td
                      className={`py-3 px-2 text-center font-black text-sm ${scoreColorClass(
                        total,
                      )}`}
                    >
                      {formatRelativeToPar(total)}
                    </td>
                  </tr>
                );
              })}
            </tbody>
          </table>
        </div>
      </div>
    </div>
  );
}
