# ClientApp — React SPA Frontend

## TECH STACK

React 16 (CRA) · TypeScript 4.9 · Redux + redux-thunk · Bulma CSS · react-router v5 · connected-react-router · @microsoft/signalr

## STRUCTURE
```
src/
├── components/          # All page/UI components (PascalCase files)
├── store/               # Redux: one file per domain slice
│   ├── Rounds.ts        # ⚠ 1144 lines — rounds state, thunks, domain logic
│   ├── User.ts          # ⚠ 1217 lines — auth, profile, feed, friends
│   ├── Courses.ts       # Course state
│   ├── Notifications.ts # Toast notification helpers
│   └── configureStore.ts # Store creation, SignalR hub instance
├── App.tsx              # Route definitions (react-router)
└── index.tsx            # Entry point, Provider + store
```

## COMPONENT PATTERN

- Class components (React 16, no hooks)
- Connected via `connect(mapStateToProps, mapDispatchToProps)` from react-redux
- Navigation: `push()` from connected-react-router dispatched in thunks

## STORE / STATE PATTERN

Each store file exports:
1. Action type string literals (e.g., `"FETCH_ROUND_SUCCESS"`)
2. `KnownAction` union type
3. `actionCreators` object — thunk functions (`AppThunkAction<KnownAction>`)
4. `reducer` — switch on `action.type`, returns new state via object spread

API calls: direct `fetch()` inside thunks with manual `Authorization: Bearer` header. No centralized API client.

## SIGNALR CLIENT

- Hub created in `configureStore.ts` — single `HubConnection` instance at `/roundHub`
- Token: passed via `accessTokenFactory` option
- Events: `roundUpdated`, `newRoundCreated`, `roundDeleted` → dispatch Redux actions
- Hub imported directly into store files (tight coupling)

## ADD NEW PAGE

1. Create component in `components/` (class component, connect to redux)
2. Add route in `App.tsx`
3. If new state needed: add to appropriate store file or create new store slice + register in `configureStore.ts`

## COMPLEXITY HOTSPOTS

- `store/Rounds.ts` — mixes API calls, reducer, and domain logic (`calculatePaceForRound`, `getNextUncompletedHole`). Domain functions should be in utils
- `store/User.ts` — auth + profile + feed + friends all in one file. Reads `localStorage` at module init
- `window.confirm()` called inside `setScore` thunk (blocking UI side-effect in state logic)
- Inconsistent async patterns: some thunks use async/await, others use `.then()` chains
- No centralized error handling — each thunk handles errors independently
