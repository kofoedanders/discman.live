import React, { useState } from "react";
import { useNavigate } from "react-router-dom";
import { useTournamentStore } from "../stores/tournamentStore";
import DashboardLayout from "../components/layout/DashboardLayout";

export default function CreateTournamentPage() {
  const navigate = useNavigate();
  const { createTournament } = useTournamentStore();
  const [name, setName] = useState("");
  const [start, setStart] = useState("");
  const [end, setEnd] = useState("");
  const [isSubmitting, setIsSubmitting] = useState(false);

  const handleSubmit = async (e: React.SyntheticEvent) => {
    e.preventDefault();
    if (!name || !start || !end) return;

    setIsSubmitting(true);
    try {
      // Inputs are YYYY-MM-DD strings
      const startDate = new Date(start).toISOString();
      const endDate = new Date(end).toISOString();
      
      const id = await createTournament(name, startDate, endDate);
      navigate(`/tournaments/${id}`);
    } catch (error) {
      console.error("Failed to create tournament", error);
      setIsSubmitting(false);
    }
  };

  const isFormValid = name.trim().length > 0 && start.length > 0 && end.length > 0;

  return (
    <DashboardLayout title="New Tournament">
      <div className="max-w-2xl mx-auto space-y-6" data-testid="create-tournament-page">
        <h1 className="text-2xl font-bold text-[var(--color-text)]">Create New Tournament</h1>
        
        <form onSubmit={handleSubmit} className="space-y-4 bg-[var(--color-surface)] p-6 rounded-xl shadow-sm shadow-[var(--color-shadow)]">
          <div>
            <label htmlFor="name" className="block text-sm font-medium text-[var(--color-text-muted)] mb-1">
              Tournament Name
            </label>
            <input
              id="name"
              type="text"
              value={name}
              onChange={(e) => setName(e.target.value)}
              className="w-full px-3 py-2 rounded-lg bg-[var(--color-bg)] border border-[var(--color-border)] text-[var(--color-text)] focus:outline-none focus:ring-2 focus:ring-[var(--color-accent)]"
              placeholder="Summer Championship 2024"
              required
            />
          </div>

          <div className="grid grid-cols-2 gap-4">
            <div>
              <label htmlFor="start" className="block text-sm font-medium text-[var(--color-text-muted)] mb-1">
                Start Date
              </label>
              <input
                id="start"
                type="date"
                value={start}
                onChange={(e) => setStart(e.target.value)}
                className="w-full px-3 py-2 rounded-lg bg-[var(--color-bg)] border border-[var(--color-border)] text-[var(--color-text)] focus:outline-none focus:ring-2 focus:ring-[var(--color-accent)]"
                required
              />
            </div>

            <div>
              <label htmlFor="end" className="block text-sm font-medium text-[var(--color-text-muted)] mb-1">
                End Date
              </label>
              <input
                id="end"
                type="date"
                value={end}
                onChange={(e) => setEnd(e.target.value)}
                className="w-full px-3 py-2 rounded-lg bg-[var(--color-bg)] border border-[var(--color-border)] text-[var(--color-text)] focus:outline-none focus:ring-2 focus:ring-[var(--color-accent)]"
                required
              />
            </div>
          </div>

          <div className="pt-4 flex justify-end">
            <button
              type="submit"
              disabled={!isFormValid || isSubmitting}
              className={`px-6 py-2 rounded-lg bg-[var(--color-accent)] text-white text-sm font-bold active:scale-95 transition-transform ${
                (!isFormValid || isSubmitting) ? "opacity-50 cursor-not-allowed" : "hover:opacity-90"
              }`}
            >
              {isSubmitting ? "Creating..." : "Create Tournament"}
            </button>
          </div>
        </form>
      </div>
    </DashboardLayout>
  );
}
