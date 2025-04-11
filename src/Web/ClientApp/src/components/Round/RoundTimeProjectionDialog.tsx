import React from "react";
import colors from "../../colors";
import { CurrentPace } from "../../store/Rounds";

interface RoundTimeProjectionDialogProps {
  currentPace: CurrentPace;
  onClose: () => void;
}

const formatTime = (date: Date | null): string => {
  if (!date) return "Unknown";
  return date.toLocaleTimeString([], { hour: '2-digit', minute: '2-digit' });
};

const formatDuration = (minutes: number): string => {
  const hours = Math.floor(minutes / 60);
  const mins = Math.floor(minutes % 60);
  return hours > 0 ? `${hours}h ${mins}m` : `${mins}m`;
};

const RoundTimeProjectionDialog: React.FC<RoundTimeProjectionDialogProps> = ({ 
  currentPace, 
  onClose 
}) => {
  // If we don't have a valid estimated finish time, show a message
  if (!currentPace.estimatedFinishTime) {
    return (
      <div className="modal is-active">
        <div className="modal-background" onClick={onClose}></div>
        <div className="modal-card">
          <header 
            className="modal-card-head" 
            style={{ backgroundColor: colors.background }}
          >
            <p className="modal-card-title">Round Time Projection</p>
            <button 
              className="delete" 
              aria-label="close" 
              onClick={onClose}
            ></button>
          </header>
          <section 
            className="modal-card-body" 
            style={{ backgroundColor: colors.background }}
          >
            <div className="content has-text-centered">
              <p>Insufficient data to estimate finish time.</p>
              <p>This can happen when playing on a new course or with limited historical data.</p>
            </div>
          </section>
          <footer 
            className="modal-card-foot" 
            style={{ backgroundColor: colors.background, justifyContent: 'center' }}
          >
            <button 
              className="button" 
              style={{ backgroundColor: colors.button }}
              onClick={onClose}
            >
              Close
            </button>
          </footer>
        </div>
      </div>
    );
  }
  
  // Calculate estimated total time based on pace data
  const startTime = new Date(currentPace.estimatedFinishTime.getTime());
  startTime.setMinutes(startTime.getMinutes() - currentPace.minutesPerHole * (18 - currentPace.completedHoles));
  
  // Calculate elapsed and remaining time in minutes
  const currentTime = new Date();
  const elapsedMinutes = Math.max(0, (currentTime.getTime() - startTime.getTime()) / 60000);
  const estimatedMinutesRemaining = Math.max(0, (currentPace.estimatedFinishTime.getTime() - currentTime.getTime()) / 60000);
  
  return (
    <div className="modal is-active">
      <div className="modal-background" onClick={onClose}></div>
      <div className="modal-card">
        <header 
          className="modal-card-head" 
          style={{ backgroundColor: colors.background }}
        >
          <p className="modal-card-title">Round Time Projection</p>
          <button 
            className="delete" 
            aria-label="close" 
            onClick={onClose}
          ></button>
        </header>
        <section 
          className="modal-card-body" 
          style={{ backgroundColor: colors.background }}
        >
          <div className="content">
            <div className="has-text-centered mb-5">
              <h2 className="title is-4">
                Estimated Finish: {formatTime(currentPace.estimatedFinishTime)}
              </h2>
            </div>
            
            <div className="columns is-mobile mb-4">
              <div className="column has-text-centered">
                <p className="heading">Elapsed</p>
                <p className="title is-5">{formatDuration(elapsedMinutes)}</p>
              </div>
              <div className="column has-text-centered">
                <p className="heading">Remaining</p>
                <p className="title is-5">{formatDuration(estimatedMinutesRemaining)}</p>
              </div>
            </div>
            
            <div className="columns is-mobile">
              <div className="column has-text-centered">
                <p className="heading">Current Pace</p>
                <p className="title is-5">{currentPace.minutesPerHole.toFixed(1)} min/hole</p>
              </div>
              <div className="column has-text-centered">
                <p className="heading">Completed</p>
                <p className="title is-5">{currentPace.completedHoles} / 18 holes</p>
              </div>
            </div>
            
            <div className="has-text-centered mt-4">
              <span className={`tag is-medium ${currentPace.isAhead ? 'is-success' : 'is-danger'}`}>
                {currentPace.isAhead 
                  ? 'Ahead of Average Pace' 
                  : 'Behind Average Pace'}
              </span>
            </div>
          </div>
        </section>
        <footer 
          className="modal-card-foot" 
          style={{ backgroundColor: colors.background, justifyContent: 'center' }}
        >
          <button 
            className="button" 
            style={{ backgroundColor: colors.button }}
            onClick={onClose}
          >
            Close
          </button>
        </footer>
      </div>
    </div>
  );
};

export default RoundTimeProjectionDialog;
