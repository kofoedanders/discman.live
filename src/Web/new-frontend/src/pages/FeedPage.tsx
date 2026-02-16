import { useCallback, useEffect, useRef, useState } from "react";
import { useNavigate } from "react-router-dom";
import { api } from "../api/client";
import { useAuthStore } from "../stores/authStore";
import DashboardLayout from "../components/layout/DashboardLayout";
import type { FeedItem } from "../types";

function formatDate(iso: string): string {
  const d = new Date(iso);
  const day = String(d.getDate()).padStart(2, "0");
  const month = d.toLocaleString("en", { month: "long" }).toLowerCase();
  const hours = String(d.getHours()).padStart(2, "0");
  const minutes = String(d.getMinutes()).padStart(2, "0");
  return `${day}. ${month} ${hours}:${minutes}`;
}

function feedIcon(item: FeedItem): string {
  switch (item.itemType) {
    case "Round":
      return item.action === "Started" ? "🔄" : "✅";
    case "Hole":
      return "🐦";
    case "Achievement":
      return "⭐";
    case "Tournament":
      return "🏆";
    case "Friend":
      return "🤝";
    case "User":
      return "👤";
  }
}

function scoreName(relativeScore: number): string {
  switch (relativeScore) {
    case -1:
      return "Birdie";
    case -2:
      return "Eagle";
    case -3:
      return "Albatross";
    default:
      return "";
  }
}

function feedText(item: FeedItem): string {
  switch (item.itemType) {
    case "User":
      return `${item.subjects[0]} started using discman.live — add your friends and start playing!`;
    case "Round":
      return `${item.subjects.join(", ")} ${item.action.toLowerCase()} a round at ${item.courseName}`;
    case "Hole":
      return `${scoreName(item.holeScore)} on ${item.courseName} hole ${item.holeNumber}`;
    case "Achievement":
      return `Earned achievement ${item.achievementName}!`;
    case "Tournament":
      return item.action === "Joined"
        ? `Signed up for tournament ${item.tournamentName}!`
        : `${item.tournamentName}`;
    case "Friend":
      return `Added ${item.friendName} as friend`;
  }
}

function formatRelScore(score: number): string {
  if (score < 0) return String(score);
  return `+${score}`;
}

function MiniScorecard({ item }: { item: FeedItem }) {
  if (item.itemType !== "Round" || item.action !== "Completed" || !item.roundScores) {
    return null;
  }

  return (
    <div className="mt-2 overflow-x-auto">
      <table className="text-xs border-collapse">
        <tbody>
          <tr>
            {item.subjects.map((s) => (
              <th
                key={s}
                className="px-2 py-0.5 font-semibold text-[var(--color-text)] border border-[var(--color-border)]/20 bg-[var(--color-surface)]"
              >
                {s}
              </th>
            ))}
          </tr>
          <tr>
            {item.roundScores.map((s, i) => (
              <td
                key={i}
                className="px-2 py-0.5 text-center border border-[var(--color-border)]/20"
              >
                {formatRelScore(s)}
              </td>
            ))}
          </tr>
        </tbody>
      </table>
    </div>
  );
}

function FeedCard({
  item,
  username,
  onLike,
  onNavigate,
}: {
  item: FeedItem;
  username: string;
  onLike: (id: string) => void;
  onNavigate: (item: FeedItem) => void;
}) {
  const liked = item.likes.some((l) => l === username);
  const subject =
    item.subjects.length === 1 ? item.subjects[0] : "Friends";

  return (
    <div className="p-3 rounded-xl bg-[var(--color-surface)] shadow-sm shadow-[var(--color-shadow)]">
      <div className="flex gap-3">
        <button
          onClick={() => onNavigate(item)}
          className="shrink-0 w-10 h-10 rounded-full bg-[var(--color-accent-light)] flex items-center justify-center text-lg"
        >
          {feedIcon(item)}
        </button>

        <div className="flex-1 min-w-0">
          <div className="flex items-baseline gap-2 flex-wrap">
            <span className="font-bold text-sm text-[var(--color-text)]">
              {subject}
            </span>
            <span className="text-xs text-[var(--color-text-muted)]">
              {formatDate(item.registeredAt)}
            </span>
          </div>

          <button
            onClick={() => onNavigate(item)}
            className="text-left mt-0.5"
          >
            <p className="text-sm text-[var(--color-text-muted)] leading-snug">
              {feedText(item)}
            </p>
          </button>

          <MiniScorecard item={item} />
        </div>

        <button
          onClick={() => onLike(item.id)}
          className="shrink-0 flex flex-col items-center justify-start pt-1 gap-0.5"
        >
          <span
            className={`text-base ${liked ? "text-[var(--color-accent)]" : "text-[var(--color-text-muted)]"}`}
          >
            👍
          </span>
          {item.likes.length > 0 && (
            <span className="text-[10px] font-bold text-[var(--color-text-muted)]">
              {item.likes.length}
            </span>
          )}
        </button>
      </div>
    </div>
  );
}

