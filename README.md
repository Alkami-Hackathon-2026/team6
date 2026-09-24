# Alkami Hackathon Sandbox API

Local demo API with member, account, share, and debit card data for Alkami microservice development. No authentication — intended for local hackathon use only.

## Run

```powershell
cd "D:\Repo\Alkami Hackathon\Sandbox"
dotnet run
```

## Gemini (free tier testing)

1. Create a free API key at [Google AI Studio](https://aistudio.google.com/apikey).
2. Copy `appsettings.example.json` to `appsettings.json` and paste your key under `Gemini:ApiKey`.

`appsettings.json` is gitignored so your key stays local.

Alternatively, use user secrets or an environment variable:

```powershell
dotnet user-secrets set "Gemini:ApiKey" "YOUR_GEMINI_API_KEY"
# or
$env:GEMINI_API_KEY = "YOUR_GEMINI_API_KEY"
```

3. Verify configuration:

```http
GET http://localhost:5199/api/gemini/status
```

4. Send a test prompt:

```http
POST http://localhost:5199/api/gemini/chat
Content-Type: application/json

{
  "prompt": "Summarize what a credit union share account is in one sentence."
}
```

Default model is `gemini-3.6-flash` (free tier). Override in `appsettings.json` under `Gemini:Model` if needed.

## Money rules (Gemini-powered)

Create rules from natural language and persist them to `Data/rules.json`.

```http
POST http://localhost:5199/api/rules/generate
Content-Type: application/json

{
  "memberId": "mbr-1001",
  "prompt": "When I transfer more than $500 from checking to Christmas Savings, send me a push notification.",
  "save": true
}
```

| Method | Endpoint | Description |
|--------|----------|-------------|
| GET | `/api/rules` | All saved rules |
| GET | `/api/rules/member/{memberId}` | Rules for a member |
| GET | `/api/rules/{ruleId}` | Single rule |
| POST | `/api/rules/generate` | Create rule via Gemini (optionally save) |
| POST | `/api/rules` | Create rule manually |
| PUT | `/api/rules/{ruleId}` | Update rule |
| DELETE | `/api/rules/{ruleId}` | Delete rule |

Set `"save": false` on generate to preview the rule JSON without writing to disk.

Base URL: **http://localhost:5199**

## Demo members

| Member | ID | Member # | Notes |
|--------|----|----------|-------|
| Mike Brady | `mbr-1001` | `0001001842` | Checking, Christmas Savings, Vacation Fund |
| Sarah Chen | `mbr-1002` | `0001002937` | Premier Checking, Emergency Savings |
| Demo Member | `mbr-1003` | `0001004510` | Simple demo accounts |

## Key endpoints

| Method | Endpoint | Description |
|--------|----------|-------------|
| GET | `/api/demo/health` | Health check |
| GET | `/api/members` | All members |
| GET | `/api/members/{memberId}` | Member by ID |
| GET | `/api/members/by-number/{memberNumber}` | Member by number |
| GET | `/api/members/{memberId}/profile` | Member + accounts + shares + cards |
| GET | `/api/members/{memberId}/accounts` | Accounts for member |
| GET | `/api/members/{memberId}/shares` | Shares for member |
| GET | `/api/members/{memberId}/debit-cards` | Debit cards for member |
| GET | `/api/accounts` | All accounts |
| GET | `/api/accounts/{accountId}/shares` | Shares on account |
| GET | `/api/shares` | All shares |
| GET | `/api/shares/{shareId}/detail` | Share + debit cards |
| GET | `/api/debit-cards` | All debit cards |
| POST | `/api/demo/transfers/simulate` | Dry-run transfer validation |

## Sample transfer simulation

```http
POST http://localhost:5199/api/demo/transfers/simulate
Content-Type: application/json

{
  "fromShareId": "shr-3001",
  "toShareId": "shr-3002",
  "amount": 7500,
  "memo": "Rule: high transfer to Christmas Savings"
}
```

## Alkami microservice usage

Call this API from your microservice using `HttpClient` or Alkami's `RestClient` pattern:

```csharp
var client = new HttpClient { BaseAddress = new Uri("http://localhost:5199") };
var profile = await client.GetFromJsonAsync<MemberProfile>("/api/members/mbr-1001/profile");
```

Configure the base URL in your microservice provider settings, e.g. `SandboxApiBaseUrl = http://localhost:5199`.

## Data files

Edit JSON under `Data/` to change demo data:

- `members.json`
- `accounts.json`
- `shares.json`
- `debit-cards.json`
- `rules.json` (generated money rules)

Restart the API after changing data files (rules are read/written at runtime).
