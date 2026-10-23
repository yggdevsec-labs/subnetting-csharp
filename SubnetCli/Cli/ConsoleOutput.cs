
using System.Globalization;
using System.IO;
using SubnetCli.Core;

namespace SubnetCli.Cli;

internal sealed class ConsoleOutput(TextWriter output, TextWriter error)
{
    public void PrintInfo(Ipv4Subnet subnet)
    {
        (string Label, string Value)[] fields =
        [
            ("Network", Ipv4Subnet.FormatAddress(subnet.NetworkAddress)),
            ("CIDR", $"/{subnet.Prefix}"),
            ("Subnet mask", subnet.Mask),
            ("Total addresses", subnet.TotalAddresses.ToString(CultureInfo.InvariantCulture)),
            ("Usable hosts", subnet.UsableHosts.ToString(CultureInfo.InvariantCulture)),
            ("First usable IP", Ipv4Subnet.FormatAddress(subnet.FirstUsable)),
            ("Last usable IP", Ipv4Subnet.FormatAddress(subnet.LastUsable)),
            ("Broadcast", subnet.Broadcast)
        ];

        foreach ((string? Label, string? Value) in fields)
        {
            output.WriteLine($"{Label,-17} {Value}");
        }
    }

    public void PrintPlan(Ipv4Subnet parent, IReadOnlyList<SubnetAllocation> allocations)
    {
        output.WriteLine($"Base network: {parent}\n");
        output.WriteLine($"{"#",2} {"Hosts",6} {"Network",-18} {"Mask",-18} " +
            $"{"First usable",-16} {"Last usable",-16} Broadcast");
        output.WriteLine(new string('-', 105));

        foreach (SubnetAllocation allocation in allocations)
        {
            Ipv4Subnet subnet = allocation.Subnet;

            output.WriteLine(
                $"{allocation.RequestNumber,2} {allocation.RequestedHosts,6} " +
                $"{subnet,-18} {subnet.Mask,-18} " +
                $"{Ipv4Subnet.FormatAddress(subnet.FirstUsable),-16} " +
                $"{Ipv4Subnet.FormatAddress(subnet.LastUsable),-16} {subnet.Broadcast}");
        }
    }

    public void PrintUsage()
    {
        output.WriteLine("IPv4 subnet calculator and VLSM planner\n");
        output.WriteLine("Commands:");
        output.WriteLine("  subnet info <CIDR>");
        output.WriteLine("  subnet plan <CIDR> <hosts1> <hosts2> ...");
        output.WriteLine("  subnet contains <CIDR> <IPv4>");
        output.WriteLine("\nExamples:");
        output.WriteLine("  subnet info 172.16.0.0/22");
        output.WriteLine("  subnet plan 172.16.0.0/22 400 180 40 18 2 2 2");
        output.WriteLine("  subnet contains 172.16.0.0/23 172.16.1.254");
    }

    public void PrintInvalidCommand()
    {
        error.WriteLine("Invalid command or argument count.\n");
        PrintUsage();
    }

    public void PrintError(string message)
    {
        error.WriteLine($"Error: {message}");
    }

    public void PrintContains(Ipv4Subnet subnet, ulong ip)
    {
        output.WriteLine(
            $"{Ipv4Subnet.FormatAddress(ip)} " +
            $"{(subnet.Contains(ip) ? "is" : "is NOT")} in {subnet}");
    }
}
