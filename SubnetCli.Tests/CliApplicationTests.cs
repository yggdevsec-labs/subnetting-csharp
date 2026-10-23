
using System.IO;
using SubnetCli.Cli;

namespace SubnetCli.Tests;

public sealed class CliApplicationTests
{
    [Fact]
    public void RunInfoReturnsSuccess()
    {
        (int ExitCode, string? Output, string? Error) = RunCli("info", "192.168.1.0/24");

        Assert.Equal(0, ExitCode);
        Assert.Contains("192.168.1.0", Output);
        Assert.Contains("255.255.255.0", Output);
        Assert.Contains("Usable hosts", Output);
        Assert.Empty(Error);
    }

    [Fact]
    public void RunPlanReturnsSuccess()
    {
        (int ExitCode, string? Output, string? Error) = RunCli("plan", "192.168.1.0/24", "50", "20");

        Assert.Equal(0, ExitCode);
        Assert.Contains("192.168.1.0/26", Output);
        Assert.Contains("192.168.1.64/27", Output);
        Assert.Empty(Error);
    }

    [Fact]
    public void RunContainsReturnsSuccess()
    {
        (int ExitCode, string? Output, string? Error) = RunCli(
            "contains", "192.168.1.0/24", "192.168.1.50");

        Assert.Equal(0, ExitCode);
        Assert.Equal(
            "192.168.1.50 is in 192.168.1.0/24",
            Output.Trim());
        Assert.Empty(Error);
    }

    [Fact]
    public void RunInvalidCommandReturnsFailure()
    {
        (int ExitCode, string? Output, string? Error) = RunCli("invalid");

        Assert.Equal(1, ExitCode);
        Assert.Contains("Commands:", Output);
        Assert.Contains("Invalid command", Error);
    }

    [Fact]
    public void RunMissingArgumentsReturnsFailure()
    {
        (int ExitCode, string? Output, string? Error) = RunCli("info");

        Assert.Equal(1, ExitCode);
        Assert.Contains("Commands:", Output);
        Assert.Contains("Invalid command", Error);
    }

    [Fact]
    public void RunHelpReturnsSuccess()
    {
        (int ExitCode, string? Output, string? Error) = RunCli("--help");

        Assert.Equal(0, ExitCode);
        Assert.Contains("Commands:", Output);
        Assert.Empty(Error);
    }

    [Theory]
    [InlineData("unknown")]
    [InlineData("info")]
    public void InvalidCommandPrintsUsageOnce(string command)
    {
        (int ExitCode, string? Output, string? Error) = RunCli(command);

        Assert.Equal(1, ExitCode);
        Assert.Equal(
            1,
            Output.Split(
                "Commands:", StringSplitOptions.None).Length - 1);
        Assert.Contains("Invalid command", Error);
    }

    [Theory]
    [InlineData("abc")]
    [InlineData("0")]
    [InlineData("-1")]
    public void RunPlanRejectsInvalidHostCount(string hosts)
    {
        (int ExitCode, string? Output, string? Error) = RunCli("plan", "192.168.1.0/24", hosts);

        Assert.Equal(1, ExitCode);
        Assert.Empty(Output);
        Assert.StartsWith("Error:", Error);
    }

    private static (int ExitCode, string Output, string Error) RunCli(
        params string[] arguments)
    {
        using StringWriter output = new();
        using StringWriter error = new();

        int exitCode = CliApplication.Run(arguments, output, error);

        return (exitCode, output.ToString(), error.ToString());
    }
}
