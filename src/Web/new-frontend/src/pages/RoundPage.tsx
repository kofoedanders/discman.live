import { useEffect, useState, useCallback, useMemo } from "react";
import { useParams, useNavigate } from "react-router-dom";
import { useAuthStore } from "../stores/authStore";
import { useRoundStore } from "../stores/roundStore";
import { connectHub, disconnectHub } from "../signalr/hub";
import type { StrokeOutcome, HoleScore, HoleStats } from "../types";

import StatusStrip from "../components/round/StatusStrip";
import PlayerList from "../components/round/PlayerList";
import StrokeTrail from "../components/round/StrokeTrail";
import ScoringGrid from "../components/round/ScoringGrid";
import BottomBar from "../components/round/BottomBar";
import WaitingSheet from "../components/round/WaitingSheet";
import ScorecardOverlay from "../components/round/ScorecardOverlay";
import StatsDialog from "../components/round/StatsDialog";
import RoundSummary from "../components/round/RoundSummary";
import RoundMenu from "../components/round/RoundMenu";

export default function RoundPage() {
  const { roundId } = useParams<{ roundId: string }>();
  const navigate = useNavigate();
  const user = useAuthStore((s) => s.user);
  const username = user?.username ?? "";

  const round = useRoundStore((s) => s.round);
  const activeHoleIndex = useRoundStore((s) => s.activeHoleIndex);
  const setActiveHole = useRoundStore((s) => s.setActiveHole);
  const currentPace = useRoundStore((s) => s.currentPace);
  const playerCourseStats = useRoundStore((s) => s.playerCourseStats);
  const finishedRoundStats = useRoundStore((s) => s.finishedRoundStats);
  const isLoading = useRoundStore((s) => s.isLoading);
  const scorecardOpen = useRoundStore((s) => s.scorecardOpen);
  const editHole = useRoundStore((s) => s.editHole);
  const fetchRound = useRoundStore((s) => s.fetchRound);
  const fetchPaceData = useRoundStore((s) => s.fetchPaceData);
  const fetchCourseStats = useRoundStore((s) => s.fetchCourseStats);
  const fetchRoundStats = useRoundStore((s) => s.fetchRoundStats);
  const setScore = useRoundStore((s) => s.setScore);
  const setScorecardOpen = useRoundStore((s) => s.setScorecardOpen);
  const clearRound = useRoundStore((s) => s.clearRound);
  const goToNextPersonalHole = useRoundStore((s) => s.goToNextPersonalHole);
  const setEditHole = useRoundStore((s) => s.setEditHole);

  const [strokeBuffer, setStrokeBuffer] = useState<StrokeOutcome[]>([]);
  const [strokeBufferHole, setStrokeBufferHole] = useState(activeHoleIndex);
  const [statsOpen, setStatsOpen] = useState(false);
  const [waitingDismissed, setWaitingDismissed] = useState(false);
  const [menuOpen, setMenuOpen] = useState(false);

  const effectiveStrokeBuffer = useMemo(
    () => (strokeBufferHole !== activeHoleIndex ? [] : strokeBuffer),
    [strokeBufferHole, activeHoleIndex, strokeBuffer],
  );
  const effectiveWaitingDismissed = useMemo(
    () => (strokeBufferHole !== activeHoleIndex ? false : waitingDismissed),
    [strokeBufferHole, activeHoleIndex, waitingDismissed],
  );
  
  useEffect(() => {
    if (editHole) {
      setStrokeBuffer([]);
      setStrokeBufferHole(activeHoleIndex);
    }
  }, [editHole, activeHoleIndex]);

  useEffect(() => {
    if (!roundId) return;
    connectHub();
    fetchRound(roundId);
    fetchPaceData(roundId);
    fetchCourseStats(roundId);
    return () => {
      disconnectHub();
      clearRound();
    };
  }, [roundId, fetchRound, fetchPaceData, fetchCourseStats, clearRound]);

  useEffect(() => {
    if (round?.isCompleted && roundId) {
      fetchRoundStats(roundId);
    }
  }, [round?.isCompleted, roundId, fetchRoundStats]);

  const myScore: HoleScore | undefined = round
    ? round.playerScores.find((p) => p.playerName === username)?.scores[
        activeHoleIndex
      ]
    : undefined;

  const holeAlreadyScored = myScore ? myScore.strokes > 0 : false;
  const showPostScoreControls = holeAlreadyScored && !editHole;

  const currentStrokeSpecs = myScore
    ? holeAlreadyScored && !editHole
      ? myScore.strokeSpecs
      : effectiveStrokeBuffer.map((o) => ({ outcome: o, putDistance: null }))
    : [];

  const myStats = playerCourseStats?.find((s) => s.playerName === username);
  const currentHoleStats: HoleStats | undefined = myStats?.holeStats?.find(
    (h) => h.holeNumber === (myScore?.hole.number ?? 0),
  );

  const previousScores: HoleScore[] = currentHoleStats
    ? currentHoleStats.last10Scores.slice(0, 5)
    : [];

  const waitingFor: string[] =
    round && !holeAlreadyScored
      ? []
      : round
        ? round.playerScores
            .filter((p) => {
              const s = p.scores[activeHoleIndex];
              return s && s.strokes === 0;
            })
            .map((p) => p.playerName)
        : [];

  const handleOutcome = useCallback(
    (outcome: StrokeOutcome) => {
      if (!round || !roundId) return;
      if (holeAlreadyScored && !editHole) return;

      if (outcome === "Basket") {
        const finalOutcomes = [...effectiveStrokeBuffer, outcome];
        setStrokeBuffer(finalOutcomes);
        setStrokeBufferHole(activeHoleIndex);
        setScore(roundId, activeHoleIndex, finalOutcomes.length, finalOutcomes, username);
      } else {
        setStrokeBuffer([...effectiveStrokeBuffer, outcome]);
        setStrokeBufferHole(activeHoleIndex);
      }
    },
    [round, roundId, holeAlreadyScored, editHole, effectiveStrokeBuffer, activeHoleIndex, username, setScore],
  );

  const handleUndoLast = useCallback(() => {
    if (holeAlreadyScored && !editHole) return;
    setStrokeBuffer(effectiveStrokeBuffer.slice(0, -1));
    setStrokeBufferHole(activeHoleIndex);
  }, [holeAlreadyScored, editHole, effectiveStrokeBuffer, activeHoleIndex]);

  const handleSelectPlayer = useCallback(() => {}, []);

  if (isLoading || !round) {
    return (
      <div className="flex-1 flex items-center justify-center bg-[var(--color-bg)]">
        <p className="text-[var(--color-text-muted)] text-base font-medium">
          Loading round...
        </p>
      </div>
    );
  }

  if (round.isCompleted) {
    return (
      <RoundSummary
        round={round}
        stats={finishedRoundStats}
        onBackToHome={() => navigate("/")}
      />
    );
  }

  const currentHole = myScore?.hole;
  const totalHoles = round.playerScores[0]?.scores.length ?? 18;

  return (
    <div className="flex-1 flex flex-col bg-[var(--color-bg)] h-full relative">
      <StatusStrip
        holeNumber={currentHole?.number ?? activeHoleIndex + 1}
        holePar={currentHole?.par ?? 3}
        holeDistance={currentHole?.distance ?? 0}
        currentPace={currentPace}
        holeStats={currentHoleStats}
        previousScores={previousScores}
        activeHoleIndex={activeHoleIndex}
        totalHoles={totalHoles}
        setActiveHole={setActiveHole}
        onMenuOpen={() => setMenuOpen(true)}
      />

      <div className="flex-1 flex flex-col overflow-y-auto">
        <PlayerList
          players={round.playerScores}
          activeHoleIndex={activeHoleIndex}
          currentUsername={username}
          onSelectPlayer={handleSelectPlayer}
        />

        <StrokeTrail
          strokeSpecs={currentStrokeSpecs}
          onUndoLast={handleUndoLast}
        />
      </div>

      {showPostScoreControls ? (
        <div className="p-4 pb-6 flex gap-3 animate-slide-up">
           <button 
             onClick={() => setEditHole(true)}
             className="flex-1 py-4 px-2 rounded-2xl border-2 border-[var(--color-accent)] text-[var(--color-accent)] font-bold active:scale-[0.98] shadow-sm bg-[var(--color-bg)]"
           >
             Edit Score
           </button>
           <button 
             onClick={() => goToNextPersonalHole(username)}
             className="flex-[2] py-4 px-2 rounded-2xl bg-[var(--color-accent)] text-white font-bold active:scale-[0.98] shadow-lg shadow-[var(--color-shadow)] flex items-center justify-center gap-2"
           >
             Next Hole 
             <svg width="20" height="20" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="3" strokeLinecap="round" strokeLinejoin="round"><path d="M5 12h14M12 5l7 7-7 7"/></svg>
           </button>
        </div>
      ) : (
        <ScoringGrid
          onOutcome={handleOutcome}
          disabled={holeAlreadyScored && !editHole}
        />
      )}

      <BottomBar
        onScorecard={() => setScorecardOpen(true)}
        onStats={() => {
          if (roundId) fetchRoundStats(roundId);
          setStatsOpen(true);
        }}
      />

      {holeAlreadyScored && waitingFor.length > 0 && !effectiveWaitingDismissed && (
        <WaitingSheet
          waitingFor={waitingFor}
          onDismiss={() => {
            setWaitingDismissed(true);
            setStrokeBufferHole(activeHoleIndex);
          }}
        />
      )}

      {scorecardOpen && (
        <ScorecardOverlay
          round={round}
          onClose={() => setScorecardOpen(false)}
        />
      )}

      {statsOpen && (
        <StatsDialog
          stats={finishedRoundStats}
          onClose={() => setStatsOpen(false)}
        />
      )}
      
      {menuOpen && roundId && (
        <RoundMenu 
          roundId={roundId}
          currentHoleNumber={currentHole?.number ?? activeHoleIndex + 1}
          onClose={() => setMenuOpen(false)}
        />
      )}
    </div>
  );
}
