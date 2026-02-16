import { useEffect, useState } from "react";
import { api } from "../api/client";
import DashboardLayout from "../components/layout/DashboardLayout";
import type { LeaderboardPlayer } from "../types";

const MONTHS = [
  "YTD",
  "January",
  "February",
  "March",
  "April",
  "May",
  "June",
  "July",
  "August",
  "September",
  "October",
  "November",
  "December",
];

const CURRENT_YEAR = new Date().getFullYear();
const YEARS = Array.from({ length: CURRENT_YEAR - 2019 }, (_, i) => CURRENT_YEAR - i);

export default function LeadersPage() {
  const [year, setYear] = useState(CURRENT_YEAR);
  const [month, setMonth] = useState(0);
  const [players, setPlayers] = useState<LeaderboardPlayer[]>([]);
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const [retryCount, setRetryCount] = useState(0);

  useEffect(() => {
    let cancelled = false;
    setLoading(true);
    setError(null);

    api
      .getLeaderboard(month, year)
      .then((data) => {
        if (!cancelled) setPlayers(data);
      })
      .catch(() => {
        if (!cancelled) setError("Could not load leaderboard");
      })
      .finally(() => {
        if (!cancelled) setLoading(false);
      });

    return () => {
      cancelled = true;
    };
  }, [month, year, retryCount]);

  function formatAvg(score: number): string {
    const prefix = score < 0 ? "" : "+";
    return `${prefix}${score.toFixed(1)}`;
  }

  return (
    <DashboardLayout title="🏆 Leaders">
      <div className="flex gap-2 mb-4">
        <select
          value={year}
          onChange={(e) => setYear(Number(e.target.value))}
          className="flex-1 px-3 py-2 rounded-lg bg-[var(--color-surface)] text-[var(--color-text)] text-sm font-semibold border border-[var(--color-border)]/20"
        >
          {YEARS.map((y) => (
            <option key={y} value={y}>
              {y}
            </option>
          ))}
        </select>
        <select
          value={month}
          onChange={(e) => setMonth(Number(e.target.value))}
          className="flex-1 px-3 py-2 rounded-lg bg-[var(--color-surface)] text-[var(--color-text)] text-sm font-semibold border border-[var(--color-border)]/20"
        >
          {MONTHS.map((m, i) => (
            <option key={m} value={i}>
              {m}
            </option>
          ))}
        </select>
      </div>

      <p className="text-xs text-[var(--color-text-muted)] mb-4">
        Average score adjusted for course difficulty among friends.
      </p>

      {error && (
        <div className="text-center py-4">
          <p className="text-sm text-[var(--color-destructive)]">{error}</p>
          <button
            onClick={() => setRetryCount((c) => c + 1)}
            className="mt-2 text-sm font-semibold text-[var(--color-accent)]"
          >
            Retry
          </button>
        </div>
      )}

      {loading && (
        <div className="flex justify-center py-12">
          <span className="text-[var(--color-text-muted)]">Loading…</span>
        </div>
      )}

      {!loading && !error && players.length === 0 && (
        <p className="text-sm text-[var(--color-text-muted)] text-center py-8">
          No leaderboard data for this period.
        </p>
      )}

      {!loading && !error && players.length > 0 && (
        <div className="rounded-xl overflow-hidden bg-[var(--color-surface)] shadow-sm shadow-[var(--color-shadow)]">
          <table className="w-full text-sm">
            <thead>
              <tr className="border-b border-[var(--color-border)]/20">
                <th className="py-2 px-3 text-left font-bold text-[var(--color-text-muted)] text-xs">
                  #
                </th>
                <th className="py-2 px-3 text-left font-bold text-[var(--color-text-muted)] text-xs">
                  Player
                </th>
                <th className="py-2 px-3 text-right font-bold text-[var(--color-text-muted)] text-xs">
                  Avg
                </th>
                <th className="py-2 px-3 text-right font-bold text-[var(--color-text-muted)] text-xs">
                  Rounds
                </th>
              </tr>
            </thead>
            <tbody>
              {players.map((p, i) => (
                <tr
                  key={p.username}
                  className={
                    i < players.length - 1
                      ? "border-b border-[var(--color-border)]/10"
                      : ""
                  }
                >
                  <td className="py-2.5 px-3 font-bold text-[var(--color-text-muted)]">
                    {i + 1}
                  </td>
                  <td className="py-2.5 px-3 font-semibold text-[var(--color-text)]">
                    {p.username}
                  </td>
                  <td className="py-2.5 px-3 text-right font-bold text-[var(--color-text)]">
                    {formatAvg(p.courseAdjustedAverageScore)}
                  </td>
                  <td className="py-2.5 px-3 text-right text-[var(--color-text-muted)]">
                    {p.roundCount}
                  </td>
                </tr>
              ))}
            </tbody>
          </table>
        </div>
      )}
    </DashboardLayout>
  );
}
