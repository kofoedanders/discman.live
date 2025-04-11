import React, { useEffect, useState } from "react";
import { connect, ConnectedProps } from "react-redux";
import { ApplicationState } from "../../store";
import * as RoundsStore from "../../store/Rounds";
import { useParams } from "react-router";
import RoundScoreCard from "./RoundScoreCard";
import HoleScoreSelector from "./HoleScoreSelectorNew";
import WindowFocusHandler from "./WindowFocusHandler";
import RoundSummary from "./RoundSummary";
import HoleScore from "./HoleScoreNew";
import RoundScoreCardModal from "./RoundScoreCardModal";
import ScoreAnimations from "./ScoreAnimations";
import NavMenu from "../NavMenu";
import Colors from "../../colors";
import SignRound from "./SignRound";
import HoleStatus from "./HoleStatus";
import RoundTimeProjection from "./RoundTimeProjection";
import RoundTimeProjectionDialog from "./RoundTimeProjectionDialog";
import RoundPaceDialog from "./RoundPaceDialog";

const mapState = (state: ApplicationState) => {
  return {
    username: state.user?.userDetails?.username || "",
    round: state.rounds?.round,
    playersStats: state.rounds?.playerCourseStats || [],
    scoreCardOpen: state.rounds?.scoreCardOpen,
    activeHoleIndex: state.rounds?.activeHoleIndex || 0,
    finishedRoundStats: state.rounds?.finishedRoundStats || [],
    editHole: state.rounds?.editHole,
    roundTimeProjection: state.rounds?.roundTimeProjection,
    // Create new object references to ensure component updates
    paceData: state.rounds?.paceData ? { ...state.rounds.paceData } : undefined,
    currentPace: state.rounds?.currentPace ? {
      ...state.rounds.currentPace,
      estimatedFinishTime: state.rounds.currentPace.estimatedFinishTime ? 
        new Date(state.rounds.currentPace.estimatedFinishTime) : undefined
    } : undefined,
  };
};

const connector = connect(mapState, RoundsStore.actionCreators);

type PropsFromRedux = ConnectedProps<typeof connector>;

type Props = PropsFromRedux & {};

const toDateString = (date: Date) => {
  const dateTimeFormat = new Intl.DateTimeFormat("en", {
    year: "numeric",
    month: "long",
    day: "2-digit",
  });
  const [
    { value: month },
    ,
    { value: day },
    // { value: year },
    ,
  ] = dateTimeFormat.formatToParts(date);

  return `${day}. ${month.toLowerCase()}`;
};

