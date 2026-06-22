# Base Exchange Challenge

Technical challenge in C# with an ASP.NET Core MVC OrderGenerator and a FIX 4.4 OrderAccumulator using QuickFIX/n.

## Technologies

- C#
- .NET 8
- ASP.NET Core MVC
- QuickFIX/n
- FIX 4.4
- HTML
- CSS

## Project Overview

This repository contains a backend technical challenge composed of two C# applications:

- **OrderGenerator**: a web application responsible for receiving order input from a form and sending a `NewOrderSingle` message.
- **OrderAccumulator**: a backend application responsible for receiving FIX messages, calculating financial exposure per symbol, and returning an `ExecutionReport` with acceptance or rejection.

## Project Structure

```text
BaseExchangeChallenge/
├── src/
│   ├── BaseExchangeChallenge.OrderGenerator/
│   └── BaseExchangeChallenge.OrderAccumulator/
├── tests/
└── README.md
```

## Requirements

- .NET 8 SDK or newer
- Visual Studio 2026 or Visual Studio Code / Rider
- Local environment capable of running two .NET applications

## How to Run

### 1. Clone the repository

```bash
git clone https://github.com/nsaraiva/base-exchange-challenge.git
cd base-exchange-challenge
```

### 2. Restore dependencies

```bash
dotnet restore
```

### 3. Run the OrderAccumulator

```bash
cd src/BaseExchangeChallenge.OrderAccumulator
dotnet run
```

### 4. Run the OrderGenerator

Open a new terminal and run:

```bash
cd src/BaseExchangeChallenge.OrderGenerator
dotnet run
```

### 5. Access the application

Open the local URL shown in the terminal and use the form to submit orders.

## Business Rules

- Allowed symbols: `PETR4`, `VALE3`, `VIIA4`
- Allowed sides: Buy and Sell
- Quantity must be a positive integer lower than `100000`
- Price must be a positive decimal multiple of `0.01` and lower than `1000`
- Financial exposure is calculated per symbol
- Buy orders increase exposure
- Sell orders decrease exposure
- Absolute exposure limit per symbol: `R$ 100,000,000`
- Accepted orders must return `ExecutionReport` with `ExecType = New`
- Rejected orders must return `ExecutionReport` with `ExecType = Rejected`

## Notes

- The solution is being developed as a technical challenge.
- The communication between applications is based on the FIX 4.4 protocol.
- Additional improvements may include automated tests, Docker support, and exposure persistence.

## Git

Make sure the repository includes a proper `.gitignore` file for .NET and Visual Studio artifacts.

## Challenge Reference

> This is a challenge by [Coodesh](https://coodesh.com/)