import { create } from "zustand";
import type { UserDetails, UserAchievement, PagedRounds } from "../types";
import { api } from "../api/client";

interface UserProfileState {
  profile: UserDetails | null;
  achievements: UserAchievement[];
  rounds: PagedRounds | null;
  isLoading: boolean;
  error: string | null;

  fetchProfile: (username: string) => Promise<void>;
  fetchAchievements: (username: string) => Promise<void>;
  fetchRounds: (username: string, page?: number, pageSize?: number) => Promise<void>;
  clear: () => void;
}

export const useUserProfileStore = create<UserProfileState>((set) => ({
  profile: null,
  achievements: [],
  rounds: null,
  isLoading: false,
  error: null,

  fetchProfile: async (username) => {
    set({ isLoading: true, error: null });
    try {
      const profile = await api.getUserDetailsByUsername(username);
      set({ profile, isLoading: false });
    } catch {
      set({ error: "Failed to load profile", isLoading: false });
    }
  },

  fetchAchievements: async (username) => {
    try {
      const achievements = await api.getUserAchievements(username);
      set({ achievements });
    } catch {
      set({ achievements: [] });
    }
  },

  fetchRounds: async (username, page = 1, pageSize = 10) => {
    try {
      const rounds = await api.getPagedRounds(username, page, pageSize);
      set({ rounds });
    } catch {
      /* non-critical */
    }
  },

  clear: () => set({ profile: null, achievements: [], rounds: null, isLoading: false, error: null }),
}));
