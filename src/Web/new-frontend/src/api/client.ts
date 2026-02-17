import type {
  Round,
  AuthenticatedUser,
  UserDetails,
  UserStats,
  UserAchievement,
  PlayerCourseStats,
  PaceData,
  PagedRounds,
  UpdatePlayerScoreCommand,
  CreateRoundCommand,
  CompleteRoundCommand,
  ScoreMode,
  CourseVm,
  StrokeOutcome,
  StrokeSpec,
  Feed,
  LeaderboardPlayer,
  TournamentListing,
  Tournament,
  TournamentCourseInfo,
  TournamentPrices,
  CreateCourseCommand,
  CreateTournamentCommand,
  AddCourseToTournamentCommand,
} from "../types";

// The backend C# enum serializes StrokeOutcome as integers.
// Map them back to the string names our UI expects.
const STROKE_OUTCOME_MAP: Record<number, StrokeOutcome> = {
  0: "Fairway",
  1: "Rough",
  2: "OB",
  3: "Circle2",
  4: "Circle1",
  5: "Basket",
};

function normalizeStrokeOutcome(outcome: string | number): StrokeOutcome {
  if (typeof outcome === "number") {
    return STROKE_OUTCOME_MAP[outcome] ?? "Fairway";
  }
  return outcome as StrokeOutcome;
}

function normalizeStrokeSpec(spec: StrokeSpec): void {
  spec.outcome = normalizeStrokeOutcome(spec.outcome);
}

export function normalizeRound(round: Round): Round {
  for (const ps of round.playerScores) {
    for (const score of ps.scores) {
      if (!score.strokeSpecs) {
        score.strokeSpecs = [];
        continue;
      }
      for (const spec of score.strokeSpecs) {
        normalizeStrokeSpec(spec);
      }
    }
  }
  return round;
}

function normalizeCourseStats(stats: PlayerCourseStats[]): PlayerCourseStats[] {
  for (const stat of stats) {
    if (stat.holeStats) {
      for (const hs of stat.holeStats) {
        if (hs.bestScore?.strokeSpecs) {
          for (const spec of hs.bestScore.strokeSpecs) {
            normalizeStrokeSpec(spec);
          }
        }
        if (hs.last10Scores) {
          for (const score of hs.last10Scores) {
            for (const spec of score.strokeSpecs) {
              normalizeStrokeSpec(spec);
            }
          }
        }
      }
    }
  }
  return stats;
}

class ApiError extends Error {
  status: number;
  constructor(
    status: number,
    message: string,
  ) {
    super(message);
    this.name = "ApiError";
    this.status = status;
  }
}

function getToken(): string | null {
  const stored = localStorage.getItem("user");
  if (!stored) return null;
  try {
    const parsed = JSON.parse(stored) as { token?: string };
    return parsed.token ?? null;
  } catch {
    return null;
  }
}

async function request<T>(
  url: string,
  options: RequestInit = {},
): Promise<T> {
  const token = getToken();
  const headers: Record<string, string> = {
    "Content-Type": "application/json",
    ...(options.headers as Record<string, string>),
  };
  if (token) {
    headers["Authorization"] = `Bearer ${token}`;
  }

  const response = await fetch(url, { ...options, headers });

  if (response.status === 401) {
    localStorage.removeItem("user");
    window.location.href = "/login";
    throw new ApiError(401, "Unauthorized");
  }

  if (!response.ok) {
    throw new ApiError(response.status, `${response.status} - ${response.statusText}`);
  }

  const text = await response.text();
  if (!text) return undefined as T;
  return JSON.parse(text) as T;
}

