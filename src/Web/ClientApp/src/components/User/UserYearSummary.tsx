import React, { useEffect, useState } from "react";
import { connect, ConnectedProps } from "react-redux";
import { ApplicationState } from "../../store";
import * as UserStore from "../../store/User";
import { useParams, useHistory } from "react-router";
import { Link } from "react-router-dom";

// Map Redux state to component props
const mapState = (state: ApplicationState) => {
  return {
    user: state.user,
  };
};

// Connect component to Redux store
const connector = connect(mapState, UserStore.actionCreators);

type PropsFromRedux = ConnectedProps<typeof connector>;
type Props = PropsFromRedux & {};

// Extend the YearSummary interface to include courseScores
interface CourseScore {
  averageScore: number;
  rounds: number;
}

interface CourseScores {
  [course: string]: CourseScore;
}

// Use the same type as in the store but extend it with our additional properties
interface ExtendedYearSummary {
  roundsPlayed: number;
  totalScore: number;
  hoursPlayed: number;
  mostPlayedCourse: string; // In the store this is a number, but we need it as a string
  mostPlayedCourseRoundsCount: number;
  bestCardmate: string;
  bestCardmateAverageScore: number;
  worstCardmate: string;
  worstCardmateAverageScore: number;
  scoreImprovement?: number;
  courseScores?: CourseScores;
}

interface PreviousYearSummary {
  roundsPlayed: number;
  totalScore: number;
  hoursPlayed: number;
  courseScores?: CourseScores;
}

