
using System.IO;
using SubnetCli.Core;

namespace SubnetCli.Cli;

internal static class CliApplication
{
    private const int InfoArgumentCount = 2;
    private const int ContainsArgumentCount = 3;
    private const int MinPlanArgumentCount = 3;

    public static int Run(string[] arguments)
    {
        return Run(arguments, Console.Out, Console.Error);
    }

    public static int Run(string[] arguments, TextWriter output, TextWriter error)
    {
        ArgumentNullException.ThrowIfNull(arguments);
        ArgumentNullException.ThrowIfNull(output);
        ArgumentNullException.ThrowIfNull(error);

        ConsoleOutput consoleOutput = new(output, error);

        if (arguments.Length == 0 || arguments[0] is "help" or "--help" or "-h")
        {
            consoleOutput.PrintUsage();
            return 0;
        }

        try
        {
            if (!Enum.TryParse(arguments[0], true, out CliCommand command) ||
                !string.Equals(command.ToString(), arguments[0], StringComparison.OrdinalIgnoreCase))
            {
                consoleOutput.PrintInvalidCommand();
                return 1;
            }

            switch (command)
            {
                case CliCommand.Info when arguments.Length == InfoArgumentCount:
                    consoleOutput.PrintInfo(Ipv4Subnet.Parse(arguments[1]));
                    return 0;

                case CliCommand.Plan when arguments.Length >= MinPlanArgumentCount:
                    Ipv4Subnet parent = Ipv4Subnet.Parse(arguments[1]);
                    int[] hosts = [.. arguments[2..].Select(ParseHostCount)];

                    consoleOutput.PrintPlan(parent, SubnetPlanner.Plan(parent, hosts));
                    return 0;

                case CliCommand.Contains when arguments.Length == ContainsArgumentCount:
                    Ipv4Subnet subnet = Ipv4Subnet.Parse(arguments[1]);
                    ulong ip = Ipv4Subnet.ParseAddress(arguments[2]);

                    consoleOutput.PrintContains(subnet, ip);
                    return 0;

                default:
                    consoleOutput.PrintInvalidCommand();
                    return 1;
            }
        }
        catch (Exception exception) when (exception is ArgumentException or InvalidOperationException)
        {
            consoleOutput.PrintError(exception.Message);
            return 1;
        }
    }

    private static int ParseHostCount(string value)
    {
        if (!int.TryParse(value, out int count))
        {
            throw new ArgumentException($"Invalid host count: {value}");
        }

        return count;
    }
}
