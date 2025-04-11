import React from "react";
import colors from "../../colors";
import { PaceData, PaceState } from "../../store/Rounds";

interface RoundPaceDialogProps {
  paceData: PaceData;
  currentPace: PaceState;
  username: string;
  onClose: () => void;
}

const formatTime = (date: Date): string => {
  return date.toLocaleTimeString([], { hour: '2-digit', minute: '2-digit' });
};

const formatDuration = (minutes: number): string => {
  const hours = Math.floor(minutes / 60);
  const mins = Math.floor(minutes % 60);
  return hours > 0 ? `${hours}h ${mins}m` : `${mins}m`;
};

const RoundPaceDialog: React.FC<RoundPaceDialogProps> = ({
  paceData,
  currentPace,
  username,
  onClose
}) => {
  if (!paceData || !currentPace?.minutesPerHole) return null;

  const estimatedFinishTime = currentPace.estimatedFinishTime!;
  const isAhead = currentPace.isAhead;
  const currentPaceValue = currentPace.minutesPerHole;
  const completedHoles = currentPace.completedHoles || 0;
  
  // Debug logs
  console.log(JSON.stringify(paceData, null, 2));
  console.log(JSON.stringify(currentPace));

  // Calculate remaining time based on the estimated finish time, not current pace
  const remainingHoles = 18 - completedHoles;
  const now = new Date();
  const remainingMinutes = Math.max(0, (new Date(estimatedFinishTime).getTime() - now.getTime()) / 60000);

  // Get player count for display
  const playerCount = Object.keys(paceData.playerAverages || {}).length || 0;

  // Determine the motivational message based on pace
  let motivationalMessage = "";
  let messageColor = "";
  if (isAhead) {
    motivationalMessage = `Great pace! You're ahead of schedule.`;
    messageColor = "has-text-success";
  } else {
    motivationalMessage = `Playing a bit slower than average.`;
    messageColor = "has-text-warning";
  }
  
  return (
    <div className="modal is-active">
      <div className="modal-background" onClick={onClose}></div>
      <div className="modal-card" style={{ maxWidth: "95%", margin: "0 auto" }}>
        <header className="modal-card-head" style={{ padding: "10px 15px" }}>
          <p className="modal-card-title is-size-5">Round Pace</p>
          <button
            className="delete"
            aria-label="close"
            onClick={onClose}
          ></button>
        </header>
        <section className="modal-card-body" style={{ padding: "15px" }}>
          <div className="content">
            <div className="has-text-centered mb-3">
              <h2 className="title is-4 mb-2">
                Finish: {formatTime(new Date(estimatedFinishTime))}
              </h2>
              <p className={`${messageColor} is-size-6 mb-1`}>
                {motivationalMessage}
              </p>
              <p className="is-size-7 has-text-grey">
                Based on {playerCount} player{playerCount !== 1 ? 's' : ''} on card
              </p>
            </div>

            <div className="columns is-mobile is-multiline">
              <div className="column is-6-mobile">
                <div className="box" style={{ padding: "10px", backgroundColor: colors.background, height: "100%" }}>
                  <h4 className="title is-6 mb-2">Status</h4>
                  <div className="field mb-2">
                    <label className="label is-size-7 mb-0">Completed</label>
                    <div className="control">
                      <span className="has-text-weight-bold">{completedHoles} of 18 holes</span>
                    </div>
                  </div>
                  <div className="field mb-2">
                    <label className="label is-size-7 mb-0">Current Pace</label>
                    <div className="control">
                      <span className="has-text-weight-bold">{currentPaceValue.toFixed(1)} min/hole</span>
                    </div>
                  </div>
                </div>
              </div>
              
              <div className="column is-6-mobile">
                <div className="box" style={{ padding: "10px", backgroundColor: colors.background, height: "100%" }}>
                  <h4 className="title is-6 mb-2">Remaining</h4>
                  <div className="field mb-2">
                    <label className="label is-size-7 mb-0">Holes Left</label>
                    <div className="control">
                      <span className="has-text-weight-bold">{remainingHoles} holes</span>
                    </div>
                  </div>
                  <div className="field mb-2">
                    <label className="label is-size-7 mb-0">Time Left</label>
                    <div className="control">
                      <span className="has-text-weight-bold">{formatDuration(remainingMinutes)}</span>
                    </div>
                  </div>
                </div>
              </div>
            </div>

            <div className="notification mt-4" style={{ backgroundColor: isAhead ? '#4caf50' : '#f44336', color: 'white' }}>
              <div className="columns is-mobile">
                <div className="column is-narrow">
                  <span className="icon is-medium">
                    <i className={`fas fa-${isAhead ? 'arrow-down' : 'arrow-up'} fa-lg`}></i>
                  </span>
                </div>
                <div className="column">
                  <p className="is-size-6">
                    {isAhead 
                      ? "You're playing faster than average! Keep up the good pace."
                      : "You're playing a bit slower than average. Consider picking up the pace if you're short on time."}
                  </p>
                </div>
              </div>
            </div>
          </div>
        </section>
        <footer className="modal-card-foot">
          <button className="button" onClick={onClose}>Close</button>
        </footer>
      </div>
    </div>
  );
};

export default RoundPaceDialog;
