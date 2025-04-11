import React from "react";
import { CurrentPace } from "../../store/Rounds";

interface RoundTimeProjectionProps {
  currentPace: CurrentPace;
}

/**
 * Component to display projected finish time for the current round in an inline box
 */
const RoundTimeProjection: React.FC<RoundTimeProjectionProps> = ({
  currentPace
}) => {
  // Format time as HH:MM
  const formatTime = (date: Date): string => {
    return new Date(date).toLocaleTimeString([], { hour: '2-digit', minute: '2-digit' });
  };

  // Format minutes as a human-readable duration (e.g., "1h 30m")
  const formatMinutes = (minutes: number): string => {
    const hours = Math.floor(minutes / 60);
    const mins = Math.floor(minutes % 60);
    return `${hours > 0 ? hours + 'h ' : ''}${mins}m`;
  };

  const {
    estimatedFinishTime,
    minutesPerHole,
    isAhead,
    completedHoles
  } = currentPace;

  // Calculate estimated total time
  const totalHoles = 18; // Assuming standard round
  const totalEstimatedMinutes = minutesPerHole * totalHoles;
  const estimatedMinutesRemaining = minutesPerHole * (totalHoles - completedHoles);
  const elapsedMinutes = totalEstimatedMinutes - estimatedMinutesRemaining;

  return (
    <div className="box has-background-light p-3 my-2">
      <h4 className="title is-6 mb-2">
        <span className="icon is-small mr-1">
          <i className="fas fa-stopwatch"></i>
        </span>
        Round Time
      </h4>
      
      <div className="columns is-mobile is-size-7">
        <div className="column is-6 pt-1">
          <div className="is-size-7 has-text-grey">Current</div>
          <div className="is-size-6">{minutesPerHole.toFixed(1)} min/hole</div>
        </div>
        
        <div className="column is-6 pt-1">
          <div className="is-size-7 has-text-grey">Finish</div>
          <div className={`is-size-6 ${isAhead ? "has-text-success" : "has-text-danger"}`}>
            {formatTime(estimatedFinishTime)}
          </div>
        </div>
      </div>
      
      <div className="has-text-centered mt-2">
        <span className="tag is-small is-light">
          {isAhead ? 
            "Playing faster than average pace" : 
            "Playing slower than average pace"}
        </span>
      </div>
    </div>
  );
};

export default RoundTimeProjection;
