import type {
  Round,
  AuthenticatedUser,
  UserDetails,
  UserStats,
  PlayerCourseStats,
  PaceData,
  PagedRounds,
  UpdatePlayerScoreCommand,
  CreateRoundCommand,
  CompleteRoundCommand,
  ScoreMode,
  CourseVm,
} from "../types";

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
    window.location.href = "/new/login";
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

  getRound(roundId: string) {
    return request<Round>(`/api/rounds/${roundId}`);
  },

  getUserRounds(username: string, page = 1, pageSize = 5) {
    return request<PagedRounds>(`/api/rounds?username=${username}&page=${page}&pageSize=${pageSize}`);
  },

  getPagedRounds(username: string, page: number, pageSize = 8) {
    return request<PagedRounds>(`/api/rounds?username=${username}&page=${page}&pageSize=${pageSize}`);
  },

  createRound(cmd: CreateRoundCommand) {
    return request<Round>("/api/rounds", {
      method: "POST",
      body: JSON.stringify(cmd),
    });
  },

  updateScore(roundId: string, cmd: UpdatePlayerScoreCommand) {
    return request<Round>(`/api/rounds/${roundId}/scores`, {
      method: "PUT",
      body: JSON.stringify(cmd),
    });
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

  getCourseStats(roundId: string) {
    return request<PlayerCourseStats[]>(`/api/rounds/${roundId}/courseStats`);
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
};
