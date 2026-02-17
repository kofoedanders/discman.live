import { useEffect } from "react";
import { useParams, Link } from "react-router-dom";
import { useCourseStore, groupCoursesByName } from "../stores/courseStore";
import DashboardLayout from "../components/layout/DashboardLayout";

function chunkArray<T>(arr: T[], size: number): T[][] {
  const chunks: T[][] = [];
  for (let i = 0; i < arr.length; i += size) {
    chunks.push(arr.slice(i, i + size));
  }
  return chunks;
}

export default function CourseDetailPage() {
  const { courseName } = useParams<{ courseName: string }>();
  const store = useCourseStore();
  const decodedName = decodeURIComponent(courseName || "");

  useEffect(() => {
    if (decodedName) {
      store.fetchCourses(decodedName);
    }
  }, [decodedName, store.fetchCourses]);

  const grouped = groupCoursesByName(store.courses);
  const variants = grouped.get(decodedName) || [];

  return (
    <DashboardLayout title={`⛳ ${decodedName}`}>
      <div className="flex flex-col gap-6" data-testid="course-detail-page">
        {/* Actions Bar */}
        <div className="flex justify-between items-center">
          <Link
            to="/courses"
            className="text-sm font-bold text-[var(--color-text-muted)] hover:text-[var(--color-text)] transition-colors"
          >
            &larr; Back to Courses
          </Link>
          <Link
            to={`/courses/new?course=${encodeURIComponent(decodedName)}`}
            className="px-4 py-2 bg-[var(--color-accent)] text-white text-sm font-bold rounded-lg shadow-sm active:scale-95 transition-transform hover:brightness-110"
          >
            + New Layout
          </Link>
        </div>

        {store.isLoading ? (
          <div className="text-center py-12 text-[var(--color-text-muted)]">Loading course details...</div>
        ) : variants.length === 0 ? (
          <div className="text-center py-12 text-red-500">Course not found (or no layouts).</div>
        ) : (
          <div className="flex flex-col gap-8">
            {variants.map((variant) => (
              <div key={variant.id} className="p-4 rounded-2xl bg-[var(--color-surface)] border border-[var(--color-border)]/10 shadow-sm">
                <div className="flex justify-between items-baseline mb-4">
                  <h2 className="text-xl font-bold text-[var(--color-text)]">{variant.layout}</h2>
                  <div className="text-sm text-[var(--color-text-muted)]">
                    {variant.holes.length} holes • Par {variant.holes.reduce((sum, h) => sum + h.par, 0)} • {variant.courseStats?.roundsOnCourse || 0} rounds
                  </div>
                </div>

                {/* Hole Table Chunks */}
                {chunkArray(variant.holes, 6).map((chunk, i) => (
                  <div key={i} className="rounded-xl overflow-hidden bg-[var(--color-background)]/50 shadow-sm shadow-[var(--color-shadow)] mb-4 border border-[var(--color-border)]/5">
                    <table className="w-full text-sm">
                      <thead>
                        <tr className="border-b border-[var(--color-border)]/20 bg-[var(--color-surface)]">
                          <th className="py-2 px-3 text-left font-bold text-[var(--color-text-muted)] text-xs uppercase tracking-wider">Hole</th>
                          {chunk.map((h) => (
                            <th key={h.number} className="py-2 px-3 text-center font-bold text-[var(--color-text-muted)] text-xs">
                              {h.number}
                            </th>
                          ))}
                        </tr>
                      </thead>
                      <tbody>
                        <tr className="border-b border-[var(--color-border)]/10 hover:bg-[var(--color-surface)] transition-colors">
                          <td className="py-2 px-3 font-semibold text-[var(--color-text)]">Par</td>
                          {chunk.map((h) => (
                            <td key={h.number} className="py-2 px-3 text-center text-[var(--color-text)]">
                              {h.par}
                            </td>
                          ))}
                        </tr>
                        <tr className="border-b border-[var(--color-border)]/10 hover:bg-[var(--color-surface)] transition-colors">
                          <td className="py-2 px-3 font-medium text-[var(--color-text-muted)]">Dist</td>
                          {chunk.map((h) => (
                            <td key={h.number} className="py-2 px-3 text-center text-[var(--color-text-muted)] text-xs">
                              {h.distance}m
                            </td>
                          ))}
                        </tr>
                        <tr className="border-b border-[var(--color-border)]/10 hover:bg-[var(--color-surface)] transition-colors">
                          <td className="py-2 px-3 font-medium text-[var(--color-text-muted)]">Avg</td>
                          {chunk.map((h) => (
                            <td key={h.number} className="py-2 px-3 text-center text-[var(--color-text-muted)] text-xs">
                              {h.average.toFixed(1)}
                            </td>
                          ))}
                        </tr>
                        <tr className="hover:bg-[var(--color-surface)] transition-colors">
                          <td className="py-2 px-3 font-medium text-[var(--color-text-muted)]">Rating</td>
                          {chunk.map((h) => (
                            <td key={h.number} className="py-2 px-3 text-center text-[var(--color-text-muted)] text-xs">
                              {h.rating}
                            </td>
                          ))}
                        </tr>
                      </tbody>
                    </table>
                  </div>
                ))}
              </div>
            ))}
          </div>
        )}
      </div>
    </DashboardLayout>
  );
}
