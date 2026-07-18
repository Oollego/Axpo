# Axpo Coding Challenge

This repository contains my solution for the Axpo coding challenge.

The application periodically retrieves power trades from the provided `PowerService.dll`, aggregates hourly power positions, and exports the result to a CSV report.

The solution is implemented as a **.NET 10 Worker Service** using a layered architecture with a clear separation between business logic and infrastructure.


## Features

- Scheduled report generation using `BackgroundService`
- Aggregation of hourly power positions
- Automatic handling of 23/24/25-hour trading days (DST)
- CSV export
- Configurable execution interval
- Configurable output folder
- Retry mechanism for `PowerService`
- Dependency Injection
- Structured logging
- Integration tests

## Architecture

The solution follows a simple layered architecture.

| Layer | Responsibility |
|-------|----------------|
| **Domain** | Business models and interfaces. No dependency on the external SDK. |
| **Application** | Coordinates the report generation workflow. |
| **Infrastructure** | Integrates with `PowerService`, writes CSV files, and provides system services. |
| **Worker** | Executes report generation on a configurable schedule. |

The Domain layer is completely independent from `PowerService.dll`. SDK models are mapped to internal domain models before entering the business logic.

## Design Decisions

Some requirements in the challenge were open to interpretation. I prepared a list of clarification questions, but since no answers were available during implementation, the following assumptions were made.

### Trading day

The trading day is determined using the current London local time. After 23:00, reports are generated for the next trading day.

### Empty result

If no trades are returned, the application logs a warning. The current implementation still generates a CSV file containing only the header.

### Daylight Saving Time

The application uses the periods returned by `PowerService` without modification.

- Spring DST → 23 rows
- Normal day → 24 rows
- Autumn DST → 25 rows

This ensures the report always reflects the data returned by the SDK.

Period-to-hour conversion uses UTC arithmetic: the trading day start time is converted to UTC, hours are added in UTC, and the result is converted back to London local time. This correctly handles DST transitions — skipped hours during spring-forward and repeated hours during fall-back.

### Retry policy

Calls to `PowerService` are retried up to three times before the exception is propagated.

### Concurrent executions

Only one report can run at a time.

If a scheduled execution starts while the previous one is still running, the new execution is skipped and a warning is written to the log.

### Domain isolation

Business logic does not depend on `PowerService.dll`.

External SDK models are mapped to internal domain models inside the repository, reducing coupling and improving testability.

## Getting Started

### Prerequisites

- .NET 10 SDK
- `PowerService.dll` (included in `Axpo/lib/`)
- Environment variable `SERVICE_MODE=Debug` — required to prevent `PowerServiceException` from the SDK's internal validation

### Restore

```bash
dotnet restore
```

### Build

```bash
dotnet build
```

### Run

```bash
dotnet run --project Axpo
```

The worker generates the first report immediately after startup and then continues according to the configured interval.

### Command-line arguments

Configuration values can also be overridden from the command line.

```bash
dotnet run --project Axpo -- --interval 15 --output Reports
```

| Argument | Description |
|----------|-------------|
| `--interval` | Report interval in minutes |
| `--output` | Output folder for generated CSV files |

Command-line arguments have the highest configuration priority.

## Configuration

Default configuration:

```json
{
  "ReportOptions": {
    "OutputFolder": "Reports",
    "IntervalMinutes": 30
  }
}
```

Configuration can be provided through:

- `appsettings.json`
- command-line arguments

Command-line arguments override values from `appsettings.json`.

## Project Structure

```
Axpo
│
├── Application
│   ├── Configuration
│   └── Services
│
├── Domain
│   ├── Interfaces
│   └── Models
│
├── Infrastructure
│   ├── Data
│   ├── Services
│   └── Time
│
├── Program.cs
├── ReportWorker.cs
└── ServiceCollectionExtensions.cs
```

- **Domain** contains business models and contracts.
- **Application** orchestrates the report generation workflow.
- **Infrastructure** integrates with external services and the file system.
- **Worker** schedules report execution.

## Testing

The project contains integration tests that use the real `PowerService.dll` to verify aggregation logic:

| Test | Verifies |
|------|----------|
| `ShouldReturn23Rows_ForSpringDst` | 23 rows during spring-forward |
| `ShouldReturn24Rows_ForNormalDay` | 24 rows on a normal day |
| `ShouldReturn25Rows_ForAutumnDst` | 25 rows during fall-back |
| `ShouldReturnSameNumberOfRows_AsPowerServicePeriods` | Row count matches period count |
| `ShouldAggregateVolumes_ForEachPeriod` | Volumes summed correctly across trades |
| `ShouldSkipHour1_ForSpringDst` | Hour 01:00 is absent (skipped by DST) |
| `ShouldContainTwoHour1_ForAutumnDst` | Hour 01:00 appears twice (repeated by DST) |
| `ShouldStartAt23_ForNormalDay` | First row is 23:00, last is 22:00 |
| `ShouldReturnEmpty_WhenTradesAreEmpty` | Empty input produces empty output |

Run all tests:

```bash
dotnet test
```

## CSV Output

Generated reports are stored in the configured output folder.

Example:

```text
Local Time,Volume
23:00,1431.81
00:00,1234.13
01:00,730.45
...
```

File naming convention:

```text
PowerPosition_yyyyMMdd_HHmm.csv
```

CSV files are written using UTF-8 encoding and `InvariantCulture` for numeric values.

## Notes

The goal of this project was to provide a clean and maintainable solution while keeping the implementation focused on the requirements of the coding challenge.

Where the specification was ambiguous, reasonable assumptions were made and documented above.