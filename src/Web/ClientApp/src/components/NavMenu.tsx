import React, { useState } from "react";
import { connect, ConnectedProps } from "react-redux";
import { ApplicationState } from "../store";
import * as RoundsStore from "../store/Rounds";
import * as UserStore from "../store/User";
import { Link } from "react-router-dom";
import SaveCourseFromRound from "./Round/SaveCourseFromRound";
import colors from "../colors";

const mapState = (state: ApplicationState) => {
  return {
    user: state.user,
    round: state.rounds?.round,
    roundInProgress: state.user?.userDetails?.activeRound,
    location: state.router.location,
  };
};

const connector = connect(mapState, {
  ...RoundsStore.actionCreators,
  ...UserStore.actionCreators,
});

type PropsFromRedux = ConnectedProps<typeof connector>;

type Props = PropsFromRedux & {};

const NavMenu = (props: Props) => {
  const [open, setOpen] = useState(false);
  const [confirmDelete, setConfirmDelete] = useState(false);
  const [confirmSkipHole, setConfirmSkipHole] = useState(false);
  const [confirmLeaveRound, setConfirmLeaveRound] = useState(false);
  return (
    <>
      <div className="navbar-menu">
        <div className="navbar-end">
          <div className="navbar-item py-0">
            <div className={`dropdown is-up is-right ${open && "is-active"}`}>
              <div className="dropdown-trigger" onClick={() => setOpen(!open)}>
                <span className="icon">
                  <i className="fas fa-lg fa-bars" aria-hidden="true"></i>
                </span>
              </div>
            </div>
          </div>
        </div>
      </div>
      <div className={open ? "modal is-active" : "modal"}>
        <div
          onClick={() => {
            setOpen(false);
          }}
        >
          <div className="modal-background"></div>
        </div>
        <div className="modal-card">
          {/* <header
            className="modal-card-head"
            style={{ backgroundColor: colors.background }}
          >
            <p className="modal-card-title">Menu</p>
          </header> */}
          <section
            className="modal-card-body"
            style={{ backgroundColor: colors.background }}
          >
            {props.round && props.location.pathname.startsWith("/rounds") && (
              <>
                <article className="panel">
                  {props.round.isCompleted && !props.round.courseName && (
                    <SaveCourseFromRound setOpen={setOpen} />
                  )}

                  <span
                    className="panel-block has-text-danger"
                    onClick={() => {
                      setConfirmLeaveRound(true);
                    }}
                  >
                    <span className="panel-icon">
                      <i className="fas fa-lg fa-door-open"></i>
                    </span>
                    &nbsp; Leave round
                  </span>

                  {props.user?.user?.username === props.round?.createdBy && (
                    <>
                      <span
                        className="panel-block has-text-danger"
                        onClick={() => setConfirmSkipHole(true)}
                      >
                        <span className="panel-icon">
                          <i className="fas fa-lg fa-forward"></i>
                        </span>
                        &nbsp; Skip hole
                      </span>
                      <span
                        className="panel-block has-text-danger"
                        onClick={() => setConfirmDelete(true)}
                      >
                        <span className="panel-icon">
                          <i className="fas fa-lg fa-backspace"></i>
                        </span>
                        &nbsp; Delete round
                      </span>
                    </>
                  )}
                </article>
              </>
            )}
            <article className="panel">
              <Link
                to="/"
                className="panel-block"
                onClick={() => setOpen(false)}
              >
                <span className="panel-icon">
                  <i className="fas fa-lg fa-home"></i>
                </span>
                &nbsp; Home
              </Link>
              <Link
                to="/courses"
                className="panel-block"
                onClick={() => setOpen(false)}
              >
                <span className="panel-icon">
                  <i className="fas fa-lg fa-cloud-sun"></i>
                </span>
                &nbsp; Courses
              </Link>
              <Link
                to="/tournaments"
                className="panel-block"
                onClick={() => setOpen(false)}
              >
                <span className="panel-icon">
                  <i className="fas fa-lg fa-trophy"></i>
                </span>
                &nbsp; Tournaments
              </Link>
              <Link
                to="/friends"
                className="panel-block"
                onClick={() => setOpen(false)}
              >
                <span className="panel-icon">
                  <i className="fas fa-lg fa-user-friends"></i>
                </span>
                &nbsp; Friends
              </Link>
              <Link
                to="/user"
                className="panel-block"
                onClick={() => setOpen(false)}
              >
                <span className="panel-icon">
                  <i className="fas fa-lg fa-user"></i>
                </span>
                &nbsp; Profile
              </Link>
              <Link
                to="/settings"
                className="panel-block"
                onClick={() => setOpen(false)}
              >
                <span className="panel-icon">
                  <i className="fas fa-lg fa-cogs"></i>
                </span>
                &nbsp; Settings
              </Link>
              <Link
                to="/about"
                className="panel-block"
                onClick={() => setOpen(false)}
              >
                <span className="panel-icon">
                  <i className="fas fa-lg fa-question-circle"></i>
                </span>
                &nbsp; About
              </Link>
              {/* Year Summary - shown in the first 3 months and last 2 months */}
              {(new Date().getMonth() <= 2 || new Date().getMonth() >= 10) && (
                <Link
                  to={`/user/${props.user?.user?.username}/yearsummary/${new Date().getMonth() >= 10 ? new Date().getFullYear() : new Date().getFullYear() - 1}`}
                  className="panel-block"
                  onClick={() => setOpen(false)}
                >
                  <span className="panel-icon">
                    <i className="fas fa-lg fa-calendar-alt"></i>
                  </span>
                  &nbsp; Year Summary
                </Link>
              )}
            </article>
            <article className="panel">
              <span
                className="panel-block"
                onClick={() => {
                  props.logout();
                  setOpen(false);
                }}
              >
                <span className="panel-icon">
                  <i className="fas fa-lg fa-sign-out-alt"></i>
                </span>
                &nbsp; Logout
              </span>
            </article>
          </section>
          {/* <footer className="modal-card-foot">
            <button className="button" onClick={() => setOpen(false)}>
              Close
            </button>
          </footer> */}
        </div>
      </div>

      <div className={`modal ${confirmLeaveRound ? "is-active" : ""}`}>
        <div onClick={() => setConfirmLeaveRound(false)}>
          <div className="modal-background"></div>{" "}
        </div>
        <div
          className="modal-card"
          style={{ backgroundColor: colors.background }}
        >
          <header
            className="modal-card-head"
            style={{ backgroundColor: colors.background }}
          >
            <p className="modal-card-title">Leave round?</p>
          </header>
          <section
            className="modal-card-body"
            style={{ backgroundColor: colors.background }}
          >
            <p>
              You will be removed from the round and all your scores deleted,
              are you sure?
            </p>
            <hr />
            <div className="field">
              <div className="control">
                <button
                  className="button is-danger"
                  onClick={() => {
                    setOpen(false);
                    setConfirmLeaveRound(false);
                    props.leaveRound(props.round?.id || "");
                  }}
                >
                  Confirm
                </button>
              </div>
            </div>
          </section>
        </div>
      </div>

      <div className={`modal ${confirmSkipHole ? "is-active" : ""}`}>
        <div onClick={() => setConfirmSkipHole(false)}>
          {" "}
          <div className="modal-background"></div>{" "}
        </div>
        <div className="modal-card">
          <header
            className="modal-card-head"
            style={{ backgroundColor: colors.background }}
          >
            <p className="modal-card-title">Skip hole?</p>
          </header>
          <section
            className="modal-card-body"
            style={{ backgroundColor: colors.background }}
          >
            <p>Hole will be removed from the current round, are you sure?</p>
            <hr />
            <div className="field">
              <div className="control">
                <button
                  className="button is-danger"
                  onClick={() => {
                    setOpen(false);
                    setConfirmSkipHole(false);
                    props.skipHole(props.round?.id || "");
                  }}
                >
                  Confirm
                </button>
              </div>
            </div>
          </section>
        </div>
      </div>

      <div className={`modal ${confirmDelete ? "is-active" : ""}`}>
        <div onClick={() => setConfirmDelete(false)}>
          {" "}
          <div className="modal-background"></div>{" "}
        </div>
        <div className="modal-card">
          <header
            className="modal-card-head"
            style={{ backgroundColor: colors.background }}
          >
            <p className="modal-card-title">Delete round?</p>
          </header>
          <section
            className="modal-card-body"
            style={{ backgroundColor: colors.background }}
          >
            <p>
              Round will be permanently deleted for all players, are you sure?
            </p>
            <hr />
            <div className="field">
              <div className="control">
                <button
                  className="button is-danger"
                  onClick={() => {
                    setOpen(false);
                    setConfirmDelete(false);
                    props.deleteRound(props.round?.id || "");
                  }}
                >
                  Confirm
                </button>
              </div>
            </div>
          </section>
        </div>
      </div>
    </>
  );
};

export default connector(NavMenu);
