import { useEffect, useState } from "react";
import { Link } from "react-router-dom";
import { useCourseStore, groupCoursesByName } from "../stores/courseStore";
import DashboardLayout from "../components/layout/DashboardLayout";

export default function CoursesPage() {
  const store = useCourseStore();
  const [filter, setFilter] = useState("");

  useEffect(() => {
    // Logic: fetch if empty or > 2 chars
    if (filter.length === 0 || filter.length > 2) {
      store.fetchCourses(filter);
    }
  }, [filter, store.fetchCourses]);

  // Group courses by name to display unique entries
  const groupedCourses = groupCoursesByName(store.courses);
  const uniqueNames = Array.from(groupedCourses.keys());

  return (
    <DashboardLayout title="⛳ Courses">
      <div className="flex flex-col gap-4" data-testid="courses-page">
        {/* Actions Bar */}
        <div className="flex justify-between items-center mb-2">
          <Link
            to="/courses/new"
            className="px-4 py-2 bg-[var(--color-accent)] text-white text-sm font-bold rounded-lg shadow-sm active:scale-95 transition-transform hover:brightness-110"
          >
            + New Course
          </Link>
        </div>

        {/* Search Input */}
        <div className="relative mb-4">
          <input
            type="text"
            placeholder="Search courses..."
            value={filter}
            onChange={(e) => setFilter(e.target.value)}
            className="w-full px-4 py-3 rounded-xl bg-[var(--color-surface)] text-[var(--color-text)] border border-[var(--color-border)]/20 shadow-sm focus:outline-none focus:ring-2 focus:ring-[var(--color-accent)]/50"
          />
        </div>

        {/* Content State */}
        {store.isLoading ? (
          <div className="text-center py-12 text-[var(--color-text-muted)]">Loading...</div>
        ) : store.error ? (
          <div className="text-center py-12 text-red-500">{store.error}</div>
        ) : uniqueNames.length === 0 ? (
          <div className="text-center py-12 text-[var(--color-text-muted)]">No courses found.</div>
        ) : (
          <div className="grid gap-4 sm:grid-cols-2 lg:grid-cols-3">
            {uniqueNames.map((name) => (
              <Link
                key={name}
                to={`/courses/${encodeURIComponent(name)}`}
                className="block p-4 rounded-xl bg-[var(--color-surface)] shadow-sm border border-[var(--color-border)]/10 hover:border-[var(--color-accent)]/50 transition-colors"
              >
                <div className="font-bold text-lg text-[var(--color-text)] mb-1">{name}</div>
                <div className="text-xs text-[var(--color-text-muted)]">
                  {groupedCourses.get(name)?.length || 0} layouts
                </div>
              </Link>
            ))}
          </div>
        )}
      </div>
    </DashboardLayout>
  );
}
