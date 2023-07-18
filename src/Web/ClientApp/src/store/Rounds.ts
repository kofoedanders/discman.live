/* eslint-disable @typescript-eslint/no-use-before-define */
import { Action, Reducer } from "redux";
import { AppThunkAction } from "./";
import { push, CallHistoryMethodAction } from "connected-react-router";
import { hub } from "./configureStore";
import * as signalR from "@microsoft/signalr";
import { actionCreators as notificationActions } from "./Notifications";
import {
  actionCreators as UserActions,
  UserAchievement,
  UserStats,
  User,
} from "./User";

export interface Hole {
  number: number;
  par: number;
  distance: number;
  rating: number;
  average: number;
}
export interface StrokeSpec {
  outcome: StrokeOutcome;
}

export type StrokeOutcome =
  | "Fairway"
  | "Rough"
  | "OB"
  | "Circle2"
  | "Circle1"
  | "Basket";

export enum ScoreMode {
  DetailedLive = 0,
  StrokesLive = 1,
  OneForAll = 2,
}

export interface PlayerScore {
  playerName: string;
  playerEmoji: string;
  courseAverageAtTheTime: number;
  numberOfHcpStrokes: number;
  scores: HoleScore[];
}

export interface PlayerCourseStats {
  courseName: string;
  layoutName: string;
  playerName: string;
  courseAverage: number;
  thisRoundVsAverage: number;
  playerCourseRecord: number;
  holeAverages: number[];
  averagePrediction: number[];
  roundsPlayed: string;
  holeStats: HoleStats[];
}

export interface HoleStats {
  holeNumber: number;
  bestScore: HoleScore;
  averageScore: number;
  birdie: boolean;
  birdies: number;
  pars: number;
  worseThanPar: number;
  last10Scores: HoleScore[];
}

export interface PlayerRoundProgression {
  courseAverage: number;
  holeAverages: [number, number][];
  averagePrediction: [number, number][];
}

export interface HoleScore {
  hole: Hole;
  strokes: number;
  relativeToPar: number;
  strokeSpecs: StrokeSpec[];
}

export interface Round {
  id: string;
  courseName: string;
  courseLayout: string;
  roundName: string;
  createdBy: string;
  startTime: string;
  completedAt: string;
  roundDuration: number;
  isCompleted: boolean;
  scoreMode: ScoreMode;
  playerScores: PlayerScore[];
  signatures: PlayerSignature[];
  achievements: UserAchievement[];
  spectators: string[];
  ratingChanges: RatingChange[];
}

export interface RatingChange {
  change: number;
  username: string;
}

export interface PlayerSignature {
  username: string;
  base64Signature: string;
  signedAt: Date;
}

export interface RoundsState {
  rounds: Round[];
  round: Round | null;
  activeHoleIndex: number;
  playerCourseStats: PlayerCourseStats[] | null;
  scoreCardOpen: boolean;
  finishedRoundStats: UserStats[];
  editHole: boolean;
}

//Actions
export interface FetchRoundsSuccessAction {
  type: "FETCH_ROUNDS_SUCCEED";
  rounds: Round[];
}

export interface GoToNextPersonalHoleAction {
  type: "GOTO_NEXT_PERSONAL_HOLE";
  username: string;
}

export interface FetchRoundSuccessAction {
  type: "FETCH_ROUND_SUCCEED";
  round: Round;
  username: string;
}

export interface NewRoundCreatedAction {
  type: "NEW_ROUND_CREATED";
  round: Round;
}

export interface RoundWasUpdatedAction {
  type: "ROUND_WAS_UPDATED";
  round: Round;
  username: string;
}

export interface ScoreUpdatedSuccessAction {
  type: "SCORE_UPDATED_SUCCESS";
  round: Round;
  username: string;
}

export interface FetchRoundStatsSuccessAction {
  type: "FETCH_ROUND_STATS_SUCCESS";
  userStats: UserStats[];
}

export interface SpectatorJoinedAction {
  type: "SPECTATOR_JOINED";
  roundId: string;
  username: string;
}

export interface SpectatorLeftAction {
  type: "SPECTATOR_LEFT";
  roundId: string;
  username: string;
}

export interface SetActiveHoleAction {
  type: "SET_ACTIVE_HOLE";
  holeIndex: number;
}

export interface CourseWasSavedAction {
  type: "COURSE_WAS_SAVED";
}

