import { useState, useEffect, useCallback, useRef } from "react";
import { useNavigate } from "react-router-dom";
import { useAuthStore } from "../stores/authStore";
import { api } from "../api/client";
import type { CourseVm, ScoreMode } from "../types";

type Step = "course" | "players" | "confirm";

function useDebouncedValue(value: string, delay: number) {
  const [debounced, setDebounced] = useState(value);
  useEffect(() => {
    const id = setTimeout(() => setDebounced(value), delay);
    return () => clearTimeout(id);
  }, [value, delay]);
  return debounced;
}

function groupCoursesByName(courses: CourseVm[]): Map<string, CourseVm[]> {
  const map = new Map<string, CourseVm[]>();
  for (const c of courses) {
    const existing = map.get(c.name);
    if (existing) {
      existing.push(c);
    } else {
      map.set(c.name, [c]);
    }
  }
  return map;
}

function StepIndicator({ step }: { step: Step }) {
  const steps: Step[] = ["course", "players", "confirm"];
  const labels = ["Course", "Players", "Confirm"];
  const currentIdx = steps.indexOf(step);

  return (
    <div className="flex items-center justify-center gap-2 py-3">
      {steps.map((s, i) => (
        <div key={s} className="flex items-center gap-2">
          <div
            className={`w-7 h-7 rounded-full flex items-center justify-center text-xs font-bold border-2 ${
              i <= currentIdx
                ? "bg-[var(--color-accent)] text-white border-[var(--color-accent)]"
                : "bg-[var(--color-bg)] text-[var(--color-text-muted)] border-[var(--color-border)]"
            }`}
          >
            {i + 1}
          </div>
          <span
            className={`text-xs font-semibold ${
              i <= currentIdx ? "text-[var(--color-text)]" : "text-[var(--color-text-muted)]"
            }`}
          >
            {labels[i]}
          </span>
          {i < steps.length - 1 && (
            <div
              className={`w-6 h-0.5 ${
                i < currentIdx ? "bg-[var(--color-accent)]" : "bg-[var(--color-border)]"
              }`}
            />
          )}
        </div>
      ))}
    </div>
  );
}

