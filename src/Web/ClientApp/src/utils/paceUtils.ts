import { PaceData, CurrentPace } from "../store/Rounds";

export function calculatePace(
  completedHoles: number,
  totalHoles: number,
  elapsedMinutes: number,
  _startTime: Date,
  paceData: PaceData
): CurrentPace {
  const { adjustedDurationMinutes, cardSpeedFactor, playerFactors } = paceData;

  if (adjustedDurationMinutes <= 0 || paceData.sampleCount < 3) {
    return {
      estimatedFinishTime: null,
      minutesPerHole: completedHoles > 0 ? elapsedMinutes / completedHoles : 0,
      isAhead: false,
      completedHoles,
      totalHoles,
      elapsedMinutes,
      estimatedTotalMinutes: 0,
      cardSpeedFactor,
      playerFactors,
    };
  }

  const clampedHoles = Math.min(completedHoles, totalHoles);
  const progress = totalHoles > 0 ? Math.min(1, Math.max(0, clampedHoles / totalHoles)) : 0;

  const actualProjectedTotal =
    completedHoles > 0
      ? (elapsedMinutes / completedHoles) * totalHoles
      : adjustedDurationMinutes;

  const estimatedTotalMinutes =
    (1 - progress) * adjustedDurationMinutes + progress * actualProjectedTotal;

  const estimatedMinutesRemaining = Math.max(
    0,
    estimatedTotalMinutes - elapsedMinutes
  );
  const estimatedFinishTime = new Date(
    Date.now() + estimatedMinutesRemaining * 60000
  );

  const expectedElapsedForCompletedHoles = adjustedDurationMinutes * progress;
  const isAhead = completedHoles > 0 && elapsedMinutes < expectedElapsedForCompletedHoles;

  const minutesPerHole = completedHoles > 0 ? elapsedMinutes / completedHoles : 0;

  return {
    estimatedFinishTime,
    minutesPerHole,
    isAhead,
    completedHoles,
    totalHoles,
    elapsedMinutes,
    estimatedTotalMinutes,
    cardSpeedFactor,
    playerFactors,
  };
}
