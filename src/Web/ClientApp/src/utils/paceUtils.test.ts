import { calculatePace } from "./paceUtils";
import { PaceData } from "../store/Rounds";

const basePaceData: PaceData = {
  averageCourseDurationMinutes: 90,
  adjustedDurationMinutes: 80,
  playerCountFactor: 1,
  cardSpeedFactor: 0.95,
  sampleCount: 10,
  totalHoles: 18,
  playerFactors: { alice: 0.9, bob: 1.05 },
};

describe("calculatePace", () => {
  it("returns null estimatedFinishTime when sampleCount < 3", () => {
    const paceData = { ...basePaceData, sampleCount: 2 };
    const result = calculatePace(3, 18, 20, new Date("2023-01-01T10:00:00Z"), paceData);
    expect(result.estimatedFinishTime).toBeNull();
    expect(result.estimatedTotalMinutes).toBe(0);
  });

  it("returns historical estimate when zero holes completed", () => {
    const result = calculatePace(0, 18, 0, new Date("2023-01-01T10:00:00Z"), basePaceData);
    expect(result.estimatedTotalMinutes).toBeCloseTo(basePaceData.adjustedDurationMinutes, 5);
    expect(result.isAhead).toBe(false);
  });

  it("blends historical and actual at mid-round matching pace", () => {
    const elapsedMinutes = 40;
    const result = calculatePace(9, 18, elapsedMinutes, new Date("2023-01-01T10:00:00Z"), basePaceData);
    expect(result.estimatedTotalMinutes).toBeCloseTo(80, 5);
    expect(result.isAhead).toBe(false);
  });

  it("marks ahead of pace when elapsed is below expected", () => {
    const result = calculatePace(9, 18, 30, new Date("2023-01-01T10:00:00Z"), basePaceData);
    expect(result.isAhead).toBe(true);
    expect(result.estimatedTotalMinutes).toBeLessThan(basePaceData.adjustedDurationMinutes);
  });

  it("marks behind pace when elapsed is above expected", () => {
    const result = calculatePace(9, 18, 50, new Date("2023-01-01T10:00:00Z"), basePaceData);
    expect(result.isAhead).toBe(false);
  });

  it("weights heavily toward actual pace near completion", () => {
    const result = calculatePace(17, 18, 76.5, new Date("2023-01-01T10:00:00Z"), basePaceData);
    const actualProjectedTotal = (76.5 / 17) * 18;
    const progress = 17 / 18;
    const expected = (1 - progress) * basePaceData.adjustedDurationMinutes + progress * actualProjectedTotal;
    expect(result.estimatedTotalMinutes).toBeCloseTo(expected, 5);
    expect(Math.abs(result.estimatedTotalMinutes - actualProjectedTotal)).toBeLessThan(1);
  });

  it("returns null estimatedFinishTime when adjustedDurationMinutes is 0", () => {
    const paceData = { ...basePaceData, adjustedDurationMinutes: 0 };
    const result = calculatePace(5, 18, 20, new Date("2023-01-01T10:00:00Z"), paceData);
    expect(result.estimatedFinishTime).toBeNull();
  });
});
