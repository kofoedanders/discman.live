import React from "react";
import { connect, ConnectedProps } from "react-redux";
import { ApplicationState } from "../../store";
import * as TournamentsStore from "../../store/Tournaments";
import { Link } from "react-router-dom";
import NewTournament from "./NewTournament";

const mapState = (state: ApplicationState) => {
  return {
    user: state.user,
    tournaments: state.tournaments?.tournaments,
  };
};

const connector = connect(mapState, TournamentsStore.actionCreators);

type PropsFromRedux = ConnectedProps<typeof connector>;

type Props = PropsFromRedux & { username?: string };

const Tournaments = (props: Props) => {
  const { fetchTournaments, tournaments } = props;
  React.useEffect(() => {
    fetchTournaments(false, props.username);
  }, [fetchTournaments, props.username]);

  return (
    <>
      <h4 className="title is-4 has-text-centered">Tournaments</h4>
      <section className="section pt-0 has-text-centered">
        {(!tournaments || tournaments.length === 0) && (
          <>
            <div>No tournaments</div>
            <br />
          </>
        )}
        {tournaments && (
          <div className="panel">
            {tournaments.map((t) => (
              <Link
                className="panel-block"
                key={t.id}
                to={`/tournaments/${t.id}`}
              >
                <span className="panel-icon">
                  <i className="fas fa-trophy"></i>
                </span>
                {t.name}&nbsp;:&nbsp;
                <i className="is-size-7">
                  {new Date(t.start).toLocaleDateString()}
                  {"-"}
                  {new Date(t.end).toLocaleDateString()}
                </i>
              </Link>
            ))}
          </div>
        )}

        <hr />
        <NewTournament />
      </section>
    </>
  );
};

export default connector(Tournaments);
