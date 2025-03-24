import React from "react";
import colors, { scoreColorStyle } from "../../colors";
import { HoleScore, PlayerCourseStats, Round } from "../../store/Rounds";
import RoundPredictions from "./RoundPredictions";
import HoleScoreIndicator from "./HoleScoreIndicator";

export interface HoleStatusProps {
  gotoNextHole: () => void;
  setEditHoleScore: (editHole: boolean) => void;
  editHoleScore: boolean | undefined;
  playersStats: PlayerCourseStats[];
  round: Round;
  activeHoleIndex: number;
  username: string;
}

const scoreText = (holeScores: HoleScore) => {
  if (holeScores.strokes === 0) return "-";
  if (holeScores.strokes === 1) return "Ace";
  switch (holeScores.relativeToPar) {
    case 0:
      return "Par";
    case -1:
      return "Birdie";
    case -2:
      return "Eagle";
    case 1:
      return "Bogey";
    case 2:
      return "Double Bogey";
    case 3:
      return "Triple Bogey";
    case 4:
      return "Quadruple Bogey";
    default:
      return holeScores.relativeToPar;
  }
};

export default ({
  gotoNextHole,
  editHoleScore,
  setEditHoleScore,
  playersStats,
  round,
  activeHoleIndex,
  username,
}: HoleStatusProps) => {
  const holeScores = round?.playerScores.map((p) => {
    return {
      username: p.playerName,
      scores: p.scores[activeHoleIndex || 0],
    };
  });
  const waitingForScores =
    holeScores &&
    holeScores.find((s) => s.username === username)?.scores?.strokes !== 0 &&
    holeScores.some((s) => s.scores && s.scores?.strokes === 0);

  const sortedScores = [...holeScores].sort(
    (a, b) => a.scores.strokes - b.scores.strokes
  );

  const playerStats = playersStats.find((s) => s.playerName === username);
  const playerScores = round.playerScores.find(
    (s) => s.playerName === username
  );

  if (!waitingForScores || editHoleScore || !holeScores || !playerScores) {
    return null;
  }

  return (
    <div className="modal is-active">
      <div className="modal-background"></div>
      <div className="modal-card">
        <div
          className="modal-card-body py-2 px-1"
          style={{ backgroundColor: colors.background }}
        >
          <header
            className="modal-card-head"
            style={{ backgroundColor: colors.background }}
          >
            <div className="modal-card-title has-text-centered">
              <h1 className="title is-4">
                Hole {sortedScores[0].scores.hole.number}
              </h1>
              <h2 className="subtitle">Waiting for other players...</h2>
            </div>
          </header>
          <div
            className="modal-card-body"
            style={{ backgroundColor: colors.background }}
          >
            <table
              className="table is-fullwidth has-text-centered"
              style={{ backgroundColor: colors.background }}
            >
              <tbody>
                {sortedScores.map((s) => (
                  <tr key={s.username}>
                    <td>{s.username}</td>
                    <td>
                      <HoleScoreIndicator holeScore={s.scores} />
                    </td>
                    <td>{scoreText(s.scores)}</td>
                  </tr>
                ))}
              </tbody>
            </table>
            <hr />
            {playerStats && (
              <RoundPredictions
                playerRoundScores={playerScores}
                playerStats={playerStats}
              />
            )}
            {/* Time projection has been moved to a separate dialog accessible from the clock badge in the Round component */}
          </div>

          <footer
            className="modal-card-foot is-flex is-flex-direction-row is-justify-content-space-evenly"
            style={{ backgroundColor: colors.background }}
          >
            <button
              className="button is-flex"
              onClick={() => setEditHoleScore(true)}
              style={{ backgroundColor: colors.button }}
            >
              Edit
            </button>
            <button
              className="button is-flex"
              onClick={() => gotoNextHole()}
              style={{ backgroundColor: colors.button }}
            >
              Next
            </button>
          </footer>
        </div>
      </div>
    </div>
  );
};
