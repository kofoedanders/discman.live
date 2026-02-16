import http from "k6/http";
import { check, sleep } from "k6";

const BASE_URL = __ENV.BASE_URL || "http://localhost:5000";

export const options = {
  vus: 1,
  iterations: 1,
};

function register(username, password) {
  const res = http.post(
    `${BASE_URL}/api/users`,
    JSON.stringify({
      Username: username,
      Password: password,
      Email: `${username}@perf.local`,
    }),
    { headers: { "Content-Type": "application/json" } }
  );
  return JSON.parse(res.body);
}

function login(username, password) {
  const res = http.post(
    `${BASE_URL}/api/users/authenticate`,
    JSON.stringify({ Username: username, Password: password }),
    { headers: { "Content-Type": "application/json" } }
  );
  return JSON.parse(res.body);
}

function authHeaders(token) {
  return {
    headers: {
      "Content-Type": "application/json",
      Authorization: `Bearer ${token}`,
    },
  };
}

function createCourse(token, name) {
  const res = http.post(
    `${BASE_URL}/api/courses`,
    JSON.stringify({
      CourseName: name,
      LayoutName: "Perf Layout",
      NumberOfHoles: 9,
      Latitude: 0,
      Longitude: 0,
    }),
    authHeaders(token)
  );
  return JSON.parse(res.body);
}

function startRound(token, courseId, players) {
  const res = http.post(
    `${BASE_URL}/api/rounds`,
    JSON.stringify({
      CourseId: courseId,
      Players: players,
      RoundName: "",
      ScoreMode: 0,
    }),
    authHeaders(token)
  );
  return JSON.parse(res.body);
}

function updateScore(token, roundId, holeIndex, strokes, username) {
  http.put(
    `${BASE_URL}/api/rounds/${roundId}/scores`,
    JSON.stringify({
      HoleIndex: holeIndex,
      Strokes: strokes,
      StrokeOutcomes: [],
      Username: username,
    }),
    authHeaders(token)
  );
}

function completeRound(token, roundId) {
  http.put(
    `${BASE_URL}/api/rounds/${roundId}/complete`,
    JSON.stringify({
      Base64Signature: "data:image/svg+xml;base64,dGVzdA==",
    }),
    authHeaders(token)
  );
}

export function setup() {
  const username = `perf${Date.now()}`;
  const password = "TestPass123!";

  const regResult = register(username, password);
  check(regResult, { "registered ok": (r) => r.token && r.token.length > 0 });

  const loginResult = login(username, password);
  const token = loginResult.token;

  const course = createCourse(token, `PerfCourse${Date.now()}`);
  const round = startRound(token, course.id, [username]);

  for (let i = 0; i < 9; i++) {
    updateScore(token, round.id, i, 3, username);
  }
  completeRound(token, round.id);

  return {
    username,
    password,
    token,
    courseId: course.id,
    roundId: round.id,
    year: new Date().getFullYear(),
  };
}

