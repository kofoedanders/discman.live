import * as signalR from "@microsoft/signalr";
import type { Round } from "../types";
import { useRoundStore } from "../stores/roundStore";
import { normalizeRound } from "../api/client";

let connection: signalR.HubConnection | null = null;

function getToken(): string {
  const stored = localStorage.getItem("user");
  if (!stored) return "";
  try {
    const parsed = JSON.parse(stored) as { token?: string };
    return parsed.token ?? "";
  } catch {
    return "";
  }
}

export function connectHub(): void {
  if (
    connection &&
    connection.state !== signalR.HubConnectionState.Disconnected
  ) {
    return;
  }

  connection = new signalR.HubConnectionBuilder()
    .withUrl("/roundHub", {
      accessTokenFactory: getToken,
    })
    .withAutomaticReconnect()
    .build();

  connection.on("roundUpdated", (roundJson: string) => {
    const round: Round = normalizeRound(JSON.parse(roundJson));
    useRoundStore.getState().onRoundUpdated(round);
  });

  connection.on("newRoundCreated", (roundJson: string) => {
    const round: Round = normalizeRound(JSON.parse(roundJson));
    useRoundStore.getState().onRoundUpdated(round);
  });

  connection.on("roundDeleted", (roundId: string) => {
    useRoundStore.getState().onRoundDeleted(roundId);
  });

  connection.start().catch((err) => {
    console.error("SignalR connection failed:", err);
  });
}

export function disconnectHub(): void {
  if (connection) {
    connection.stop();
    connection = null;
  }
}

export function getHubState(): signalR.HubConnectionState {
  return connection?.state ?? signalR.HubConnectionState.Disconnected;
}