export const api = {
  authenticate(username: string, password: string) {
    return request<AuthenticatedUser>("/api/users/authenticate", {
      method: "POST",
      body: JSON.stringify({ username, password }),
    });
  },

  register(username: string, password: string, email?: string) {
    return request<AuthenticatedUser>("/api/users", {
      method: "POST",
      body: JSON.stringify({ username, password, email }),
    });
  },

  getUserDetails() {
    return request<UserDetails>("/api/users/details");
  },

  async getRound(roundId: string) {
    const round = await request<Round>(`/api/rounds/${roundId}`);
    return normalizeRound(round);
  },

  async getUserRounds(username: string, page = 1, pageSize = 5) {
    const result = await request<PagedRounds>(`/api/rounds?username=${username}&page=${page}&pageSize=${pageSize}`);
    result.rounds = result.rounds.map(normalizeRound);
    return result;
  },

  async getPagedRounds(username: string, page: number, pageSize = 8) {
    const result = await request<PagedRounds>(`/api/rounds?username=${username}&page=${page}&pageSize=${pageSize}`);
    result.rounds = result.rounds.map(normalizeRound);
    return result;
  },

  async createRound(cmd: CreateRoundCommand) {
    const round = await request<Round>("/api/rounds", {
      method: "POST",
      body: JSON.stringify(cmd),
    });
    return normalizeRound(round);
  },

  async updateScore(roundId: string, cmd: UpdatePlayerScoreCommand) {
    const round = await request<Round>(`/api/rounds/${roundId}/scores`, {
      method: "PUT",
      body: JSON.stringify(cmd),
    });
    return normalizeRound(round);
  },

  completeRound(roundId: string, cmd: CompleteRoundCommand) {
    return request<void>(`/api/rounds/${roundId}/complete`, {
      method: "PUT",
      body: JSON.stringify(cmd),
    });
  },

  deleteRound(roundId: string) {
    return request<void>(`/api/rounds/${roundId}`, { method: "DELETE" });
  },

  leaveRound(roundId: string) {
    return request<void>(`/api/rounds/${roundId}/users`, { method: "DELETE" });
  },

  skipHole(roundId: string, holeNumber: number) {
    return request<void>(`/api/rounds/${roundId}/holes/${holeNumber}`, { method: "DELETE" });
  },

  getRoundStats(roundId: string) {
    return request<UserStats[]>(`/api/rounds/${roundId}/stats`);
  },

  async getCourseStats(roundId: string) {
    const stats = await request<PlayerCourseStats[]>(`/api/rounds/${roundId}/courseStats`);
    return normalizeCourseStats(stats);
  },

  getPaceData(roundId: string) {
    return request<PaceData>(`/api/rounds/${roundId}/pace-data`);
  },

  setScoringMode(roundId: string, scoreMode: ScoreMode) {
    return request<void>(`/api/rounds/${roundId}/scoremode`, {
      method: "PUT",
      body: JSON.stringify({ scoreMode }),
    });
  },

  searchUsers(searchString: string) {
    return request<string[]>(`/api/users?searchString=${searchString}`);
  },

  getCourses(filter = "", latitude = 0, longitude = 0) {
    return request<CourseVm[]>(`/api/courses?filter=${encodeURIComponent(filter)}&latitude=${latitude}&longitude=${longitude}`);
  },

  getFeed(itemType = "", pageNumber = 1, pageSize = 10) {
    const qs = new URLSearchParams({
      itemType,
      pageNumber: String(pageNumber),
      pageSize: String(pageSize),
    });
    return request<Feed>(`/api/feeds?${qs.toString()}`);
  },

  toggleFeedLike(feedItemId: string) {
    return request<void>(`/api/feeds/feedItems/${feedItemId}/like`, {
      method: "PUT",
    });
  },

  getLeaderboard(month = 0, year = 0) {
    const qs = new URLSearchParams({
      onlyFriends: "true",
      month: String(month),
      year: String(year),
    });
    return request<LeaderboardPlayer[]>(`/api/leaderboard?${qs.toString()}`);
  },

  getUserDetailsByUsername(username: string) {
    return request<UserDetails>(`/api/users/${username}/details`);
  },

  getUserAchievements(username: string) {
    return request<UserAchievement[]>(`/api/users/${username}/achievements`);
  },

  createCourse(cmd: CreateCourseCommand) {
    return request<CourseVm>("/api/courses", {
      method: "POST",
      body: JSON.stringify(cmd),
    });
  },

  getTournaments(onlyActive = true, username = "") {
    const qs = new URLSearchParams({
      onlyActive: String(onlyActive),
      username,
    });
    return request<TournamentListing[]>(`/api/tournaments?${qs.toString()}`);
  },

  getTournament(tournamentId: string) {
    return request<Tournament>(`/api/tournaments/${tournamentId}`);
  },

  createTournament(cmd: CreateTournamentCommand) {
    return request<string>("/api/tournaments", {
      method: "POST",
      body: JSON.stringify(cmd),
    });
  },

  addCourseToTournament(cmd: AddCourseToTournamentCommand) {
    return request<TournamentCourseInfo>(`/api/tournaments/${cmd.tournamentId}/courses`, {
      method: "PUT",
      body: JSON.stringify(cmd),
    });
  },

  joinTournament(tournamentId: string) {
    return request<void>(`/api/tournaments/${tournamentId}/players`, {
      method: "PUT",
      body: JSON.stringify({ tournamentId }),
    });
  },

  calculateTournamentPrices(tournamentId: string) {
    return request<TournamentPrices>(`/api/tournaments/${tournamentId}/calculate`, {
      method: "POST",
    });
  },
};
