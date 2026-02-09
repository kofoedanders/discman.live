# next/ — Discman 2.0 Rewrite

**Status:** Early-stage migration. Domain model + tests only. Not deployed.

## TECH STACK

.NET 8 · Blazor Server (Razor Components, interactive server render) · Event-sourced DDD · NUnit + FluentAssertions

## STRUCTURE
```
├── Discman.Next.sln
├── Domain/
│   ├── EventSourcedAggregate.cs   # Base class: Append(), Mutate(), PendingEvents
│   ├── Round.cs                   # Aggregate: StartRound factory, Apply handlers
│   ├── Player.cs                  # Player + Score types
│   ├── Course.cs                  # Course aggregate
│   ├── Layout.cs                  # Layout + Hole value objects
│   ├── RoundScores.cs             # Scoring logic helpers
│   ├── Events/EventBase.cs        # Event types: NewRoundWasStarted, PlayerWasAddedToRound
│   └── Primitives/
│       ├── ValueObject.cs         # DDD value object base (equality by properties)
│       └── Username.cs            # Username primitive type
├── Domain.UnitTests/
│   ├── Scenario.cs                # BDD base: abstract Given() + When(), [SetUp] runs both
│   ├── ThenAttribute.cs           # Custom [Then] = NUnit [Test] (semantic alias)
│   └── Round/                     # Test scenarios per aggregate
│       ├── WhenStartingANewRound.cs
│       └── GivenExistingRound_AddingPlayersAndStartingTheRound.cs
└── Web/
    ├── Program.cs                 # AddRazorComponents().AddInteractiveServerComponents()
    ├── Components/App.razor       # Blazor app root
    └── wwwroot/index.html         # Loads blazor.web.js
```

## EVENT SOURCING PATTERN

1. Aggregates extend `EventSourcedAggregate`
2. State changes via `Append(new SomeEvent(...))` — adds to `PendingEvents` and calls `Mutate()`
3. `Mutate()` dispatches to `Apply(SomeEvent e)` methods that update in-memory state
4. No persistence layer wired yet — events are in-memory only

## TEST PATTERN (BDD)

```csharp
public class WhenStartingANewRound : Scenario
{
    protected override Task Given() { /* set up preconditions */ }
    protected override Task When()  { /* execute action under test */ }

    [Then] public void RoundShouldHaveCourse() => _round.Course.Should().Be(...);
    [Then] public void RoundShouldHavePlayers() => _round.Players.Should().HaveCount(2);
}
```
- `Scenario` base [SetUp] runs `Given()` then `When()` before each [Then] method
- Each [Then] is a separate NUnit test case
- Run: `dotnet test next/Domain.UnitTests`

## WHAT'S MISSING (vs src/Web)

No persistence (Marten/event store), no API controllers, no auth, no SignalR, no NServiceBus, no background workers. Web/ has only Blazor scaffold pages (Counter, Weather).