function CourseStep({
  onSelect,
  selected,
}: {
  onSelect: (course: CourseVm) => void;
  selected: CourseVm | null;
}) {
  const [filter, setFilter] = useState("");
  const [courses, setCourses] = useState<CourseVm[]>([]);
  const [loading, setLoading] = useState(false);
  const [selectedName, setSelectedName] = useState<string | null>(
    selected?.name ?? null,
  );
  const debouncedFilter = useDebouncedValue(filter, 300);
  const inputRef = useRef<HTMLInputElement>(null);

  useEffect(() => {
    let cancelled = false;
    setLoading(true);

    const fetchCourses = async () => {
      try {
        let lat = 0;
        let lng = 0;
        if (!debouncedFilter && navigator.geolocation) {
          const pos = await new Promise<GeolocationPosition>(
            (resolve, reject) =>
              navigator.geolocation.getCurrentPosition(resolve, reject, {
                timeout: 5000,
              }),
          ).catch(() => null);
          if (pos) {
            lat = pos.coords.latitude;
            lng = pos.coords.longitude;
          }
        }
        const result = await api.getCourses(debouncedFilter, lat, lng);
        if (!cancelled) {
          setCourses(result);
          setLoading(false);
        }
      } catch {
        if (!cancelled) setLoading(false);
      }
    };

    void fetchCourses();
    return () => {
      cancelled = true;
    };
  }, [debouncedFilter]);

  useEffect(() => {
    inputRef.current?.focus();
  }, []);

  const grouped = groupCoursesByName(courses);
  const courseNames = [...grouped.keys()].slice(0, 8);
  const layouts = selectedName ? (grouped.get(selectedName) ?? []) : [];

  return (
    <div className="flex-1 flex flex-col gap-3">
      {selectedName ? (
        <>
          <button
            onClick={() => setSelectedName(null)}
            className="flex items-center gap-2 text-sm font-semibold text-[var(--color-accent)] active:opacity-60"
          >
            ← Back to search
          </button>
          <div className="px-1">
            <h3 className="text-base font-bold text-[var(--color-text)] mb-2">
              {selectedName}
            </h3>
            <div className="space-y-2">
              {layouts.map((layout) => (
                <button
                  key={layout.id}
                  onClick={() => onSelect(layout)}
                  className={`w-full text-left p-3 rounded-lg border-2 active:opacity-80 ${
                    selected?.id === layout.id
                      ? "border-[var(--color-accent)] bg-[var(--color-surface)]"
                      : "border-[var(--color-border)] bg-[var(--color-bg)]"
                  }`}
                >
                  <div className="flex items-center justify-between mb-1">
                    <span className="text-sm font-bold text-[var(--color-text)]">
                      {layout.layout || "Default"}
                    </span>
                    <span className="text-xs text-[var(--color-text-muted)]">
                      {layout.holes.length} holes
                    </span>
                  </div>
                  <div className="flex gap-1 flex-wrap">
                    {layout.holes.map((h) => (
                      <span
                        key={h.number}
                        className="text-[10px] w-5 h-5 flex items-center justify-center rounded bg-[var(--color-navbar)] text-[var(--color-text-muted)] font-medium"
                      >
                        {h.par}
                      </span>
                    ))}
                  </div>
                  {layout.courseStats?.roundsOnCourse ? (
                    <p className="text-xs text-[var(--color-text-muted)] mt-1">
                      {layout.courseStats.roundsOnCourse} rounds played
                      {layout.courseStats.previousRound &&
                        ` · Last ${new Date(layout.courseStats.previousRound).toLocaleDateString()}`}
                    </p>
                  ) : null}
                </button>
              ))}
            </div>
          </div>
        </>
      ) : (
        <>
          <input
            ref={inputRef}
            type="text"
            placeholder="Search courses..."
            value={filter}
            onChange={(e) => setFilter(e.target.value)}
            className="w-full px-3 py-2.5 rounded-lg border-2 border-[var(--color-border)] bg-[var(--color-bg)] text-sm text-[var(--color-text)] placeholder:text-[var(--color-text-muted)] focus:outline-none focus:border-[var(--color-accent)]"
          />
          {loading ? (
            <p className="text-sm text-[var(--color-text-muted)] text-center py-4">
              Loading courses...
            </p>
          ) : courseNames.length === 0 ? (
            <p className="text-sm text-[var(--color-text-muted)] text-center py-4">
              {filter ? "No courses found" : "Type to search or allow location access"}
            </p>
          ) : (
            <div className="space-y-1">
              {courseNames.map((name) => {
                const layouts = grouped.get(name)!;
                const first = layouts[0];
                return (
                  <button
                    key={name}
                    onClick={() => {
                      setSelectedName(name);
                      if (layouts.length === 1) {
                        onSelect(layouts[0]);
                      }
                    }}
                    className="w-full flex items-center justify-between p-3 rounded-lg bg-[var(--color-surface)] border border-[var(--color-border)] active:opacity-80"
                  >
                    <div className="text-left">
                      <p className="text-sm font-bold text-[var(--color-text)]">{name}</p>
                      <p className="text-xs text-[var(--color-text-muted)]">
                        {layouts.length} layout{layouts.length !== 1 ? "s" : ""}
                        {first.distance > 0 &&
                          ` · ${(first.distance / 1000).toFixed(1)} km`}
                      </p>
                    </div>
                    <span className="text-[var(--color-text-muted)] text-lg">›</span>
                  </button>
                );
              })}
            </div>
          )}
        </>
      )}
    </div>
  );
}

