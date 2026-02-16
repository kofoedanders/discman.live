import { useNavigate } from "react-router-dom";
import { useRoundStore } from "../../stores/roundStore";

interface RoundMenuProps {
  roundId: string;
  currentHoleNumber: number;
  onClose: () => void;
}

export default function RoundMenu({
  roundId,
  currentHoleNumber,
  onClose,
}: RoundMenuProps) {
  const navigate = useNavigate();
  const completeRound = useRoundStore((s) => s.completeRound);
  const skipHole = useRoundStore((s) => s.skipHole);
  const leaveRound = useRoundStore((s) => s.leaveRound);
  const deleteRound = useRoundStore((s) => s.deleteRound);

  const handleComplete = async () => {
    if (confirm("Are you sure you want to complete this round?")) {
      await completeRound(roundId, "");
      onClose();
    }
  };

  const handleSkip = async () => {
    if (confirm(`Skip hole ${currentHoleNumber}?`)) {
      await skipHole(roundId, currentHoleNumber);
      onClose();
    }
  };

  const handleLeave = async () => {
    if (confirm("Leave this round? You can rejoin later.")) {
      await leaveRound(roundId);
      navigate("/");
    }
  };

  const handleDelete = async () => {
    if (confirm("DELETE this round? This cannot be undone.")) {
      await deleteRound(roundId);
      navigate("/");
    }
  };

  return (
    <div 
      className="fixed inset-0 z-50 flex items-end justify-center bg-black/50 backdrop-blur-sm" 
      onClick={onClose}
    >
      <div
        className="w-full bg-[var(--color-surface)] rounded-t-2xl p-4 shadow-xl animate-slide-up pb-8"
        onClick={(e) => e.stopPropagation()}
      >
        <div className="flex justify-center mb-6">
          <div className="w-12 h-1.5 bg-[var(--color-border)] rounded-full opacity-20" />
        </div>
        
        <div className="space-y-3">
          <button
            onClick={handleComplete}
            className="w-full p-4 text-left font-bold text-[var(--color-text)] bg-[var(--color-bg)] rounded-xl active:scale-[0.98] shadow-sm flex items-center gap-3 transition-transform"
          >
            <span className="text-xl">🏁</span> Complete Round
          </button>
          
          <button
            onClick={handleSkip}
            className="w-full p-4 text-left font-bold text-[var(--color-text)] bg-[var(--color-bg)] rounded-xl active:scale-[0.98] shadow-sm flex items-center gap-3 transition-transform"
          >
            <span className="text-xl">⏭</span> Skip Hole {currentHoleNumber}
          </button>
          
          <div className="h-px bg-[var(--color-border)] opacity-10 my-2" />
          
          <button
            onClick={handleLeave}
            className="w-full p-4 text-left font-bold text-[var(--color-destructive)] bg-[var(--color-bg)] rounded-xl active:scale-[0.98] shadow-sm flex items-center gap-3 transition-transform"
          >
            <span className="text-xl">👋</span> Leave Round
          </button>
          
          <button
            onClick={handleDelete}
            className="w-full p-4 text-left font-bold text-[var(--color-destructive)] bg-[var(--color-bg)] rounded-xl active:scale-[0.98] shadow-sm flex items-center gap-3 transition-transform"
          >
            <span className="text-xl">🗑</span> Delete Round
          </button>
        </div>

        <button
          onClick={onClose}
          className="w-full mt-6 p-4 font-bold text-[var(--color-text-muted)] text-center active:opacity-70 transition-opacity"
        >
          Cancel
        </button>
      </div>
    </div>
  );
}
