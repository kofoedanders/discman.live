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
    <div className="px-3 py-2 flex items-center gap-1 overflow-x-auto">
      {strokeSpecs.map((spec, i) => {
        const isLast = i === strokeSpecs.length - 1;
        return (
          <div key={i} className="flex items-center gap-1">
            {i > 0 && (
              <span className="text-xs text-[var(--color-text-muted)]">→</span>
            )}
            <button
              disabled={!isLast}
              onClick={isLast ? onUndoLast : undefined}
              className={`px-2 py-1 rounded text-xs font-bold ${
                outcomeColor[spec.outcome] ?? "bg-gray-300"
              } ${isLast ? "ring-2 ring-[var(--color-border)]" : ""}`}
            >
              {outcomeLabel[spec.outcome] ?? spec.outcome}
            </button>
          </div>
        );
      })}
    </div>
  );
}
