#!/bin/bash

# Run all test scenarios against the Harmony.Api server
# Usage: ./run-test-scenarios.sh [server_url]
# Default server URL: http://localhost:5199

set -e

SERVER_URL="${1:-http://localhost:5199}"
SCRIPT_DIR="$(dirname "$0")"
SCENARIOS_DIR="$SCRIPT_DIR/test-scenarios"
OUTPUT_DIR="$SCRIPT_DIR/../pairings-output"
HTML_GENERATOR="$SCRIPT_DIR/generate-pairing-html.py"

echo "Testing Harmony API at $SERVER_URL"
echo "=================================="
echo ""

# Create output directory
mkdir -p "$OUTPUT_DIR"

# Check if server is running
if ! curl -s -f "$SERVER_URL/api/pairing/generate" -X POST -H "Content-Type: application/json" -d '{"teams":[],"roundNumber":1}' > /dev/null 2>&1; then
    echo "ERROR: Server not running at $SERVER_URL"
    echo "Start the server with: ./scripts/start-server.sh"
    exit 1
fi

# Run each scenario
for scenario in "$SCENARIOS_DIR"/*.json; do
    filename=$(basename "$scenario")
    scenario_name="${filename%.json}"
    echo "Running scenario: $filename"
    echo "---"

    # Send request and save response
    response=$(curl -s -X POST "$SERVER_URL/api/pairing/generate" \
        -H "Content-Type: application/json" \
        -d @"$scenario")

    # Save response JSON
    response_file="$OUTPUT_DIR/${scenario_name}-response.json"
    echo "$response" | python3 -m json.tool > "$response_file"

    # Generate HTML
    html_file="$OUTPUT_DIR/${scenario_name}.html"
    python3 "$HTML_GENERATOR" "$scenario" "$response_file" "$scenario_name" "$html_file"

    # Print response
    cat "$response_file"
    echo ""
    echo "📄 HTML: $html_file"
    echo "=================================="
    echo ""
done

echo "All scenarios completed!"
echo ""
echo "📂 Output directory: $OUTPUT_DIR"
echo "🌐 Open HTML files in your browser to view formatted pairings"
