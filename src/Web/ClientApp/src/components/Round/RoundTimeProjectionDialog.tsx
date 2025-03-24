import React from "react";
import colors from "../../colors";
import { RoundTimeProjection as RoundTimeProjectionModel } from "../../store/Rounds";

interface RoundTimeProjectionDialogProps {
  timeProjection: RoundTimeProjectionModel;
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

const RoundTimeProjectionDialog: React.FC<RoundTimeProjectionDialogProps> = ({ 
  timeProjection, 
  onClose 
}) => {
  if (!timeProjection) return null;

  const estimatedFinishTime = new Date(timeProjection.estimatedFinishTime);
  const now = new Date();
  
  const elapsedMinutes = timeProjection.totalEstimatedMinutes - timeProjection.estimatedMinutesRemaining;
  const remainingMinutes = timeProjection.estimatedMinutesRemaining;
  
  const currentPace = timeProjection.currentAverageMinutesPerHole;
  const historicalPace = timeProjection.historicalAverageMinutesPerHole;
  const paceComparison = currentPace - historicalPace;
  
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
                Estimated Finish: {formatTime(estimatedFinishTime)}
              </h2>
            </div>
            
            <div className="columns is-mobile mb-4">
              <div className="column has-text-centered">
                <p className="heading">Elapsed</p>
                <p className="title is-5">{formatDuration(elapsedMinutes)}</p>
              </div>
              <div className="column has-text-centered">
                <p className="heading">Remaining</p>
                <p className="title is-5">{formatDuration(remainingMinutes)}</p>
              </div>
            </div>
            
            <div className="columns is-mobile">
              <div className="column has-text-centered">
                <p className="heading">Current Pace</p>
                <p className="title is-5">{currentPace.toFixed(1)} min/hole</p>
              </div>
              <div className="column has-text-centered">
                <p className="heading">Your Average</p>
                <p className="title is-5">{historicalPace.toFixed(1)} min/hole</p>
              </div>
            </div>
            
            {paceComparison !== 0 && (
              <div className="has-text-centered mt-4">
                <p className={`${paceComparison > 0 ? 'has-text-danger' : 'has-text-success'}`}>
                  You're playing {Math.abs(paceComparison).toFixed(1)} min/hole 
                  {paceComparison > 0 ? ' slower' : ' faster'} than usual
                </p>
              </div>
            )}
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
