using System;
using System.Collections.Generic;

namespace Web.Rounds
{
    /// <summary>
    /// Represents time projections for a round, including estimated finish time
    /// </summary>
    public class RoundTimeProjection
    {
        /// <summary>
        /// The round ID this projection belongs to
        /// </summary>
        public Guid RoundId { get; set; }
        
        /// <summary>
        /// Time estimates for each hole in the round
        /// </summary>
        public List<HoleTimeEstimate> HoleTimeEstimates { get; set; } = new List<HoleTimeEstimate>();
        
        /// <summary>
        /// Estimated finish time for the round
        /// </summary>
        public DateTime EstimatedFinishTime { get; set; }
        
        /// <summary>
        /// Total estimated minutes for the entire round
        /// </summary>
        public int TotalEstimatedMinutes { get; set; }
        
        /// <summary>
        /// Estimated minutes remaining in the round
        /// </summary>
        public int EstimatedMinutesRemaining { get; set; }
        
        /// <summary>
        /// Current average minutes per hole based on completed holes
        /// </summary>
        public double CurrentAverageMinutesPerHole { get; set; }
        
        /// <summary>
        /// Historical average minutes per hole for this course and layout
        /// </summary>
        public double HistoricalAverageMinutesPerHole { get; set; }
        
        /// <summary>
        /// Whether the current pace is ahead of the historical average
        /// </summary>
        public bool IsAheadOfHistoricalPace { get; set; }
    }

    /// <summary>
    /// Represents time estimate for a specific hole
    /// </summary>
    public class HoleTimeEstimate
    {
        /// <summary>
        /// Hole number
        /// </summary>
        public int HoleNumber { get; set; }
        
        /// <summary>
        /// Average minutes to complete this hole
        /// </summary>
        public double AverageMinutesToComplete { get; set; }
    }
}
