import React from "react";
import { connect, ConnectedProps } from "react-redux";
import { ApplicationState } from "../../store";
import * as TournamentsStore from "../../store/Tournaments";
import { Link } from "react-router-dom";

const mapState = (state: ApplicationState) => {
  return {
    user: state.user,
    tournaments: state.tournaments?.tournaments,
  };
};

const connector = connect(mapState, TournamentsStore.actionCreators);

type PropsFromRedux = ConnectedProps<typeof connector>;

type Props = PropsFromRedux & { onlyActive: boolean };

const Tournaments = (props: Props) => {
  const { fetchTournaments, tournaments } = props;
  React.useEffect(() => {
    fetchTournaments(props.onlyActive);
  }, [fetchTournaments, props.onlyActive]);

  return (
    <>
      <section className="has-text-centered">
        <h3 className="title is-3 has-text-centered">
          {props.onlyActive ? "Active Tournaments" : "Tournaments"}
        </h3>
        {(!tournaments || tournaments.length === 0) && (
          <>
            <div>No active tournaments</div>
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
      </section>
    </>
  );
};

export default connector(Tournaments);
