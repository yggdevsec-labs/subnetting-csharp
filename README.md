# Subnet CLI

A command-line IPv4 subnet calculator and Variable Length Subnet Masking (VLSM) planner built with C# and .NET 10.

The application provides subnet calculations, IPv4 address validation, subnet membership checks, and VLSM-based network allocation.

## Features

- Calculate IPv4 network information from CIDR notation.
- Display subnet masks, network addresses, and broadcast addresses.
- Calculate usable host ranges and address capacity.
- Check whether an IPv4 address belongs to a subnet.
- Generate VLSM allocation plans based on host requirements.
- Allocate larger subnet requirements first.
- Validate IPv4 addresses using strict dotted-decimal notation.
- Handle special prefixes `/0`, `/31`, and `/32`.
- Provide a standalone CLI with explicit exit codes and error handling.

## Requirements

- .NET 10 SDK

## Project Structure

```text
subnetting-csharp/
├── SubnetCli/
│   ├── Cli/
│   │   ├── CliApplication.cs
│   │   ├── CliCommand.cs
│   │   └── ConsoleOutput.cs
│   ├── Program.cs
│   └── SubnetCli.csproj
│
├── SubnetCli.Core/
│   ├── Ipv4Subnet.cs
│   ├── SubnetPlanner.cs
│   └── SubnetCli.Core.csproj
│
├── SubnetCli.Tests/
│   
│   └── SubnetCli.Tests.csproj
│   ├── Ipv4SubnetTests.cs
│   ├── SubnetPlannerTests.cs
│   └── CliApplicationTests.cs
│
├── SubnetCli.SmokeTests/
│   ├── Program.cs
│   └── SubnetCli.SmokeTests.csproj
│
├── .editorconfig
├── .gitignore
├── CLAUDE.md
├── Directory.Build.props
├── README.md
└── SubnetCli.slnx
```

### Architecture

**SubnetCli.Core**

Contains the IPv4 subnet model, address calculations, validation, and VLSM allocation logic. It has no dependency on the CLI presentation layer.

**SubnetCli**

Handles command parsing, argument validation, execution flow, and console output.

Console output is separated from command execution. `TextWriter` dependencies allow output and error streams to be tested independently.

**SubnetCli.Tests**

Contains automated xUnit tests covering subnet calculations, VLSM allocation, CLI behavior, validation, and boundary conditions.

**SubnetCli.SmokeTests**

Contains standalone smoke tests for core calculations and basic error handling.

## Usage

Run commands from the solution root directory.

### Subnet Information

```powershell
dotnet run --project SubnetCli -- info 192.168.1.0/24
```

Displays the network address, prefix, subnet mask, total addresses, usable hosts, usable address range, and broadcast address.

### VLSM Planning

```powershell
dotnet run --project SubnetCli -- plan 172.16.0.0/22 400 180 40 18 2 2 2
```

Creates aligned subnet allocations based on the requested host capacities.

The planner processes the largest requests first and preserves the original request order when requirements have equal sizes.

### Address Membership

```powershell
dotnet run --project SubnetCli -- contains 192.168.1.0/24 192.168.1.50
```

Checks whether the specified address belongs to the given subnet.

### Help

```powershell
dotnet run --project SubnetCli -- --help
```

## IPv4 Address Rules

IPv4 addresses must use four decimal octets separated by periods.

Valid examples:

- `192.168.1.1`
- `10.0.0.1`
- `0.0.0.0`
- `255.255.255.255`

Abbreviated IPv4 addresses, IPv6 addresses, and octets containing leading zeros are rejected.

The calculator uses RFC 3021-style usable address handling for `/31` networks and supports single-host `/32` networks. VLSM host requirements use conventional network and broadcast address reservations.

## Build

Build the complete solution:

```powershell
dotnet build SubnetCli.slnx -c Release
```

## Testing

The project uses xUnit for automated testing.

Run all registered test projects:

```powershell
dotnet test SubnetCli.slnx
```

Run the xUnit project independently:

```powershell
dotnet test SubnetCli.Tests/SubnetCli.Tests.csproj
```

Run standalone smoke tests:

```powershell
dotnet run --project SubnetCli.SmokeTests
```

### Test Coverage

The test suite covers:

- CIDR parsing and normalization.
- Strict IPv4 address validation.
- Network address and subnet mask calculations.
- Special IPv4 prefix handling.
- Address membership and boundary conditions.
- VLSM subnet allocation and capacity checks.
- Allocation ordering and non-overlapping subnet ranges.
- Error handling and invalid input.
- CLI output, error streams, and exit codes.
- Regression tests for previously identified issues.

### Generate Code Coverage

Coverage collection uses Coverlet.

```powershell
dotnet test SubnetCli.Tests/SubnetCli.Tests.csproj --collect:"XPlat Code Coverage"
```

Coverage results are generated in:

```text
SubnetCli.Tests/TestResults/
```

### HTML Coverage Report

Install ReportGenerator once:

```powershell
dotnet tool install --global dotnet-reportgenerator-globaltool
```

Generate the HTML report:

```powershell
reportgenerator -reports:"SubnetCli.Tests\TestResults\**\coverage.cobertura.xml" -targetdir:"coverage-report" -reporttypes:"Html"
```

Open:

```text
coverage-report/index.html
```

The report provides line and branch coverage metrics and highlights uncovered code paths.

## Code Quality

Coding conventions and formatting rules are defined in `.editorconfig`.

Additional .NET analyzer settings are configured in `Directory.Build.props`.

### Formatting

```powershell
dotnet format SubnetCli.slnx
```

Verify formatting without modifying files:

```powershell
dotnet format SubnetCli.slnx --verify-no-changes
```

### Static Analysis

```powershell
dotnet build SubnetCli.slnx -c Release
```

The project uses built-in .NET analyzers and configured code-style rules.

Development guidelines are documented in `CLAUDE.md`.

Code comments and documentation must be written in English.

## Publishing

Publish a self-contained Windows x64 executable:

```powershell
dotnet publish SubnetCli/SubnetCli.csproj -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true
```

## Project Scope

Subnet CLI is intended for IPv4 subnet calculation, network planning, and educational use.

The application performs local calculations and does not actively scan networks or contact remote hosts.