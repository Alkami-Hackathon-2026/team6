# MyMoneyRules — Cursor Chat Handoff

Use this file to restore context when opening this project on another machine or in the Alkami SDK environment.

**Created:** 2026-09-23  
**Project:** `HACK26.MS.MyMoneyRules` (Alkami hackathon microservice)  
**Related:** `D:\Repo\Alkami Hackathon\Sandbox` (Gemini source)

---

## Chat summary

### 1. Initial request — summarize the codebase

**MyMoneyRules** is an Alkami **Provider Service** template (.NET Framework 4.8) scaffolded for a **user-defined money/transaction rules engine** hackathon feature.

**Solution layout:**

| Project | Role |
|---|---|
| `HACK26.MS.MyMoneyRules.Contracts` | WCF service interface |
| `HACK26.MS.MyMoneyRules.Service` | Business logic (`ServiceImp`) |
| `HACK26.MS.MyMoneyRules.Service.Host` | Windows service (Topshelf, log4net, ZeroMQ) |
| `HACK26.MS.MyMoneyRules.Service.Client` | Provider-based client proxy |
| `HACK26.MS.MyMoneyRules.Data` | Domain models + JSON sample payloads |
| `HACK26.MS.MyMoneyRules.Data.Validations` | Request validators |
| `HACK26.MS.MyMoneyRules.Service.Tests` | NUnit tests |

**Provider identity:** type `HACK26`, name `HACK26.MS.MyMoneyRules`

**What the service did at that point (template + small additions):**

- `GetSettingsAsync` — reads two template provider settings
- `GetDataAsync` — returns hardcoded James Bond / MI6 demo data
- `GetFDICConfigurationAsync` — calls Alkami FDIC compliance API via RestSharp

