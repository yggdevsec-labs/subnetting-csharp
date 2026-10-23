
using System.IO;
using SubnetCli.Cli;

namespace SubnetCli.Tests;

public sealed class CliApplicationTests
{
    [Fact]
    public void RunInfoReturnsSuccess()
    {
        (int exitCode, string output, string error) =
            RunCli("info", "192.168.1.0/24");

        Assert.Equal(0, exitCode);
        Assert.Contains("192.168.1.0", output);
        Assert.Contains("255.255.255.0", output);
        Assert.Contains("Usable hosts", output);
        Assert.Empty(error);
    }

    [Fact]
    public void RunPlanReturnsSuccess()
    {
        (int exitCode, string output, string error) =
            RunCli("plan", "192.168.1.0/24", "50", "20");

        Assert.Equal(0, exitCode);
        Assert.Contains("192.168.1.0/26", output);
        Assert.Contains("192.168.1.64/27", output);
        Assert.Empty(error);
    }

    [Fact]
    public void RunContainsReturnsSuccess()
    {
        (int exitCode, string output, string error) =
            RunCli("contains", "192.168.1.0/24", "192.168.1.50");

        Assert.Equal(0, exitCode);
        Assert.Equal(
            "192.168.1.50 is in 192.168.1.0/24",
            output.Trim());
        Assert.Empty(error);
    }

    [Fact]
    public void RunContainsReturnsFalseForAddressOutsideSubnet()
    {
        (int exitCode, string output, string error) =
            RunCli("contains", "192.168.1.0/24", "192.168.2.1");

        Assert.Equal(0, exitCode);
        Assert.Equal(
            "192.168.2.1 is NOT in 192.168.1.0/24",
            output.Trim());
        Assert.Empty(error);
    }

    [Fact]
    public void RunHelpReturnsSuccess()
    {
        (int exitCode, string output, string error) =
            RunCli("--help");

        Assert.Equal(0, exitCode);
        Assert.Contains("Commands:", output);
        Assert.Empty(error);
    }

    [Fact]
    public void RunInvalidCommandReturnsFailure()
    {
        (int exitCode, string output, string error) =
            RunCli("invalid");

        Assert.Equal(1, exitCode);
        Assert.Contains("Commands:", output);
        Assert.Contains("Invalid command", error);
    }

    [Fact]
    public void RunMissingArgumentsReturnsFailure()
    {
        (int exitCode, string output, string error) =
            RunCli("info");

        Assert.Equal(1, exitCode);
        Assert.Contains("Commands:", output);
        Assert.Contains("Invalid command", error);
    }

    [Theory]
    [InlineData("unknown")]
    [InlineData("info")]
    public void InvalidCommandPrintsUsageOnce(string command)
    {
        (int exitCode, string output, string error) =
            RunCli(command);

        Assert.Equal(1, exitCode);
        Assert.Equal(
            1,
            output.Split(
                "Commands:", StringSplitOptions.None).Length - 1);
        Assert.Contains("Invalid command", error);
    }

    [Theory]
    [InlineData("abc")]
    [InlineData("0")]
    [InlineData("-1")]
    public void RunPlanRejectsInvalidHostCount(string hosts)
    {
        (int exitCode, string output, string error) =
            RunCli("plan", "192.168.1.0/24", hosts);

        Assert.Equal(1, exitCode);
        Assert.Empty(output);
        Assert.StartsWith("Error:", error);
    }

    [Fact]
    public void RunInfoRejectsInvalidCidr()
    {
        (int exitCode, string output, string error) =
            RunCli("info", "192.168.1.1/33");

        Assert.Equal(1, exitCode);
        Assert.Empty(output);
        Assert.StartsWith("Error:", error);
    }

    [Fact]
    public void RunContainsRejectsInvalidAddress()
    {
        (int exitCode, string output, string error) =
            RunCli("contains", "192.168.1.0/24", "invalid");

        Assert.Equal(1, exitCode);
        Assert.Empty(output);
        Assert.StartsWith("Error:", error);
    }

    [Fact]
    public void RunPlanRejectsInsufficientAddressSpace()
    {
        (int exitCode, string output, string error) =
            RunCli("plan", "10.0.0.0/24", "200", "50");

        Assert.Equal(1, exitCode);
        Assert.Empty(output);
        Assert.StartsWith("Error:", error);
    }

    [Fact]
    public void RunRejectsNumericCommand()
    {
        (int exitCode, string output, string error) =
            RunCli("0");

        Assert.Equal(1, exitCode);
        Assert.Contains("Commands:", output);
        Assert.Contains("Invalid command", error);
    }

    [Fact]
    public void RunAcceptsUppercaseCommand()
    {
        (int exitCode, string output, string error) =
            RunCli("INFO", "192.168.1.0/24");

        Assert.Equal(0, exitCode);
        Assert.Contains("192.168.1.0", output);
        Assert.Empty(error);
    }

    [Fact]
    public void RunInfoRejectsExtraArguments()
    {
        (int exitCode, string output, string error) =
            RunCli("info", "192.168.1.0/24", "extra");

        Assert.Equal(1, exitCode);
        Assert.Contains("Commands:", output);
        Assert.Contains("Invalid command", error);
    }

    [Fact]
    public void RunContainsRejectsExtraArguments()
    {
        (int exitCode, string output, string error) =
            RunCli(
                "contains",
                "192.168.1.0/24",
                "192.168.1.50",
                "extra");

        Assert.Equal(1, exitCode);
        Assert.Contains("Commands:", output);
        Assert.Contains("Invalid command", error);
    }

    [Fact]
    public void RunPlanRejectsMissingHostRequests()
    {
        (int exitCode, string output, string error) =
            RunCli("plan", "192.168.1.0/24");

        Assert.Equal(1, exitCode);
        Assert.Contains("Commands:", output);
        Assert.Contains("Invalid command", error);
    }

    [Fact]
    public void RunContainsRejectsMissingAddress()
    {
        (int exitCode, string output, string error) =
            RunCli("contains", "192.168.1.0/24");

        Assert.Equal(1, exitCode);
        Assert.Contains("Commands:", output);
        Assert.Contains("Invalid command", error);
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