const RoundComponent = (props: Props) => {
  const [showTimeProjectionDialog, setShowTimeProjectionDialog] = useState(false);
  const [showPaceDialog, setShowPaceDialog] = useState(false);
  const [localPaceState, setLocalPaceState] = useState<RoundsStore.PaceState | undefined>(props.currentPace);
  const {
    round,
    activeHoleIndex,
    fetchRound,
    fetchStatsOnCourse,
    fetchUserStats,
    finishedRoundStats,
    username,
    goToNextPersonalHole,
    editHole,
    setEditHole,
    playersStats,
    fetchActiveRoundTimeProjection,
    roundTimeProjection,
    fetchPaceData,
    currentPace,
    paceData,
  } = props;
  let { roundId } = useParams<{ roundId: string }>();
  const roundCompleted = round?.isCompleted;
  useEffect(() => {
    if (!roundId) return;
    fetchRound(roundId as string);
    fetchStatsOnCourse(roundId);
    roundCompleted && fetchUserStats(roundId);
    
    // Fetch time projection and pace data for active rounds
    if (!roundCompleted) {
      fetchActiveRoundTimeProjection();
      fetchPaceData(roundId);
    }
  }, [roundId, roundCompleted, fetchRound, fetchStatsOnCourse, fetchUserStats, fetchActiveRoundTimeProjection, fetchPaceData]);

  useEffect(() => {
    if (currentPace) {
      setLocalPaceState({...currentPace, 
        estimatedFinishTime: currentPace.estimatedFinishTime ? new Date(currentPace.estimatedFinishTime) : undefined
      });
    }
  }, [currentPace]);

  useEffect(() => {
    if (localPaceState && localPaceState.estimatedFinishTime) {
      setShowPaceDialog(showPaceDialog);
    }
  }, [localPaceState]);

  const allScoresSet = round?.playerScores.every((p) =>
    p.scores.every((s) => s.strokes !== 0)
  );
  if (!round) return null;

  const isPartOfRound = round?.playerScores.some(
    (s) => s.playerName === username
  );

  // Helper function to format time for display
  const formatTime = (date: Date): string => {
    return date.toLocaleTimeString([], { hour: '2-digit', minute: '2-digit' });
  };

  return (
    <div
      className="is-flex is-flex-direction-column "
      style={{ height: "100%" }}
    >
      <nav
        className="navbar is-light level is-mobile mb-0 is-flex"
        style={{ backgroundColor: Colors.navbar }}
      >
        <div className="level-item has-text-centered">
          <div className="is-size-7">
            {toDateString(new Date(round.startTime))} &nbsp;
          </div>
        </div>
        <div className="level-item has-text-centered">
          <div className="is-size-5">
            {`${round.courseName} ` || round?.roundName}
          </div>
        </div>
        <div className="level-item has-text-centered">
          {!roundCompleted && (
            <div 
              className="is-size-7 has-text-grey" 
              style={{ cursor: 'pointer' }}
              onClick={() => localPaceState && paceData ? setShowPaceDialog(true) : setShowTimeProjectionDialog(true)}
            >
              <span className="icon is-small mr-1">
                <i className="fas fa-clock"></i>
              </span>
              {/* Show the estimated finish time from either source, with preference for currentPace */}
              {localPaceState?.estimatedFinishTime 
                ? formatTime(localPaceState.estimatedFinishTime)
                : roundTimeProjection 
                  ? formatTime(new Date(roundTimeProjection.estimatedFinishTime))
                  : 'Calculating...'}
              {localPaceState?.isAhead !== undefined && (
                <span className={localPaceState.isAhead ? 'has-text-success ml-1' : 'has-text-danger ml-1'}>
                  <i className={`fas fa-${localPaceState.isAhead ? 'arrow-down' : 'arrow-up'}`}></i>
                </span>
              )}
            </div>
          )}
        </div>
        <div className="level-item has-text-centered">
          <NavMenu />
        </div>
      </nav>
      <>
        {roundCompleted || !isPartOfRound ? (
          <RoundSummary
            round={round}
            finishedRoundStats={finishedRoundStats}
            username={username || ""}
          />
        ) : (
          <>
            {activeHoleIndex === -1 && isPartOfRound ? ( //all holes registered, show scorecard + complete button
              <div className="has-text-centered pt-6 mx-1">
                <RoundScoreCard
                  username={username || ""}
                  round={round}
                  activeHole={activeHoleIndex}
                  setActiveHole={props.setActiveHole}
                  closeDialog={() => props.setScorecardOpen(false)}
                  playersStats={props.playersStats}
                />
                {allScoresSet && isPartOfRound && (
                  <SignRound
                    completeRound={props.completeRound}
                    signatures={round.signatures}
                    username={username || ""}
                  />
                )}
              </div>
            ) : (
              //regular live view, hole scores and score selctor
              <>
                <>
                  <HoleScore
                    username={username || ""}
                    round={round}
                    activeHole={activeHoleIndex}
                    setActiveHole={props.setActiveHole}
                    playersStats={props.playersStats}
                  />
                  <HoleScoreSelector />
                </>
                {
                  <HoleStatus
                    gotoNextHole={goToNextPersonalHole}
                    setEditHoleScore={setEditHole}
                    editHoleScore={editHole}
                    playersStats={playersStats}
                    round={round}
                    activeHoleIndex={activeHoleIndex}
                    username={username}
                  />
                }
                
              </>
            )}
            {props.scoreCardOpen && (
              <RoundScoreCardModal
                username={username || ""}
                round={round}
                activeHole={activeHoleIndex}
                setActiveHole={props.setActiveHole}
                closeDialog={() => props.setScorecardOpen(false)}
                playersStats={props.playersStats}
              />
            )}
            <WindowFocusHandler />
            <ScoreAnimations />
          </>
        )}
      </>
      {showTimeProjectionDialog && roundTimeProjection && (
        <RoundTimeProjectionDialog
          timeProjection={roundTimeProjection}
          onClose={() => setShowTimeProjectionDialog(false)}
        />
      )}
      {showPaceDialog && paceData && localPaceState && (
        <RoundPaceDialog
          paceData={paceData}
          currentPace={localPaceState}
          username={username || ""}
          onClose={() => setShowPaceDialog(false)}
        />
      )}
    </div>
  );
};

export default connector(RoundComponent);
