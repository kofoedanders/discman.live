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
    <div className="fixed inset-x-0 bottom-0 z-30">
      <div
        className="absolute inset-0 bg-black/20"
        onClick={onDismiss}
      />
      <div className="relative bg-[var(--color-surface)] border-t-2 border-[var(--color-border)] rounded-t-2xl px-4 py-4 shadow-lg">
        <div className="w-10 h-1 rounded-full bg-[var(--color-border)] mx-auto mb-3" />
        <p className="text-sm font-bold text-[var(--color-text)] mb-2">
          Waiting for
        </p>
        <div className="space-y-1">
          {waitingFor.map((name) => (
            <p
              key={name}
              className="text-sm text-[var(--color-text-muted)] font-medium"
            >
              {name}
            </p>
          ))}
        </div>
      </div>
    </div>
  );
}
