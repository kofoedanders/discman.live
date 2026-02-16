import { useState } from "react";
import { Link, useNavigate } from "react-router-dom";
import { useAuthStore } from "../stores/authStore";

export default function Login() {
  const [username, setUsername] = useState("");
  const [password, setPassword] = useState("");
  const login = useAuthStore((s) => s.login);
  const loginError = useAuthStore((s) => s.loginError);
  const isLoggedIn = useAuthStore((s) => s.isLoggedIn);
  const navigate = useNavigate();

  if (isLoggedIn) {
    navigate("/", { replace: true });
    return null;
  }

  async function handleSubmit(e: React.SyntheticEvent) {
    e.preventDefault();
    await login(username, password);
    if (useAuthStore.getState().isLoggedIn) {
      navigate("/", { replace: true });
    }
  }

  return (
    <div className="flex-1 flex flex-col items-center justify-center px-6 bg-[var(--color-bg)]">
      <h1 className="text-2xl font-bold mb-6 text-[var(--color-text)]">
        Sign In
      </h1>

      <form onSubmit={handleSubmit} className="w-full max-w-sm space-y-4">
        <input
          type="text"
          placeholder="Username"
          value={username}
          onChange={(e) => setUsername(e.target.value)}
          autoComplete="username"
          required
          className="w-full px-4 py-3 rounded-lg border-2 border-[var(--color-border)] bg-[var(--color-surface)] text-[var(--color-text)] text-base font-medium placeholder:text-[var(--color-text-muted)] focus:outline-none focus:border-[var(--color-accent)]"
        />
        <input
          type="password"
          placeholder="Password"
          value={password}
          onChange={(e) => setPassword(e.target.value)}
          autoComplete="current-password"
          required
          className="w-full px-4 py-3 rounded-lg border-2 border-[var(--color-border)] bg-[var(--color-surface)] text-[var(--color-text)] text-base font-medium placeholder:text-[var(--color-text-muted)] focus:outline-none focus:border-[var(--color-accent)]"
        />

        {loginError && (
          <p className="text-sm font-semibold text-[var(--color-bogey)]">
            {loginError}
          </p>
        )}

        <button
          type="submit"
          className="w-full py-3 rounded-lg bg-[var(--color-accent)] text-white text-base font-bold border-2 border-[var(--color-button-border)] active:opacity-80"
        >
          Sign In
        </button>
      </form>

      <p className="mt-6 text-sm text-[var(--color-text-muted)]">
        Don't have an account?{" "}
        <Link to="/register" className="font-bold text-[var(--color-accent)] underline">
          Register
        </Link>
      </p>
    </div>
  );
}
