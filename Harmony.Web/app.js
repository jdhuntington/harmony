function pairingApp() {
    return {
        // Configuration
        apiEndpoint: 'http://localhost:5199',
        selectedEventId: '',
        roundToMatch: 1,
        baseRankingsOnRound: 1,

        // Data
        sourceData: null,
        mungedData: null,
        response: null,
        loading: false,

        // Computed properties
        get availableEvents() {
            return this.sourceData?.events || [];
        },

        get selectedEvent() {
            if (!this.selectedEventId) return null;
            return this.availableEvents.find(e => e.event_id === parseInt(this.selectedEventId));
        },

        get maxRound() {
            if (!this.selectedEvent) return 0;

            // Find the maximum round number from all teams' history
            let max = 0;
            for (const team of this.selectedEvent.teams) {
                for (const round of team.history) {
                    if (round.round > max) max = round.round;
                }
            }
            return max;
        },

        // Handle file upload
        async handleFileUpload(event) {
            const file = event.target.files[0];
            if (!file) return;

            try {
                const text = await file.text();
                this.sourceData = JSON.parse(text);

                // Auto-select first event
                if (this.availableEvents.length > 0) {
                    this.selectedEventId = this.availableEvents[0].event_id;
                }
            } catch (error) {
                alert('Error reading file: ' + error.message);
            }
        },

        // Transform source data into API request format
        mungeData() {
            if (!this.selectedEvent) {
                this.mungedData = null;
                return;
            }

            this.mungedData = {
                roundNumber: this.roundToMatch,
                teams: this.transformTeams(this.selectedEvent.teams)
            };
        },

        // Transform teams from source format to API format
        transformTeams(teams) {
            // First pass: build a lookup map for cross-referencing results
            const teamMap = new Map();
            teams.forEach(team => {
                teamMap.set(team.mask, team);
            });

            // Helper function to determine actual result for a round
            const getActualResult = (team, round) => {
                if (round.result === 'bye') return 'bye';
                if (round.result === 'win') return 'win';
                if (round.result === 'loss') return 'loss';

                // Result is 'unknown' - likely on neg side, need to check opponent
                if (round.result === 'unknown' && round.opponent_mask) {
                    const opponent = teamMap.get(round.opponent_mask);
                    if (opponent) {
                        // Find the matching round in opponent's history
                        const opponentRound = opponent.history.find(r =>
                            r.round === round.round && r.opponent_mask === team.mask
                        );

                        if (opponentRound) {
                            // Flip the opponent's result
                            if (opponentRound.result === 'win') return 'loss';
                            if (opponentRound.result === 'loss') return 'win';
                        }
                    }
                }

                // If we still don't know, it's truly unknown
                return 'unknown';
            };

            // For round N matching, we need history through N-1
            const throughRound = this.roundToMatch - 1;

            return teams.map(team => {
                let wins = 0;
                let losses = 0;
                let affRounds = 0;
                let negRounds = 0;
                const opponentHistory = [];

                for (const round of team.history) {
                    // W/L: only through baseRankingsOnRound
                    if (round.round <= this.baseRankingsOnRound) {
                        const actualResult = getActualResult(team, round);
                        if (actualResult === 'win') wins++;
                        if (actualResult === 'loss') losses++;
                    }

                    // Aff/Neg counts and opponent history: through roundToMatch - 1
                    if (round.round <= throughRound) {
                        // Count side assignments
                        if (round.side === 'aff') affRounds++;
                        if (round.side === 'neg') negRounds++;

                        // Track opponent history (exclude byes)
                        const actualResult = getActualResult(team, round);
                        if (round.opponent_mask && actualResult !== 'bye') {
                            opponentHistory.push(round.opponent_mask);
                        }
                    }
                }

                // Get ranking/seed from the base round
                const seed = team.rankings?.[String(this.baseRankingsOnRound)] || 999;

                // Check if team has had a bye through all rounds (not just base)
                const hadBye = team.history.some(r => r.round <= throughRound && r.result === 'bye');

                return {
                    name: team.mask,
                    isByeEligible: !hadBye, // Can only get bye if haven't had one
                    wins: wins,
                    losses: losses,
                    affRounds: affRounds,
                    negRounds: negRounds,
                    seed: seed,
                    club: null, // Not in source data
                    opponentHistory: opponentHistory
                };
            });
        },

        // Generate pairings by calling the API
        async generatePairings() {
            if (!this.mungedData) return;

            this.loading = true;
            this.response = null;

            try {
                const response = await fetch(`${this.apiEndpoint}/api/pairing/generate`, {
                    method: 'POST',
                    headers: {
                        'Content-Type': 'application/json',
                    },
                    body: JSON.stringify(this.mungedData)
                });

                if (!response.ok) {
                    throw new Error(`HTTP error! status: ${response.status}`);
                }

                this.response = await response.json();
            } catch (error) {
                this.response = {
                    success: false,
                    error: error.message
                };
            } finally {
                this.loading = false;
            }
        },

        // Helper: Get team data by name from munged request
        getTeam(teamName) {
            if (!this.mungedData?.teams) return null;
            return this.mungedData.teams.find(t => t.name === teamName);
        },

        // Helper: Get team seed
        getTeamSeed(teamName) {
            const team = this.getTeam(teamName);
            return team?.seed || '?';
        },

        // Helper: Get W-L record for a team
        getTeamRecord(teamName) {
            const team = this.getTeam(teamName);
            if (!team) return '?-?';
            return `${team.wins}-${team.losses}`;
        },

        // Helper: Get aff/neg display
        getAffNegDisplay(teamName) {
            const team = this.getTeam(teamName);
            if (!team) return '?-?';
            return `${team.affRounds}-${team.negRounds}`;
        },

        // Helper: Get opponent history list
        getOpponentList(teamName) {
            const team = this.getTeam(teamName);
            if (!team?.opponentHistory?.length) return 'none';
            return team.opponentHistory.join(', ');
        },

        // Helper: Check if team is sidelocked (1+ imbalance and on the side they need)
        isSidelocked(teamName, currentSide) {
            const team = this.getTeam(teamName);
            if (!team) return false;

            const diff = Math.abs(team.affRounds - team.negRounds);
            if (diff < 1) return false; // Not sidelocked if balanced

            // Check if they're on the side they need to be on
            const needsAff = team.affRounds < team.negRounds;
            const needsNeg = team.negRounds < team.affRounds;

            return (currentSide === 'aff' && needsAff) || (currentSide === 'neg' && needsNeg);
        },

        // Helper: Check if matchup is a pullup (teams have different records)
        isPullup(matchup) {
            if (matchup.isBye) return false;

            const affTeam = this.getTeam(matchup.aff);
            const negTeam = this.getTeam(matchup.neg);

            if (!affTeam || !negTeam) return false;

            // Pullup if W/L records differ
            return affTeam.wins !== negTeam.wins || affTeam.losses !== negTeam.losses;
        },

        // Computed: Flatten brackets into rows for single table rendering
        get flattenedRows() {
            const rows = [];
            let rowKey = 0;

            for (const [bracket, matchups] of this.groupedMatchups) {
                // Add bracket header row
                rows.push({
                    key: `header-${rowKey++}`,
                    isHeader: true,
                    bracket: bracket
                });

                // Add matchup rows
                for (let i = 0; i < matchups.length; i++) {
                    rows.push({
                        key: `matchup-${rowKey++}`,
                        isHeader: false,
                        index: i,
                        matchup: matchups[i]
                    });
                }
            }

            return rows;
        },

        // Computed: Group matchups by bracket (W/L record of better team)
        get groupedMatchups() {
            if (!this.response?.matchups) return [];

            const brackets = new Map();

            for (const matchup of this.response.matchups) {
                let bracketKey;

                if (matchup.isBye) {
                    const team = this.getTeam(matchup.aff);
                    bracketKey = team ? `${team.wins}-${team.losses}` : '?-?';
                } else {
                    const affTeam = this.getTeam(matchup.aff);
                    const negTeam = this.getTeam(matchup.neg);

                    // Use the better (more wins) team's record for bracket
                    if (affTeam && negTeam) {
                        if (affTeam.wins > negTeam.wins) {
                            bracketKey = `${affTeam.wins}-${affTeam.losses}`;
                        } else if (negTeam.wins > affTeam.wins) {
                            bracketKey = `${negTeam.wins}-${negTeam.losses}`;
                        } else {
                            // Same wins, use fewer losses (or just use first team)
                            bracketKey = affTeam.losses <= negTeam.losses
                                ? `${affTeam.wins}-${affTeam.losses}`
                                : `${negTeam.wins}-${negTeam.losses}`;
                        }
                    } else {
                        bracketKey = '?-?';
                    }
                }

                if (!brackets.has(bracketKey)) {
                    brackets.set(bracketKey, []);
                }
                brackets.get(bracketKey).push(matchup);
            }

            // Sort brackets by wins (descending), then losses (ascending)
            return Array.from(brackets.entries()).sort((a, b) => {
                const [aWins, aLosses] = a[0].split('-').map(Number);
                const [bWins, bLosses] = b[0].split('-').map(Number);

                if (bWins !== aWins) return bWins - aWins; // More wins first
                return aLosses - bLosses; // Fewer losses first
            });
        },

        // Watch for configuration changes and re-munge data
        init() {
            this.$watch('selectedEventId', () => {
                // Reset round selections when event changes
                if (this.selectedEventId) {
                    this.roundToMatch = 1;
                    this.baseRankingsOnRound = 1;
                    this.mungeData();
                }
            });

            this.$watch('roundToMatch', () => {
                // Ensure baseRankingsOnRound doesn't exceed roundToMatch - 1
                if (this.baseRankingsOnRound >= this.roundToMatch) {
                    this.baseRankingsOnRound = Math.max(1, this.roundToMatch - 1);
                }
                if (this.selectedEventId) {
                    this.mungeData();
                }
            });

            this.$watch('baseRankingsOnRound', () => {
                if (this.selectedEventId) {
                    this.mungeData();
                }
            });
        }
    }
}
