import { Action, Reducer } from "redux";
import { AppThunkAction } from ".";
import { CallHistoryMethodAction } from "connected-react-router";
import { actionCreators as notificationActions } from "./Notifications";
import { Round } from "./Rounds";

export interface User {
  username: string;
  token: string;
}
export interface UserStats {
  roundsPlayed: number;
  holesPlayed: number;
  putsPerHole: number;
  fairwayHitRate: number;
  scrambleRate: number;
  onePutRate: number;
  totalScore: number;
  strokesGained: number;
}

export interface UserState {
  loggedIn: boolean;
  user: User | null;
  failedLoginMessage: string | null;
  friendUsers: string[];
  userStats: UserStats | null;
  userRounds: Round[];
}

export interface FetchFriendUsersSuccessAction {
  type: "FETCH_FRIEND_USERS_SUCCEED";
  friends: string[];
}

export interface FetchUserRoundsSuccessAction {
  type: "FETCH_USER_ROUNDS_SUCCEED";
  rounds: Round[];
}

export interface LoginSuccessAction {
  type: "LOGIN_SUCCEED";
  user: User;
}
export interface LoginFailedAction {
  type: "LOGIN_FAILED";
  errorMessage: string;
}

export interface LogUserOutAction {
  type: "LOG_USER_OUT";
}

export interface FetchUserStatsSuccessAction {
  type: "FETCH_USERSTATS_SUCCESS";
  stats: UserStats;
}

export interface FriendAddedSuccessAction {
  type: "FRIEND_ADDED";
  friend: string;
}

export type KnownAction =
  | CallHistoryMethodAction
  | LoginSuccessAction
  | LoginFailedAction
  | FetchFriendUsersSuccessAction
  | LogUserOutAction
  | FriendAddedSuccessAction
  | FetchUserStatsSuccessAction
  | FetchUserRoundsSuccessAction;

let user: User | null = null;
const userString = localStorage.getItem("user");
if (userString) {
  user = JSON.parse(userString);
}
const initialState: UserState = user
  ? {
      loggedIn: true,
      user,
      failedLoginMessage: null,
      friendUsers: [],
      userStats: null,
      userRounds: [],
    }
  : {
      loggedIn: false,
      user: null,
      failedLoginMessage: null,
      friendUsers: [],
      userStats: null,
      userRounds: [],
    };

const logout = (dispatch: (action: KnownAction) => void) => {
  localStorage.removeItem("user");
  dispatch({ type: "LOG_USER_OUT" });
};

