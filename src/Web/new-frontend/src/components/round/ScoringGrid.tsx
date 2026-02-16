import type { StrokeOutcome } from "../../types";

interface ScoringGridProps {
  onOutcome: (outcome: StrokeOutcome) => void;
  disabled: boolean;
}

interface GridButton {
  outcome: StrokeOutcome;
  label: string;
  sublabel: string;
  gradientClass: string;
}

const gridButtons: GridButton[] = [
  {
    outcome: "Rough",
    label: "R",
    sublabel: "Rough",
    gradientClass: "bg-gradient-to-br from-[var(--color-rough)] to-[#2d6a4f]",
  },
  {
    outcome: "Circle2",
    label: "20m",
    sublabel: "Circle 2",
    gradientClass: "bg-gradient-to-br from-[var(--color-circle2)] to-[#3a5a40]",
  },
  {
    outcome: "OB",
    label: "OB",
    sublabel: "",
    gradientClass: "bg-gradient-to-br from-[var(--color-ob)] to-[#9b2226]",
  },
  {
    outcome: "Fairway",
    label: "F",
    sublabel: "Fairway",
    gradientClass: "bg-gradient-to-br from-[var(--color-fairway)] to-[#1b4332]",
  },
  {
    outcome: "Circle1",
    label: "10m",
    sublabel: "Circle 1",
    gradientClass: "bg-gradient-to-br from-[var(--color-circle1)] to-[#081c15]",
  },
  {
    outcome: "Basket",
    label: "●",
    sublabel: "Basket",
    gradientClass: "bg-gradient-to-br from-[var(--color-basket)] to-[#000000]",
  },
];

export default function ScoringGrid({ onOutcome, disabled }: ScoringGridProps) {
  return (
    <div className="px-4 py-3">
      <div className="grid grid-cols-3 gap-3">
        {gridButtons.map((btn) => (
          <button
            key={btn.outcome}
            disabled={disabled}
            onClick={() => onOutcome(btn.outcome)}
            className={`flex flex-col items-center justify-center py-5 rounded-2xl shadow-inner text-white transition-all active:scale-95 disabled:opacity-40 disabled:active:scale-100 ${
              btn.gradientClass
            } shadow-[inset_0_-2px_4px_rgba(0,0,0,0.2)] border border-white/10`}
          >
            <span className="text-2xl font-black drop-shadow-md">{btn.label}</span>
            {btn.sublabel && (
              <span className="text-[10px] font-bold mt-0.5 opacity-90 uppercase tracking-wide">
                {btn.sublabel}
              </span>
            )}
          </button>
        ))}
      </div>
    </div>
  );
}
