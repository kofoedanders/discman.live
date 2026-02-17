import { useEffect } from "react";
import { Link } from "react-router-dom";
import { useTournamentStore } from "../stores/tournamentStore";
import DashboardLayout from "../components/layout/DashboardLayout";

function formatDate(iso: string): string {
  const d = new Date(iso);
  const day = String(d.getDate()).padStart(2, "0");
  const month = d.toLocaleString("en", { month: "short" }).toLowerCase();
  return `${day}. ${month}`;
}

export default function TournamentsPage() {
  const { tournaments, isLoading, error, fetchTournaments } = useTournamentStore();

  useEffect(() => {
    fetchTournaments(false);
  }, [fetchTournaments]);

  if (isLoading && tournaments.length === 0) {
    return (
      <DashboardLayout title="Tournaments">
        <div className="flex justify-center p-8">
          <div className="animate-spin rounded-full h-8 w-8 border-b-2 border-[var(--color-accent)]"></div>
        </div>
      </DashboardLayout>
    );
  }

  if (error) {
    return (
      <DashboardLayout title="Tournaments">
        <div className="p-4 text-red-500 bg-red-100 rounded-lg">{error}</div>
      </DashboardLayout>
    );
  }

  return (
    <DashboardLayout title="Tournaments">
      <div className="space-y-6" data-testid="tournaments-page">
        <div className="flex items-center justify-between">
          <h1 className="text-2xl font-bold text-[var(--color-text)]">Tournaments</h1>
          <Link
            to="/tournaments/new"
            className="px-6 py-2 rounded-lg bg-[var(--color-accent)] text-white text-sm font-bold active:scale-95 transition-transform hover:opacity-90"
          >
            New Tournament
          </Link>
        </div>

        <div className="grid gap-4 sm:grid-cols-2 lg:grid-cols-3">
          {tournaments.map((t) => (
            <Link
              key={t.id}
              to={`/tournaments/${t.id}`}
              className="p-3 rounded-xl bg-[var(--color-surface)] shadow-sm shadow-[var(--color-shadow)] block hover:bg-[var(--color-bg)] transition-colors group"
            >
              <h2 className="text-lg font-bold text-[var(--color-text)] mb-1 group-hover:text-[var(--color-accent)] transition-colors">{t.name}</h2>
              <div className="text-sm text-[var(--color-text-muted)]">
                {formatDate(t.start)} - {formatDate(t.end)}
              </div>
            </Link>
          ))}
          
          {tournaments.length === 0 && (
            <div className="col-span-full text-center py-8 text-[var(--color-text-muted)]">
              No tournaments found.
            </div>
          )}
        </div>
      </div>
    </DashboardLayout>
  );
}