**Intended domain (designed in JSON, not yet implemented in C#):**

Under `HACK26.MS.MyMoneyRules.Data/json/` there are 12 sample files modeling:

```
Transaction Event → Transaction Context → Decision Rule → Trigger + Conditions
  → Rule Evaluation → Actions → Action Execution
```

Example rule: **"Large purchase or low balance alert"** — fires on debit ≥ $200 OR balance < $1,000 OR electronics merchant; actions include push notification and transaction tag.

**State at handoff:** infrastructure is ready; rules engine logic exists only as JSON fixtures.

---

### 2. Second request — add Gemini from Sandbox project

**Goal:** Port Gemini functionality from `Alkami Hackathon/Sandbox` into MyMoneyRules.

**Sandbox Gemini (source):**

- .NET 8 ASP.NET Core app
- Uses `Google.GenAI` NuGet package (v1.22.0)
- `GeminiService` + `IGeminiService`
- Endpoints: `GET /api/gemini/status`, `POST /api/gemini/chat`
- Config: `Gemini:ApiKey`, `Gemini:Model` (default `gemini-3.6-flash`)
- Env fallback: `GEMINI_API_KEY`

**Integration approach chosen:**

MyMoneyRules is **.NET Framework 4.8**, so `Google.GenAI` SDK was **not** used. Instead, Gemini was integrated via the **REST API** using **RestSharp + Newtonsoft.Json** (same pattern as the existing FDIC call).

---

## Gemini integration — what was implemented

### New contract methods (`IMyMoneyRulesServiceContract`)

```csharp
Task<GeminiStatusResponse> GetGeminiStatusAsync(GetGeminiStatusRequest request);
Task<GeminiChatResponse> GenerateGeminiChatAsync(GeminiChatRequest request);
```

### New files

| File | Purpose |
|---|---|
| `Data/Gemini/GeminiDefaults.cs` | Default model + env var name |
| `Contracts/Requests/GeminiChatRequest.cs` | `Prompt`, `SystemInstruction` |
| `Contracts/Requests/GetGeminiStatusRequest.cs` | Status check request |
| `Contracts/Responses/GeminiChatResponse.cs` | `Model`, `Text` |
| `Contracts/Responses/GeminiStatusResponse.cs` | `Configured`, `Model`, `Message` |
| `Service/Gemini/GeminiService.cs` | REST client for `generateContent` |
| `Service/ServiceImp.Gemini.cs` | ServiceImp wiring |
| `Data.Validations/GeminiChatRequestValidator.cs` | Requires `Prompt` |

### Modified files

- `Data/ProviderSettings/SettingNames.cs` — added `GeminiApiKey`, `GeminiModel`
- `Service/ServiceImp.SettingManagement.cs` — defaults, descriptors, validation
- `Service.Host/DistributedService.cs` — validator registration + pass-through methods
- `Service.Client/MyMoneyRulesServiceClient.cs` — client proxy methods
- `Service.Tests/ServiceTests.cs` — `CanGetGeminiStatusWhenNotConfigured` test
- All relevant `.csproj` files — compile includes

### Provider settings (Alkami Admin Portal)

| Setting | Default | Notes |
|---|---|---|
| **Gemini Api Key** | empty | Sensitive; marked sensitive in descriptor |
| **Gemini Model** | `gemini-3.6-flash` | Same default as Sandbox |

**API key resolution order:**

1. Provider setting `Gemini Api Key`
2. Environment variable `GEMINI_API_KEY`

### Gemini REST API details

```
POST https://generativelanguage.googleapis.com/v1beta/models/{model}:generateContent?key={apiKey}
```

Request body includes `contents` (user prompt) and optional `systemInstruction`.

### Usage from client

```csharp
var client = new MyMoneyRulesServiceClient();

var status = await client.GetGeminiStatusAsync(new GetGeminiStatusRequest
{
    // BankIdentifier, BankUri, UserIdentifier, etc.
});

var response = await client.GenerateGeminiChatAsync(new GeminiChatRequest
{
    Prompt = "Suggest a money rule for large grocery purchases.",
    SystemInstruction = "You help credit union members create transaction alert rules."
});
```

### Configuration steps (SDK / local)

1. Get a free API key from https://aistudio.google.com/apikey
2. Set **Gemini Api Key** in provider settings, **or** set `GEMINI_API_KEY` on the service host machine
3. Optionally override **Gemini Model** (default: `gemini-3.6-flash`)
4. Run `insert_provider_setting.sql` if provider is not yet registered
5. Restore NuGet packages and build the solution in Visual Studio

### Test added

`CanGetGeminiStatusWhenNotConfigured` — verifies `Configured == false` and default model when no API key is set.

---

## Build notes

- Solution: `HACK26.MS.MyMoneyRules.sln`
- Requires Alkami SDK NuGet packages restored (`packages.config` projects)
- Build failed in one environment due to missing NuGet restore (not due to Gemini code specifically)
- Use Visual Studio NuGet Package Restore before first build on a new machine

---

## Suggested next steps (not yet done)

- Wire Gemini into the **rules engine** — e.g. natural-language rule creation from the JSON domain model
- Call Sandbox demo API (`http://localhost:5199`) for member/account data if needed alongside Gemini
- Replace template `GetDataAsync` demo data with real rules-engine endpoints
- Implement rule evaluation pipeline using the JSON schemas in `Data/json/`

---

## Key paths

```
D:\Repo\Alkami Hackathon\team6\HACK26.MS.MyMoneyRules\
D:\Repo\Alkami Hackathon\Sandbox\   (original Gemini + demo API)
```

## Sandbox Gemini reference files

```
Sandbox/Services/GeminiService.cs
Sandbox/Services/IGeminiService.cs
Sandbox/Controllers/GeminiController.cs
Sandbox/Configuration/GeminiOptions.cs
Sandbox/Models/GeminiChatRequest.cs
Sandbox/Models/GeminiChatResponse.cs
```

---

## Prompt to resume in Cursor on another machine

Paste this into a new Cursor chat after opening the SDK copy of the project:

> I'm continuing work on HACK26.MS.MyMoneyRules. Read `CURSOR_CHAT_HANDOFF.md` in the project root for full context. Gemini was integrated from the Sandbox project via REST API (not Google.GenAI SDK). I need help testing Gemini in the SDK environment and/or wiring it into the money rules engine.
