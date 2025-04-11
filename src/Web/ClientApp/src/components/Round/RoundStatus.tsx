import React, { useState } from "react";
import { connect, ConnectedProps } from "react-redux";
import { ApplicationState } from "../../store";
import * as RoundsStore from "../../store/Rounds";
import { Chart } from "react-charts";
import InformationDialogue from "../InformationDialogue";
import colors from "../../colors";
import RoundPredictions from "./RoundPredictions";

const mapState = (state: ApplicationState) => {
  return {
    user: state.user,
    round: state.rounds?.round,
    stats: state.rounds?.playerCourseStats,
  };
};

const connector = connect(mapState, RoundsStore.actionCreators);

type PropsFromRedux = ConnectedProps<typeof connector>;

type Props = PropsFromRedux & {};

const RoundStatus = (props: Props) => {
  const { stats, user, round } = props;
  const [showDialog, setShowDialog] = useState(false);
  const [showPlayer] = useState(user?.user?.username);

  interface PlayerStat {
    playerName: string;
    averagePrediction: number[];
    courseAverage: number;
    playerCourseRecord: number;
    roundsPlayed: number;
  }

  interface PlayerScore {
    playerName: string;
    scores: Score[];
  }

  interface Score {
    hole: { number: number };
    relativeToPar: number;
    strokes: number;
  }

  const playerStats = stats && stats.find((s: PlayerStat) => s.playerName === showPlayer);

  const playerRoundScores =
    round && round.playerScores.find((s: PlayerScore) => s.playerName === showPlayer);

  const roundScores = playerRoundScores && playerRoundScores?.scores;

  const data = React.useMemo(() => {
    let total = 0;
    return (
      playerStats && [
        {
          label: "Current",
          data:
            roundScores &&
            [[0, 0]].concat(
              roundScores
                .filter((s: Score) => s.strokes !== 0)
                .map((s: Score) => {
                  total += s.relativeToPar;
                  return [s.hole.number, total];
                })
            ),
        },
        {
          label: "Average Progression",
          data: [[0, 0]].concat(
            playerStats.averagePrediction.map((s: number, i: number) => [i + 1, s])
          ),
        },
      ]
    );
  }, [playerStats, roundScores]);
  const axes = React.useMemo(
    () => [
      { primary: true, type: "linear", position: "bottom" },
      { type: "linear", position: "left" },
    ],
    []
  );

  if (!playerRoundScores || !playerStats || +playerStats.roundsPlayed < 1)
    return null;

  return (
    <>
      {showDialog && (
        <div className={showDialog ? "modal is-active" : "modal"}>
          <div onClick={() => setShowDialog(false)}>
            <div className="modal-background"></div>
          </div>
          <div className="modal-card">
            {/* <header className="modal-card-head">
              <p className="modal-card-title">Round Status</p>
            </header> */}
            <section
              className="modal-card-body px-2"
              style={{ backgroundColor: colors.background }}
            >
              <RoundPredictions
                playerStats={playerStats}
                playerRoundScores={playerRoundScores}
              />
              <div className="columns is-centered is-mobile mb-0 pb-0">
                <span className="column has-text-centered">
                  <h6 className="title is-6">Course Average</h6>
                  {playerStats?.courseAverage.toFixed(1)}
                </span>
                <span className="column has-text-centered">
                  <h6 className="title is-6">Course Record</h6>
                  {playerStats?.playerCourseRecord}
                </span>
              </div>
              <div className="has-text-centered pt-0 mt-0">
                <span className="title is-6">Round Progression</span>
                <InformationDialogue
                  title="Round Status"
                  text={`Shows how the current round compares to your previous rounds on the course.

The blue line shows your average round progression based on averages on each hole. The red line is the current round.`}
                />{" "}
              </div>
              <div style={{ width: "350px", height: "200px" }}>
                <Chart data={data} axes={axes} tooltip />
              </div>
            </section>
            {/* <footer className="modal-card-foot">
              <button className="button" onClick={() => setShowDialog(false)}>
                Close
              </button>
              {playersInRound && (
                <div className="field is-grouped">
                  <div className="control">
                    <div className="select is-primary">
                      <select
                        value={showPlayer}
                        onChange={(e) => setShowPlayer(e.target.value)}
                      >
                        {playersInRound.map((u) => (
                          <option key={u}>{u}</option>
                        ))}
                      </select>
                    </div>
                  </div>
                </div>
              )}
            </footer> */}
          </div>
        </div>
      )}

      <a
        className="button pr-1 pl-3 is-small"
        onClick={() => setShowDialog(true)}
        style={{ backgroundColor: colors.button }}
      >
        <span className="icon">
          <i className="fas fa-lg fa-chart-line" aria-hidden="true"></i>
        </span>
        <span className="is-size-7">Status</span>
      </a>
    </>
  );
};

export default connector(RoundStatus);
