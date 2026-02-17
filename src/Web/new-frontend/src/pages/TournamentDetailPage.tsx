import { useEffect, useState } from "react";
import { useParams, Link } from "react-router-dom";
import { useAuthStore } from "../stores/authStore";
import { useTournamentStore } from "../stores/tournamentStore";
import { useCourseStore, groupCoursesByName } from "../stores/courseStore";
import DashboardLayout from "../components/layout/DashboardLayout";
import type { TournamentPrices } from "../types";

function formatDate(iso: string): string {
  const d = new Date(iso);
  const day = String(d.getDate()).padStart(2, "0");
  const month = d.toLocaleString("en", { month: "short" }).toLowerCase();
  return `${day}. ${month}`;
}

export default function TournamentDetailPage() {
  const { tournamentId } = useParams<{ tournamentId: string }>();
  const { selectedTournament: tournament, isLoading, error, fetchTournament, joinTournament, addCourse, calculatePrices } = useTournamentStore();
  const username = useAuthStore(s => s.user?.username ?? "");
  const { courses: searchCourses, fetchCourses } = useCourseStore();

  const [tab, setTab] = useState<'info' | 'leaderboard' | 'prices'>('info');
  const [isAddingCourse, setIsAddingCourse] = useState(false);
  const [courseSearch, setCourseSearch] = useState("");
  const [selectedCourseName, setSelectedCourseName] = useState<string | null>(null);
  const [selectedCourseId, setSelectedCourseId] = useState<string | null>(null);
  const [isCalculateConfirmOpen, setIsCalculateConfirmOpen] = useState(false);
  const [copied, setCopied] = useState(false);

  useEffect(() => {
    if (tournamentId) {
      fetchTournament(tournamentId);
    }
  }, [tournamentId, fetchTournament]);

  useEffect(() => {
    if (tournament?.info.hasStarted) {
      setTab('leaderboard');
    }
  }, [tournament?.info.hasStarted]);

  useEffect(() => {
    if (courseSearch.length > 2) {
      fetchCourses(courseSearch);
    }
  }, [courseSearch, fetchCourses]);

  if (isLoading || !tournament) {
    return (
      <DashboardLayout title="Loading...">
        <div className="flex justify-center p-8">
          <div className="animate-spin rounded-full h-8 w-8 border-b-2 border-[var(--color-accent)]"></div>
        </div>
      </DashboardLayout>
    );
  }

  if (error) {
    return (
      <DashboardLayout title="Error">
        <div className="p-4 text-red-500 bg-red-100 rounded-lg">{error}</div>
      </DashboardLayout>
    );
  }

  const isAdmin = tournament.info.admins.includes(username);
  const isPlayer = tournament.info.players.includes(username);
  
  const handleJoin = () => {
    if (tournamentId && username) {
      void joinTournament(tournamentId, username);
    }
  };

  const handleCopyLink = () => {
    void navigator.clipboard.writeText(window.location.origin + "/tournaments/" + tournamentId);
    setCopied(true);
    setTimeout(() => setCopied(false), 2000);
  };

  const handleAddCourse = async () => {
    if (tournamentId && selectedCourseId) {
      await addCourse(tournamentId, selectedCourseId);
      setIsAddingCourse(false);
      setSelectedCourseName(null);
      setSelectedCourseId(null);
      setCourseSearch("");
    }
  };

  const handleCalculatePrices = async () => {
    if (tournamentId) {
      await calculatePrices(tournamentId);
      setIsCalculateConfirmOpen(false);
    }
  };

  const groupedCourses = groupCoursesByName(searchCourses);

  return (
    <DashboardLayout title={tournament.info.name}>
      <div className="space-y-6" data-testid="tournament-detail-page">
        <h1 className="text-2xl font-bold text-[var(--color-text)]">{tournament.info.name}</h1>

        <div className="flex border-b border-[var(--color-border)]/20 mb-4">
          {(['info', 'leaderboard', 'prices'] as const).map((t) => (
            <button
              key={t}
              onClick={() => setTab(t)}
              className={`flex-1 py-2 text-sm font-bold text-center transition-colors capitalize ${
                tab === t
                  ? 'text-[var(--color-accent)] border-b-2 border-[var(--color-accent)]'
                  : 'text-[var(--color-text-muted)]'
              }`}
            >
              {t}
            </button>
          ))}
        </div>

        {tab === 'info' && (
          <div className="space-y-6">
            {!tournament.info.isCompleted && !isPlayer && (
              <div className="flex justify-center">
                <button onClick={handleJoin} className="px-6 py-2 rounded-lg bg-[var(--color-accent)] text-white text-sm font-bold active:scale-95 transition-transform">
                  Sign up!
                </button>
              </div>
            )}

            <div className="bg-[var(--color-surface)] p-4 rounded-xl shadow-sm shadow-[var(--color-shadow)]">
               <h3 className="text-sm font-bold text-[var(--color-text-muted)] mb-2 uppercase">Date</h3>
               <p className="text-[var(--color-text)]">
                 {formatDate(tournament.info.start)} - {formatDate(tournament.info.end)}
               </p>
            </div>

            <div className="bg-[var(--color-surface)] p-4 rounded-xl shadow-sm shadow-[var(--color-shadow)] space-y-4">
              <div className="flex justify-between items-center">
                <h3 className="text-sm font-bold text-[var(--color-text-muted)] uppercase">Courses</h3>
                {isAdmin && !tournament.info.isCompleted && (
                  <button 
                    onClick={() => setIsAddingCourse(!isAddingCourse)}
                    className="px-3 py-1 rounded-lg bg-[var(--color-accent)] text-white text-xs font-bold active:scale-95 transition-transform"
                  >
                    Add
                  </button>
                )}
              </div>

              {isAddingCourse && (
                <div className="p-4 border border-[var(--color-border)] rounded-lg space-y-3">
                   <input 
                      type="text" 
                      placeholder="Search course..." 
                      value={courseSearch}
                      onChange={e => setCourseSearch(e.target.value)}
                      className="w-full px-3 py-2 rounded-lg bg-[var(--color-bg)] border border-[var(--color-border)] text-[var(--color-text)]"
                   />
                   {courseSearch.length > 2 && (
                     <div className="max-h-40 overflow-y-auto space-y-1">
                       {Array.from(groupedCourses.keys()).map(name => (
                         <div 
                           key={name}
                           onClick={() => setSelectedCourseName(name)}
                           className={`p-2 rounded cursor-pointer ${selectedCourseName === name ? 'bg-[var(--color-accent)] text-white' : 'hover:bg-[var(--color-bg)] text-[var(--color-text)]'}`}
                         >
                           {name}
                         </div>
                       ))}
                     </div>
                   )}
                   {selectedCourseName && (
                     <div>
                       <label className="block text-xs text-[var(--color-text-muted)] mb-1">Layout</label>
                       <select 
                         onChange={e => setSelectedCourseId(e.target.value)}
                         className="w-full px-3 py-2 rounded-lg bg-[var(--color-bg)] border border-[var(--color-border)] text-[var(--color-text)]"
                         defaultValue=""
                       >
                         <option value="" disabled>Select layout</option>
                         {groupedCourses.get(selectedCourseName)?.map(c => (
                           <option key={c.id} value={c.id}>{c.layout}</option>
                         ))}
                       </select>
                     </div>
                   )}
                   <button 
                     onClick={handleAddCourse}
                     disabled={!selectedCourseId}
                     className="w-full px-6 py-2 rounded-lg bg-[var(--color-accent)] text-white text-sm font-bold active:scale-95 transition-transform disabled:opacity-50"
                   >
                     Save
                   </button>
                </div>
              )}

              <div className="space-y-2">
                {tournament.info.courses.map(c => {
                   const played = tournament.leaderboard.scores.find(s => s.name === username)?.coursesPlayed.includes(c.id);
                   return (
                     <div key={c.id} className="flex justify-between items-center">
                       <Link to={`/courses/${encodeURIComponent(c.name)}`} className="text-[var(--color-accent)] font-semibold">
                         {c.name} <span className="text-[var(--color-text-muted)] font-normal">{c.layout}</span>
                       </Link>
                       {played ? (
                         <span>✅</span>
                       ) : (
                         <span className="text-xs font-bold text-[var(--color-accent)]">▶ Play</span>
                       )}
                     </div>
                   );
                })}
              </div>
            </div>

            <div className="bg-[var(--color-surface)] p-4 rounded-xl shadow-sm shadow-[var(--color-shadow)]">
              <h3 className="text-sm font-bold text-[var(--color-text-muted)] mb-4 uppercase">Players</h3>
              <div className="flex flex-wrap gap-2">
                {tournament.info.players.map(p => (
                  <Link key={p} to={`/users/${p}`} className="px-3 py-1 rounded-full bg-[var(--color-bg)] text-[var(--color-text)] text-sm">
                    {p}
                  </Link>
                ))}
              </div>
            </div>

            <button onClick={handleCopyLink} className="w-full px-6 py-2 rounded-lg bg-[var(--color-surface)] text-[var(--color-text)] text-sm font-bold border border-[var(--color-border)] active:scale-95 transition-transform">
              {copied ? "Copied!" : "Copy Link"}
            </button>
          </div>
        )}

        {tab === 'leaderboard' && (
          <div className="rounded-xl overflow-hidden bg-[var(--color-surface)] shadow-sm shadow-[var(--color-shadow)] w-full">
            <table className="w-full text-sm">
              <thead className="bg-[var(--color-bg)] text-[var(--color-text-muted)]">
                <tr>
                  <th className="p-3 text-left">#</th>
                  <th className="p-3 text-left">Player</th>
                  <th className="p-3 text-right">Score</th>
                  <th className="p-3 text-right">Hcp</th>
                  <th className="p-3 text-right">Crs</th>
                </tr>
              </thead>
              <tbody>
                {tournament.leaderboard.scores.map((s, i) => (
                  <tr key={s.name} className={`border-t border-[var(--color-border)]/10 ${s.name === username ? 'bg-[var(--color-accent)]/10' : ''}`}>
                    <td className="p-3 text-[var(--color-text-muted)]">{i + 1}</td>
                    <td className="p-3 font-medium text-[var(--color-text)]">{s.name}</td>
                    <td className="p-3 text-right font-bold text-[var(--color-text)]">
                      {s.totalScore > 0 ? `+${s.totalScore}` : s.totalScore}
                    </td>
                    <td className="p-3 text-right text-[var(--color-text-muted)]">
                      {s.totalHcpScore > 0 ? `+${s.totalHcpScore}` : s.totalHcpScore}
                    </td>
                    <td className="p-3 text-right text-[var(--color-text-muted)]">
                      {s.coursesPlayed.length}/{tournament.info.courses.length}
                    </td>
                  </tr>
                ))}
              </tbody>
            </table>
          </div>
        )}

        {tab === 'prices' && (
          <div className="space-y-6">
            {!tournament.prices ? (
               <div className="text-center py-8 text-[var(--color-text-muted)]">
                 Prices not calculated yet
               </div>
            ) : (
               <div className="grid gap-4 grid-cols-2">
                 {[
                   { label: 'Most birdies', get: (p: TournamentPrices) => p.mostBirdies, suffix: '', neg: false },
                   { label: 'Least bogeys or worse', get: (p: TournamentPrices) => p.leastBogeysOrWorse, suffix: '', neg: false },
                   { label: 'Fastest player', get: (p: TournamentPrices) => p.fastestPlayer, suffix: ' min per hole', neg: false },
                   { label: 'Slowest player', get: (p: TournamentPrices) => p.slowestPlayer, suffix: ' min per hole', neg: true },
                   { label: 'Longest bogey free streak', get: (p: TournamentPrices) => p.longestCleanStreak, suffix: ' holes', neg: false },
                   { label: 'Longest bird-less streak', get: (p: TournamentPrices) => p.longestDrySpell, suffix: ' holes', neg: true },
                   { label: 'Most bounce-backs', get: (p: TournamentPrices) => p.bounceBacks, suffix: ' holes', neg: false },
                 ].map(item => {
                   const price = item.get(tournament.prices!);
                   if (!price) return null;
                   return (
                     <div key={item.label} className={`p-3 rounded-xl text-center ${item.neg ? 'bg-amber-900/20' : 'bg-green-900/20'}`}>
                       <p className="text-xs text-[var(--color-text-muted)]">{item.label}</p>
                       <p className="text-lg font-bold text-[var(--color-text)]">{price.username}</p>
                       <p className="text-sm text-[var(--color-text-muted)]">
                         {price.scoreValue}{item.suffix || ''}
                       </p>
                     </div>
                   );
                 })}
               </div>
            )}

            {isAdmin && (
              <div className="pt-4">
                {!isCalculateConfirmOpen ? (
                  <button 
                    onClick={() => setIsCalculateConfirmOpen(true)}
                    className="w-full px-4 py-2 rounded-lg bg-[var(--color-destructive)] text-white text-sm font-bold active:scale-95 transition-transform"
                  >
                    Calculate prices
                  </button>
                ) : (
                  <div className="flex gap-2">
                    <button 
                      onClick={handleCalculatePrices}
                      className="flex-1 px-4 py-2 rounded-lg bg-[var(--color-destructive)] text-white text-sm font-bold active:scale-95 transition-transform"
                    >
                      Confirm
                    </button>
                    <button 
                      onClick={() => setIsCalculateConfirmOpen(false)}
                      className="flex-1 px-4 py-2 rounded-lg bg-[var(--color-surface)] text-[var(--color-text)] text-sm font-bold border border-[var(--color-border)] active:scale-95 transition-transform"
                    >
                      Cancel
                    </button>
                  </div>
                )}
              </div>
            )}
          </div>
        )}
      </div>
    </DashboardLayout>
  );
}
