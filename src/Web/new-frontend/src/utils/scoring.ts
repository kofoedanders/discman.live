export function formatRelativeToPar(total: number): string {
  if (total === 0) return "E";
  return total > 0 ? `+${total}` : `${total}`;
}

export function scoreColorClass(relativeToPar: number): string {
  if (relativeToPar < 0) return "text-[var(--color-birdie)] font-bold";
  if (relativeToPar > 0) return "text-[var(--color-bogey)] font-bold";
  return "text-[var(--color-par)]";
}

export function formatTime(date: Date): string {
  return date.toLocaleTimeString([], { hour: "2-digit", minute: "2-digit" });
}

export function totalRelativeToPar(
  scores: { strokes: number; hole: { par: number } }[],
): number {
  return scores.reduce((acc, s) => {
    if (s.strokes === 0) return acc;
    return acc + (s.strokes - s.hole.par);
  }, 0);
}
