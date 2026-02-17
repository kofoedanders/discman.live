// ─── Hole & Stroke Types ───

export interface Hole {
  number: number;
  par: number;
  distance: number;
  rating: number;
  average: number;
}

export type StrokeOutcome =
  | "Fairway"
  | "Rough"
  | "OB"
  | "Circle2"
  | "Circle1"
  | "Basket";

export interface StrokeSpec {
  outcome: StrokeOutcome;
  putDistance: number | null;
}

export const ScoreMode = {
  DetailedLive: 0,
  StrokesLive: 1,
  OneForAll: 2,
} as const;

export type ScoreMode = (typeof ScoreMode)[keyof typeof ScoreMode];

// ─── Score Types ───

export interface HoleScore {
  hole: Hole;
  strokes: number;
  relativeToPar: number;
  strokeSpecs: StrokeSpec[];
}

export interface PlayerScore {
  playerName: string;
  playerEmoji: string;
  playerRoundStatusEmoji: string;
  courseAverageAtTheTime: number;
  numberOfHcpStrokes: number;
  scores: HoleScore[];
}

// ─── Round Types ───

export interface Round {
  id: string;
  courseName: string;
  courseLayout: string;
  roundName: string;
  createdBy: string;
  startTime: string;
  completedAt: string;
  roundDuration: number;
  isCompleted: boolean;
  scoreMode: ScoreMode;
  playerScores: PlayerScore[];
  signatures: PlayerSignature[];
  achievements: UserAchievement[];
  spectators: string[];
  ratingChanges: RatingChange[];
}

export interface RatingChange {
  change: number;
  username: string;
}

export interface PlayerSignature {
  username: string;
  base64Signature: string;
  signedAt: string;
}

// ─── Pace / Time Types ───

export interface CurrentPace {
  estimatedFinishTime: Date | null;
  minutesPerHole: number;
  isAhead: boolean;
  completedHoles: number;
  totalHoles: number;
  elapsedMinutes: number;
  estimatedTotalMinutes: number;
  cardSpeedFactor: number;
  playerFactors: Record<string, number>;
}

export interface PaceData {
  averageCourseDurationMinutes: number;
  adjustedDurationMinutes: number;
  playerCountFactor: number;
  cardSpeedFactor: number;
  sampleCount: number;
  totalHoles: number;
  playerFactors: Record<string, number>;
}

// ─── Course Stats Types ───

export interface PlayerCourseStats {
  courseName: string;
  layoutName: string;
  playerName: string;
  courseAverage: number;
  thisRoundVsAverage: number;
  playerCourseRecord: number;
  holeAverages: number[];
  averagePrediction: number[];
  roundsPlayed: string;
  holeStats: HoleStats[];
}

export interface HoleStats {
  holeNumber: number;
  bestScore: HoleScore;
  averageScore: number;
  birdie: boolean;
  birdies: number;
  pars: number;
  worseThanPar: number;
  last10Scores: HoleScore[];
}

// ─── User Types ───

export interface AuthenticatedUser {
  username: string;
  token: string;
  email: string;
}

export interface User {
  username: string;
  token: string;
}

export interface UserDetails {
  email: string;
  username: string;
  simpleScoring: boolean;
  newsIdsSeen: string[];
  friends: string[];
  settingsInitialized: boolean;
  emoji: string;
  country: string;
  registerPutDistance: boolean;
  activeRound: string | null;
  discmanPoints: number;
  elo: number;
  ratingHistory: Rating[];
}

export interface Rating {
  elo: number;
  datetime: string;
}

export interface UserStats {
  username: string;
  roundsPlayed: number;
  holesPlayed: number;
  fairwayHitRate: number;
  scrambleRate: number;
  circle1Rate: number;
  circle2Rate: number;
  obRate: number;
  birdieRate: number;
  parRate: number;
  averageScore: number;
  strokesGained: number;
}

