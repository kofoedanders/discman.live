import { create } from "zustand";
import type {
  TournamentListing,
  Tournament,
  TournamentCourseInfo,
  TournamentPrices,
} from "../types";
import { api } from "../api/client";

interface TournamentState {
  tournaments: TournamentListing[];
  selectedTournament: Tournament | null;
  isLoading: boolean;
  error: string | null;

  fetchTournaments: (onlyActive?: boolean) => Promise<void>;
  fetchTournament: (id: string) => Promise<void>;
  createTournament: (name: string, start: string, end: string) => Promise<string>;
  joinTournament: (tournamentId: string, username: string) => Promise<void>;
  addCourse: (tournamentId: string, courseId: string) => Promise<void>;
  calculatePrices: (tournamentId: string) => Promise<void>;
  clear: () => void;
}

export const useTournamentStore = create<TournamentState>((set, get) => ({
  tournaments: [],
  selectedTournament: null,
  isLoading: false,
  error: null,

  fetchTournaments: async (onlyActive = true) => {
    set({ isLoading: true, error: null });
    try {
      const tournaments = await api.getTournaments(onlyActive);
      set({ tournaments, isLoading: false });
    } catch {
      set({ error: "Failed to load tournaments", isLoading: false });
    }
  },

  fetchTournament: async (id) => {
    set({ isLoading: true, error: null });
    try {
      const tournament = await api.getTournament(id);
      set({ selectedTournament: tournament, isLoading: false });
    } catch {
      set({ error: "Failed to load tournament", isLoading: false });
    }
  },

  createTournament: async (name, start, end) => {
    const id = await api.createTournament({ name, start, end });
    return id;
  },

  joinTournament: async (tournamentId, username) => {
    await api.joinTournament(tournamentId);
    const { selectedTournament } = get();
    if (selectedTournament) {
      set({
        selectedTournament: {
          ...selectedTournament,
          info: {
            ...selectedTournament.info,
            players: [...selectedTournament.info.players, username],
          },
        },
      });
    }
  },

  addCourse: async (tournamentId, courseId) => {
    const course: TournamentCourseInfo = await api.addCourseToTournament({
      tournamentId,
      courseId,
    });
    const { selectedTournament } = get();
    if (selectedTournament) {
      set({
        selectedTournament: {
          ...selectedTournament,
          info: {
            ...selectedTournament.info,
            courses: [...selectedTournament.info.courses, course],
          },
        },
      });
    }
  },

  calculatePrices: async (tournamentId) => {
    const prices: TournamentPrices = await api.calculateTournamentPrices(tournamentId);
    const { selectedTournament } = get();
    if (selectedTournament) {
      set({
        selectedTournament: {
          ...selectedTournament,
          prices,
        },
      });
    }
  },

  clear: () => set({ tournaments: [], selectedTournament: null, isLoading: false, error: null }),
}));
