# Harmony Web UI

A lightweight web interface for checking pairings against the Harmony pairing algorithm.

## Features

- Upload JSON data from system of record (~200KB)
- Configure seeding round and opponent history
- Transform data and call Harmony API
- View generated pairings in a compact table

## Running Locally

### Option 1: Simple File Server (Python)
```bash
cd Harmony.Web
python3 -m http.server 8000
```

Then open http://localhost:8000

### Option 2: Using Node.js
```bash
npx serve Harmony.Web
```

### Option 3: VS Code Live Server
Install the "Live Server" extension and right-click `index.html` → "Open with Live Server"

## API Endpoints

The UI can connect to:
- **Localhost**: `http://localhost:5199` (start with `scripts/start-server.sh`)
- **Lambda**: Your deployed Lambda function URL

## Data Format

The UI imports tournament data exports with this structure:

```json
{
  "tournament": "Tournament Name",
  "events": [
    {
      "event_id": 2281,
      "event_name": "Lincoln Douglas",
      "teams": [
        {
          "entry_id": 35366,
          "mask": "MD-126",
          "history": [
            {
              "round": 1,
              "side": "aff/neg/bye",
              "opponent_mask": "WB-112",
              "result": "win/loss/unknown/bye"
            }
          ],
          "rankings": {
            "1": 1,
            "2": 2
          }
        }
      ]
    }
  ]
}
```

The transformation logic:
- Calculates W/L record through the "base rankings on round"
- Counts aff/neg assignments
- Tracks opponent history (excluding byes and results after base round)
- Uses ranking from base round as seed
- Teams with a bye are not bye-eligible

## CORS Configuration

To call the Lambda endpoint from the web UI, ensure CORS headers are enabled on your API:

```
Access-Control-Allow-Origin: *
Access-Control-Allow-Methods: POST, OPTIONS
Access-Control-Allow-Headers: Content-Type
```

## Deployment

### S3 + CloudFront (Recommended)

1. Build is not needed - just upload the files
2. Upload `index.html` and `app.js` to S3 bucket
3. Enable static website hosting
4. Create CloudFront distribution (gets HTTPS automatically)
5. Update CORS on your Lambda to allow your CloudFront domain

### Quick Deploy Script
```bash
aws s3 sync Harmony.Web/ s3://your-bucket-name/ --exclude "README.md"
```
