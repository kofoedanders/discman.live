interface WaitingSheetProps {
  waitingFor: string[];
  onDismiss: () => void;
}

export default function WaitingSheet({
  waitingFor,
  onDismiss,
}: WaitingSheetProps) {
  if (waitingFor.length === 0) return null;

  return (
    <div className="fixed inset-0 z-50 flex items-end justify-center">
      <div
        className="absolute inset-0 bg-black/30 backdrop-blur-sm transition-opacity"
        onClick={onDismiss}
      />
      <div className="relative w-full bg-[var(--color-surface)] rounded-t-2xl px-6 py-6 shadow-2xl animate-slide-up pb-safe">
        <div className="w-12 h-1.5 rounded-full bg-[var(--color-border)] opacity-20 mx-auto mb-6" />
        
        <h3 className="text-base font-bold text-[var(--color-text)] mb-4 flex items-center gap-2">
          <span className="w-2 h-2 rounded-full bg-[var(--color-accent)] animate-pulse"></span>
          Waiting for players...
        </h3>
        
        <div className="space-y-3">
          {waitingFor.map((name) => (
            <div
              key={name}
              className="flex items-center gap-3 p-3 rounded-xl bg-[var(--color-bg)] shadow-sm"
            >
              <div className="w-8 h-8 rounded-full bg-[var(--color-accent)]/10 flex items-center justify-center text-sm">
                ⏳
              </div>
              <span className="text-sm font-bold text-[var(--color-text)]">
                {name}
              </span>
              <div className="flex gap-1 ml-auto">
                <span className="w-1.5 h-1.5 rounded-full bg-[var(--color-text-muted)]/30 animate-bounce delay-0"></span>
                <span className="w-1.5 h-1.5 rounded-full bg-[var(--color-text-muted)]/30 animate-bounce delay-100"></span>
                <span className="w-1.5 h-1.5 rounded-full bg-[var(--color-text-muted)]/30 animate-bounce delay-200"></span>
              </div>
            </div>
          ))}
        </div>
        
        <button 
          onClick={onDismiss}
          className="w-full mt-6 py-3 text-sm font-bold text-[var(--color-text-muted)] opacity-60 active:opacity-100"
        >
          Dismiss
        </button>
      </div>
    </div>
  );
}
