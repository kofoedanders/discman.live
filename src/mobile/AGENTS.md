# src/mobile — Expo React Native App

## TECH STACK

Expo SDK 39 · React Native · TypeScript 3.9 · Redux + redux-thunk · @react-navigation v5 · @microsoft/signalr

**Warning:** Expo SDK 39 is from ~2020, very outdated. Major upgrade needed for modern Expo.

## STRUCTURE
```
├── App.tsx                    # Entry: SafeAreaProvider + Redux Provider
├── navigation/
│   ├── index.tsx              # NavigationContainer, login gate (Login vs Home stack)
│   └── DrawerNavigation.tsx   # Drawer: Play, Rounds, Discman.live, Settings
├── screens/
│   ├── LoginScreen.tsx
│   ├── Live/LiveScreen.tsx    # Active round scoring
│   ├── Rounds/               # Round listing + details
│   ├── CreateRoundScreen.tsx
│   ├── ScorecardScreen.tsx
│   └── SettingsScreen.tsx
├── store/
│   ├── configureStore.ts      # Store + SignalR hub + middlewares
│   ├── index.ts               # Combined reducers (user, activeRound, courses, rounds)
│   ├── User.ts                # Auth, profile, AsyncStorage persistence
│   ├── ActiveRound.ts         # Live round state + score API calls
│   ├── Rounds.ts              # Round listing/pagination
│   └── Courses.ts             # Course data
├── constants/
│   └── Urls.ts                # API base: localhost:5000 (dev) / discman.live (prod)
├── hooks/
│   └── dispatchAppStateChanges.ts  # Reconnects hub on app foreground
└── types.tsx                  # Typed navigation params (StackParamList, PlayStackParamList, etc.)
```

## NAVIGATION FLOW

```
Root Stack
├── Login (loggedIn === false)
└── Home (Drawer)
    ├── Play (Stack)
    │   ├── CreateRound (no active round)
    │   └── Live (active round exists)
    ├── Rounds
    ├── Discman.live (WebView)
    └── Settings
```

## STORE CONVENTIONS

Same pattern as web ClientApp:
- `actionCreators` object with thunks → direct `fetch()` calls
- Auth token: stored in AsyncStorage, read via `getState().user.user.token`
- Custom middlewares in `configureStore.ts`:
  - `socketsMiddleware` — connects SignalR hub on LOGIN_SUCCEED / APPSTATE_FOREGROUND
  - `activeHoleMiddleware` — computes active hole on ROUND_WAS_UPDATED

## ADD NEW SCREEN

1. Create screen in `screens/` (connected component)
2. Add route in `navigation/DrawerNavigation.tsx` or appropriate stack
3. Add typed params to `types.tsx`

## API BASE URL

`constants/Urls.ts` — `__DEV__` toggles between localhost:5000 and https://discman.live
