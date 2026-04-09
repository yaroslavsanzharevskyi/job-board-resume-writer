# job-board-resume-writer

AI-powered resume generator: reads Adzuna job postings from Azure Databricks Delta Lake, and generates tailored resumes via Azure AI Foundry (GPT-4o).

## Project structure

```
job-board-resume-writer/
├── src/
│   └── DatabricksJobsFunction/   ← Azure Functions backend (.NET 8, isolated worker)
│       ├── DatabricksJobsFunction.csproj
│       ├── Program.cs            ← DI wiring
│       ├── host.json
│       ├── local.settings.json   ← credentials (never commit real values)
│       ├── Models.cs             ← JobPosting, ResumeResult, request/response records
│       ├── DatabricksService.cs  ← Databricks SQL Statements REST API client
│       ├── FoundryService.cs     ← Azure AI Foundry / GPT-4o client
│       ├── GetJobsFunction.cs    ← GET /api/jobs, GET /api/jobs/{id}
│       └── GenerateResumeFunction.cs  ← POST /api/resume/generate
├── frontend/                     ← React/HTML SWA (not yet built — Step 4)
├── docs/
│   └── AI-Resume-Generator-Handoff.docx
├── job-board-resume-writer.sln
└── CLAUDE.md
```

## Architecture

| Service | Role |
|---|---|
| Azure Databricks | Source of truth — Adzuna job postings in Delta Lake |
| Azure Functions (.NET 8) | Serverless backend — queries Databricks, calls Foundry |
| Azure AI Foundry | Hosts GPT-4o deployment for resume generation |
| Azure Blob Storage | Stores CV document (future — currently passed in request body) |
| Azure Static Web Apps | Frontend UI — job browser and resume output (not yet built) |

## API endpoints

- `GET /api/jobs` — list jobs; optional `?search=<term>&limit=<n>` (default 50, max 200)
- `GET /api/jobs/{id}` — single job by ID
- `POST /api/resume/generate` — generate tailored resume
  ```json
  { "jobId": "abc123", "cvText": "Full CV text..." }
  ```
  Returns `{ resumeMarkdown, jobTitle, company, tokensUsed, generatedAt }`.

## Local development

### Prerequisites
- .NET 8 SDK
- Azure Functions Core Tools v4: `npm install -g azure-functions-core-tools@4`
- Azurite (local storage emulator): `npm install -g azurite`, run `azurite` in a terminal

### Run
```bash
cd src/DatabricksJobsFunction
dotnet restore
func start
```

### Configuration — `local.settings.json`
Fill in all 9 values before running. **Never commit real credentials.**

```jsonc
{
  "DATABRICKS_HOST":      "https://<workspace>.azuredatabricks.net",
  "DATABRICKS_TOKEN":     "<personal-access-token>",
  "DATABRICKS_HTTP_PATH": "/sql/1.0/warehouses/<warehouse-id>",
  "DATABRICKS_CATALOG":   "hive_metastore",
  "DATABRICKS_SCHEMA":    "default",
  "DATABRICKS_TABLE":     "adzuna_jobs",
  "FOUNDRY_ENDPOINT":     "https://<project>.openai.azure.com/",
  "FOUNDRY_API_KEY":      "<key>",
  "FOUNDRY_DEPLOYMENT_NAME": "gpt-4o-resume"
}
```

**Databricks token:** Settings → Developer → Access tokens  
**Foundry values:** ai.azure.com → project → deployment → Keys & Endpoints

## Delta table schema

`DatabricksService.cs` expects these column names. If your Adzuna export differs, update the `SELECT` statement, `MapRow`, and `Models.cs`:

| Column | Maps to |
|---|---|
| `id` | `JobPosting.Id` |
| `title` | `JobPosting.Title` |
| `company` | `JobPosting.Company` |
| `location` | `JobPosting.Location` |
| `description` | `JobPosting.Description` (passed to GPT-4o) |
| `salary_min` | `JobPosting.SalaryMin` |
| `salary_max` | `JobPosting.SalaryMax` |
| `contract_type` | `JobPosting.ContractType` |
| `category` | `JobPosting.Category` |
| `created` | `JobPosting.CreatedAt` |

## What to build next

1. **Verify Delta table schema** — `SELECT * FROM your_table LIMIT 5` in Databricks; update column names in `DatabricksService.cs` / `Models.cs` if needed.
2. **Test Databricks connection** — fill `local.settings.json`, run `func start`, hit `GET /api/jobs`.
3. **Test resume generation** — `POST /api/resume/generate` with a real job ID and sample CV text.
4. **Build the frontend** — React or plain HTML on Azure Static Web Apps. Job list on left, CV input + generated resume on right.
5. **CV upload from Blob Storage** — replace `cvText` in request body with a Blob Storage reference so the user uploads once and reuses.

## Key files

- [src/DatabricksJobsFunction/DatabricksService.cs](src/DatabricksJobsFunction/DatabricksService.cs) — edit SQL column names here if schema differs
- [src/DatabricksJobsFunction/Models.cs](src/DatabricksJobsFunction/Models.cs) — edit `JobPosting` record if schema differs
- [src/DatabricksJobsFunction/local.settings.json](src/DatabricksJobsFunction/local.settings.json) — credentials (fill in, never commit)
- [src/DatabricksJobsFunction/FoundryService.cs](src/DatabricksJobsFunction/FoundryService.cs) — GPT-4o prompt lives here