const UserYearSummary = (props: Props) => {
  const { usernameParam, yearParam } = useParams<{
    usernameParam: string | undefined;
    yearParam: string | undefined;
  }>();
  const history = useHistory();
  const username = usernameParam || props.user?.user?.username;
  const currentYear = new Date().getFullYear();
  const currentMonth = new Date().getMonth();
  
  // Only allow access to current year in November-December
  const maxYear = currentMonth >= 10 ? currentYear : currentYear - 1;
  
  // Start from 2020 or first active year
  const startYear = 2020;
  
  const year = yearParam ? parseInt(yearParam) : maxYear;
  const { fetchUserYearSummary } = props;
  
  // Generate available years
  const availableYears = Array.from(
    { length: maxYear - startYear + 1 },
    (_, i) => startYear + i
  ).reverse(); // Most recent years first
  
  const [previousYearSummary, setPreviousYearSummary] = useState<PreviousYearSummary | null>(null);
  
  useEffect(() => {
    if (username) {
      fetchUserYearSummary(username, year);
      
      // Fetch previous year data for comparison if not the first year
      if (year > startYear) {
        // This is just for UI display, we don't need to store it in Redux
        fetch(`api/users/${username}/yearsummary/${year - 1}`, {
          method: "GET",
          headers: {
            "Content-Type": "application/json",
            Authorization: `Bearer ${props.user?.user?.token}`,
          },
        })
          .then(response => response.json())
          .then(data => {
            setPreviousYearSummary(data);
          })
          .catch(error => {
            console.error("Error fetching previous year data:", error);
          });
      } else {
        setPreviousYearSummary(null);
      }
    }
  }, [username, year, props.user?.user?.token, fetchUserYearSummary]);
  
  const handleYearChange = (selectedYear: number) => {
    history.push(`/user/${username}/yearsummary/${selectedYear}`);
  };
  
  const yearSummary = props.user?.userYearSummary as unknown as ExtendedYearSummary;
  
  // Calculate average round score
  const averageRoundScore = yearSummary?.roundsPlayed > 0
    ? (yearSummary.totalScore / yearSummary.roundsPlayed).toFixed(1)
    : "0.0";
  
  // Calculate average round time in minutes
  const averageRoundTime = yearSummary?.roundsPlayed > 0
    ? Math.round((yearSummary.hoursPlayed * 60) / yearSummary.roundsPlayed)
    : 0;
  
  // Calculate score improvement from first half to second half of the year
  const scoreImprovement = yearSummary?.scoreImprovement
    ? yearSummary.scoreImprovement.toFixed(1)
    : null;

  const getCourseImprovement = () => {
    if (!yearSummary || !previousYearSummary) return null;
    
    // Find common courses between years
    const commonCourses: Array<{
      name: string;
      improvement: number;
      currentAvg: number;
      prevAvg: number;
      rounds: number;
    }> = [];
    
    if (yearSummary.courseScores && previousYearSummary.courseScores) {
      for (const course in yearSummary.courseScores) {
        if (previousYearSummary.courseScores[course]) {
          const currentAvg = yearSummary.courseScores[course].averageScore;
          const prevAvg = previousYearSummary.courseScores[course].averageScore;
          const roundsCurrentYear = yearSummary.courseScores[course].rounds;
          const roundsPrevYear = previousYearSummary.courseScores[course].rounds;
          
          if (roundsCurrentYear >= 3 && roundsPrevYear >= 3) {
            commonCourses.push({
              name: course,
              improvement: prevAvg - currentAvg,
              currentAvg,
              prevAvg,
              rounds: roundsCurrentYear
            });
          }
        }
      }
    }
    
    // Sort by improvement (best first)
    return commonCourses.sort((a, b) => b.improvement - a.improvement);
  };

  const courseImprovements = getCourseImprovement();

  // Calculate overall stats comparison
  const getOverallComparison = () => {
    if (!yearSummary || !previousYearSummary || !previousYearSummary.roundsPlayed) return null;
    
    const prevAvgScore = previousYearSummary.roundsPlayed > 0 
      ? previousYearSummary.totalScore / previousYearSummary.roundsPlayed
      : 0;
    
    return {
      roundsChange: yearSummary.roundsPlayed - previousYearSummary.roundsPlayed,
      scoreChange: parseFloat(averageRoundScore) - prevAvgScore,
      timeChange: averageRoundTime - Math.round((previousYearSummary.hoursPlayed * 60) / previousYearSummary.roundsPlayed)
    };
  };

  const overallComparison = getOverallComparison();

  return (
    <div className="container">
      <div className="mt-2 px-2">
        <h3 className="title is-4 mb-2 has-text-centered">{username}'s {year} Summary</h3>
        
        <div className="has-text-centered mb-2">
          <div className="field is-grouped is-grouped-centered">
            {year > startYear && (
              <div className="control">
                <button 
                  className="button is-small is-info is-light" 
                  onClick={() => handleYearChange(year - 1)}
                >
                  <span className="icon is-small">
                    <i className="fas fa-chevron-left"></i>
                  </span>
                  <span>{year - 1}</span>
                </button>
              </div>
            )}
            
            <div className="control">
              <div className="select is-small">
                <select 
                  value={year} 
                  onChange={(e) => handleYearChange(parseInt(e.target.value))}
                >
                  {availableYears.map((y) => (
                    <option key={y} value={y}>{y}</option>
                  ))}
                </select>
              </div>
            </div>
            
            {year < maxYear && (
              <div className="control">
                <button 
                  className="button is-small is-info is-light" 
                  onClick={() => handleYearChange(year + 1)}
                >
                  <span>{year + 1}</span>
                  <span className="icon is-small">
                    <i className="fas fa-chevron-right"></i>
                  </span>
                </button>
              </div>
            )}
          </div>
        </div>
        
        {yearSummary && yearSummary.roundsPlayed > 0 ? (
          <div>
            {/* Main Stats Section - Compact row */}
            <div className="columns is-mobile is-multiline is-variable is-0 mb-2">
              <div className="column is-3-mobile is-narrow px-1">
                <div className="notification is-primary is-light py-2 px-2 mb-0">
                  <p className="heading is-size-7 mb-0">Rounds</p>
                  <p className="title is-4 mb-0">{yearSummary.roundsPlayed}</p>
                </div>
              </div>
              <div className="column is-3-mobile is-narrow px-1">
                <div className="notification is-info is-light py-2 px-2 mb-0">
                  <p className="heading is-size-7 mb-0">Hours</p>
                  <p className="title is-4 mb-0">{yearSummary.hoursPlayed.toFixed(1)}</p>
                </div>
              </div>
              <div className="column is-3-mobile is-narrow px-1">
                <div className="notification is-warning is-light py-2 px-2 mb-0">
                  <p className="heading is-size-7 mb-0">Total</p>
                  <p className="title is-4 mb-0">
                    {yearSummary.totalScore > 0 ? "+" : ""}
                    {yearSummary.totalScore.toFixed(0)}
                  </p>
                </div>
              </div>
              <div className="column is-3-mobile is-narrow px-1">
                <div className="notification is-success is-light py-2 px-2 mb-0">
                  <p className="heading is-size-7 mb-0">Avg/Rd</p>
                  <p className="title is-4 mb-0">
                    {parseFloat(averageRoundScore) > 0 ? "+" : ""}
                    {averageRoundScore}
                  </p>
                </div>
              </div>
            </div>
            
            {/* Course and Partner Stats - Side by side */}
            <div className="columns is-variable is-1 mb-2">
              {/* Course Stats - Left column */}
              <div className="column is-6 px-1">
                <div className="card has-background-primary-light">
                  <header className="card-header py-1 has-background-primary-light">
                    <p className="card-header-title is-centered py-1 is-size-6">
                      Course Stats
                    </p>
                  </header>
                  <div className="card-content py-2 px-2">
                    <div className="content is-small">
                      <div className="mb-1">
                        <span className="icon is-small has-text-primary">
                          <i className="fas fa-map-marker-alt"></i>
                        </span>
                        <strong>Most:</strong> {yearSummary.mostPlayedCourse}
                        <span className="tag is-primary is-light is-small ml-1">
                          {yearSummary.mostPlayedCourseRoundsCount}
                        </span>
                      </div>
                      
                      <div className="mb-1">
                        <span className="icon is-small has-text-primary">
                          <i className="fas fa-clock"></i>
                        </span>
                        <strong>Avg Time:</strong> {averageRoundTime}m
                      </div>
                      
                      {scoreImprovement && (
                        <div>
                          <span className="icon is-small has-text-primary">
                            <i className="fas fa-chart-line"></i>
                          </span>
                          <strong>Improvement:</strong>
                          <span className={parseFloat(scoreImprovement) < 0 ? "has-text-success" : "has-text-danger"}>
                            {parseFloat(scoreImprovement) < 0 ? " " : " +"}
                            {scoreImprovement}
                          </span>
                        </div>
                      )}
                    </div>
                  </div>
                </div>
              </div>
              
              {/* Playing Partners - Right column */}
              <div className="column is-6 px-1">
                <div className="card has-background-link-light">
                  <header className="card-header py-1 has-background-link-light">
                    <p className="card-header-title is-centered py-1 is-size-6">
                      Partners
                    </p>
                  </header>
                  <div className="card-content py-2 px-2">
                    {yearSummary.bestCardmate ? (
                      <div className="content is-small">
                        <div className="mb-1">
                          <span className="icon is-small has-text-link">
                            <i className="fas fa-thumbs-up"></i>
                          </span>
                          <strong>Best:</strong> {yearSummary.bestCardmate}
                          <span className="tag is-success is-light is-small ml-1">
                            {yearSummary.bestCardmateAverageScore > 0 ? "+" : ""}
                            {yearSummary.bestCardmateAverageScore.toFixed(1)}
                          </span>
                        </div>
                        
                        <div>
                          <span className="icon is-small has-text-link">
                            <i className="fas fa-thumbs-down"></i>
                          </span>
                          <strong>Challenging:</strong> {yearSummary.worstCardmate}
                          <span className="tag is-danger is-light is-small ml-1">
                            {yearSummary.worstCardmateAverageScore > 0 ? "+" : ""}
                            {yearSummary.worstCardmateAverageScore.toFixed(1)}
                          </span>
                        </div>
                      </div>
                    ) : (
                      <p className="has-text-centered is-size-7">
                        Not enough data for partner stats
                      </p>
                    )}
                  </div>
                </div>
              </div>
            </div>
            
            {/* Year-to-Year Evolution - New box */}
            {year > startYear && previousYearSummary && previousYearSummary.roundsPlayed > 0 && (
              <div className="card has-background-success-light mb-2">
                <header className="card-header py-1 has-background-success-light">
                  <p className="card-header-title is-centered py-1 is-size-6">
                    Evolution from {year-1}
                  </p>
                </header>
                <div className="card-content py-2 px-2">
                  {overallComparison && (
                    <div className="content is-small">
                      <div className="is-flex is-justify-content-space-between mb-1">
                        <span>
                          <span className="icon is-small has-text-success">
                            <i className="fas fa-exchange-alt"></i>
                          </span>
                          <strong>Rounds:</strong>
                        </span>
                        <span className={overallComparison.roundsChange >= 0 ? "has-text-success" : "has-text-danger"}>
                          {overallComparison.roundsChange > 0 ? "+" : ""}
                          {overallComparison.roundsChange}
                        </span>
                      </div>
                      
                      <div className="is-flex is-justify-content-space-between mb-1">
                        <span>
                          <span className="icon is-small has-text-success">
                            <i className="fas fa-golf-ball"></i>
                          </span>
                          <strong>Avg Score:</strong>
                        </span>
                        <span className={overallComparison.scoreChange <= 0 ? "has-text-success" : "has-text-danger"}>
                          {overallComparison.scoreChange <= 0 ? "" : "+"}
                          {overallComparison.scoreChange.toFixed(1)}
                        </span>
                      </div>
                      
                      <div className="is-flex is-justify-content-space-between">
                        <span>
                          <span className="icon is-small has-text-success">
                            <i className="fas fa-stopwatch"></i>
                          </span>
                          <strong>Avg Time:</strong>
                        </span>
                        <span className={overallComparison.timeChange <= 0 ? "has-text-success" : "has-text-danger"}>
                          {overallComparison.timeChange > 0 ? "+" : ""}
                          {overallComparison.timeChange} min
                        </span>
                      </div>
                      
                      {courseImprovements && courseImprovements.length > 0 && (
                        <div className="mt-2">
                          <div className="is-flex is-justify-content-space-between">
                            <strong>Course Improvements:</strong>
                          </div>
                          {courseImprovements.slice(0, 2).map((course, index) => (
                            <div key={index} className="is-flex is-justify-content-space-between">
                              <span className="is-ellipsis" style={{maxWidth: "60%", overflow: "hidden", textOverflow: "ellipsis"}}>
                                {course.name}:
                              </span>
                              <span className={course.improvement >= 0 ? "has-text-success" : "has-text-danger"}>
                                {course.improvement > 0 ? "-" : "+"}
                                {Math.abs(course.improvement).toFixed(1)}
                              </span>
                            </div>
                          ))}
                        </div>
                      )}
                    </div>
                  )}
                </div>
              </div>
            )}
            
            {/* Motivational Message - Compact */}
            <div className="notification is-warning is-light py-2 px-3 has-text-centered mb-2">
              <span className="icon is-small">
                <i className={yearSummary.roundsPlayed > 20 ? "fas fa-award" : yearSummary.roundsPlayed > 10 ? "fas fa-star" : "fas fa-smile"}></i>
              </span>
              <span className="is-size-7">
                {yearSummary.roundsPlayed > 20 
                  ? <><strong>Wow!</strong> {yearSummary.roundsPlayed} rounds in {year}!</>
                  : yearSummary.roundsPlayed > 10 
                    ? <><strong>Great job!</strong> {yearSummary.roundsPlayed} rounds in {year}!</>
                    : <><strong>Good start!</strong> Keep playing to improve!</>
                }
              </span>
            </div>
          </div>
        ) : (
          <div className="notification is-info is-light py-2 px-3 mb-2">
            <p className="has-text-centered is-size-7">
              <span className="icon is-small">
                <i className="fas fa-info-circle"></i>
              </span>
              No rounds played in {year}. Select a different year or start playing!
            </p>
          </div>
        )}
      </div>
    </div>
  );
};

export default connector(UserYearSummary);
