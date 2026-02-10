import { Action, Reducer } from "redux";
import { AppThunkAction } from ".";
import { CallHistoryMethodAction, push } from "connected-react-router";
import { actionCreators as notificationActions } from "./Notifications";
import { hub } from "./configureStore";
import {
  Round,
  NewRoundCreatedAction,
  RoundWasDeletedAction,
  ConnectToHubAction,
} from "./Rounds";
import * as signalR from "@microsoft/signalr";

export interface User {
  username: string;
  token: string;
}
export interface UserStats {
  username: string;
  roundsPlayed: number;
  holesPlayed: number;
  fairwayHitRate: number;
  scrambleRate: number;
  circle1Rate: number;
  circle2Rate: number;
  obRate: number;
  birdieRate: number;
  parRate: number;
  averageScore: number;
  strokesGained: number;
}

export interface UserDetails {
  email: string;
  username: string;
  simpleScoring: boolean;
  newsIdsSeen: string[];
  friends: string[];
  settingsInitialized: boolean;
  emoji: string;
  country: string;
  activeRound: string | null;
  discmanPoints: number;
  elo: number;
  ratingHistory: Rating[];
}

export interface Rating {
  elo: number;
  datetime: Date;
}

export interface YearSummary {
  hoursPlayed: number;
  roundsPlayed: number;
  totalScore: number;
  bestCardmate: string;
  bestCardmateAverageScore: number;
  worstCardmate: string;
  worstCardmateAverageScore: number;
  mostPlayedCourse: string;
  mostPlayedCourseRoundsCount: number;
}

export interface UserAchievement {
  achievementName: string;
  username: string;
  achievedAt: Date;
  roundId: string;
}
export interface GroupedAchievement {
  achievement: UserAchievement;
  count: number;
}
export interface UserState {
  loggedIn: boolean;
  user: User | null;
  userDetails: UserDetails | null;
  usersDetails: UserDetails[] | null;
  failedLoginMessage: string | null;
  userStats: UserStats | null;
  userRounds: PagedRounds | null;
  userYearSummary: YearSummary | null;
  userAchievements: GroupedAchievement[];
  searchedUsers: string[];
  feed: Feed | null;
}

export interface PagedRounds {
  rounds: Round[];
  totalItemCount: number;
  pageNumber: number;
  pages: number;
}

export interface Feed {
  feedItems: FeedItem[];
  isLastPage: boolean;
}

export interface FeedItem {
  id: string;
  subjects: string[];
  itemType: string;
  courseName: string;
  holeScore: number;
  holeNumber: number;
  action: string;
  registeredAt: string;
  roundId: string;
  roundScores: number[];
  likes: string[];
  achievementName: string;
  friendName: string;
  tournamentName: string;
  tournamentId: string;
}

export interface FetchFeedSuccessAction {
  type: "FETCH_FEED_SUCCESS";
  feed: Feed;
}

export interface SpectatorJoindAction {
  type: "SPEC_JOINED";
  roundId: string;
}

export interface FetchUserYearSummarySuccessAction {
  type: "FETCH_USER_YEARS_SUMMARY_SUCCESS";
  yearSummary: YearSummary;
}

export interface ClearUserYearSummaryAction {
  type: "CLEAR_USER_YEAR_SUMMARY";
}

export interface SpectatorLeftAction {
  type: "SPEC_LEFT";
  roundId: string;
}

export interface SearchUsersSuccessAction {
  type: "SEARCH_USERS_SUCCESS";
  users: string[];
}

export interface FetchUserAchievementsSuccessAction {
  type: "FETCH_USER_ACHIEVEMENTS_SUCCESS";
  achievements: GroupedAchievement[];
}

export interface FetchUserRoundsSuccessAction {
  type: "FETCH_USER_ROUNDS_SUCCEED";
  pagedRounds: PagedRounds;
}

export interface LoginSuccessAction {
  type: "LOGIN_SUCCEED";
  user: User;
}
export interface LoginFailedAction {
  type: "LOGIN_FAILED";
  errorMessage: string;
}

export interface SimpleScoringUpdatedSuccessACtion {
  type: "SIMPLE_SCORING_UPDATED_SUCCESS";
  simpleScoring: boolean;
}

export interface EmojiUpdatedSuccessAction {
  type: "EMOJI_UPDATED_SUCCESS";
  emoji: string;
}

