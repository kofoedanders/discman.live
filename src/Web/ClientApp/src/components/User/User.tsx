import React, { useEffect, useState } from "react";
import { connect, ConnectedProps } from "react-redux";
import { ApplicationState } from "../../store";
import * as UserStore from "../../store/User";
import UserStats from "./UserStats";
import UserRounds from "./UserRounds";
import { useParams } from "react-router";
import UserAchievements from "./UserAchievements";
import Tournaments from "../Tournaments/Tournaments";
import { countries } from "./CountryPicker";
import RatingHistory from "./RatingHistory";
import { Link } from "react-router-dom";

const mapState = (state: ApplicationState) => {
  return {
    user: state.user,
  };
};

const connector = connect(mapState, UserStore.actionCreators);

type PropsFromRedux = ConnectedProps<typeof connector>;

type Props = PropsFromRedux & {};

const UserComponent = (props: Props) => {
  const { usernameParam } = useParams<{ usernameParam: string }>();
  const [active, setActive] = useState(1);
  const username = usernameParam || props.user?.user?.username;
  useEffect(() => {
    username && props.fetchMoreUserDetails(username);
  });
  const userDetails =
    props.user?.usersDetails &&
    props.user.usersDetails.find((d) => d.username === username);

  const thisYear = new Date().getFullYear();
  const thisMonth = new Date().getMonth();
  const summaryYear = thisMonth > 10 ? thisYear : thisYear - 1;

  return (
    <div>
      <h3 className="title is-3 has-text-centered">
        {countries[(userDetails && userDetails.country) || "unknown"]} &nbsp;
        {username}&nbsp;
        {userDetails?.emoji}&nbsp;
        {Math.round(userDetails?.elo || 0)}
      </h3>
      {(thisMonth > 10 || thisMonth < 3) && (
        <div className="has-text-centered">
          <Link to={`/user/${username}/yearsummary/${summaryYear}`}>
            <button className="button is-medium is-warning is-rounded m-3">
              See your yearly summary!
            </button>
          </Link>
        </div>
      )}
      <div className="tabs is-small is-fullwidth is-centered">
        <ul>
          <li
            className={active === 1 ? "is-active" : ""}
            onClick={() => setActive(1)}
          >
            <a>Rounds</a>
          </li>
          <li
            className={active === 2 ? "is-active" : ""}
            onClick={() => setActive(2)}
          >
            <a>Rating</a>
          </li>
          <li
            className={active === 3 ? "is-active" : ""}
            onClick={() => setActive(3)}
          >
            <a>Stats</a>
          </li>
          <li
            className={active === 4 ? "is-active" : ""}
            onClick={() => setActive(4)}
          >
            <a>Achievements</a>
          </li>
          <li
            className={active === 5 ? "is-active" : ""}
            onClick={() => setActive(5)}
          >
            <a>Tourneys</a>
          </li>
        </ul>
      </div>
      {active === 1 && (
        <div className="section pt-0">
          <UserRounds />
        </div>
      )}
      {active === 2 && <RatingHistory username={username} />}
      {active === 3 && <UserStats />}
      {active === 4 && <UserAchievements />}
      {active === 5 && <Tournaments username={username} />}
    </div>
  );
};

export default connector(UserComponent);