function PlayersStep({
  selectedPlayers,
  onTogglePlayer,
}: {
  selectedPlayers: string[];
  onTogglePlayer: (name: string) => void;
}) {
  const userDetails = useAuthStore((s) => s.userDetails);
  const friends = userDetails?.friends ?? [];
  const [searchQuery, setSearchQuery] = useState("");
  const [searchResults, setSearchResults] = useState<string[]>([]);
  const [searching, setSearching] = useState(false);
  const debouncedSearch = useDebouncedValue(searchQuery, 300);

  useEffect(() => {
    if (debouncedSearch.length < 2) {
      setSearchResults([]);
      return;
    }
    let cancelled = false;
    setSearching(true);
    api
      .searchUsers(debouncedSearch)
      .then((results) => {
        if (!cancelled) {
          setSearchResults(results);
          setSearching(false);
        }
      })
      .catch(() => {
        if (!cancelled) setSearching(false);
      });
    return () => {
      cancelled = true;
    };
  }, [debouncedSearch]);

  const unselectedFriends = friends.filter(
    (f) => !selectedPlayers.includes(f),
  );

  return (
    <div className="flex-1 flex flex-col gap-4">
      <div>
        <h3 className="text-sm font-bold text-[var(--color-text)] mb-2">
          Playing ({selectedPlayers.length})
        </h3>
        <div className="flex flex-wrap gap-2">
          {selectedPlayers.map((p) => (
            <button
              key={p}
              onClick={() => onTogglePlayer(p)}
              className="px-3 py-1.5 rounded-full bg-[var(--color-accent)] text-white text-sm font-semibold active:opacity-80"
            >
              {p} ✕
            </button>
          ))}
        </div>
      </div>

      {unselectedFriends.length > 0 && (
        <div>
          <h3 className="text-sm font-bold text-[var(--color-text)] mb-2">
            Friends
          </h3>
          <div className="flex flex-wrap gap-2">
            {unselectedFriends.map((f) => (
              <button
                key={f}
                onClick={() => onTogglePlayer(f)}
                className="px-3 py-1.5 rounded-full bg-[var(--color-surface)] border border-[var(--color-border)] text-[var(--color-text)] text-sm font-semibold active:opacity-80"
              >
                + {f}
              </button>
            ))}
          </div>
        </div>
      )}

      <div>
        <h3 className="text-sm font-bold text-[var(--color-text)] mb-2">
          Search players
        </h3>
        <input
          type="text"
          placeholder="Find by username..."
          value={searchQuery}
          onChange={(e) => setSearchQuery(e.target.value)}
          className="w-full px-3 py-2.5 rounded-lg border-2 border-[var(--color-border)] bg-[var(--color-bg)] text-sm text-[var(--color-text)] placeholder:text-[var(--color-text-muted)] focus:outline-none focus:border-[var(--color-accent)]"
        />
        {searching && (
          <p className="text-xs text-[var(--color-text-muted)] mt-1">Searching...</p>
        )}
        {searchResults.length > 0 && (
          <div className="mt-2 space-y-1">
            {searchResults
              .filter((r) => !selectedPlayers.includes(r))
              .map((username) => (
                <button
                  key={username}
                  onClick={() => {
                    onTogglePlayer(username);
                    setSearchQuery("");
                    setSearchResults([]);
                  }}
                  className="w-full text-left p-2.5 rounded-lg bg-[var(--color-surface)] border border-[var(--color-border)] text-sm font-medium text-[var(--color-text)] active:opacity-80"
                >
                  + {username}
                </button>
              ))}
          </div>
        )}
      </div>
    </div>
  );
}