export interface SetLoggedInUserAction {
  type: "SET_LOGGEDIN_USER";
  user: User;
}

export interface LogUserOutAction {
  type: "LOG_USER_OUT";
}

export interface ChangePasswordSuccessAction {
  type: "CHANGE_PASSWORD_SUCCESS";
}

export interface ChangeEmailSuccessAction {
  type: "CHANGE_EMAIL_SUCCESS";
  email: string;
}

export interface FetchUserStatsSuccessAction {
  type: "FETCH_USERSTATS_SUCCESS";
  stats: UserStats;
}

export interface FriendAddedSuccessAction {
  type: "FRIEND_ADDED";
  friend: string;
}
export interface FetchOtherUserSuccessAction {
  type: "FETCH_OTHER_USER";
  username: string;
}

export interface SetNewsSeenSuccessAction {
  type: "NEWS_SEEN_SUCCESS";
  newsId: string;
}

export interface FetchMoreUserDetailsSuccessAction {
  type: "FETCH_MORE_USER_DETAILS_SUCCESS";
  userDetails: UserDetails;
}

export interface FetchUserDetailsSuccessAction {
  type: "FETCH_USER_DETAILS_SUCCESS";
  userDetails: UserDetails;
}

export interface LikeToggledAction {
  type: "LIKE_TOGGLED";
  id: string;
}
export interface SetSettingsInitSuccessAction {
  type: "SET_SETTINGS_INIT_SUCCESS";
}

export interface SetCountrySuccessAction {
  type: "COUNTRY_UPDATED_SUCCESS";
  country: string;
}

export interface ClearFeedAction {
  type: "CLEAR_FEED";
}

export type KnownAction =
  | CallHistoryMethodAction
  | SimpleScoringUpdatedSuccessACtion
  | LoginSuccessAction
  | LoginFailedAction
  | SetCountrySuccessAction
  | LogUserOutAction
  | ConnectToHubAction
  | SetSettingsInitSuccessAction
  | SetNewsSeenSuccessAction
  | FriendAddedSuccessAction
  | FetchUserStatsSuccessAction
  | FetchUserRoundsSuccessAction
  | FetchOtherUserSuccessAction
  | FetchUserAchievementsSuccessAction
  | ChangeEmailSuccessAction
  | SearchUsersSuccessAction
  | SpectatorJoindAction
  | FetchUserDetailsSuccessAction
  | ChangePasswordSuccessAction
  | FetchMoreUserDetailsSuccessAction
  | LikeToggledAction
  | SetLoggedInUserAction
  | FetchFeedSuccessAction
  | EmojiUpdatedSuccessAction
  | ClearFeedAction
  | NewRoundCreatedAction
  | FetchUserYearSummarySuccessAction
  | ClearUserYearSummaryAction
  | RoundWasDeletedAction
  | SpectatorLeftAction;

let user: User | null = null;
const userString = localStorage.getItem("user");
if (userString) {
  user = JSON.parse(userString);
}
const initialState: UserState = user
  ? {
      userDetails: { settingsInitialized: true } as UserDetails,
      usersDetails: [],
      loggedIn: true,
      user,
      failedLoginMessage: null,
      userStats: null,
      userRounds: null,
      userAchievements: [],
      userYearSummary: null,
      searchedUsers: [],
      feed: null,
    }
  : {
      loggedIn: false,
      userDetails: { settingsInitialized: true } as UserDetails,
      usersDetails: [],
      user: null,
      failedLoginMessage: null,
      userStats: null,
      userRounds: null,
      userAchievements: [],
      userYearSummary: null,
      searchedUsers: [],
      feed: null,
    };

const logout = (dispatch: (action: KnownAction) => void) => {
  localStorage.removeItem("user");
  document.cookie = `authentication=; expires=Thu, 01 Jan 1970 00:00:00 UTC; path=/;`;
  dispatch({ type: "LOG_USER_OUT" });
  dispatch(push("/"));
};