export default function (data) {
  const warmupIterations = 5;

  // --- POST /api/users/authenticate (login) ---
  console.log("\n=== POST /api/users/authenticate ===");
  {
    const coldStart = Date.now();
    const coldRes = http.post(
      `${BASE_URL}/api/users/authenticate`,
      JSON.stringify({ Username: data.username, Password: data.password }),
      { headers: { "Content-Type": "application/json" } }
    );
    const coldMs = Date.now() - coldStart;
    check(coldRes, { "login cold 200": (r) => r.status === 200 });
    console.log(`  Cold: ${coldMs}ms (status ${coldRes.status})`);

    const warmTimes = [];
    for (let i = 0; i < warmupIterations; i++) {
      const s = Date.now();
      http.post(
        `${BASE_URL}/api/users/authenticate`,
        JSON.stringify({ Username: data.username, Password: data.password }),
        { headers: { "Content-Type": "application/json" } }
      );
      warmTimes.push(Date.now() - s);
    }
    warmTimes.sort((a, b) => a - b);
    const median = warmTimes[Math.floor(warmTimes.length / 2)];
    console.log(
      `  Warm (${warmupIterations}x): ${warmTimes.join(", ")}ms  median=${median}ms`
    );
  }

  // --- GET /api/rounds/{roundId} ---
  console.log("\n=== GET /api/rounds/{roundId} ===");
  {
    const coldStart = Date.now();
    const coldRes = http.get(
      `${BASE_URL}/api/rounds/${data.roundId}`,
      authHeaders(data.token)
    );
    const coldMs = Date.now() - coldStart;
    check(coldRes, { "get round cold 200": (r) => r.status === 200 });
    console.log(`  Cold: ${coldMs}ms (status ${coldRes.status})`);

    const warmTimes = [];
    for (let i = 0; i < warmupIterations; i++) {
      const s = Date.now();
      http.get(
        `${BASE_URL}/api/rounds/${data.roundId}`,
        authHeaders(data.token)
      );
      warmTimes.push(Date.now() - s);
    }
    warmTimes.sort((a, b) => a - b);
    const median = warmTimes[Math.floor(warmTimes.length / 2)];
    console.log(
      `  Warm (${warmupIterations}x): ${warmTimes.join(", ")}ms  median=${median}ms`
    );
  }

  // --- GET /api/feeds ---
  console.log("\n=== GET /api/feeds ===");
  {
    const coldStart = Date.now();
    const coldRes = http.get(
      `${BASE_URL}/api/feeds?pageNumber=1&pageSize=20&itemType=`,
      authHeaders(data.token)
    );
    const coldMs = Date.now() - coldStart;
    check(coldRes, { "feeds cold 200": (r) => r.status === 200 });
    console.log(`  Cold: ${coldMs}ms (status ${coldRes.status})`);

    const warmTimes = [];
    for (let i = 0; i < warmupIterations; i++) {
      const s = Date.now();
      http.get(
        `${BASE_URL}/api/feeds?pageNumber=1&pageSize=20&itemType=`,
        authHeaders(data.token)
      );
      warmTimes.push(Date.now() - s);
    }
    warmTimes.sort((a, b) => a - b);
    const median = warmTimes[Math.floor(warmTimes.length / 2)];
    console.log(
      `  Warm (${warmupIterations}x): ${warmTimes.join(", ")}ms  median=${median}ms`
    );
  }

  // --- GET /api/leaderboard ---
  console.log("\n=== GET /api/leaderboard ===");
  {
    const coldStart = Date.now();
    const coldRes = http.get(
      `${BASE_URL}/api/leaderboard?onlyFriends=false&month=0`,
      authHeaders(data.token)
    );
    const coldMs = Date.now() - coldStart;
    check(coldRes, { "leaderboard cold 200": (r) => r.status === 200 });
    console.log(`  Cold: ${coldMs}ms (status ${coldRes.status})`);

    const warmTimes = [];
    for (let i = 0; i < warmupIterations; i++) {
      const s = Date.now();
      http.get(
        `${BASE_URL}/api/leaderboard?onlyFriends=false&month=0`,
        authHeaders(data.token)
      );
      warmTimes.push(Date.now() - s);
    }
    warmTimes.sort((a, b) => a - b);
    const median = warmTimes[Math.floor(warmTimes.length / 2)];
    console.log(
      `  Warm (${warmupIterations}x): ${warmTimes.join(", ")}ms  median=${median}ms`
    );
  }

  // --- GET /api/users/{username}/yearsummary/{year} ---
  console.log("\n=== GET /api/users/{username}/yearsummary/{year} ===");
  {
    const coldStart = Date.now();
    const coldRes = http.get(
      `${BASE_URL}/api/users/${data.username}/yearsummary/${data.year}`,
      authHeaders(data.token)
    );
    const coldMs = Date.now() - coldStart;
    check(coldRes, { "yearsummary cold 200": (r) => r.status === 200 });
    console.log(`  Cold: ${coldMs}ms (status ${coldRes.status})`);

    const warmTimes = [];
    for (let i = 0; i < warmupIterations; i++) {
      const s = Date.now();
      http.get(
        `${BASE_URL}/api/users/${data.username}/yearsummary/${data.year}`,
        authHeaders(data.token)
      );
      warmTimes.push(Date.now() - s);
    }
    warmTimes.sort((a, b) => a - b);
    const median = warmTimes[Math.floor(warmTimes.length / 2)];
    console.log(
      `  Warm (${warmupIterations}x): ${warmTimes.join(", ")}ms  median=${median}ms`
    );
  }

  // --- POST /api/rounds (create round) ---
  console.log("\n=== POST /api/rounds ===");
  {
    const coldStart = Date.now();
    const coldRes = http.post(
      `${BASE_URL}/api/rounds`,
      JSON.stringify({
        CourseId: data.courseId,
        Players: [data.username],
        RoundName: "",
        ScoreMode: 0,
      }),
      authHeaders(data.token)
    );
    const coldMs = Date.now() - coldStart;
    check(coldRes, { "create round cold 200": (r) => r.status === 200 });
    console.log(`  Cold: ${coldMs}ms (status ${coldRes.status})`);

    const warmTimes = [];
    for (let i = 0; i < warmupIterations; i++) {
      const s = Date.now();
      http.post(
        `${BASE_URL}/api/rounds`,
        JSON.stringify({
          CourseId: data.courseId,
          Players: [data.username],
          RoundName: "",
          ScoreMode: 0,
        }),
        authHeaders(data.token)
      );
      warmTimes.push(Date.now() - s);
    }
    warmTimes.sort((a, b) => a - b);
    const median = warmTimes[Math.floor(warmTimes.length / 2)];
    console.log(
      `  Warm (${warmupIterations}x): ${warmTimes.join(", ")}ms  median=${median}ms`
    );
  }
}
