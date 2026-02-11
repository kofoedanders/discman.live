import React, { useState } from "react";
import { Link } from "react-router-dom";
import { Round, PlayerCourseStats } from "../../store/Rounds";
import colors from "../../colors";
import HoleScoreIndicator from "./HoleScoreIndicator";
import SmallInfoWithHeader from "./SmallInfoWithHeader";
import { emojiDescriptions } from "../../utils/emojiDescriptions";

export interface ScoreCardProps {
  username: string;
  round: Round;
  activeHole: number;
  setActiveHole: (hole: number) => void;
  playersStats: PlayerCourseStats[];
}

const HoleScoreComponent = ({
  username,
  round,
  activeHole,
  setActiveHole,
  playersStats,
}: ScoreCardProps) => {
  const [activeTooltip, setActiveTooltip] = useState<string | null>(null);
  const activePlayerScores = round.playerScores.find(
    (p) => p.playerName === username
  );
  if (!activePlayerScores) return null;
  const playerScores =
    activePlayerScores?.scores || round.playerScores[0].scores;

  const holeScore = playerScores[activeHole];

  const playersHoleStats = playersStats.map((p) => {
    return {
      name: p.playerName,
      holeStats: p.holeStats[activeHole],
    };
  });

  const playerHoleStats = playersHoleStats.find(
    (x) => x.name === username
  )?.holeStats;

  const courseHoles = playerScores;
  const currentHole = courseHoles[activeHole];
  const nextHole =
    courseHoles.length - 1 >= activeHole + 1
      ? courseHoles[activeHole + 1]
      : null;
  const prevHole = activeHole - 1 > -1 ? courseHoles[activeHole - 1] : null;

  const playersToDisplay = round.playerScores; //consider not showing all players

  return (
    <div className="pt-1 pb-0">
      <div className="tour-stats">
        <div className="columns is-centered is-mobile">
          <span className="column has-text-centered pb-0">
            <SmallInfoWithHeader title="Hole" value={holeScore?.hole.number} />
          </span>
          <span className="column has-text-centered pb-0">
            <SmallInfoWithHeader title="Par" value={holeScore?.hole.par} />
          </span>
          <span className="column has-text-centered  pb-0">
            <SmallInfoWithHeader
              title="avg"
              value={holeScore?.hole.average.toFixed(1)}
            />
          </span>
          <span className="column has-text-centered pb-0">
            <SmallInfoWithHeader title="rank" value={holeScore?.hole.rating} />
          </span>
        </div>
      </div>
      <div>
        <div className="columns is-centered is-mobile">
          <span className="column has-text-centered pb-0">
            <SmallInfoWithHeader
              title="best"
              small
              value={
                <HoleScoreIndicator
                  holeScore={playerHoleStats?.bestScore}
                  tight
                />
              }
            />
          </span>
          <span className="column has-text-centered pb-0">
            <SmallInfoWithHeader
              title="avg"
              small
              value={playerHoleStats?.averageScore.toFixed(1)}
            />
          </span>
          <span className="column has-text-centered pb-0">
            {playerHoleStats?.last10Scores.length && (
              <>
                <SmallInfoWithHeader
                  title="prev"
                  small
                  value={
                    <>
                      {playerHoleStats?.last10Scores.slice(0, 5).map((x, i) => {
                        return (
                          <HoleScoreIndicator holeScore={x} key={i} tight />
                        );
                      })}
                    </>
                  }
                />
              </>
            )}
          </span>
        </div>
      </div>
      <br />
      <div className="table-container">
        <table
          className="table is-fullwidth is-narrower"
          style={{ backgroundColor: colors.table }}
        >
          <thead>
            <tr>
              <td></td>
              <td></td>
              {prevHole && (
                <th
                  className="has-text-centered px-2"
                  onClick={() => setActiveHole(activeHole - 1)}
                >
                  {prevHole.hole.number}
                </th>
              )}
              <th className="has-text-centered bordered-cell px-4">
                {currentHole.hole.number}
              </th>
              {nextHole && (
                <th
                  className="has-text-centered px-2"
                  onClick={() => setActiveHole(activeHole + 1)}
                >
                  {nextHole.hole.number}
                </th>
              )}
            </tr>
          </thead>
          <tbody>
            {playersToDisplay.map((p, i) => {
              const currentHole = p.scores[activeHole];
              const nextHole =
                p.scores.length - 1 >= activeHole + 1
                  ? p.scores[activeHole + 1]
                  : null;
              const prevHole =
                activeHole - 1 > -1 ? p.scores[activeHole - 1] : null;

              return (
                <tr
                  key={i}
                  className={p.playerName === username ? "active-user-row" : ""}
                >
                  <td className="emoji-cell">
                    <div className="emoji-tooltip-container" style={{ position: "relative" }}>
                      <span 
                        className="emoji-icon" 
                        onClick={() => setActiveTooltip(activeTooltip === p.playerName ? null : p.playerName)}
                        style={{ cursor: "pointer" }}
                      >
                        {p.playerRoundStatusEmoji}
                      </span>
                      {activeTooltip === p.playerName && p.playerRoundStatusEmoji && (
                        <div 
                          className="emoji-tooltip" 
                          style={{
                            position: "absolute",
                            left: "100%",
                            top: "-5px",
                            backgroundColor: "#333",
                            color: "white",
                            padding: "5px 10px",
                            borderRadius: "4px",
                            zIndex: 10,
                            whiteSpace: "nowrap",
                            fontSize: "0.9rem",
                            boxShadow: "0 2px 5px rgba(0,0,0,0.2)"
                          }}
                        >
                          {emojiDescriptions[p.playerRoundStatusEmoji] || "Player status"}
                        </div>
                      )}
                    </div>
                  </td>
                  <td
                    style={{
                      minWidth: "75px",
                      maxWidth: "75px",
                      overflow: "hidden",
                    }}
                    className="has-text-centered"
                  >
                    &nbsp;
                    <Link to={`/users/${p.playerName}`}>{p.playerName}</Link> (
                    {p.scores.reduce((total, score) => {
                      return total + score.relativeToPar;
                    }, 0)}
                    )
                    {playersHoleStats.find((x) => x.name === p.playerName)
                      ?.holeStats.birdie && (
                      <span className="icon is-small">
                        <i className="fas fa-dove"></i>
                      </span>
                    )}
                  </td>
                  {prevHole && (
                    <td
                      className="has-text-centered px-1 has-text-grey-light"
                      onClick={() => setActiveHole(activeHole - 1)}
                    >
                      {prevHole.strokes}
                    </td>
                  )}
                  <td className="has-text-centered bordered-cell is-size-5">
                    {currentHole.strokes}
                  </td>

                  {nextHole && (
                    <td
                      className="has-text-centered px-1 has-text-grey-light"
                      onClick={() => setActiveHole(activeHole + 1)}
                    >
                      {nextHole.strokes}
                    </td>
                  )}
                </tr>
              );
            })}
          </tbody>
        </table>
      </div>
    </div>
  );
};

export default HoleScoreComponent;