export const actionCreators = {
  spectatorJoined: (roundId: string): AppThunkAction<KnownAction> => (
    dispatch,
    getState
  ) => {
    dispatch({ type: "SPEC_JOINED", roundId });
  },
  spectatorLeft: (roundId: string): AppThunkAction<KnownAction> => (
    dispatch,
    getState
  ) => {
    dispatch({ type: "SPEC_LEFT", roundId });
  },
  setLoggedInUser: (userJson: string): AppThunkAction<KnownAction> => (
    dispatch,
    getState
  ) => {
    dispatch({ type: "SET_LOGGEDIN_USER", user: JSON.parse(userJson) });
    localStorage.setItem("user", userJson);
  },

  createUser: (
    username: string,
    password: string,
    email?: string
  ): AppThunkAction<KnownAction> => (dispatch, getState) => {
    fetch(`api/users`, {
      method: "POST",
      headers: {
        "Content-Type": "application/json",
      },
      body: JSON.stringify({ username, password, email }),
    })
      .then((response) => {
        if (response.ok) {
          return response.json() as Promise<User>;
        }
        throw new Error("Error creating user");
      })
      .then((data) => {
        dispatch({
          type: "LOGIN_SUCCEED",
          user: data,
        });
        localStorage.setItem("user", JSON.stringify(data));
        actionCreators.fetchUserDetails()(dispatch, getState);
        dispatch(push("/"));
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
        document.cookie = `authentication=${data.token}`;
        dispatch(push("/"));
      })
      .catch((err: Error) => {
        dispatch({ type: "LOGIN_FAILED", errorMessage: err.message });
        setTimeout(() => {
          dispatch({ type: "LOG_USER_OUT" });
        }, 2000);
      });
  },
  forgotPassword: (email: string): AppThunkAction<KnownAction> => (
    dispatch,
    getState
  ) => {
    fetch(`api/users/resetpassword`, {
      method: "POST",
      headers: {
        "Content-Type": "application/json",
      },
      body: JSON.stringify({ email }),
    })
      .then((response) => {
        if (response.ok) {
          return response.json() as Promise<User>;
        }
        throw new Error("No joy!");
      })
      .then((data) => {})
      .catch((err: Error) => {
        setTimeout(() => {
          dispatch({ type: "LOG_USER_OUT" });
        }, 2000);
      });
  },
  setSettingsInitialized: (): AppThunkAction<KnownAction> => (
    dispatch,
    getState
  ) => {
    const appState = getState();
    if (!appState.user || !appState.user.loggedIn || !appState.user.user)
      return;
    fetch(`api/users/settingsInitialized`, {
      method: "POST",
      headers: {
        "Content-Type": "application/json",
        Authorization: `Bearer ${appState.user.user.token}`,
      },
    })
      .then((res) => {
        if (!res.ok) throw new Error(`${res.status} - ${res.statusText}`);
        return res;
      })
      .then((data) => {
        dispatch({ type: "SET_SETTINGS_INIT_SUCCESS" });
      })
      .catch((err: Error) => {
        dispatch({ type: "CLEAR_USER_YEAR_SUMMARY" });
        notificationActions.showNotification(
          `Fetch year summary failed: ${err.message}`,
          "error",
          dispatch
        );
      });
  },
  setSimpleScoring: (simpleScoring: boolean): AppThunkAction<KnownAction> => (
    dispatch,
    getState
  ) => {
    const appState = getState();
    if (!appState.user || !appState.user.loggedIn || !appState.user.user)
      return;
    fetch(`api/users/simpleScoring`, {
      method: "POST",
      headers: {
        "Content-Type": "application/json",
        Authorization: `Bearer ${appState.user.user.token}`,
      },
      body: JSON.stringify({ simpleScoring }),
    })
      .then((res) => {
        if (!res.ok) throw new Error(`${res.status} - ${res.statusText}`);
        return res;
      })
      .then((data) => {
        dispatch({ type: "SIMPLE_SCORING_UPDATED_SUCCESS", simpleScoring });
      })
      .catch((err: Error) => {
        notificationActions.showNotification(
          `Set news seen failed: ${err.message}`,
          "error",
          dispatch
        );
      });
  },
  setCountry: (country: string): AppThunkAction<KnownAction> => (
    dispatch,
    getState
  ) => {
    const appState = getState();
    if (!appState.user || !appState.user.loggedIn || !appState.user.user)
      return;
    fetch(`api/users/country`, {
      method: "POST",
      headers: {
        "Content-Type": "application/json",
        Authorization: `Bearer ${appState.user.user.token}`,
      },
      body: JSON.stringify({ country }),
    })
      .then((res) => {
        if (!res.ok) throw new Error(`${res.status} - ${res.statusText}`);
        return res;
      })
      .then((data) => {
        dispatch({ type: "COUNTRY_UPDATED_SUCCESS", country });
      })
      .catch((err: Error) => {
        notificationActions.showNotification(
          `Set coutnry failed: ${err.message}`,
          "error",
          dispatch
        );
      });
  },
  setEmoji: (emoji: string): AppThunkAction<KnownAction> => (
    dispatch,
    getState
  ) => {
    const appState = getState();
    if (!appState.user || !appState.user.loggedIn || !appState.user.user)
      return;
    fetch(`api/users/emoji`, {
      method: "POST",
      headers: {
        "Content-Type": "application/json",
        Authorization: `Bearer ${appState.user.user.token}`,
      },
      body: JSON.stringify({ emoji }),
    })
      .then((res) => {
        if (!res.ok) throw new Error(`${res.status} - ${res.statusText}`);
        return res;
      })
      .then((data) => {
        dispatch({ type: "EMOJI_UPDATED_SUCCESS", emoji });
      })
      .catch((err: Error) => {
        notificationActions.showNotification(
          `Set emoji failed: ${err.message}`,
          "error",
          dispatch
        );
      });
  },
  setNewsSeen: (newsId: string): AppThunkAction<KnownAction> => (
    dispatch,
    getState
  ) => {
    const appState = getState();
    if (!appState.user || !appState.user.loggedIn || !appState.user.user)
      return;
    fetch(`api/users/newsSeen`, {
      method: "POST",
      headers: {
        "Content-Type": "application/json",
        Authorization: `Bearer ${appState.user.user.token}`,
      },
      body: JSON.stringify({ newsId }),
    })
      .then((res) => {
        if (!res.ok) throw new Error(`${res.status} - ${res.statusText}`);
        return res;
      })
      .then((data) => {
        dispatch({ type: "NEWS_SEEN_SUCCESS", newsId: newsId });
      })
      .catch((err: Error) => {
        notificationActions.showNotification(
          `Set news seen failed: ${err.message}`,
          "error",
          dispatch
        );
      });
  },
  setPassword: (
    newPassword: string,
    resetId: string
  ): AppThunkAction<KnownAction> => (dispatch, getState) => {
    fetch(`api/users/setpassword`, {
      method: "POST",
      headers: {
        "Content-Type": "application/json",
      },
      body: JSON.stringify({ resetId, newPassword }),
    })
      .then((response) => {
        if (response.ok) {
          return response;
        }
        throw new Error("No joy!");
      })
      .then((data) => {
        notificationActions.showNotification(
          "Your password was successfully set",
          "info",
          dispatch
        );
        dispatch(push("/"));
      })
      .catch((err: Error) => {
        setTimeout(() => {
          dispatch({ type: "LOG_USER_OUT" });
        }, 2000);
      });
  },
  logout: () => (dispatch: (action: any) => void) => {
    logout(dispatch);
  },
  changePassword: (password: string): AppThunkAction<KnownAction> => (
    dispatch,
    getState
  ) => {
    const appState = getState();
    if (!appState.user || !appState.user.loggedIn || !appState.user.user)
      return;
    fetch(`api/users/${appState.user.user.username}/password`, {
      method: "PUT",
      headers: {
        "Content-Type": "application/json",
        Authorization: `Bearer ${appState.user.user.token}`,
      },
      body: JSON.stringify({
        newPassword: password,
      }),
    })
      .then((res) => {
        if (!res.ok) throw new Error(`${res.status} - ${res.statusText}`);
        return res;
      })
      .then((data) => {
        if (!data) return;
        dispatch({
          type: "CHANGE_PASSWORD_SUCCESS",
        });
      });
  },
  changeEmail: (email: string): AppThunkAction<KnownAction> => (
    dispatch,
    getState
  ) => {
    const appState = getState();
    if (!appState.user || !appState.user.loggedIn || !appState.user.user)
      return;
    fetch(`api/users/email`, {
      method: "PUT",
      headers: {
        "Content-Type": "application/json",
        Authorization: `Bearer ${appState.user.user.token}`,
      },
      body: JSON.stringify({
        newEmail: email,
      }),
    })
      .then((res) => {
        if (!res.ok) throw new Error(`${res.status} - ${res.statusText}`);
        return res.text() as Promise<string>;
      })
      .then((data) => {
        if (!data) return;
        dispatch({
          type: "CHANGE_EMAIL_SUCCESS",
          email: data,
        });
      });
  },
  toggleLike: (itemId: string): AppThunkAction<KnownAction> => (
    dispatch,
    getState
  ) => {
    const appState = getState();
    if (!appState.user || !appState.user.loggedIn || !appState.user.user)
      return;
    fetch(`api/feeds/feedItems/${itemId}/like`, {
      method: "PUT",
      headers: {
        "Content-Type": "application/json",
        Authorization: `Bearer ${appState.user.user.token}`,
      },
    })
      .then((res) => {
        if (!res.ok) throw new Error(`${res.status} - ${res.statusText}`);
        return res;
      })
      .then(() => {
        dispatch({
          type: "LIKE_TOGGLED",
          id: itemId,
        });
      })
      .catch((err: Error) => {
        notificationActions.showNotification(
          `Fetch feed failed: ${err.message}`,
          "error",
          dispatch
        );
      });
  },
  fetchFeed: (itemType: string, page: number): AppThunkAction<KnownAction> => (
    dispatch,
    getState
  ) => {
    const appState = getState();
    if (!appState.user || !appState.user.loggedIn || !appState.user.user)
      return;

    if (appState.user.feed?.feedItems && page === 1) {
      dispatch({ type: "CLEAR_FEED" });
    }
    fetch(`api/feeds?itemType=${itemType}&pageNumber=${page}&pageSize=${10}`, {
      method: "GET",
      headers: {
        "Content-Type": "application/json",
        Authorization: `Bearer ${appState.user.user.token}`,
      },
    })
      .then((res) => {
        if (!res.ok) throw new Error(`${res.status} - ${res.statusText}`);
        return res;
      })
      .then((response) => response.json() as Promise<Feed>)
      .then((data) => {
        dispatch({
          type: "FETCH_FEED_SUCCESS",
          feed: data,
        });
      })
      .catch((err: Error) => {
        notificationActions.showNotification(
          `Fetch feed failed: ${err.message}`,
          "error",
          dispatch
        );
      });
  },
  fetchUserRounds: (
    page: number,
    usernameToFetch?: string
  ): AppThunkAction<KnownAction> => (dispatch, getState) => {
    const appState = getState();
    if (!appState.user || !appState.user.loggedIn || !appState.user.user)
      return;
    const username = usernameToFetch || appState.user.user.username;
    fetch(`api/rounds?username=${username}&page=${page}&pageSize=8`, {
      method: "GET",
      headers: {
        "Content-Type": "application/json",
        Authorization: `Bearer ${appState.user.user.token}`,
      },
    })
      .then((res) => {
        if (!res.ok) throw new Error(`${res.status} - ${res.statusText}`);
        return res;
      })
      .then((response) => response.json() as Promise<PagedRounds>)
      .then((data) => {
        dispatch({
          type: "FETCH_USER_ROUNDS_SUCCEED",
          pagedRounds: data,
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
  searchUsers: (searchString: string): AppThunkAction<KnownAction> => (
    dispatch,
    getState
  ) => {
    if (searchString.length < 3) return;
    const appState = getState();
    if (!appState.user || !appState.user.loggedIn || !appState.user.user)
      return;
    fetch(`api/users?searchString=${searchString}`, {
      method: "GET",
      headers: {
        "Content-Type": "application/json",
        Authorization: `Bearer ${appState.user.user.token}`,
      },
    })
      .then((res) => {
        if (!res.ok) throw new Error(`${res.status} - ${res.statusText}`);
        return res;
      })
      .then((response) => response.json() as Promise<string[]>)
      .then((data) => {
        dispatch({
          type: "SEARCH_USERS_SUCCESS",
          users: data,
        });
      })
      .catch((err: Error) => {
        notificationActions.showNotification(
          `Search friends failed: ${err.message}`,
          "error",
          dispatch
        );
      });
  },
  fetchUserStats: (
    sinceMonths: number,
    username?: string
  ): AppThunkAction<KnownAction> => (dispatch, getState) => {
    const appState = getState();
    if (!appState.user || !appState.user.loggedIn || !appState.user.user)
      return;
    const user = username || appState.user.user.username;
    fetch(`api/users/${user}/stats?includeMonths=${sinceMonths}`, {
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
  connectToHub: (): AppThunkAction<KnownAction> => (dispatch, getState) => {
    dispatch({ type: "CONNECT_TO_HUB" });
  },
  fetchMoreUserDetails: (username: string): AppThunkAction<KnownAction> => (
    dispatch,
    getState
  ) => {
    const appState = getState();
    if (!appState.user || !appState.user.loggedIn || !appState.user.user)
      return;

    if (appState.user.usersDetails?.some((u) => u.username === username))
      return;

    fetch(`api/users/${username}/details`, {
      method: "GET",
      headers: {
        "Content-Type": "application/json",
        Authorization: `Bearer ${appState.user.user.token}`,
      },
    })
      .then((response) => {
        if (response.ok) {
          return response.json() as Promise<UserDetails>;
        }
        throw new Error("No joy!");
      })
      .then((data) => {
        dispatch({
          type: "FETCH_MORE_USER_DETAILS_SUCCESS",
          userDetails: data,
        });
      })
      .catch((err: Error) => {
        notificationActions.showNotification(
          `Fetch user details failed: ${err.message}`,
          "error",
          dispatch
        );
      });
  },
  fetchUserYearSummary: (
    username: string,
    year: number
  ): AppThunkAction<KnownAction> => (dispatch, getState) => {
    const appState = getState();
    if (!appState.user || !appState.user.loggedIn || !appState.user.user)
      return;

    dispatch({ type: "CLEAR_USER_YEAR_SUMMARY" });

    fetch(`api/users/${username}/yearsummary/${year}`, {
      method: "GET",
      headers: {
        "Content-Type": "application/json",
        Authorization: `Bearer ${appState.user.user.token}`,
      },
    })
      .then((response) => {
        if (response.ok) {
          return response.json() as Promise<YearSummary>;
        }
        throw new Error("No joy!");
      })
      .then((data) => {
        dispatch({
          type: "FETCH_USER_YEARS_SUMMARY_SUCCESS",
          yearSummary: data,
        });
      })
      .catch((err: Error) => {
        notificationActions.showNotification(
          `Fetch user details failed: ${err.message}`,
          "error",
          dispatch
        );
      });
  },
  fetchUserDetails: (
    onlyIfDisconnected?: boolean
  ): AppThunkAction<KnownAction> => (dispatch, getState) => {
    const appState = getState();
    if (!appState.user || !appState.user.loggedIn || !appState.user.user)
      return;

    if (
      onlyIfDisconnected &&
      hub.state === signalR.HubConnectionState.Connected
    ) {
      return;
    }

    fetch(`api/users/details`, {
      method: "GET",
      headers: {
        "Content-Type": "application/json",
        Authorization: `Bearer ${appState.user.user.token}`,
      },
    })
      .then((response) => {
        if (response.ok) {
          return response.json() as Promise<UserDetails>;
        }
        throw new Error("No joy!");
      })
      .then((data) => {
        dispatch({
          type: "FETCH_USER_DETAILS_SUCCESS",
          userDetails: data,
        });
      })
      .catch((err: Error) => {
        notificationActions.showNotification(
          `Fetch user details failed: ${err.message}`,
          "error",
          dispatch
        );
      });
  },
  fetchUserAchievements: (username?: string): AppThunkAction<KnownAction> => (
    dispatch,
    getState
  ) => {
    const appState = getState();
    if (!appState.user || !appState.user.loggedIn || !appState.user.user)
      return;
    const user = username || appState.user.user.username;
    fetch(`api/users/${user}/achievements`, {
      method: "GET",
      headers: {
        "Content-Type": "application/json",
        Authorization: `Bearer ${appState.user.user.token}`,
      },
    })
      .then((response) => {
        if (response.ok) {
          return response.json() as Promise<GroupedAchievement[]>;
        }
        throw new Error("No joy!");
      })
      .then((data) => {
        dispatch({
          type: "FETCH_USER_ACHIEVEMENTS_SUCCESS",
          achievements: data,
        });
      })
      .catch((err: Error) => {
        notificationActions.showNotification(
          `Fetch user achievements failed: ${err.message}`,
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
    case "SET_LOGGEDIN_USER":
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
    case "CLEAR_FEED":
      return {
        ...state,
        feed: null,
      };
    case "CHANGE_EMAIL_SUCCESS":
      return {
        ...state,
        userDetails: state.userDetails && {
          ...state.userDetails,
          email: action.email,
        },
      };
    case "SET_SETTINGS_INIT_SUCCESS":
      return {
        ...state,
        userDetails: state.userDetails && {
          ...state.userDetails,
          settingsInitialized: true,
        },
      };
    case "COUNTRY_UPDATED_SUCCESS":
      return {
        ...state,
        userDetails: state.userDetails && {
          ...state.userDetails,
          country: action.country,
        },
      };
    case "EMOJI_UPDATED_SUCCESS":
      return {
        ...state,
        userDetails: state.userDetails && {
          ...state.userDetails,
          emoji: action.emoji,
        },
      };
    case "SIMPLE_SCORING_UPDATED_SUCCESS":
      return {
        ...state,
        userDetails: state.userDetails && {
          ...state.userDetails,
          simpleScoring: action.simpleScoring,
        },
      };
    case "NEWS_SEEN_SUCCESS":
      return {
        ...state,
        userDetails: state.userDetails && {
          ...state.userDetails,
          newsIdsSeen: [...state.userDetails.newsIdsSeen, action.newsId],
        },
      };
    case "FETCH_FEED_SUCCESS":
      return {
        ...state,
        feed: state.feed
          ? {
              ...state.feed,
              feedItems: [...state.feed.feedItems, ...action.feed.feedItems],
              isLastPage: action.feed.isLastPage,
            }
          : action.feed,
      };
    case "LIKE_TOGGLED":
      const item =
        state.feed && state.feed.feedItems.find((i) => i.id === action.id);
      const username = state.user?.username;
      if (!item || !state.feed || !item.likes || !username) return state;
      return {
        ...state,
        feed: {
          ...state.feed,
          feedItems: state.feed.feedItems.map((f) => {
            if (f.id !== action.id) return f;
            return {
              ...f,
              likes: item.likes.some((l) => l === username)
                ? item.likes.filter((l) => l !== username)
                : [...item.likes, username],
            };
          }),
        },
      };
    case "FRIEND_ADDED":
      return {
        ...state,
        // searchedUsers: state.searchedUsers.filter((u) => u !== action.friend),
        searchedUsers: [],
        userDetails: state.userDetails
          ? {
              ...state.userDetails,
              friends: [...state.userDetails.friends, action.friend],
            }
          : null,
      };
    case "FETCH_USERSTATS_SUCCESS":
      return {
        ...state,
        userStats: action.stats,
      };
    case "FETCH_USER_ROUNDS_SUCCEED":
      return {
        ...state,
        userRounds: action.pagedRounds,
        // roundInProgress:
        //   action.pagedRounds.rounds.find((r) => !r.isCompleted) || null,
      };
    case "FETCH_USER_YEARS_SUMMARY_SUCCESS":
      return {
        ...state,
        userYearSummary: action.yearSummary,
      };
    case "CLEAR_USER_YEAR_SUMMARY":
      return {
        ...state,
        userYearSummary: null,
      };
    case "FETCH_USER_DETAILS_SUCCESS":
      return {
        ...state,
        userDetails: action.userDetails,
        usersDetails: state.usersDetails
          ? [...state.usersDetails, action.userDetails]
          : [action.userDetails],
      };
    case "FETCH_MORE_USER_DETAILS_SUCCESS":
      return {
        ...state,
        usersDetails: state.usersDetails
          ? [...state.usersDetails, action.userDetails]
          : [action.userDetails],
      };
    case "NEW_ROUND_CREATED":
      return {
        ...state,
        userDetails: state.userDetails && {
          ...state.userDetails,
          activeRound: action.round.id,
        },
      };
    case "ROUND_WAS_DELETED":
      return {
        ...state,
        userDetails: state.userDetails && {
          ...state.userDetails,
          activeRound: null,
        },
      };
    case "FETCH_USER_ACHIEVEMENTS_SUCCESS":
      return {
        ...state,
        userAchievements: action.achievements,
      };
    case "SEARCH_USERS_SUCCESS":
      return {
        ...state,
        searchedUsers: action.users.filter(
          (u) => !state.userDetails?.friends.some((f) => f === u)
        ),
      };
    default:
      return state;
  }
};