export interface HoleWasDeletedAction {
  type: "HOLE_WAS_DELETED";
  holeNumber: number;
}

export interface RoundWasCompletedAction {
  type: "ROUND_WAS_COMPLETED";
}

export interface RoundWasDeletedAction {
  type: "ROUND_WAS_DELETED";
  roundId: string;
}

export interface ConnectToHubAction {
  type: "CONNECT_TO_HUB";
}

export interface DisconnectToHubAction {
  type: "DISCONNECT_TO_HUB";
}
export interface PlayerCourseStatsFethSuceed {
  type: "FETCH_COURSE_STATS_SUCCEED";
  stats: PlayerCourseStats[];
}

export interface ToggleScoreCardAction {
  type: "TOGGLE_SCORECARD";
  open: boolean;
}

export interface SetEditHoleAction {
  type: "SET_EDIT_HOLE";
  editHole: boolean;
}

export type KnownAction =
  | FetchRoundsSuccessAction
  | FetchRoundSuccessAction
  | ScoreUpdatedSuccessAction
  | NewRoundCreatedAction
  | HoleWasDeletedAction
  | CallHistoryMethodAction
  | ConnectToHubAction
  | DisconnectToHubAction
  | RoundWasUpdatedAction
  | SetActiveHoleAction
  | RoundWasCompletedAction
  | PlayerCourseStatsFethSuceed
  | ToggleScoreCardAction
  | SpectatorJoinedAction
  | RoundWasDeletedAction
  | SpectatorLeftAction
  | FetchRoundStatsSuccessAction
  | GoToNextPersonalHoleAction
  | SetEditHoleAction
  | CourseWasSavedAction;

const fetchRound = (
  roundId: string,
  user: User,
  dispatch: (action: KnownAction) => void
) => {
  fetch(`api/rounds/${roundId}`, {
    method: "GET",
    headers: {
      "Content-Type": "application/json",
      Authorization: `Bearer ${user.token}`,
    },
  })
    .then((res) => {
      if (res.status === 401) {
        UserActions.logout();
      }
      if (!res.ok) throw new Error(`${res.status} - ${res.statusText}`);
      return res;
    })
    .then((response) => response.json() as Promise<Round>)
    .then((data) => {
      dispatch({
        type: "FETCH_ROUND_SUCCEED",
        round: data,
        username: user.username,
      });
      dispatch({ type: "CONNECT_TO_HUB" });
    })
    .catch((err: Error) => {
      notificationActions.showNotification(
        `Fetch rounds failed: ${err.message}`,
        "error",
        dispatch
      );
    });
};

const initialState: RoundsState = {
  rounds: [],
  round: null,
  activeHoleIndex: 0,
  playerCourseStats: null,
  scoreCardOpen: false,
  finishedRoundStats: [],
  editHole: false,
};

