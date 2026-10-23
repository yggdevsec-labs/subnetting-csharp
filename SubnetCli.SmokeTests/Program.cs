using SubnetCli.Core;

static void Check(bool condition, string description)
{
    if (!condition)
    {
        throw new InvalidOperationException($"FAILED: {description}");
    }
}

static void CheckThrows<TException>(Action action, string description) where TException : Exception
{
    try
    {
        action();
    }
    catch (TException)
    {
        return;
    }

    throw new InvalidOperationException("Test failed");
}

Ipv4Subnet baseNetwork = Ipv4Subnet.Parse("172.16.0.0/22");
IReadOnlyList<SubnetAllocation> allocations = SubnetPlanner.Plan(
    baseNetwork, [400, 180, 40, 18, 2, 2, 2]);
string[] expected =
[
    "172.16.0.0/23", "172.16.2.0/24", "172.16.3.0/26",
    "172.16.3.64/27", "172.16.3.96/30", "172.16.3.100/30",
    "172.16.3.104/30"
];

Check(allocations.Select(x => x.Subnet.ToString()).SequenceEqual(expected), "Packet Tracer VLSM plan");
Check(baseNetwork.TotalAddresses == 1024, "Parent total addresses");
Check(baseNetwork.UsableHosts == 1022, "Parent usable hosts");
Check(SubnetPlanner.PrefixForHosts(400) == 23, "400 hosts need /23");
Check(SubnetPlanner.PrefixForHosts(2) == 30, "2 hosts need /30");
Check(baseNetwork.Contains(Ipv4Subnet.ParseAddress("172.16.3.255")), "Contains last IP");
Check(!baseNetwork.Contains(Ipv4Subnet.ParseAddress("172.16.4.0")), "Excludes first outside IP");
Check(Ipv4Subnet.Parse("172.16.1.20/23").ToString() == "172.16.0.0/23", "CIDR normalization");
Check(Ipv4Subnet.Parse("10.0.0.0/31").UsableHosts == 2, "RFC 3021 /31");
Check(Ipv4Subnet.Parse("10.0.0.4/32").UsableHosts == 1, "Single host /32");
Check(Ipv4Subnet.Parse("0.0.0.0/0").TotalAddresses == 4294967296UL, "Full IPv4 range /0");
Check(Ipv4Subnet.FormatAddress(uint.MaxValue) == "255.255.255.255", "Format highest IPv4");

CheckThrows<InvalidOperationException>(
    () => SubnetPlanner.Plan(Ipv4Subnet.Parse("10.0.0.0/24"), [200, 50]),
    "Rejects subnet plan too large for parent");
CheckThrows<ArgumentException>(() => SubnetPlanner.PrefixForHosts(0), "Rejects zero hosts");
CheckThrows<ArgumentException>(() => Ipv4Subnet.Parse("192.0.2.1/33"), "Rejects invalid prefix");
CheckThrows<ArgumentException>(() => Ipv4Subnet.ParseAddress("2001:db8::1"), "Rejects IPv6 address");
CheckThrows<ArgumentException>(
    () => SubnetPlanner.Plan(Ipv4Subnet.Parse("10.0.0.0/24"), []),
    "Rejects empty allocation plan");

Console.WriteLine("All C# smoke tests passed.");
