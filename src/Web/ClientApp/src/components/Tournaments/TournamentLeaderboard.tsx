import React from "react";
import { Tournament } from "../../store/Tournaments";

export interface TournamentLeaderboardProps {
  tournament: Tournament;
}

export default ({ tournament }: TournamentLeaderboardProps) => (
  <table className="table is-marginless is-paddingless is-narrow is-striped is-fullwidth">
    <thead>
      <tr>
        <th></th>
        <th>Player</th>
        <th>Score</th>
        <th>Courses</th>
      </tr>
    </thead>
    <tbody>
      {tournament &&
        tournament.leaderboard.scores.map((s, i) => {
          return (
            <tr key={s.name}>
              <th>{i + 1}</th>
              <td>{s.name}</td>
              <td>{s.totalScore}</td>
              <td>
                {s.coursesPlayed.length} / {tournament.info.courses.length}{" "}
              </td>
            </tr>
          );
        })}
    </tbody>
  </table>
);
