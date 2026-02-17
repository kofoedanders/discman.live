import { useState, useEffect } from "react";
import { useNavigate, useSearchParams } from "react-router-dom";
import { useCourseStore } from "../stores/courseStore";
import DashboardLayout from "../components/layout/DashboardLayout";

export default function CreateCoursePage() {
  const navigate = useNavigate();
  const [searchParams] = useSearchParams();
  const existingCourseName = searchParams.get("course");
  const store = useCourseStore();

  const [courseName, setCourseName] = useState(existingCourseName || "");
  const [layoutName, setLayoutName] = useState("");
  const [numberOfHoles, setNumberOfHoles] = useState(18);
  const [par4s, setPar4s] = useState<Set<number>>(new Set());
  const [par5s, setPar5s] = useState<Set<number>>(new Set());
  const [isSubmitting, setIsSubmitting] = useState(false);

  useEffect(() => {
    if (existingCourseName) {
      setCourseName(existingCourseName);
    }
  }, [existingCourseName]);

  const togglePar4 = (hole: number) => {
    const newPar4s = new Set(par4s);
    if (newPar4s.has(hole)) {
      newPar4s.delete(hole);
    } else {
      newPar4s.add(hole);
      // Ensure it's not also par 5
      if (par5s.has(hole)) {
        const newPar5s = new Set(par5s);
        newPar5s.delete(hole);
        setPar5s(newPar5s);
      }
    }
    setPar4s(newPar4s);
  };

  const togglePar5 = (hole: number) => {
    const newPar5s = new Set(par5s);
    if (newPar5s.has(hole)) {
      newPar5s.delete(hole);
    } else {
      newPar5s.add(hole);
      // Ensure it's not also par 4
      if (par4s.has(hole)) {
        const newPar4s = new Set(par4s);
        newPar4s.delete(hole);
        setPar4s(newPar4s);
      }
    }
    setPar5s(newPar5s);
  };

  const handleSubmit = async (e: React.SyntheticEvent) => {
    e.preventDefault();
    if (!courseName.trim()) return;

    setIsSubmitting(true);
    try {
      const finalLayoutName = layoutName.trim() || (existingCourseName ? "New Layout" : "Main Layout");

      await store.createCourse({
        courseName: courseName,
        layoutName: finalLayoutName,
        latitude: 0,
        longitude: 0,
        numberOfHoles: numberOfHoles,
        par4s: Array.from(par4s),
        par5s: Array.from(par5s),
      });

      navigate(`/courses/${encodeURIComponent(courseName)}`);
    } catch (error) {
      console.error("Failed to create course", error);
      setIsSubmitting(false);
    }
  };

  const holeNumbers = Array.from({ length: numberOfHoles }, (_, i) => i + 1);

  return (
    <DashboardLayout title={existingCourseName ? "⛳ New Layout" : "⛳ New Course"}>
      <form onSubmit={handleSubmit} className="flex flex-col gap-6 max-w-2xl mx-auto" data-testid="create-course-page">
        {/* Course Name */}
        <div>
          <label className="block text-sm font-bold text-[var(--color-text)] mb-1">
            Course Name
          </label>
          <input
            type="text"
            value={courseName}
            onChange={(e) => setCourseName(e.target.value)}
            disabled={!!existingCourseName}
            className="w-full px-3 py-2 rounded-lg bg-[var(--color-surface)] text-[var(--color-text)] text-sm border border-[var(--color-border)]/20 disabled:opacity-50 disabled:cursor-not-allowed"
            placeholder="e.g. Valbyparken"
            required
          />
        </div>

        {/* Layout Name - Show if adding to existing course */}
        {existingCourseName && (
          <div>
            <label className="block text-sm font-bold text-[var(--color-text)] mb-1">
              Layout Name
            </label>
            <input
              type="text"
              value={layoutName}
              onChange={(e) => setLayoutName(e.target.value)}
              className="w-full px-3 py-2 rounded-lg bg-[var(--color-surface)] text-[var(--color-text)] text-sm border border-[var(--color-border)]/20"
              placeholder="e.g. 2024 Layout"
              required
            />
          </div>
        )}

        {/* Number of Holes */}
        <div>
          <label className="block text-sm font-bold text-[var(--color-text)] mb-1">
            Number of Holes
          </label>
          <select
            value={numberOfHoles}
            onChange={(e) => {
                const newCount = parseInt(e.target.value);
                setNumberOfHoles(newCount);
                // Clear pars for holes that no longer exist
                const newPar4s = new Set(Array.from(par4s).filter(h => h <= newCount));
                const newPar5s = new Set(Array.from(par5s).filter(h => h <= newCount));
                setPar4s(newPar4s);
                setPar5s(newPar5s);
            }}
            className="px-3 py-2 rounded-lg bg-[var(--color-surface)] text-[var(--color-text)] text-sm font-semibold border border-[var(--color-border)]/20 w-full"
          >
            {Array.from({ length: 25 }, (_, i) => i + 5).map((num) => (
              <option key={num} value={num}>
                {num} Holes
              </option>
            ))}
          </select>
        </div>

        {/* Par 4 Selector */}
        <div>
          <label className="block text-sm font-bold text-[var(--color-text)] mb-2">
            Select Par 4s
          </label>
          <div className="flex flex-wrap gap-2">
            {holeNumbers.map((hole) => {
               const isPar5 = par5s.has(hole);
               if (isPar5) return null; // Skip if it's par 5
               
               const isActive = par4s.has(hole);
               return (
                <button
                  key={`p4-${hole}`}
                  type="button"
                  onClick={() => togglePar4(hole)}
                  className={`w-8 h-8 flex items-center justify-center rounded-lg text-sm font-bold transition-all ${
                    isActive
                      ? "bg-[var(--color-accent)] text-white shadow-md transform scale-105"
                      : "bg-[var(--color-surface)] text-[var(--color-text-muted)] border border-[var(--color-border)]/20 hover:border-[var(--color-accent)]/50"
                  }`}
                >
                  {hole}
                </button>
              );
            })}
          </div>
          <p className="text-xs text-[var(--color-text-muted)] mt-1">
            Tap hole numbers to mark them as Par 4.
          </p>
        </div>

        {/* Par 5 Selector */}
        <div>
          <label className="block text-sm font-bold text-[var(--color-text)] mb-2">
            Select Par 5s
          </label>
          <div className="flex flex-wrap gap-2">
            {holeNumbers.map((hole) => {
               const isPar4 = par4s.has(hole);
               if (isPar4) return null; // Skip if it's par 4
               
               const isActive = par5s.has(hole);
               return (
                <button
                  key={`p5-${hole}`}
                  type="button"
                  onClick={() => togglePar5(hole)}
                  className={`w-8 h-8 flex items-center justify-center rounded-lg text-sm font-bold transition-all ${
                    isActive
                      ? "bg-[var(--color-accent)] text-white shadow-md transform scale-105"
                      : "bg-[var(--color-surface)] text-[var(--color-text-muted)] border border-[var(--color-border)]/20 hover:border-[var(--color-accent)]/50"
                  }`}
                >
                  {hole}
                </button>
              );
            })}
          </div>
          <p className="text-xs text-[var(--color-text-muted)] mt-1">
            Tap hole numbers to mark them as Par 5.
          </p>
        </div>

        {/* Submit Button */}
        <div className="pt-4">
          <button
            type="submit"
            disabled={isSubmitting || (!!existingCourseName && !layoutName)}
            className="w-full px-6 py-3 rounded-xl bg-[var(--color-accent)] text-white text-sm font-bold shadow-lg shadow-[var(--color-accent)]/20 active:scale-95 transition-all disabled:opacity-50 disabled:shadow-none hover:brightness-110"
          >
            {isSubmitting ? "Creating..." : existingCourseName ? "Create Layout" : "Create Course"}
          </button>
        </div>
      </form>
    </DashboardLayout>
  );
}
