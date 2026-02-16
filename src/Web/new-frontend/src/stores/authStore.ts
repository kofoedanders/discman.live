import { create } from "zustand";
import type { User, UserDetails } from "../types";
import { api } from "../api/client";

interface AuthState {
  user: User | null;
  userDetails: UserDetails | null;
  isLoggedIn: boolean;
  loginError: string | null;

  login: (username: string, password: string) => Promise<void>;
  register: (username: string, password: string, email?: string) => Promise<void>;
  logout: () => void;
  loadFromStorage: () => void;
  fetchUserDetails: () => Promise<void>;
  clearActiveRound: () => void;
}

export const useAuthStore = create<AuthState>((set, get) => ({
  user: null,
  userDetails: null,
  isLoggedIn: false,
  loginError: null,

  login: async (username, password) => {
    try {
      set({ loginError: null });
      const result = await api.authenticate(username, password);
      const user: User = { username: result.username, token: result.token };
      localStorage.setItem("user", JSON.stringify(user));
      set({ user, isLoggedIn: true, loginError: null });
      await get().fetchUserDetails();
    } catch {
      set({ loginError: "Login failed. Check your credentials." });
    }
  },

  register: async (username, password, email) => {
    try {
      set({ loginError: null });
      const result = await api.register(username, password, email);
      const user: User = { username: result.username, token: result.token };
      localStorage.setItem("user", JSON.stringify(user));
      set({ user, isLoggedIn: true, loginError: null });
      await get().fetchUserDetails();
    } catch {
      set({ loginError: "Registration failed." });
    }
  },

  logout: () => {
    localStorage.removeItem("user");
    set({ user: null, userDetails: null, isLoggedIn: false, loginError: null });
  },

  loadFromStorage: () => {
    const stored = localStorage.getItem("user");
    if (stored) {
      try {
        const user = JSON.parse(stored) as User;
        set({ user, isLoggedIn: true });
      } catch {
        localStorage.removeItem("user");
      }
    }
  },

  fetchUserDetails: async () => {
    try {
      const details = await api.getUserDetails();
      set({ userDetails: details });
    } catch {
      /* details fetch is non-critical */
    }
  },

  clearActiveRound: () => {
    const { userDetails } = get();
    if (userDetails) {
      set({ userDetails: { ...userDetails, activeRound: null } });
    }
  },
}));
