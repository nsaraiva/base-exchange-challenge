# Base Exchange Challenge

Technical challenge implemented in C# with two applications:

- **OrderGenerator** (ASP.NET Core MVC): receives order input from a web form and sends FIX `NewOrderSingle` messages.
- **OrderAccumulator** (QuickFIX/n app): receives FIX orders, applies validation and exposure rules per symbol, and returns FIX `ExecutionReport` acceptance/rejection.

---

## Technologies

- C#
- .NET 8
- ASP.NET Core MVC
- QuickFIX/n Core
- QuickFIX/n FIX 4.4
- xUnit
- Moq
- Shouldly

---

## Solution Architecture

The solution is split into two independent projects that communicate through FIX 4.4 over TCP:

1. **OrderGenerator** (initiator)
   - UI for order entry
   - input validation
   - FIX session startup
   - sends `NewOrderSingle`
   - receives and displays `ExecutionReport`

2. **OrderAccumulator** (acceptor)
   - listens for FIX connections
   - validates incoming orders
   - calculates cumulative financial exposure by symbol
   - applies the absolute exposure limit rule
   - sends `ExecutionReport` with acceptance or rejection reason

---

## Business Rules

- Allowed symbols: `PETR4`, `VALE3`, `VIIA4`
- Allowed sides: Buy (`1`) and Sell (`2`)
- Quantity:
  - integer
  - greater than `0`
  - lower than `100000`
- Price:
  - decimal
  - greater than `0`
  - multiple of `0.01`
  - lower than `1000`
- Exposure is calculated **per symbol**
- Buy increases exposure
- Sell decreases exposure
- Absolute exposure limit per symbol: `100,000,000`
- Accepted orders return `ExecutionReport` with `ExecType = New`
- Rejected orders return `ExecutionReport` with `ExecType = Rejected`

---

## Repository Structure

> Below is the complete expected structure for using this solution (projects + main folders).  
> If your local tree differs slightly, keep the same project responsibilities and FIX config placement.

```text
BaseExchangeChallenge/
├── src/
│   ├── BaseExchangeChallenge.OrderGenerator/
│   │   ├── Controllers/
│   │   ├── Models/
│   │   ├── Services/
│   │   ├── Views/
│   │   │   ├── Home/
│   │   │   └── Shared/
│   │   ├── wwwroot/
│   │   │   ├── css/
│   │   │   ├── js/
│   │   │   └── lib/
│   │   ├── Fix/
│   │   │   └── ordergenerator.cfg
│   │   ├── Properties/
│   │   ├── Program.cs
│   │   ├── appsettings.json
│   │   └── BaseExchangeChallenge.OrderGenerator.csproj
│   │
│   └── BaseExchangeChallenge.OrderAccumulator/
│       ├── Fix/
│       │   ├── Contracts/
│       │   ├── Services/
│       │   ├── QuickFixApp.cs
│       │   └── orderaccumulator.cfg
│       ├── Program.cs
│       └── BaseExchangeChallenge.OrderAccumulator.csproj
│
├── tests/
│   ├── BaseExchangeChallenge.OrderAccumulator.Tests/
│   │   ├── Unit/
│   │   ├── Integration/
│   │   └── BaseExchangeChallenge.OrderAccumulator.Tests.csproj
│   │
│   └── BaseExchangeChallenge.OrderGenerator.Tests/
│       ├── Unit/
│       ├── Integration/
│       ├── fixtures/
│       │   ├── ordergenerator-test.cfg
│       │   └── orderaccumulator-test.cfg
│       └── BaseExchangeChallenge.OrderGenerator.Tests.csproj
│
├── BaseExchangeChallenge.sln
├── .gitignore
└── README.md
```

---

## Prerequisites

- .NET SDK 8.0+
- Windows, Linux, or macOS
- IDE (optional): Visual Studio, Rider, or VS Code

---

## Setup and Run

### 1) Clone repository

```bash
git clone https://github.com/nsaraiva/base-exchange-challenge.git
cd base-exchange-challenge
```

### 2) Restore dependencies

```bash
dotnet restore
```

### 3) Build solution

```bash
dotnet build
```

### 4) Run OrderAccumulator (terminal 1)

```bash
cd src/BaseExchangeChallenge.OrderAccumulator
dotnet run
```

### 5) Run OrderGenerator (terminal 2)

```bash
cd src/BaseExchangeChallenge.OrderGenerator
dotnet run
```

### 6) Open browser

Access the URL printed by `OrderGenerator` (typically similar to `https://localhost:xxxx`), fill the form, and submit orders.

---

## Running Tests

From repository root:

```bash
dotnet test
```

Run specific test project:

```bash
dotnet test tests/BaseExchangeChallenge.OrderAccumulator.Tests
dotnet test tests/BaseExchangeChallenge.OrderGenerator.Tests
```

---

## FIX Configuration Notes

- Runtime configs:
  - `src/BaseExchangeChallenge.OrderGenerator/Fix/ordergenerator.cfg`
  - `src/BaseExchangeChallenge.OrderAccumulator/Fix/orderaccumulator.cfg`
- Integration test configs:
  - `tests/BaseExchangeChallenge.OrderGenerator.Tests/fixtures/ordergenerator-test.cfg`
  - `tests/BaseExchangeChallenge.OrderGenerator.Tests/fixtures/orderaccumulator-test.cfg`

If you run into port conflicts, change test ports in both fixture files consistently.

---

## Troubleshooting

- **Session not connecting**
  - verify `SenderCompID` / `TargetCompID` pairing in both cfg files
  - verify host/port match between initiator and acceptor
- **Config file not found in tests**
  - ensure fixture files are under `tests/.../fixtures`
  - ensure test `.csproj` copies `fixtures/*.cfg` to output
- **Order rejected unexpectedly**
  - confirm symbol/side/quantity/price constraints
  - check cumulative exposure already stored for that symbol

---

## Git Ignore

This repository should include a `.gitignore` for .NET and IDE artifacts (`bin/`, `obj/`, `.vs/`, etc.).

---

## Challenge Reference

> This is a challenge by [Coodesh](https://coodesh.com/)