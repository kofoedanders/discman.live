import type { Round, UserStats } from "../../types";
import {
  formatRelativeToPar,
  scoreColorClass,
  totalRelativeToPar,
} from "../../utils/scoring";

interface RoundSummaryProps {
  round: Round;
  stats: UserStats[];
  onBackToHome: () => void;
}

export default function RoundSummary({
  round,
  stats,
  onBackToHome,
}: RoundSummaryProps) {
  const sorted = [...round.playerScores].sort((a, b) => {
    const aTotal = totalRelativeToPar(a.scores);
    const bTotal = totalRelativeToPar(b.scores);
    return aTotal - bTotal;
  });

  return (
    <div className="flex-1 flex flex-col bg-[var(--color-bg)]">
      <header className="px-4 py-3 bg-[var(--color-navbar)] border-b-2 border-[var(--color-border)]">
        <p className="text-lg font-bold text-[var(--color-text)]">
          Round Complete
        </p>
        <p className="text-sm text-[var(--color-text-muted)]">
          {round.courseName}
          {round.courseLayout ? ` — ${round.courseLayout}` : ""}
        </p>
      </header>

      <div className="flex-1 overflow-y-auto px-4 py-4 space-y-4">
        <section>
          <h2 className="text-sm font-bold text-[var(--color-text)] mb-2 uppercase tracking-wider">
            Leaderboard
          </h2>
          <div className="space-y-1">
            {sorted.map((p, i) => {
              const total = totalRelativeToPar(p.scores);
              const ratingChange = round.ratingChanges?.find(
                (r) => r.username === p.playerName,
              );
              return (
                <div
                  key={p.playerName}
                  className="flex items-center justify-between px-3 py-2 rounded-lg bg-[var(--color-surface)] border border-[var(--color-border)]"
                >
                  <div className="flex items-center gap-2">
                    <span className="text-sm font-bold text-[var(--color-text-muted)] w-5">
                      {i + 1}.
                    </span>
                    <span className="text-sm font-bold text-[var(--color-text)]">
                      {p.playerEmoji} {p.playerName}
                    </span>
                  </div>
                  <div className="flex items-center gap-3">
                    <span
                      className={`text-base font-bold ${scoreColorClass(total)}`}
                    >
                      {formatRelativeToPar(total)}
                    </span>
                    {ratingChange && (
                      <span
                        className={`text-xs font-semibold ${
                          ratingChange.change >= 0
                            ? "text-[var(--color-birdie)]"
                            : "text-[var(--color-bogey)]"
                        }`}
                      >
                        {ratingChange.change >= 0 ? "+" : ""}
                        {ratingChange.change.toFixed(0)} elo
                      </span>
                    )}
                  </div>
                </div>
              );
            })}
          </div>
        </section>

        {round.achievements && round.achievements.length > 0 && (
          <section>
            <h2 className="text-sm font-bold text-[var(--color-text)] mb-2 uppercase tracking-wider">
              Achievements
            </h2>
            <div className="space-y-1">
              {round.achievements.map((a, i) => (
                <div
                  key={i}
                  className="px-3 py-2 rounded-lg bg-[var(--color-surface)] border border-[var(--color-border)] text-sm"
                >
                  <span className="font-bold text-[var(--color-text)]">
                    {a.username}
                  </span>{" "}
                  <span className="text-[var(--color-text-muted)]">
                    {a.achievementName}
                  </span>
                </div>
              ))}
            </div>
          </section>
        )}

        {stats.length > 0 && (
          <section>
            <h2 className="text-sm font-bold text-[var(--color-text)] mb-2 uppercase tracking-wider">
              Stats
            </h2>
            {stats.map((s) => (
              <div
                key={s.username}
                className="p-3 rounded-lg bg-[var(--color-surface)] border border-[var(--color-border)] mb-2"
              >
                <p className="text-sm font-bold text-[var(--color-text)] mb-1">
                  {s.username}
                </p>
                <div className="grid grid-cols-3 gap-y-1 gap-x-3 text-xs">
                  <StatCell label="FW" value={`${(s.fairwayHitRate * 100).toFixed(0)}%`} />
                  <StatCell label="C1" value={`${(s.circle1Rate * 100).toFixed(0)}%`} />
                  <StatCell label="C2" value={`${(s.circle2Rate * 100).toFixed(0)}%`} />
                  <StatCell label="OB" value={`${(s.obRate * 100).toFixed(0)}%`} />
                  <StatCell label="🐦" value={`${(s.birdieRate * 100).toFixed(0)}%`} />
                  <StatCell label="Scramble" value={`${(s.scrambleRate * 100).toFixed(0)}%`} />
                </div>
              </div>
            ))}
          </section>
        )}
      </div>

      <div className="px-4 py-3 border-t-2 border-[var(--color-border)] bg-[var(--color-navbar)]">
        <button
          onClick={onBackToHome}
          className="w-full py-3 rounded-lg bg-[var(--color-accent)] text-white text-base font-bold border-2 border-[var(--color-button-border)] active:opacity-80"
        >
          Back to Home
        </button>
      </div>
    </div>
  );
}

function StatCell({ label, value }: { label: string; value: string }) {
  return (
    <div className="text-center">
      <p className="text-[var(--color-text-muted)]">{label}</p>
      <p className="font-bold text-[var(--color-text)]">{value}</p>
    </div>
  );
}
