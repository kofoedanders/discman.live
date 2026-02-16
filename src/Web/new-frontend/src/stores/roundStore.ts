import { create } from "zustand";
import type {
  Round,
  PlayerScore,
  HoleScore,
  StrokeOutcome,
  PaceData,
  CurrentPace,
  PlayerCourseStats,
  UserStats,
} from "../types";
import { api } from "../api/client";
import { calculatePace } from "../utils/paceUtils";

interface RoundState {
  round: Round | null;
  recentRounds: Round[];
  activeHoleIndex: number;
  editHole: boolean;
  scorecardOpen: boolean;
  paceData: PaceData | null;
  currentPace: CurrentPace | null;
  playerCourseStats: PlayerCourseStats[] | null;
  finishedRoundStats: UserStats[];
  isLoading: boolean;

  fetchRound: (roundId: string) => Promise<void>;
  fetchActiveRound: (activeRoundId?: string | null) => Promise<void>;
  fetchRecentRounds: (username: string, count?: number) => Promise<void>;
  fetchPaceData: (roundId: string) => Promise<void>;
  fetchCourseStats: (roundId: string) => Promise<void>;
  fetchRoundStats: (roundId: string) => Promise<void>;
  setScore: (
    roundId: string,
    holeIndex: number,
    strokes: number,
    strokeOutcomes: StrokeOutcome[],
    username: string,
  ) => Promise<void>;
  completeRound: (roundId: string, base64Signature: string) => Promise<void>;
  deleteRound: (roundId: string) => Promise<void>;
  skipHole: (roundId: string, holeNumber: number) => Promise<void>;
  leaveRound: (roundId: string) => Promise<void>;
  setActiveHole: (index: number) => void;
  goToNextPersonalHole: (username: string) => void;
  setEditHole: (editing: boolean) => void;
  setScorecardOpen: (open: boolean) => void;
  onRoundUpdated: (round: Round) => void;
  onRoundDeleted: (roundId: string) => void;
  clearRound: () => void;
}

function getNextUncompletedHole(round: Round): number {
  const activeHole = round.playerScores
    .map((p: PlayerScore) => p.scores.find((s: HoleScore) => s.strokes === 0))
    .sort((a, b) => (a && b ? a.hole.number - b.hole.number : 0))
    .find(() => true);

  if (!activeHole) {
    const firstPlayer = round.playerScores[0];
    return firstPlayer ? firstPlayer.scores.length - 1 : 0;
  }

  const idx = round.playerScores[0].scores.findIndex(
    (x) => x.hole.number === activeHole.hole.number,
  );
  return idx >= 0 ? idx : 0;
}

function getNextPlayerHole(round: Round, username: string): number {
  const holeScores =
    round.playerScores.find((p) => p.playerName === username)?.scores ?? [];
  const activeHole = holeScores.find((s) => s.strokes === 0);
  if (!activeHole) {
    const firstPlayer = round.playerScores[0];
    return firstPlayer ? firstPlayer.scores.length - 1 : 0;
  }
  const idx = round.playerScores[0].scores.findIndex(
    (x) => x.hole.number === activeHole.hole.number,
  );
  return idx >= 0 ? idx : 0;
}

function recalcPace(round: Round, paceData: PaceData): CurrentPace {
  const scoredHoleNumbers = new Set<number>();
  round.playerScores.forEach((p) => {
    p.scores.forEach((s) => {
      if (s.strokes > 0) scoredHoleNumbers.add(s.hole.number);
    });
  });
  const completedHoles = scoredHoleNumbers.size;
  const totalHoles =
    paceData.totalHoles || round.playerScores[0]?.scores.length || 18;
  const startTime = new Date(round.startTime);
  const elapsedMinutes = Math.max(
    0,
    (Date.now() - startTime.getTime()) / 60000,
  );
  return calculatePace(
    completedHoles,
    totalHoles,
    elapsedMinutes,
    startTime,
    paceData,
  );
}

