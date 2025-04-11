import React, { useEffect } from "react";
import { connect, ConnectedProps } from "react-redux";
import colors from "../../colors";
import { ApplicationState } from "../../store";
import { actionCreators as roundsActionCreator } from "../../store/Rounds";

const mapState = (state: ApplicationState) => {
  return {
    round: state.rounds?.round,
    playersCourseStats: state.rounds?.playerCourseStats,
  };
};

const connector = connect(mapState, roundsActionCreator);

type PropsFromRedux = ConnectedProps<typeof connector>;

type Props = PropsFromRedux & {};

interface PlayerCourseStats {
  playerName: string;
  thisRoundVsAverage: number;
  courseAverage: number;
}

const PlayerCourseImprovments = (props: Props) => {
  const { playersCourseStats, round, fetchStatsOnCourse } = props;
  const roundId = round?.id;
  const roundCourseName = round?.courseName;
  useEffect(() => {
    if (!playersCourseStats && roundId && roundCourseName)
      fetchStatsOnCourse(roundId);
  }, [roundCourseName, fetchStatsOnCourse, playersCourseStats, round, roundId]);

  if (!playersCourseStats || playersCourseStats.length === 0) return null;
  return (
    <table
      className="table is-narrow"
      style={{ backgroundColor: colors.table }}
    >
      <thead>
        <tr>
          <th>Player</th>
          <th
            className="has-tooltip-info"
            data-tooltip={`Player average score on ${roundCourseName} \n over the last 5 rounds`}
          >
            Avg.
          </th>
          <th
            className="has-tooltip-info"
            data-tooltip={`Improvement versus personal average \n on ${roundCourseName}`}
          >
            Impr.
          </th>
        </tr>
      </thead>
      <tbody>
        {playersCourseStats.map((s: PlayerCourseStats) => {
          const improv = Math.round(s.thisRoundVsAverage);
          const courseAvg = Math.round(s.courseAverage);
          return (
            <tr key={s.playerName}>
              <td>{s.playerName}</td>
              <td>
                {courseAvg > 0 ? "+" : courseAvg < 0 ? " - " : ""}
                {Math.abs(courseAvg)}
              </td>
              <td
                className={
                  improv > 0
                    ? "has-text-danger"
                    : improv < 0
                    ? "has-text-primary"
                    : ""
                }
              >
                {improv > 0 ? "+" : improv < 0 ? " - " : ""}
                {Math.abs(improv)}
              </td>
            </tr>
          );
        })}
      </tbody>
    </table>
  );
};

export default connector(PlayerCourseImprovments);