export interface UserAchievement {
  achievementName: string;
  username: string;
  achievedAt: string;
  roundId: string;
}

// ─── Course Types ───

export interface CourseHole {
  number: number;
  par: number;
  distance: number;
  average: number;
  rating: number;
}

export interface CourseStats {
  roundsOnCourse: number;
  previousRound: string | null;
}

export interface CourseCoordinates {
  latitude: number;
  longitude: number;
}

export interface CourseVm {
  id: string;
  name: string;
  createdAt: string;
  layout: string;
  coordinates: CourseCoordinates;
  holes: CourseHole[];
  admins: string[];
  courseStats: CourseStats | null;
  distance: number;
}

// ─── Paged Results ───

export interface PagedRounds {
  rounds: Round[];
  totalItemCount: number;
  pageNumber: number;
  pages: number;
}

// ─── API Command Types ───

export interface UpdatePlayerScoreCommand {
  holeIndex: number;
  strokes: number;
  strokeOutcomes: StrokeOutcome[];
  username: string;
}

export interface CreateRoundCommand {
  courseId: string | undefined;
  players: string[];
  roundName: string;
  scoreMode: ScoreMode;
}

export interface CompleteRoundCommand {
  base64Signature: string;
}

export type FeedItemType =
  | "Round"
  | "Hole"
  | "Achievement"
  | "Tournament"
  | "Friend"
  | "User";

export interface FeedItem {
  itemType: FeedItemType;
  id: string;
  registeredAt: string;
  subjects: string[];
  courseName: string;
  holeScore: number;
  holeNumber: number;
  roundScores: number[];
  likes: string[];
  action: string;
  roundId: string;
  achievementName: string;
  tournamentId: string;
  tournamentName: string;
  friendName: string;
}

export interface Feed {
  isLastPage: boolean;
  feedItems: FeedItem[];
}

export interface LeaderboardPlayer {
  username: string;
  averageHoleScore: number;
  courseAdjustedAverageScore: number;
  roundCount: number;
  elo: number;
  birdieCount: number;
  bogeyCount: number;
}

// ─── Tournament Types ───

export interface TournamentListing {
  id: string;
  name: string;
  start: string;
  end: string;
}

export interface TournamentCourseInfo {
  id: string;
  name: string;
  layout: string;
}

export interface TournamentInfo {
  id: string;
  name: string;
  players: string[];
  admins: string[];
  start: string;
  end: string;
  courses: TournamentCourseInfo[];
  hasStarted: boolean;
  isCompleted: boolean;
}

export interface TournamentScore {
  name: string;
  totalScore: number;
  totalHcpScore: number;
  coursesPlayed: string[];
}

export interface TournamentLeaderboard {
  scores: TournamentScore[];
}

export interface TournamentPrice {
  username: string;
  scoreValue: string;
  negativePrice?: boolean;
}

export interface TournamentFinalScore {
  username: string;
  score: number;
}

export interface TournamentPrices {
  scoreboard: TournamentFinalScore[];
  fastestPlayer: TournamentPrice | null;
  slowestPlayer: TournamentPrice | null;
  mostBirdies: TournamentPrice | null;
  leastBogeysOrWorse: TournamentPrice | null;
  longestCleanStreak: TournamentPrice | null;
  longestDrySpell: TournamentPrice | null;
  bounceBacks: TournamentPrice | null;
}

export interface Tournament {
  info: TournamentInfo;
  leaderboard: TournamentLeaderboard;
  prices: TournamentPrices | null;
}

// ─── Course Command Types ───

export interface CreateCourseCommand {
  courseName: string;
  layoutName: string;
  latitude: number;
  longitude: number;
  numberOfHoles: number;
  par4s: number[];
  par5s: number[];
}

// ─── Tournament Command Types ───

export interface CreateTournamentCommand {
  name: string;
  start: string;
  end: string;
}

export interface AddCourseToTournamentCommand {
  tournamentId: string;
  courseId: string;
}
