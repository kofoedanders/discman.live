import type { StrokeSpec } from "../../types";

interface StrokeTrailProps {
  strokeSpecs: StrokeSpec[];
  onUndoLast: () => void;
}

const outcomeLabel: Record<string, string> = {
  Fairway: "F",
  Rough: "R",
  OB: "OB",
  Circle2: "20m",
  Circle1: "10m",
  Basket: "●",
};

const outcomeColor: Record<string, string> = {
  Fairway: "bg-[var(--color-fairway)] text-white",
  Rough: "bg-[var(--color-rough)] text-white",
  OB: "bg-[var(--color-ob)] text-white",
  Circle2: "bg-[var(--color-circle2)] text-white",
  Circle1: "bg-[var(--color-circle1)] text-white",
  Basket: "bg-[var(--color-basket)] text-white",
};

export default function StrokeTrail({
  strokeSpecs,
  onUndoLast,
}: StrokeTrailProps) {
  if (strokeSpecs.length === 0) return null;

  return (
    <div className="px-4 py-3 flex items-center gap-1 overflow-x-auto no-scrollbar">
      {strokeSpecs.map((spec, i) => {
        const isLast = i === strokeSpecs.length - 1;
        return (
          <div key={i} className="flex items-center">
            {i > 0 && (
              <svg width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="3" className="text-[var(--color-border)] opacity-20 mx-1">
                <path d="M5 12h14m-4 4l4-4-4-4" />
              </svg>
            )}
            <button
              disabled={!isLast}
              onClick={isLast ? onUndoLast : undefined}
              className={`px-3 py-1.5 rounded-lg text-xs font-bold shadow-sm transition-all flex items-center justify-center min-w-[2.5rem] ${
                outcomeColor[spec.outcome] ?? "bg-gray-300"
              } ${isLast ? "ring-2 ring-[var(--color-accent)] ring-offset-2 ring-offset-[var(--color-bg)] scale-110 shadow-md" : "opacity-90 grayscale-[0.3]"}`}
            >
              {outcomeLabel[spec.outcome] ?? spec.outcome}
            </button>
          </div>
        );
      })}
    </div>
  );
}
