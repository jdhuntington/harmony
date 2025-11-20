#!/usr/bin/env python3
"""
Generate HTML pairing sheets from Harmony API responses.
Inspired by classic debate tournament software from the 90s.
"""

import json
import sys
from datetime import datetime
from pathlib import Path


def generate_html(request_data, response_data, scenario_name, output_file):
    """Generate HTML pairing sheet."""

    # Build team lookup
    teams_by_name = {team['name']: team for team in request_data['teams']}

    # Build matchups with team data
    matchups = []
    for matchup in response_data['matchups']:
        aff_name = matchup['aff']
        neg_name = matchup.get('neg')

        aff_team = teams_by_name.get(aff_name, {})
        neg_team = teams_by_name.get(neg_name, {}) if neg_name else None

        # Determine if this is a pullup (teams with different records)
        is_pullup = False
        if neg_team:
            aff_wins = aff_team.get('wins', 0)
            neg_wins = neg_team.get('wins', 0)
            is_pullup = abs(aff_wins - neg_wins) > 0

        matchups.append({
            'aff_name': aff_name,
            'neg_name': neg_name,
            'is_bye': matchup['isBye'],
            'is_pullup': is_pullup,
            'aff': aff_team,
            'neg': neg_team
        })

    # Sort matchups by seed (best first)
    matchups.sort(key=lambda m: teams_by_name.get(m['aff_name'], {}).get('seed', 999))

    # Group by record brackets
    brackets = {}
    for m in matchups:
        record = f"{m['aff'].get('wins', 0)}-{m['aff'].get('losses', 0)}"
        if record not in brackets:
            brackets[record] = []
        brackets[record].append(m)

    # Sort bracket keys by wins (descending)
    sorted_brackets = sorted(brackets.items(), key=lambda x: int(x[0].split('-')[0]), reverse=True)

    round_num = request_data.get('roundNumber', '?')

    html = f"""<!DOCTYPE html>
<html lang="en">
<head>
    <meta charset="UTF-8">
    <meta name="viewport" content="width=device-width, initial-scale=1.0">
    <title>Round {round_num} Pairings - {scenario_name}</title>
    <script src="https://cdn.tailwindcss.com"></script>
    <style>
        @media print {{
            .no-print {{ display: none; }}
        }}
        .team-name {{
            cursor: pointer;
            transition: background-color 0.2s;
        }}
        .team-highlighted {{
            background-color: #fef3c7 !important;
            font-weight: bold;
        }}
    </style>
    <script>
        function highlightTeam(teamName) {{
            // Remove all existing highlights
            document.querySelectorAll('.team-highlighted').forEach(el => {{
                el.classList.remove('team-highlighted');
            }});

            // Add highlight to all instances of this team
            if (teamName) {{
                document.querySelectorAll(`[data-team="${{teamName}}"]`).forEach(el => {{
                    el.classList.add('team-highlighted');
                }});
            }}
        }}

        function clearHighlight() {{
            document.querySelectorAll('.team-highlighted').forEach(el => {{
                el.classList.remove('team-highlighted');
            }});
        }}
    </script>
</head>
<body class="bg-gray-50 p-8">
    <div class="max-w-7xl mx-auto">
        <!-- Header -->
        <div class="bg-white rounded-lg shadow-sm p-6 mb-6 no-print">
            <h1 class="text-3xl font-bold text-gray-900 mb-2">Round {round_num} Pairings</h1>
            <div class="text-sm text-gray-600">
                <p class="mb-1"><strong>Scenario:</strong> {scenario_name}</p>
                <p class="mb-1"><strong>Total Teams:</strong> {len(request_data['teams'])}</p>
                <p class="mb-1"><strong>Matchups:</strong> {len(response_data['matchups'])}</p>
                <p><strong>Generated:</strong> {datetime.now().strftime('%Y-%m-%d %I:%M %p')}</p>
            </div>
        </div>

        <!-- Pairings Table -->
        <div class="bg-white rounded-lg shadow-sm overflow-hidden">
            <table class="min-w-full divide-y divide-gray-200">
                <thead class="bg-gray-800 text-white">
                    <tr>
                        <th class="px-4 py-3 text-left text-xs font-medium uppercase tracking-wider">Side</th>
                        <th class="px-4 py-3 text-left text-xs font-medium uppercase tracking-wider">Seeds</th>
                        <th class="px-4 py-3 text-left text-xs font-medium uppercase tracking-wider">W/L</th>
                        <th class="px-4 py-3 text-left text-xs font-medium uppercase tracking-wider">Aff</th>
                        <th class="px-4 py-3 text-left text-xs font-medium uppercase tracking-wider">Neg</th>
                        <th class="px-4 py-3 text-left text-xs font-medium uppercase tracking-wider">School Code</th>
                        <th class="px-4 py-3 text-left text-xs font-medium uppercase tracking-wider">History</th>
                    </tr>
                </thead>
                <tbody class="bg-white divide-y divide-gray-200">
"""

    current_bracket = None
    for bracket_record, bracket_matchups in sorted_brackets:
        for matchup in bracket_matchups:
            aff = matchup['aff']
            neg = matchup['neg']

            # Bracket header
            if current_bracket != bracket_record:
                current_bracket = bracket_record
                html += f"""
                    <tr class="bg-gray-100">
                        <td colspan="7" class="px-4 py-2 text-sm font-semibold text-gray-700">{bracket_record}</td>
                    </tr>
"""

            # Highlight pullups
            row_class = "bg-yellow-50" if matchup['is_pullup'] else ""

            aff_seed = aff.get('seed', '-')
            neg_seed = neg.get('seed', '-') if neg else '-'

            aff_record = f"{aff.get('wins', 0)}-{aff.get('losses', 0)}"
            neg_record = f"{neg.get('wins', 0)}-{neg.get('losses', 0)}" if neg else "-"

            aff_sides = f"{aff.get('affRounds', 0)}/{aff.get('negRounds', 0)}"
            neg_sides = f"{neg.get('affRounds', 0)}/{neg.get('negRounds', 0)}" if neg else "-"

            aff_club = aff.get('club', '')
            neg_club = neg.get('club', '') if neg else ''

            # Format opponent history (show all)
            aff_hist = ', '.join(aff.get('opponentHistory', [])) if aff.get('opponentHistory') else ''
            neg_hist = ', '.join(neg.get('opponentHistory', [])) if neg and neg.get('opponentHistory') else ''

            if matchup['is_bye']:
                html += f"""
                    <tr class="bg-blue-50">
                        <td class="px-4 py-3 text-sm">-</td>
                        <td class="px-4 py-3 text-sm font-medium">{aff_seed}</td>
                        <td class="px-4 py-3 text-sm">{aff_record}</td>
                        <td class="px-4 py-3 text-sm font-semibold team-name" data-team="{matchup['aff_name']}" onmouseover="highlightTeam('{matchup['aff_name']}')" onmouseout="clearHighlight()">{matchup['aff_name']}</td>
                        <td class="px-4 py-3 text-sm italic text-gray-500">** BYE **</td>
                        <td class="px-4 py-3 text-sm">{aff_club}</td>
                        <td class="px-4 py-3 text-xs text-gray-600">{aff_hist}</td>
                    </tr>
"""
            else:
                pullup_badge = ' <span class="text-xs bg-yellow-200 text-yellow-800 px-2 py-1 rounded">PULLUP</span>' if matchup['is_pullup'] else ''

                html += f"""
                    <tr class="{row_class}">
                        <td class="px-4 py-3 text-sm">A-N</td>
                        <td class="px-4 py-3 text-sm font-medium">{aff_seed} - {neg_seed}</td>
                        <td class="px-4 py-3 text-sm">{aff_record} - {neg_record}</td>
                        <td class="px-4 py-3 text-sm font-semibold team-name" data-team="{matchup['aff_name']}" onmouseover="highlightTeam('{matchup['aff_name']}')" onmouseout="clearHighlight()">{matchup['aff_name']}</td>
                        <td class="px-4 py-3 text-sm font-semibold team-name" data-team="{matchup['neg_name']}" onmouseover="highlightTeam('{matchup['neg_name']}')" onmouseout="clearHighlight()">{matchup['neg_name']}{pullup_badge}</td>
                        <td class="px-4 py-3 text-sm">{aff_club} - {neg_club}</td>
                        <td class="px-4 py-3 text-xs text-gray-600">
                            <div>A: {aff_hist}</div>
                            <div>N: {neg_hist}</div>
                        </td>
                    </tr>
"""

    html += """
                </tbody>
            </table>
        </div>

        <!-- Footer -->
        <div class="mt-6 text-center text-sm text-gray-500 no-print">
            <p>Generated by Harmony Pairing System</p>
        </div>
    </div>
</body>
</html>
"""

    # Write to file
    with open(output_file, 'w') as f:
        f.write(html)

    return output_file


def main():
    if len(sys.argv) < 4:
        print("Usage: generate-pairing-html.py <request.json> <response.json> <scenario_name> <output.html>")
        sys.exit(1)

    request_file = sys.argv[1]
    response_file = sys.argv[2]
    scenario_name = sys.argv[3]
    output_file = sys.argv[4]

    # Load JSON files
    with open(request_file) as f:
        request_data = json.load(f)

    with open(response_file) as f:
        response_data = json.load(f)

    # Generate HTML
    output_path = generate_html(request_data, response_data, scenario_name, output_file)
    print(f"Generated: {output_path}")


if __name__ == '__main__':
    main()
