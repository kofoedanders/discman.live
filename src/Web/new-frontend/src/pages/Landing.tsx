import { useEffect } from "react";
import { Link, useNavigate } from "react-router-dom";
import { useAuthStore } from "../stores/authStore";
import { useRoundStore } from "../stores/roundStore";
import { connectHub, disconnectHub } from "../signalr/hub";
import { formatRelativeToPar, scoreColorClass } from "../utils/scoring";

function LoggedOutLanding() {
  return (
    <div className="flex-1 flex flex-col items-center justify-center px-6 bg-[var(--color-bg)]">
      <div className="text-6xl mb-4">🥏</div>
      <h1 className="text-3xl font-bold text-[var(--color-text)] mb-2">
        Discman
      </h1>
      <p className="text-[var(--color-text-muted)] text-base mb-10 text-center">
        Live disc golf scoring
      </p>

      <div className="w-full max-w-sm space-y-3">
        <Link
          to="/register"
          className="block w-full py-3 rounded-lg bg-[var(--color-accent)] text-white text-center text-base font-bold border-2 border-[var(--color-button-border)] active:opacity-80"
        >
          Sign Up
        </Link>
        <Link
          to="/login"
          className="block w-full py-3 rounded-lg bg-[var(--color-button-bg)] text-[var(--color-text)] text-center text-base font-bold border-2 border-[var(--color-button-border)] active:opacity-80"
        >
          Log In
        </Link>
      </div>
    </div>
  );
}

function ActiveRoundCard() {
  const round = useRoundStore((s) => s.round);
  const navigate = useNavigate();

  if (!round || round.isCompleted) return null;

  const totalHoles = round.playerScores[0]?.scores.length ?? 0;
  const completedHoles = round.playerScores[0]?.scores.filter(
    (s) => s.strokes > 0,
  ).length ?? 0;

  return (
    <button
      onClick={() => navigate(`/rounds/${round.id}`)}
      className="w-full p-4 rounded-xl bg-[var(--color-surface)] border-2 border-[var(--color-border)] text-left active:opacity-80"
    >
      <div className="flex items-center justify-between mb-1">
        <span className="font-bold text-lg text-[var(--color-text)]">
          Active Round
        </span>
        <span className="text-sm font-semibold text-[var(--color-accent)]">
          Continue →
        </span>
      </div>
      <p className="text-sm text-[var(--color-text-muted)] font-medium">
        {round.courseName}
        {round.courseLayout ? ` — ${round.courseLayout}` : ""}
      </p>
      <p className="text-sm text-[var(--color-text-muted)]">
        Hole {completedHoles}/{totalHoles} · {round.playerScores.length} players
      </p>
    </button>
  );
}

function RecentRoundsList() {
  const recentRounds = useRoundStore((s) => s.recentRounds);
  const navigate = useNavigate();

  if (recentRounds.length === 0) {
    return (
      <p className="text-sm text-[var(--color-text-muted)] text-center py-4">
        No recent rounds
      </p>
    );
  }

  return (
    <div className="space-y-2">
      {recentRounds.map((r) => {
        const myScore = r.playerScores[0];
        const total = myScore
          ? myScore.scores.reduce((acc, s) => {
              if (s.strokes === 0) return acc;
              return acc + s.relativeToPar;
            }, 0)
          : 0;

        return (
          <button
            key={r.id}
            onClick={() => navigate(`/rounds/${r.id}`)}
            className="w-full flex items-center justify-between p-3 rounded-lg bg-[var(--color-surface)] border border-[var(--color-border)] active:opacity-80"
          >
            <div className="text-left">
              <p className="text-sm font-bold text-[var(--color-text)]">
                {r.courseName}
              </p>
              <p className="text-xs text-[var(--color-text-muted)]">
                {new Date(r.startTime).toLocaleDateString()} ·{" "}
                {r.playerScores.length} players
              </p>
            </div>
            <span
              className={`text-lg font-bold ${scoreColorClass(total)}`}
            >
              {formatRelativeToPar(total)}
            </span>
          </button>
        );
      })}
    </div>
  );
}

function LoggedInLanding() {
  const user = useAuthStore((s) => s.user);
  const userDetails = useAuthStore((s) => s.userDetails);
  const logout = useAuthStore((s) => s.logout);
  const fetchActiveRound = useRoundStore((s) => s.fetchActiveRound);
  const fetchRecentRounds = useRoundStore((s) => s.fetchRecentRounds);
  const fetchUserDetails = useAuthStore((s) => s.fetchUserDetails);
  const navigate = useNavigate();

  useEffect(() => {
    connectHub();
    fetchUserDetails();
    if (user?.username) {
      fetchRecentRounds(user.username, 5);
    }
    return () => {
      disconnectHub();
    };
  }, [user?.username, fetchRecentRounds, fetchUserDetails]);

  useEffect(() => {
    if (userDetails?.activeRound) {
      fetchActiveRound(userDetails.activeRound);
    }
  }, [userDetails?.activeRound, fetchActiveRound]);

  return (
    <div className="flex-1 flex flex-col bg-[var(--color-bg)]">
      <header className="flex items-center justify-between px-4 py-3 bg-[var(--color-navbar)] border-b-2 border-[var(--color-border)]">
        <span className="text-lg font-bold text-[var(--color-text)]">
          🥏 Discman
        </span>
        <button
          onClick={logout}
          className="text-sm font-semibold text-[var(--color-text-muted)] active:opacity-60"
        >
          Log Out
        </button>
      </header>

      <div className="flex-1 px-4 py-4 space-y-6 overflow-y-auto pb-20">
        <ActiveRoundCard />

        <section>
          <h2 className="text-base font-bold text-[var(--color-text)] mb-3">
            Recent Rounds
          </h2>
          <RecentRoundsList />
        </section>
      </div>

      <button
        onClick={() => navigate("/new-round")}
        className="fixed bottom-6 right-6 w-14 h-14 rounded-full bg-[var(--color-accent)] text-white text-2xl font-bold shadow-lg border-2 border-[var(--color-button-border)] active:opacity-80 flex items-center justify-center"
      >
        +
      </button>
    </div>
  );
}

export default function Landing() {
  const isLoggedIn = useAuthStore((s) => s.isLoggedIn);
  return isLoggedIn ? <LoggedInLanding /> : <LoggedOutLanding />;
}
