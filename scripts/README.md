# Harmony.Api Scripts

This directory contains scripts for running and testing the Harmony.Api pairing service.

## Starting the Server

To start the API server:

```bash
./scripts/start-server.sh
```

The server will run on `http://localhost:5199` and output structured JSON logs to stdout.

## Testing Scenarios

To run all test scenarios:

```bash
./scripts/run-test-scenarios.sh
```

Or test against a different server:

```bash
./scripts/run-test-scenarios.sh http://production-server:8080
```

## Test Scenarios

The `test-scenarios/` directory contains JSON files representing different pairing scenarios:

1. **01-simple-two-teams.json** - Basic two-team pairing
2. **02-three-teams-bye.json** - Three teams requiring a bye
3. **03-high-low-pairing.json** - Six teams demonstrating high-low fold pattern
4. **04-powermatch-pullup.json** - Four teams with different records (tests win-balancing)
5. **05-bye-eligibility.json** - Tests bye eligibility constraint (only eligible team gets bye)
6. **06-avoid-rematches.json** - Tests opponent history avoidance

## Manual Testing

You can also send requests manually using curl:

```bash
curl -X POST http://localhost:5199/api/pairing/generate \
  -H "Content-Type: application/json" \
  -d @scripts/test-scenarios/01-simple-two-teams.json
```

## Viewing Logs

The server outputs structured JSON logs to stdout. Each log entry includes:

- `@t` - Timestamp
- `@mt` - Message template
- `@l` - Log level (Information, Debug, Error, etc.)
- Additional context properties (team counts, matchups, errors, etc.)

The logs capture:

- Incoming request details (round number, team count)
- Individual team states (wins, losses, AFF/NEG rounds, bye eligibility)
- Generated matchup decisions
- Error messages if pairing fails

This makes it easy to debug pairing decisions in production.