export const actionCreators = {
  createUser: (
    username: string,
    password: string
  ): AppThunkAction<KnownAction> => (dispatch, getState) => {
    fetch(`api/users`, {
      method: "POST",
      headers: {
        "Content-Type": "application/json",
      },
      body: JSON.stringify({ username, password }),
    })
      .then((response) => {
        if (response.ok) {
          return response.json() as Promise<User>;
        }
        throw new Error("No joy!");
      })
      .then((data) => {
        dispatch({
          type: "LOGIN_SUCCEED",
          user: data,
        });
        localStorage.setItem("user", JSON.stringify(data));
      })
      .catch((err: Error) => {
        dispatch({ type: "LOGIN_FAILED", errorMessage: err.message });
        setTimeout(() => {
          dispatch({ type: "LOG_USER_OUT" });
        }, 2000);
      });
  },
  requestLogin: (
    username: string,
    password: string
  ): AppThunkAction<KnownAction> => (dispatch, getState) => {
    fetch(`api/users/authenticate`, {
      method: "POST",
      headers: {
        "Content-Type": "application/json",
      },
      body: JSON.stringify({ username, password }),
    })
      .then((response) => {
        if (response.ok) {
          return response.json() as Promise<User>;
        }
        throw new Error("No joy!");
      })
      .then((data) => {
        dispatch({
          type: "LOGIN_SUCCEED",
          user: data,
        });
        localStorage.setItem("user", JSON.stringify(data));
      })
      .catch((err: Error) => {
        dispatch({ type: "LOGIN_FAILED", errorMessage: err.message });
        setTimeout(() => {
          dispatch({ type: "LOG_USER_OUT" });
        }, 2000);
      });
  },
  logout: () => (dispatch: (action: any) => void) => {
    logout(dispatch);
  },
  fetchUsers: (): AppThunkAction<KnownAction> => (dispatch, getState) => {
    const appState = getState();
    if (!appState.user || !appState.user.loggedIn || !appState.user.user)
      return;
    fetch(`api/users/friends`, {
      method: "GET",
      headers: {
        "Content-Type": "application/json",
        Authorization: `Bearer ${appState.user.user.token}`,
      },
    })
      .then((response) => {
        if (response.ok && response.status !== 204) {
          return response.json() as Promise<string[]>;
        }
      })
      .then((data) => {
        if (!data) return;
        dispatch({
          type: "FETCH_FRIEND_USERS_SUCCEED",
          friends: data,
        });
      });
  },
  fetchUserRounds: (
    start?: number,
    count?: number
  ): AppThunkAction<KnownAction> => (dispatch, getState) => {
    const appState = getState();
    if (!appState.user || !appState.user.loggedIn || !appState.user.user)
      return;
    const username = appState.user.user.username;
    fetch(`api/rounds?username=${username}&count=${count}`, {
      method: "GET",
      headers: {
        "Content-Type": "application/json",
        Authorization: `Bearer ${appState.user.user.token}`,
      },
    })
      .then((res) => {
        if (res.status === 401) {
          logout(dispatch);
        }
        if (!res.ok) throw new Error(`${res.status} - ${res.statusText}`);
        return res;
      })
      .then((response) => response.json() as Promise<Round[]>)
      .then((data) => {
        dispatch({
          type: "FETCH_USER_ROUNDS_SUCCEED",
          rounds: data,
        });
      })
      .catch((err: Error) => {
        notificationActions.showNotification(
          `Fetch rounds failed: ${err.message}`,
          "error",
          dispatch
        );
      });
  },
  addFriend: (username: string): AppThunkAction<KnownAction> => (
    dispatch,
    getState
  ) => {
    const appState = getState();
    if (!appState.user || !appState.user.loggedIn || !appState.user.user)
      return;
    fetch(`api/users/friends`, {
      method: "POST",
      headers: {
        "Content-Type": "application/json",
        Authorization: `Bearer ${appState.user.user.token}`,
      },
      body: JSON.stringify({ username }),
    })
      .then((res) => {
        if (!res.ok) throw new Error(`${res.status} - ${res.statusText}`);
        return res;
      })
      .then((response) => {
        dispatch({
          type: "FRIEND_ADDED",
          friend: username,
        });
      })
      .catch((err: Error) => {
        notificationActions.showNotification(
          `Add friend failed: ${err.message}`,
          "error",
          dispatch
        );
      });
  },
  fetchUserStats: (sinceMonths: number): AppThunkAction<KnownAction> => (
    dispatch,
    getState
  ) => {
    const appState = getState();
    if (!appState.user || !appState.user.loggedIn || !appState.user.user)
      return;
    fetch(`api/users/stats?includeMonths=${sinceMonths}`, {
      method: "GET",
      headers: {
        "Content-Type": "application/json",
        Authorization: `Bearer ${appState.user.user.token}`,
      },
    })
      .then((response) => {
        if (response.ok) {
          return response.json() as Promise<UserStats>;
        }
        throw new Error("No joy!");
      })
      .then((data) => {
        dispatch({
          type: "FETCH_USERSTATS_SUCCESS",
          stats: data,
        });
      })
      .catch((err: Error) => {
        notificationActions.showNotification(
          `Fetch user stats failed: ${err.message}`,
          "error",
          dispatch
        );
      });
  },
};

// ----------------
// REDUCER - For a given state and action, returns the new state. To support time travel, this must not mutate the old state.

export const reducer: Reducer<UserState> = (
  state: UserState | undefined,
  incomingAction: Action
): UserState => {
  if (state === undefined) {
    return initialState;
  }

  const action = incomingAction as KnownAction;
  switch (action.type) {
    case "LOGIN_SUCCEED":
      return {
        ...state,
        loggedIn: true,
        user: action.user,
        failedLoginMessage: null,
      };
    case "LOGIN_FAILED":
      return {
        ...state,
        loggedIn: false,
        failedLoginMessage: action.errorMessage,
      };
    case "LOG_USER_OUT":
      return {
        ...state,
        loggedIn: false,
        failedLoginMessage: null,
      };
    case "FETCH_FRIEND_USERS_SUCCEED":
      return {
        ...state,
        friendUsers: action.friends,
      };
    case "FRIEND_ADDED":
      return {
        ...state,
        friendUsers: [...state.friendUsers, action.friend],
      };
    case "FETCH_USERSTATS_SUCCESS":
      return {
        ...state,
        userStats: action.stats,
      };
    case "FETCH_USER_ROUNDS_SUCCEED":
      return {
        ...state,
        userRounds: action.rounds,
      };
    default:
      return state;
  }
};