function ConfirmStep({
  course,
  players,
}: {
  course: CourseVm;
  players: string[];
}) {
  return (
    <div className="flex-1 flex flex-col gap-4">
      <div className="p-4 rounded-xl bg-[var(--color-surface)] border-2 border-[var(--color-border)]">
        <h3 className="text-lg font-bold text-[var(--color-text)] mb-1">
          {course.name}
        </h3>
        <p className="text-sm text-[var(--color-text-muted)]">
          {course.layout || "Default"} · {course.holes.length} holes ·
          Par{" "}
          {course.holes.reduce((sum, h) => sum + h.par, 0)}
        </p>
      </div>

      <div>
        <h3 className="text-sm font-bold text-[var(--color-text)] mb-2">
          Players ({players.length})
        </h3>
        <div className="flex flex-wrap gap-2">
          {players.map((p) => (
            <span
              key={p}
              className="px-3 py-1.5 rounded-full bg-[var(--color-accent)] text-white text-sm font-semibold"
            >
              {p}
            </span>
          ))}
        </div>
      </div>

      <div className="p-3 rounded-lg bg-[var(--color-navbar)] border border-[var(--color-border)]">
        <p className="text-sm text-[var(--color-text)]">
          <span className="font-bold">Scoring:</span> Detailed live
        </p>
      </div>
    </div>
  );
}

export default function NewRound() {
  const navigate = useNavigate();
  const user = useAuthStore((s) => s.user);
  const username = user?.username ?? "";

  const [step, setStep] = useState<Step>("course");
  const [selectedCourse, setSelectedCourse] = useState<CourseVm | null>(null);
  const [selectedPlayers, setSelectedPlayers] = useState<string[]>([username]);
  const [starting, setStarting] = useState(false);

  const togglePlayer = useCallback(
    (name: string) => {
      setSelectedPlayers((prev) =>
        prev.includes(name) ? prev.filter((p) => p !== name) : [...prev, name],
      );
    },
    [],
  );

  const handleStart = useCallback(async () => {
    if (!selectedCourse || starting) return;
    setStarting(true);
    try {
      const round = await api.createRound({
        courseId: selectedCourse.id,
        players: selectedPlayers,
        roundName: "",
        scoreMode: 0 as ScoreMode,
      });
      navigate(`/rounds/${round.id}`);
    } catch {
      setStarting(false);
    }
  }, [selectedCourse, selectedPlayers, starting, navigate]);

  const canProceed =
    step === "course"
      ? selectedCourse !== null
      : step === "players"
        ? selectedPlayers.length > 0
        : selectedCourse !== null;

  const handleNext = () => {
    if (step === "course" && selectedCourse) setStep("players");
    else if (step === "players") setStep("confirm");
    else if (step === "confirm") void handleStart();
  };

  const handleBack = () => {
    if (step === "confirm") setStep("players");
    else if (step === "players") setStep("course");
    else navigate("/");
  };

  return (
    <div className="flex-1 flex flex-col bg-[var(--color-bg)]">
      <header className="flex items-center justify-between px-4 py-3 bg-[var(--color-navbar)] border-b-2 border-[var(--color-border)]">
        <button
          onClick={handleBack}
          className="text-sm font-semibold text-[var(--color-text)] active:opacity-60"
        >
          ← Back
        </button>
        <span className="text-base font-bold text-[var(--color-text)]">
          New Round
        </span>
        <div className="w-12" />
      </header>

      <StepIndicator step={step} />

      <div className="flex-1 px-4 pb-4 overflow-y-auto">
        {step === "course" && (
          <CourseStep
            onSelect={(c) => setSelectedCourse(c)}
            selected={selectedCourse}
          />
        )}
        {step === "players" && (
          <PlayersStep
            selectedPlayers={selectedPlayers}
            onTogglePlayer={togglePlayer}
          />
        )}
        {step === "confirm" && selectedCourse && (
          <ConfirmStep course={selectedCourse} players={selectedPlayers} />
        )}
      </div>

      <div className="px-4 py-3 bg-[var(--color-navbar)] border-t-2 border-[var(--color-border)]">
        <button
          onClick={handleNext}
          disabled={!canProceed || starting}
          className="w-full py-3 rounded-lg bg-[var(--color-accent)] text-white text-base font-bold border-2 border-[var(--color-button-border)] active:opacity-80 disabled:opacity-40"
        >
          {step === "confirm"
            ? starting
              ? "Starting..."
              : "Start Round"
            : "Next"}
        </button>
      </div>
    </div>
  );
}
