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
    <div className="flex-1 flex flex-col bg-[var(--color-bg)] h-full">
      <header className="px-5 py-6 bg-gradient-to-r from-[var(--color-navbar)] to-[var(--color-gradient-end)] shadow-[0_1px_3px_var(--color-shadow)] z-10 text-center">
        <div className="w-16 h-16 rounded-full bg-[var(--color-accent)] text-white text-3xl flex items-center justify-center mx-auto mb-3 shadow-md">
          🏆
        </div>
        <h1 className="text-2xl font-bold text-[var(--color-text)] mb-1">
          Round Complete
        </h1>
        <p className="text-sm font-medium text-[var(--color-text-muted)]">
          {round.courseName}
          {round.courseLayout ? ` — ${round.courseLayout}` : ""}
        </p>
      </header>

      <div className="flex-1 overflow-y-auto px-4 py-6 space-y-8 pb-8">
        <section>
          <h2 className="text-sm font-bold text-[var(--color-text-muted)] mb-3 uppercase tracking-wider flex items-center gap-2">
            Leaderboard
            <div className="h-px flex-1 bg-[var(--color-border)] opacity-20" />
          </h2>
          <div className="space-y-3">
            {sorted.map((p, i) => {
              const total = totalRelativeToPar(p.scores);
              const ratingChange = round.ratingChanges?.find(
                (r) => r.username === p.playerName,
              );
              const isWinner = i === 0;
              
              return (
                <div
                  key={p.playerName}
                  className={`flex items-center justify-between px-4 py-3 rounded-xl transition-all ${
                    isWinner
                      ? "bg-gradient-to-r from-[var(--color-accent-light)] to-transparent border border-[var(--color-accent)]/30 shadow-md scale-[1.02]"
                      : "bg-[var(--color-surface)] shadow-sm"
                  }`}
                >
                  <div className="flex items-center gap-3">
                    <span className={`text-base font-bold w-6 ${isWinner ? "text-[var(--color-accent)]" : "text-[var(--color-text-muted)]"}`}>
                      {i + 1}.
                    </span>
                    <span className="text-base font-bold text-[var(--color-text)]">
                      {p.playerEmoji} {p.playerName}
                    </span>
                  </div>
                  <div className="flex items-center gap-4">
                    {ratingChange && (
                      <span
                        className={`text-xs font-bold px-2 py-1 rounded-md bg-[var(--color-bg)]/50 ${
                          ratingChange.change >= 0
                            ? "text-[var(--color-birdie)]"
                            : "text-[var(--color-bogey)]"
                        }`}
                      >
                        {ratingChange.change >= 0 ? "▲" : "▼"} {Math.abs(ratingChange.change).toFixed(0)}
                      </span>
                    )}
                    <span
                      className={`text-xl font-black ${scoreColorClass(total)}`}
                    >
                      {formatRelativeToPar(total)}
                    </span>
                  </div>
                </div>
              );
            })}
          </div>
        </section>

        {round.achievements && round.achievements.length > 0 && (
          <section>
            <h2 className="text-sm font-bold text-[var(--color-text-muted)] mb-3 uppercase tracking-wider flex items-center gap-2">
              Achievements
              <div className="h-px flex-1 bg-[var(--color-border)] opacity-20" />
            </h2>
            <div className="grid grid-cols-1 gap-2">
              {round.achievements.map((a, i) => (
                <div
                  key={i}
                  className="px-4 py-3 rounded-xl bg-[var(--color-surface)] border-l-4 border-l-[var(--color-accent)] shadow-sm flex items-center justify-between"
                >
                  <span className="font-bold text-[var(--color-text)] text-sm">
                    {a.achievementName}
                  </span>
                  <span className="text-xs font-medium text-[var(--color-text-muted)] px-2 py-1 bg-[var(--color-bg)] rounded-md">
                    {a.username}
                  </span>
                </div>
              ))}
            </div>
          </section>
        )}

        {stats.length > 0 && (
          <section>
             <h2 className="text-sm font-bold text-[var(--color-text-muted)] mb-3 uppercase tracking-wider flex items-center gap-2">
              Round Stats
              <div className="h-px flex-1 bg-[var(--color-border)] opacity-20" />
            </h2>
            <div className="space-y-4">
              {stats.map((s) => (
                <div
                  key={s.username}
                  className="p-4 rounded-xl bg-gradient-to-br from-[var(--color-surface)] to-[var(--color-bg)] shadow-sm border border-[var(--color-border)]/10"
                >
                  <p className="text-sm font-bold text-[var(--color-text)] mb-3 border-b border-[var(--color-border)]/10 pb-2">
                    {s.username}
                  </p>
                  <div className="grid grid-cols-3 gap-y-4 gap-x-2">
                    <StatCell label="Fairway" value={`${(s.fairwayHitRate * 100).toFixed(0)}%`} color="text-[var(--color-fairway)]" />
                    <StatCell label="C1 in Reg" value={`${(s.circle1Rate * 100).toFixed(0)}%`} color="text-[var(--color-circle1)]" />
                    <StatCell label="C2 in Reg" value={`${(s.circle2Rate * 100).toFixed(0)}%`} color="text-[var(--color-circle2)]" />
                    <StatCell label="Scramble" value={`${(s.scrambleRate * 100).toFixed(0)}%`} />
                    <StatCell label="Birdie Rate" value={`${(s.birdieRate * 100).toFixed(0)}%`} color="text-[var(--color-birdie)]" />
                    <StatCell label="OB Rate" value={`${(s.obRate * 100).toFixed(0)}%`} color="text-[var(--color-ob)]" />
                  </div>
                </div>
              ))}
            </div>
          </section>
        )}
      </div>

      <div className="p-4 bg-[var(--color-navbar)]/50 backdrop-blur-sm border-t border-[var(--color-border)]/10">
        <button
          onClick={onBackToHome}
          className="w-full py-4 rounded-xl bg-[var(--color-accent)] text-white text-lg font-bold shadow-lg shadow-[var(--color-shadow)] active:scale-[0.98] transition-transform"
        >
          Back to Home
        </button>
      </div>
    </div>
  );
}

function StatCell({ label, value, color }: { label: string; value: string; color?: string }) {
  return (
    <div className="text-center flex flex-col items-center">
      <span className={`text-lg font-black ${color || "text-[var(--color-text)]"}`}>{value}</span>
      <span className="text-[10px] uppercase font-bold text-[var(--color-text-muted)] tracking-wide">{label}</span>
    </div>
  );
}