export default function FeedPage() {
  const username = useAuthStore((s) => s.user?.username ?? "");
  const navigate = useNavigate();

  const [items, setItems] = useState<FeedItem[]>([]);
  const [page, setPage] = useState(1);
  const [isLastPage, setIsLastPage] = useState(false);
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const loadedPages = useRef(new Set<number>());

  const fetchPage = useCallback(async (pageNum: number) => {
    if (loadedPages.current.has(pageNum)) return;
    loadedPages.current.add(pageNum);

    setLoading(true);
    setError(null);
    try {
      const feed = await api.getFeed("", pageNum, 10);
      setItems((prev) =>
        pageNum === 1 ? feed.feedItems : [...prev, ...feed.feedItems],
      );
      setIsLastPage(feed.isLastPage);
    } catch {
      setError("Could not load feed");
      loadedPages.current.delete(pageNum);
    } finally {
      setLoading(false);
    }
  }, []);

  useEffect(() => {
    fetchPage(page);
  }, [page, fetchPage]);

  const handleLike = async (id: string) => {
    setItems((prev) =>
      prev.map((item) => {
        if (item.id !== id) return item;
        const alreadyLiked = item.likes.includes(username);
        return {
          ...item,
          likes: alreadyLiked
            ? item.likes.filter((l) => l !== username)
            : [...item.likes, username],
        };
      }),
    );

    try {
      await api.toggleFeedLike(id);
    } catch {
      setItems((prev) =>
        prev.map((item) => {
          if (item.id !== id) return item;
          const alreadyLiked = item.likes.includes(username);
          return {
            ...item,
            likes: alreadyLiked
              ? item.likes.filter((l) => l !== username)
              : [...item.likes, username],
          };
        }),
      );
    }
  };

  const handleNavigate = (item: FeedItem) => {
    switch (item.itemType) {
      case "Tournament":
      case "Friend":
      case "User":
        break;
      default:
        if (item.roundId) navigate(`/rounds/${item.roundId}`);
    }
  };

  return (
    <DashboardLayout title="📰 Feed">
      {error && (
        <div className="text-center py-4">
          <p className="text-sm text-[var(--color-destructive)]">{error}</p>
          <button
            onClick={() => {
              loadedPages.current.clear();
              setPage(1);
              fetchPage(1);
            }}
            className="mt-2 text-sm font-semibold text-[var(--color-accent)]"
          >
            Retry
          </button>
        </div>
      )}

      {!error && items.length === 0 && !loading && (
        <p className="text-sm text-[var(--color-text-muted)] text-center py-8">
          No activity yet. Start a round or add friends!
        </p>
      )}

      <div className="space-y-3">
        {items.map((item) => (
          <FeedCard
            key={item.id}
            item={item}
            username={username}
            onLike={handleLike}
            onNavigate={handleNavigate}
          />
        ))}
      </div>

      {!isLastPage && items.length > 0 && (
        <div className="flex justify-center py-6">
          <button
            onClick={() => setPage((p) => p + 1)}
            disabled={loading}
            className="px-6 py-2 rounded-lg bg-[var(--color-accent)] text-white text-sm font-bold active:scale-95 transition-transform disabled:opacity-50"
          >
            {loading ? "Loading…" : "Load More"}
          </button>
        </div>
      )}

      {loading && items.length === 0 && (
        <div className="flex justify-center py-12">
          <span className="text-[var(--color-text-muted)]">Loading…</span>
        </div>
      )}
    </DashboardLayout>
  );
}