export const actionCreators = {
  setEditHole:
    (editHole: boolean): AppThunkAction<KnownAction> =>
    (dispatch) => {
      dispatch({ type: "SET_EDIT_HOLE", editHole });
    },
  setScorecardOpen:
    (open: boolean): AppThunkAction<KnownAction> =>
    (dispatch) => {
      dispatch({ type: "TOGGLE_SCORECARD", open });
    },
  roundWasUpdated: (round: Round) => {
    return { type: "ROUND_WAS_UPDATED", round: round };
  },
  roundWasCreated: (round: Round) => {
    return { type: "NEW_ROUND_CREATED", round: round };
  },
  specJoined: (roundId: string, username: string) => {
    return { type: "SPECTATOR_JOINED", roundId, username };
  },
  specLeft: (roundId: string, username: string) => {
    return { type: "SPECTATOR_LEFT", roundId, username };
  },
  fetchLastRounds:
    (numberOfRounds?: number): AppThunkAction<KnownAction> =>
    (dispatch, getState) => {
      const appState = getState();
      if (!appState.user || !appState.user.loggedIn || !appState.user.user)
        return;
      const username = appState.user.user.username;
      fetch(`api/rounds?username=${username}&count=${numberOfRounds || 5}`, {
        method: "GET",
        headers: {
          "Content-Type": "application/json",
          Authorization: `Bearer ${appState.user.user.token}`,
        },
      })
        .then((res) => {
          if (res.status === 401) {
            UserActions.logout()(dispatch);
          }
          if (!res.ok) throw new Error(`${res.status} - ${res.statusText}`);
          return res;
        })
        .then((response) => response.json() as Promise<Round[]>)
        .then((data) => {
          dispatch({
            type: "FETCH_ROUNDS_SUCCEED",
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
  fetchUserStats:
    (roundId: string): AppThunkAction<KnownAction> =>
    (dispatch, getState) => {
      const appState = getState();
      if (!appState.user || !appState.user.loggedIn || !appState.user.user)
        return;
      fetch(`api/rounds/${roundId}/stats`, {
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
        .then((response) => response.json() as Promise<UserStats[]>)
        .then((data) => {
          dispatch({
            type: "FETCH_ROUND_STATS_SUCCESS",
            userStats: data,
          });
        })
        .catch((err: Error) => {
          notificationActions.showNotification(
            `Fetch round stats failed: ${err.message}`,
            "error",
            dispatch
          );
        });
    },
  fetchStatsOnCourse:
    (roundId: string): AppThunkAction<KnownAction> =>
    (dispatch, getState) => {
      const appState = getState();
      if (!appState.user || !appState.user.loggedIn || !appState.user.user)
        return;
      fetch(`api/rounds/${roundId}/courseStats`, {
        method: "GET",
        headers: {
          "Content-Type": "application/json",
          Authorization: `Bearer ${appState.user.user.token}`,
        },
      })
        .then((res) => {
          if (res.status === 401) {
            UserActions.logout()(dispatch);
          }
          if (!res.ok) throw new Error(`${res.status} - ${res.statusText}`);
          return res;
        })
        .then((response) => response.json() as Promise<PlayerCourseStats[]>)
        .then((data) => {
          dispatch({
            type: "FETCH_COURSE_STATS_SUCCEED",
            stats: data,
          });
        })
        .catch((err: Error) => {
          notificationActions.showNotification(
            `Fetch course stats failed: ${err.message}`,
            "error",
            dispatch
          );
        });
    },
  fetchRound:
    (roundId: string): AppThunkAction<KnownAction> =>
    (dispatch, getState) => {
      const appState = getState();
      if (!appState.user || !appState.user.loggedIn || !appState.user.user)
        return;
      fetchRound(roundId, appState.user.user, dispatch);
    },
  refreshRound: (): AppThunkAction<KnownAction> => (dispatch, getState) => {
    const appState = getState();
    const activeRound = appState.rounds?.round?.id;
    if (!appState.user || !appState.user.loggedIn || !appState.user.user)
      return;

    activeRound &&
      hub.state !== signalR.HubConnectionState.Connected &&
      fetchRound(activeRound, appState.user.user, dispatch);
  },
  dissconnectHub: (): AppThunkAction<KnownAction> => (dispatch, getState) => {
    dispatch({ type: "DISCONNECT_TO_HUB" });
  },
  newRound:
    (
      courseId: string | undefined,
      players: string[],
      roundName: string,
      scoreMode: ScoreMode
    ): AppThunkAction<KnownAction> =>
    (dispatch, getState) => {
      const appState = getState();
      if (!appState.user || !appState.user.loggedIn || !appState.user.user)
        return;
      const username = appState.user.user.username;
      if (!players.some((p) => p === username)) {
        players = [...players, username];
      }
      fetch(`api/rounds`, {
        method: "POST",
        headers: {
          "Content-Type": "application/json",
          Authorization: `Bearer ${appState.user.user.token}`,
        },
        body: JSON.stringify({
          courseId: courseId,
          players: players,
          roundName,
          scoreMode,
        }),
      })
        .then((response) => {
          if (response.status === 401) {
            UserActions.logout()(dispatch);
          }
          if (response.status === 409) {
            window.alert(
              "A round with you in it was just started, redirecting to that round"
            );
          }
          if (!response.ok && response.status !== 409)
            throw new Error(`${response.status} - ${response.statusText}`);
          return response.json() as Promise<Round>;
        })
        .then((data) => {
          dispatch({
            type: "NEW_ROUND_CREATED",
            round: data,
          });
          dispatch(push(`/rounds/${data.id}`));
        })
        .catch((err: Error) => {
          notificationActions.showNotification(
            `Create round failed: ${err.message}`,
            "error",
            dispatch
          );
        });
    },
  addHole:
    (
      holeNumber: number,
      par: number,
      length: number
    ): AppThunkAction<KnownAction> =>
    (dispatch, getState) => {
      const appState = getState();
      if (!appState.user || !appState.user.loggedIn || !appState.user.user)
        return;
      const roundId = appState.rounds?.round?.id;
      fetch(`api/rounds/${roundId}/holes`, {
        method: "POST",
        headers: {
          "Content-Type": "application/json",
          Authorization: `Bearer ${appState.user.user.token}`,
        },
        body: JSON.stringify({ holeNumber, par, length }),
      })
        .then((response) => {
          if (!response.ok)
            throw new Error(`${response.status} - ${response.statusText}`);
          return response.json() as Promise<Round>;
        })
        .then((data) => {})
        .catch((err: Error) => {
          notificationActions.showNotification(
            `Add hole failed: ${err.message}`,
            "error",
            dispatch
          );
        });
    },

  roundWasDeleted: (roundId: string) => {
    return { type: "ROUND_WAS_DELETED", roundId: roundId };
  },
  deleteRound:
    (roundId: string): AppThunkAction<KnownAction> =>
    (dispatch, getState) => {
      const appState = getState();
      if (!appState.user || !appState.user.loggedIn || !appState.user.user)
        return;

      fetch(`api/rounds/${roundId}`, {
        method: "DELETE",
        headers: {
          "Content-Type": "application/json",
          Authorization: `Bearer ${appState.user.user.token}`,
        },
      })
        .then((res) => {
          if (res.status === 401) {
            UserActions.logout()(dispatch);
          }
          if (!res.ok) throw new Error(`${res.status} - ${res.statusText}`);
          return res;
        })
        .then((response) => {
          dispatch(push("/"));
          dispatch({ type: "ROUND_WAS_DELETED", roundId });
        })
        .catch((err: Error) => {
          notificationActions.showNotification(
            `Delete round failed: ${err.message}`,
            "error",
            dispatch
          );
        });
    },
  skipHole:
    (roundId: string): AppThunkAction<KnownAction> =>
    (dispatch, getState) => {
      const appState = getState();
      if (!appState.user || !appState.user.loggedIn || !appState.user.user)
        return;

      const activeHoleIndex = appState.rounds?.activeHoleIndex;
      if (activeHoleIndex === null || activeHoleIndex === undefined) return;
      const holeNumber =
        appState.rounds?.round?.playerScores[0].scores[activeHoleIndex].hole
          .number;
      if (!holeNumber || !roundId) return;
      fetch(`api/rounds/${roundId}/holes/${holeNumber}`, {
        method: "DELETE",
        headers: {
          "Content-Type": "application/json",
          Authorization: `Bearer ${appState.user.user.token}`,
        },
      })
        .then((res) => {
          if (!res.ok) throw new Error(`${res.status} - ${res.statusText}`);
          dispatch({ type: "HOLE_WAS_DELETED", holeNumber });
          return res;
        })
        .catch((err: Error) => {
          notificationActions.showNotification(
            `Delete hole failed: ${err.message}`,
            "error",
            dispatch
          );
        });
    },
  leaveRound:
    (roundId: string): AppThunkAction<KnownAction> =>
    (dispatch, getState) => {
      const appState = getState();
      if (!appState.user || !appState.user.loggedIn || !appState.user.user)
        return;

      if (!roundId) return;
      fetch(`api/rounds/${roundId}/users`, {
        method: "DELETE",
        headers: {
          "Content-Type": "application/json",
          Authorization: `Bearer ${appState.user.user.token}`,
        },
      })
        .then((res) => {
          if (!res.ok) throw new Error(`${res.status} - ${res.statusText}`);
          dispatch(push("/"));
          return res;
        })
        .catch((err: Error) => {
          notificationActions.showNotification(
            `Leave round failed: ${err.message}`,
            "error",
            dispatch
          );
        });
    },
  setScore:
    (score: number, strokes: StrokeOutcome[]): AppThunkAction<KnownAction> =>
    (dispatch, getState) => {
      const appState = getState();
      const loggedInUser = appState?.user?.user;

      const round = appState.rounds?.round;
      const holeIndex = appState.rounds?.activeHoleIndex;
      if (!loggedInUser || !round || holeIndex === undefined || holeIndex < 0)
        return;

      const playerScores = round.playerScores.find(
        (p) => p.playerName === loggedInUser.username
      );
      const holeScore = playerScores && playerScores.scores[holeIndex];
      if (holeScore && holeScore.strokes !== 0) {
        const goOn = window.confirm(
          "You are overwriting an existing score, continue?"
        );
        if (!goOn) return;
      }

      fetch(`api/rounds/${round.id}/scores`, {
        method: "PUT",
        headers: {
          "Content-Type": "application/json",
          Authorization: `Bearer ${loggedInUser.token}`,
        },
        body: JSON.stringify({
          holeIndex: holeIndex,
          strokes: score,
          strokeOutcomes: strokes,
          username: loggedInUser.username,
        }),
      })
        .then((res) => {
          if (res.status === 401) {
            UserActions.logout()(dispatch);
          }
          if (!res.ok) throw new Error(`${res.status} - ${res.statusText}`);
          return res;
        })
        .then((response) => response.json() as Promise<Round>)
        .then((data) => {
          dispatch({
            type: "SCORE_UPDATED_SUCCESS",
            round: data,
            username: loggedInUser.username,
          });
        })
        .catch((err: Error) => {
          notificationActions.showNotification(
            `Set score failed: ${err.message}`,
            "error",
            dispatch
          );
        });
    },
  setScoringMode:
    (mode: ScoreMode): AppThunkAction<KnownAction> =>
    (dispatch, getState) => {
      const appState = getState();
      const loggedInUser = appState?.user?.user;
      const roundId = appState.rounds?.round?.id;
      if (!loggedInUser || !roundId) return;

      fetch(`api/rounds/${roundId}/scoremode`, {
        method: "PUT",
        headers: {
          "Content-Type": "application/json",
          Authorization: `Bearer ${loggedInUser.token}`,
        },
        body: JSON.stringify({ scoreMode: mode }),
      })
        .then((res) => {
          if (res.status === 401) {
            UserActions.logout()(dispatch);
          }
          if (!res.ok) throw new Error(`${res.status} - ${res.statusText}`);
          return res;
        })
        .then((response) => {})
        .catch((err: Error) => {
          notificationActions.showNotification(
            `Set scoring mode failed: ${err.message}`,
            "error",
            dispatch
          );
        });
    },
  completeRound:
    (base64Signature: string): AppThunkAction<KnownAction> =>
    (dispatch, getState) => {
      const appState = getState();
      const loggedInUser = appState?.user?.user;

      const roundId = appState.rounds?.round?.id;
      if (!loggedInUser || !roundId) return;

      fetch(`api/rounds/${roundId}/complete`, {
        method: "PUT",
        headers: {
          "Content-Type": "application/json",
          Authorization: `Bearer ${loggedInUser.token}`,
        },
        body: JSON.stringify({ base64Signature }),
      })
        .then((res) => {
          if (res.status === 401) {
            UserActions.logout()(dispatch);
          }
          if (!res.ok) throw new Error(`${res.status} - ${res.statusText}`);
          return res;
        })
        .then((response) => {
          if (response.ok) dispatch({ type: "ROUND_WAS_COMPLETED" });
        })
        .catch((err: Error) => {
          notificationActions.showNotification(
            `Complete round failed: ${err.message}`,
            "error",
            dispatch
          );
        });
    },
  saveAsCourse:
    (courseName: string): AppThunkAction<KnownAction> =>
    (dispatch, getState) => {
      const appState = getState();
      const loggedInUser = appState?.user?.user;
      const roundId = appState.rounds?.round?.id;
      if (!loggedInUser || !roundId) return;

      fetch(`api/rounds/${roundId}/savecourse`, {
        method: "POST",
        headers: {
          "Content-Type": "application/json",
          Authorization: `Bearer ${loggedInUser.token}`,
        },
        body: JSON.stringify({ courseName }),
      })
        .then((res) => {
          if (!res.ok) throw new Error(`${res.status} - ${res.statusText}`);
          return res;
        })
        .then((response) => {
          if (response.ok) dispatch({ type: "COURSE_WAS_SAVED" });
        })
        .catch((err: Error) => {
          notificationActions.showNotification(
            `Complete round failed: ${err.message}`,
            "error",
            dispatch
          );
        });
    },
  setActiveHole:
    (holeIndex: number): AppThunkAction<KnownAction> =>
    (dispatch) => {
      dispatch({ type: "SET_ACTIVE_HOLE", holeIndex: holeIndex });
    },
  goToNextPersonalHole:
    (): AppThunkAction<KnownAction> => (dispatch, getState) => {
      const appState = getState();
      const loggedInUser = appState?.user?.user;
      loggedInUser &&
        dispatch({
          type: "GOTO_NEXT_PERSONAL_HOLE",
          username: loggedInUser.username,
        });
    },
};

//wait for all to score
const getNextUncompletedHole = (round: Round, user: string) => {
  const activeHole = round.playerScores
    .map((p) => p.scores.find((s) => s.strokes === 0))
    .sort((a, b) => {
      return a && b ? a.hole.number - b.hole.number : 0;
    })
    .find(() => true);

  const activeHoleIndex =
    activeHole &&
    round.playerScores[0].scores.findIndex(
      (x) => x.hole.number === activeHole.hole.number
    );

  return activeHoleIndex !== undefined ? activeHoleIndex : -1;
};

const getNextPlayerHole = (round: Round, user: string) => {
  const holeScores =
    round.playerScores.find((p) => p.playerName === user)?.scores ||
    ([] as HoleScore[]);
  const activeHole = holeScores.find((s) => s.strokes === 0);
  const activeHoleIndex =
    activeHole &&
    round.playerScores[0].scores.findIndex(
      (x) => x.hole.number === activeHole.hole.number
    );
  return activeHoleIndex !== undefined ? activeHoleIndex : -1;
  // : round.playerScores[0].scores.length - 1;
};

export const reducer: Reducer<RoundsState> = (
  state: RoundsState | undefined,
  incomingAction: Action
): RoundsState => {
  if (state === undefined) {
    return initialState;
  }

  const action = incomingAction as KnownAction;
  switch (action.type) {
    case "ROUND_WAS_UPDATED":
      if (state.round?.id !== action.round.id) return state;
      return {
        ...state,
        round: action.round,
        //if holes has been shifted on the server, the activeHoleIndex is no longer valid
        // activeHoleIndex:
        //   action.round.playerScores[0].scores[0].hole.number !==
        //   state.round.playerScores[0].scores[0].hole.number
        //     ? getActiveHole(action.round, action.username)
        //     : state.activeHoleIndex,
        activeHoleIndex: getNextUncompletedHole(action.round, action.username),
      };
    case "TOGGLE_SCORECARD":
      return { ...state, scoreCardOpen: action.open };
    case "SET_EDIT_HOLE":
      return { ...state, editHole: action.editHole };
    case "FETCH_ROUNDS_SUCCEED":
      return { ...state, rounds: action.rounds };
    case "FETCH_ROUND_SUCCEED":
      return {
        ...state,
        round: action.round,
        activeHoleIndex: getNextUncompletedHole(action.round, action.username),
      };
    case "NEW_ROUND_CREATED":
      return { ...state, round: action.round };
    case "FETCH_ROUND_STATS_SUCCESS":
      return { ...state, finishedRoundStats: action.userStats };
    case "SCORE_UPDATED_SUCCESS":
      return {
        ...state,
        round: action.round,
        activeHoleIndex: getNextUncompletedHole(action.round, action.username),
        editHole: false,
      };

    case "GOTO_NEXT_PERSONAL_HOLE":
      return {
        ...state,
        activeHoleIndex: state.round
          ? getNextPlayerHole(state.round, action.username)
          : state.activeHoleIndex,
      };
    case "ROUND_WAS_COMPLETED":
      if (!state.round) return state;
      return {
        ...state,
        round: { ...state.round, isCompleted: true },
      };
    case "HOLE_WAS_DELETED":
      if (!state.round) return state;
      return {
        ...state,
        round: {
          ...state.round,
          playerScores: state.round?.playerScores.map((p) => {
            return {
              ...p,
              scores: p.scores.filter(
                (s) => s.hole.number !== action.holeNumber
              ),
            };
          }),
        },
      };
    case "ROUND_WAS_DELETED":
      if (state.round?.id === action.roundId) {
        return initialState;
      }
      return state;
    case "SET_ACTIVE_HOLE":
      // if (action.hole > nextHole) return state;
      return {
        ...state,
        activeHoleIndex: action.holeIndex,
      };
    case "FETCH_COURSE_STATS_SUCCEED":
      return {
        ...state,
        playerCourseStats: action.stats,
      };
    default:
      return state;
  }
};