export const useRoundStore = create<RoundState>((set, get) => ({
  round: null,
  recentRounds: [],
  activeHoleIndex: 0,
  editHole: false,
  scorecardOpen: false,
  paceData: null,
  currentPace: null,
  playerCourseStats: null,
  finishedRoundStats: [],
  isLoading: false,

  fetchRound: async (roundId) => {
    set({ isLoading: true });
    try {
      const round = await api.getRound(roundId);
      const { paceData } = get();
      const currentPace =
        paceData && round ? recalcPace(round, paceData) : null;
      set({
        round,
        activeHoleIndex: getNextUncompletedHole(round),
        currentPace,
        isLoading: false,
      });
    } catch {
      set({ isLoading: false });
    }
  },

  fetchActiveRound: async (activeRoundId?: string | null) => {
    if (!activeRoundId) return;
    try {
      const round = await api.getRound(activeRoundId);
      if (round && !round.isCompleted) {
        set({
          round,
          activeHoleIndex: getNextUncompletedHole(round),
        });
        await get().fetchPaceData(round.id);
      }
    } catch {
      /* active round may not exist */
    }
  },

  fetchRecentRounds: async (username, count = 5) => {
    try {
      const result = await api.getUserRounds(username, 1, count);
      set({ recentRounds: result.rounds });
    } catch {
      /* non-critical */
    }
  },

  fetchPaceData: async (roundId) => {
    try {
      const paceData = await api.getPaceData(roundId);
      const { round } = get();
      const currentPace =
        round && paceData ? recalcPace(round, paceData) : null;
      set({ paceData, currentPace });
    } catch {
      /* pace data may be unavailable */
    }
  },

  fetchCourseStats: async (roundId) => {
    try {
      const stats = await api.getCourseStats(roundId);
      set({ playerCourseStats: stats });
    } catch {
      // stats not available
    }
  },

  fetchRoundStats: async (roundId) => {
    try {
      const stats = await api.getRoundStats(roundId);
      set({ finishedRoundStats: stats });
    } catch {
      // stats not available
    }
  },

  setScore: async (roundId, holeIndex, strokes, strokeOutcomes, username) => {
    try {
      const round = await api.updateScore(roundId, {
        holeIndex,
        strokes,
        strokeOutcomes,
        username,
      });
      const { paceData } = get();
      const currentPace =
        paceData && round ? recalcPace(round, paceData) : get().currentPace;
      set({
        round,
        activeHoleIndex: getNextUncompletedHole(round),
        editHole: false,
        currentPace,
      });
    } catch {
      // score update failed — keep current state
    }
  },

  completeRound: async (roundId, base64Signature) => {
    try {
      await api.completeRound(roundId, { base64Signature });
      const { round } = get();
      if (round) {
        set({ round: { ...round, isCompleted: true } });
      }
    } catch {
      // completion failed
    }
  },

  deleteRound: async (roundId) => {
    await api.deleteRound(roundId);
    set({ round: null });
  },

  skipHole: async (roundId, holeNumber) => {
    await api.skipHole(roundId, holeNumber);
  },

  leaveRound: async (roundId) => {
    await api.leaveRound(roundId);
    set({ round: null });
  },

  setActiveHole: (index) => set({ activeHoleIndex: index }),

  goToNextPersonalHole: (username) => {
    const { round } = get();
    if (!round) return;
    set({ activeHoleIndex: getNextPlayerHole(round, username) });
  },

  setEditHole: (editing) => set({ editHole: editing }),
  setScorecardOpen: (open) => set({ scorecardOpen: open }),

  onRoundUpdated: (round) => {
    const { paceData } = get();
    const currentPace = paceData ? recalcPace(round, paceData) : get().currentPace;
    set({ round, currentPace });
  },

  onRoundDeleted: (roundId) => {
    const { round } = get();
    if (round?.id === roundId) {
      set({ round: null });
    }
  },

  clearRound: () =>
    set({
      round: null,
      paceData: null,
      currentPace: null,
      playerCourseStats: null,
      finishedRoundStats: [],
      activeHoleIndex: 0,
    }),
}));
