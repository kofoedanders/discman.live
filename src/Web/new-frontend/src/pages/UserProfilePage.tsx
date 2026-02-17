import { useEffect, useState } from "react";
import { useNavigate, useParams } from "react-router-dom";
import { useUserProfileStore } from "../stores/userProfileStore";
import DashboardLayout from "../components/layout/DashboardLayout";
import type { Round, PlayerScore, HoleScore } from "../types";

export default function UserProfilePage() {
  const navigate = useNavigate();
  const { username } = useParams<{ username: string }>();
  const { 
    profile, 
    achievements, 
    rounds, 
    isLoading, 
    error, 
    fetchProfile, 
    fetchAchievements, 
    fetchRounds,
    clear 
  } = useUserProfileStore();
  
  const [activeTab, setActiveTab] = useState<'rounds' | 'achievements'>('rounds');

  useEffect(() => {
    if (username) {
      void fetchProfile(username);
      void fetchAchievements(username);
      void fetchRounds(username, 1);
    }
    
    return () => {
      clear();
    };
  }, [username, fetchProfile, fetchAchievements, fetchRounds, clear]);

  const handlePageChange = (newPage: number) => {
    if (username) {
      void fetchRounds(username, newPage);
    }
  };

  const retry = () => {
    if (username) {
      void fetchProfile(username);
      void fetchAchievements(username);
      void fetchRounds(username, 1);
    }
  };

  const getRoundScore = (round: Round): number => {
    if (!username) return 0;
    const ps = round.playerScores?.find((p: PlayerScore) => p.playerName === username);
    if (!ps) return 0;
    
    return ps.scores.reduce((sum: number, s: HoleScore) => sum + s.relativeToPar, 0);
  };

  if (isLoading && !profile) {
    return (
      <DashboardLayout title="👤 User Profile">
        <div data-testid="user-profile-page" className="flex justify-center py-12">
          <span className="text-[var(--color-text-muted)]">Loading…</span>
        </div>
      </DashboardLayout>
    );
  }

  if (error) {
    return (
      <DashboardLayout title="👤 User Profile">
        <div data-testid="user-profile-page" className="text-center py-4">
          <p className="text-sm text-[var(--color-destructive)]">{error}</p>
          <button onClick={retry} className="mt-2 text-sm font-semibold text-[var(--color-accent)]">Retry</button>
        </div>
      </DashboardLayout>
    );
  }

  return (
    <DashboardLayout title="👤 User Profile">
      <div data-testid="user-profile-page">
      {profile && (
        <div className="flex items-center gap-4 mb-6 p-4 bg-[var(--color-surface)] rounded-xl shadow-sm shadow-[var(--color-shadow)]">
          <div className="text-4xl">{profile.emoji}</div>
          <div>
            <h2 className="text-xl font-bold">{profile.username}</h2>
            <div className="flex gap-3 text-sm text-[var(--color-text-muted)]">
              <span>Elo: {Math.round(profile.elo)}</span>
              {profile.country && <span>{profile.country}</span>}
            </div>
          </div>
        </div>
      )}

      <div className="flex border-b border-[var(--color-border)]/20 mb-4">
        <button
          onClick={() => setActiveTab('rounds')}
          className={`flex-1 py-2 text-sm font-bold text-center transition-colors ${
            activeTab === 'rounds'
              ? 'text-[var(--color-accent)] border-b-2 border-[var(--color-accent)]'
              : 'text-[var(--color-text-muted)]'
          }`}
        >
          Rounds
        </button>
        <button
          onClick={() => setActiveTab('achievements')}
          className={`flex-1 py-2 text-sm font-bold text-center transition-colors ${
            activeTab === 'achievements'
              ? 'text-[var(--color-accent)] border-b-2 border-[var(--color-accent)]'
              : 'text-[var(--color-text-muted)]'
          }`}
        >
          Achievements
        </button>
      </div>

      {activeTab === 'rounds' && rounds && (
        <div className="space-y-3">
          {rounds.rounds.map((round) => {
            const score = getRoundScore(round);
            const scoreDisplay = score > 0 ? `+${score}` : score;
            const scoreColor = score < 0 ? 'text-green-500' : score > 0 ? 'text-red-500' : 'text-[var(--color-text-muted)]';
            
            return (
              <button 
                key={round.id}
                onClick={() => navigate(`/rounds/${round.id}`)}
                className="w-full text-left p-3 rounded-xl bg-[var(--color-surface)] shadow-sm shadow-[var(--color-shadow)] active:scale-[0.99] transition-transform"
              >
                <div className="flex justify-between items-center mb-1">
                  <h3 className="font-bold text-sm truncate pr-2">{round.courseName}</h3>
                  <span className={`font-mono font-bold ${scoreColor}`}>{scoreDisplay}</span>
                </div>
                <div className="flex justify-between items-center text-xs text-[var(--color-text-muted)]">
                  <span>{round.startTime.substring(0, 10)}</span>
                  <span>{round.isCompleted ? '✅' : '🔄'}</span>
                </div>
              </button>
            );
          })}
          
          {rounds.pages > 1 && (
            <div className="flex justify-between items-center mt-4 pt-2">
              <button 
                onClick={() => handlePageChange(rounds.pageNumber - 1)}
                disabled={rounds.pageNumber <= 1}
                className="px-3 py-1 text-xs rounded bg-[var(--color-surface)] disabled:opacity-50"
              >
                Previous
              </button>
              <span className="text-xs text-[var(--color-text-muted)]">
                Page {rounds.pageNumber} of {rounds.pages}
              </span>
              <button 
                onClick={() => handlePageChange(rounds.pageNumber + 1)}
                disabled={rounds.pageNumber >= rounds.pages}
                className="px-3 py-1 text-xs rounded bg-[var(--color-surface)] disabled:opacity-50"
              >
                Next
              </button>
            </div>
          )}
          
          {rounds.rounds.length === 0 && (
            <div className="text-center py-8 text-[var(--color-text-muted)] text-sm">
              No rounds played yet.
            </div>
          )}
        </div>
      )}

      {activeTab === 'achievements' && (
        <div className="space-y-3">
          {achievements.map((achievement, idx) => {
            const hasRoundLink = achievement.roundId && achievement.roundId !== "00000000-0000-0000-0000-000000000000";
            
            const content = (
              <>
                <div className="font-bold text-sm mb-1">{achievement.achievementName}</div>
                <div className="flex justify-between text-xs text-[var(--color-text-muted)]">
                  <span>{achievement.username}</span>
                  <span>{new Date(achievement.achievedAt).toLocaleDateString()}</span>
                </div>
              </>
            );

            return hasRoundLink ? (
              <button 
                key={`${achievement.achievementName}-${idx}`}
                onClick={() => navigate(`/rounds/${achievement.roundId}`)}
                className="w-full text-left p-3 rounded-xl bg-[var(--color-surface)] shadow-sm shadow-[var(--color-shadow)] active:scale-[0.99] transition-transform"
              >
                {content}
              </button>
            ) : (
              <div 
                key={`${achievement.achievementName}-${idx}`}
                className="p-3 rounded-xl bg-[var(--color-surface)] shadow-sm shadow-[var(--color-shadow)]"
              >
                {content}
              </div>
            );
          })}
          
          {achievements.length === 0 && (
            <div className="text-center py-8 text-[var(--color-text-muted)] text-sm">
              No achievements yet.
            </div>
          )}
        </div>
      )}
      </div>
    </DashboardLayout>
  );
}
