import React, { useEffect, useState } from "react";
import { ApplicationState } from "../../store";
import { connect } from "react-redux";
import { actionCreators as RoundsActions } from "../../store/Rounds";
import { RoundTimeProjection as RoundTimeProjectionModel } from "../../store/Rounds";

// Define props that come from Redux state
interface StateProps {
  roundTimeProjection: RoundTimeProjectionModel | null;
}

// Define props that come from Redux action creators
interface DispatchProps {
  fetchActiveRoundTimeProjection: () => void;
}

// Combined props type
type RoundTimeProjectionProps = StateProps & DispatchProps;

/**
 * Component to display projected finish time for the current round
 */
const RoundTimeProjection: React.FC<RoundTimeProjectionProps> = ({
  fetchActiveRoundTimeProjection,
  roundTimeProjection
}) => {
  const [loading, setLoading] = useState(true);

  useEffect(() => {
    // Fetch time projection when component mounts
    fetchActiveRoundTimeProjection();
    setLoading(false);
  }, [fetchActiveRoundTimeProjection]);

  if (loading || !roundTimeProjection) {
    return (
      <div className="box has-background-light p-3 my-2">
        <h4 className="title is-6 mb-2">
          <span className="icon is-small mr-1">
            <i className="fas fa-stopwatch"></i>
          </span>
          Round Time
        </h4>
        <div className="has-text-centered">
          <span className="is-size-7">Calculating projected finish time...</span>
        </div>
      </div>
    );
  }

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
    estimatedMinutesRemaining,
    currentAverageMinutesPerHole,
    historicalAverageMinutesPerHole,
    isAheadOfHistoricalPace,
    totalEstimatedMinutes
  } = roundTimeProjection;

  // Calculate elapsed time
  const elapsedMinutes = totalEstimatedMinutes - estimatedMinutesRemaining;

  return (
    <div className="box has-background-light p-3 my-2">
      <h4 className="title is-6 mb-2">
        <span className="icon is-small mr-1">
          <i className="fas fa-stopwatch"></i>
        </span>
        Round Time
      </h4>
      
      <div className="columns is-mobile is-multiline">
        <div className="column is-6 pb-1">
          <div className="is-size-7 has-text-grey">Elapsed</div>
          <div className="is-size-6">{formatMinutes(elapsedMinutes)}</div>
        </div>
        
        <div className="column is-6 pb-1">
          <div className="is-size-7 has-text-grey">Remaining</div>
          <div className="is-size-6">{formatMinutes(estimatedMinutesRemaining)}</div>
        </div>
        
        <div className="column is-6 pt-1">
          <div className="is-size-7 has-text-grey">Pace</div>
          <div className="is-size-6">
            {currentAverageMinutesPerHole > 0 ? (
              <>
                {currentAverageMinutesPerHole.toFixed(1)} min/hole
                <span className="is-size-7 has-text-grey ml-1">
                  (avg: {historicalAverageMinutesPerHole.toFixed(1)})
                </span>
              </>
            ) : (
              <>{historicalAverageMinutesPerHole.toFixed(1)} min/hole</>
            )}
          </div>
        </div>
        
        <div className="column is-6 pt-1">
          <div className="is-size-7 has-text-grey">Finish At</div>
          <div className={`is-size-6 ${isAheadOfHistoricalPace ? "has-text-success" : "has-text-danger"}`}>
            {formatTime(estimatedFinishTime)}
          </div>
        </div>
      </div>
      
      {currentAverageMinutesPerHole > 0 && (
        <div className="has-text-centered mt-2">
          <span className="tag is-small is-light">
            {isAheadOfHistoricalPace 
              ? "Playing faster than typical pace" 
              : "Playing slower than typical pace"}
          </span>
        </div>
      )}
    </div>
  );
};

const mapStateToProps = (state: ApplicationState): StateProps => {
  return {
    roundTimeProjection: state.rounds?.roundTimeProjection || null
  };
};

export default connect<StateProps, DispatchProps, {}, ApplicationState>(
  mapStateToProps,
  {
    fetchActiveRoundTimeProjection: RoundsActions.fetchActiveRoundTimeProjection
  }
)(RoundTimeProjection);
