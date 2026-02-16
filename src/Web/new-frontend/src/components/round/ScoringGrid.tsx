import type { StrokeOutcome } from "../../types";

interface ScoringGridProps {
  onOutcome: (outcome: StrokeOutcome) => void;
  disabled: boolean;
}

interface GridButton {
  outcome: StrokeOutcome;
  label: string;
  sublabel: string;
  colorClass: string;
}

const gridButtons: GridButton[] = [
  {
    outcome: "Rough",
    label: "R",
    sublabel: "Rough",
    colorClass:
      "bg-[var(--color-rough)] text-white border-[var(--color-border)]",
  },
  {
    outcome: "Circle2",
    label: "20m",
    sublabel: "Circle 2",
    colorClass:
      "bg-[var(--color-circle2)] text-white border-[var(--color-border)]",
  },
  {
    outcome: "OB",
    label: "OB",
    sublabel: "",
    colorClass: "bg-[var(--color-ob)] text-white border-[var(--color-border)]",
  },
  {
    outcome: "Fairway",
    label: "F",
    sublabel: "Fairway",
    colorClass:
      "bg-[var(--color-fairway)] text-white border-[var(--color-border)]",
  },
  {
    outcome: "Circle1",
    label: "10m",
    sublabel: "Circle 1",
    colorClass:
      "bg-[var(--color-circle1)] text-white border-[var(--color-border)]",
  },
  {
    outcome: "Basket",
    label: "●",
    sublabel: "Basket",
    colorClass:
      "bg-[var(--color-basket)] text-white border-[var(--color-border)]",
  },
];

export default function ScoringGrid({ onOutcome, disabled }: ScoringGridProps) {
  return (
    <div className="px-3 py-2">
      <div className="grid grid-cols-3 gap-2">
        {gridButtons.map((btn) => (
          <button
            key={btn.outcome}
            disabled={disabled}
            onClick={() => onOutcome(btn.outcome)}
            className={`flex flex-col items-center justify-center py-4 rounded-xl border-2 font-bold active:opacity-70 transition-opacity ${
              btn.colorClass
            } ${disabled ? "opacity-40" : ""}`}
          >
            <span className="text-xl font-black">{btn.label}</span>
            {btn.sublabel && (
              <span className="text-xs font-semibold mt-0.5 opacity-80">
                {btn.sublabel}
              </span>
            )}
          </button>
        ))}
      </div>
    </div>
  );
}
