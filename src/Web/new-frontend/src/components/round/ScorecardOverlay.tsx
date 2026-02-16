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
    <div className="fixed inset-0 z-40 flex flex-col bg-[var(--color-bg)]">
      <header className="flex items-center justify-between px-4 py-3 bg-[var(--color-navbar)] border-b-2 border-[var(--color-border)]">
        <span className="text-base font-bold text-[var(--color-text)]">
          Scorecard
        </span>
        <button
          onClick={onClose}
          className="text-sm font-bold text-[var(--color-accent)] active:opacity-60"
        >
          Close
        </button>
      </header>

      <div className="flex-1 overflow-auto p-2">
        <table className="w-full text-xs border-collapse">
          <thead>
            <tr className="border-b-2 border-[var(--color-border)]">
              <th className="text-left py-1 px-1 font-bold text-[var(--color-text)]">
                Hole
              </th>
              {holes.map((h) => (
                <th
                  key={h.hole.number}
                  className="py-1 px-1 font-bold text-[var(--color-text)] text-center"
                >
                  {h.hole.number}
                </th>
              ))}
              <th className="py-1 px-1 font-bold text-[var(--color-text)] text-center">
                Tot
              </th>
            </tr>
            <tr className="border-b border-[var(--color-border)]">
              <td className="py-1 px-1 text-[var(--color-text-muted)]">Par</td>
              {holes.map((h) => (
                <td
                  key={h.hole.number}
                  className="py-1 px-1 text-center text-[var(--color-text-muted)]"
                >
                  {h.hole.par}
                </td>
              ))}
              <td className="py-1 px-1 text-center text-[var(--color-text-muted)]">
                {holes.reduce((acc, h) => acc + h.hole.par, 0)}
              </td>
            </tr>
          </thead>
          <tbody>
            {round.playerScores.map((p) => {
              const total = totalRelativeToPar(p.scores);
              return (
                <tr
                  key={p.playerName}
                  className="border-b border-[var(--color-border)]"
                >
                  <td className="py-1.5 px-1 font-bold text-[var(--color-text)] whitespace-nowrap">
                    {p.playerName}
                  </td>
                  {p.scores.map((s) => (
                    <td
                      key={s.hole.number}
                      className={`py-1.5 px-1 text-center font-bold ${
                        s.strokes === 0
                          ? "text-[var(--color-text-muted)]"
                          : scoreColorClass(s.relativeToPar)
                      }`}
                    >
                      {s.strokes === 0 ? "-" : s.strokes}
                    </td>
                  ))}
                  <td
                    className={`py-1.5 px-1 text-center font-bold ${scoreColorClass(
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
  );
}
