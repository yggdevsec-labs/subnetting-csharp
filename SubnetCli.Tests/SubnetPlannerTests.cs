using SubnetCli.Core;

namespace SubnetCli.Tests;

public sealed class SubnetPlannerTests
{
    [Theory]
    [InlineData(1, 30)]
    [InlineData(2, 30)]
    [InlineData(3, 29)]
    [InlineData(30, 27)]
    [InlineData(100, 25)]
    [InlineData(400, 23)]
    public void PrefixForHostsCalculatesCorrectPrefix(
        int hosts,
        int expectedPrefix)
    {
        int prefix = SubnetPlanner.PrefixForHosts(hosts);

        Assert.Equal(expectedPrefix, prefix);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void PrefixForHostsRejectsInvalidHostCount(int hosts)
    {
        Assert.Throws<ArgumentException>(() => SubnetPlanner.PrefixForHosts(hosts));
    }

    [Fact]
    public void PlanAllocatesLargestSubnetFirst()
    {
        Ipv4Subnet parent = Ipv4Subnet.Parse("172.16.0.0/22");

        IReadOnlyList<SubnetAllocation> allocations = SubnetPlanner.Plan(parent, [40, 400, 180]);

        string[] expected =
        [
            "172.16.0.0/23",
            "172.16.2.0/24",
            "172.16.3.0/26"
        ];

        Assert.Equal(
            expected,
            allocations.Select(x => x.Subnet.ToString()));

        Assert.Equal(
            [2, 3, 1],
            allocations.Select(x => x.RequestNumber));
    }

    [Fact]
    public void PlanRejectsInsufficientAddressSpace()
    {
        Ipv4Subnet parent = Ipv4Subnet.Parse("10.0.0.0/24");

        Assert.Throws<InvalidOperationException>(() => SubnetPlanner.Plan(parent, [200, 50]));
    }

    [Fact]
    public void PlanRejectsEmptyRequests()
    {
        Ipv4Subnet parent = Ipv4Subnet.Parse("10.0.0.0/24");

        Assert.Throws<ArgumentException>(() => SubnetPlanner.Plan(parent, []));
    }

    [Fact]
    public void PlanRejectsNullArguments()
    {
        Ipv4Subnet parent = Ipv4Subnet.Parse("10.0.0.0/24");

        Assert.Throws<ArgumentNullException>(() => SubnetPlanner.Plan(null!, [10]));

        Assert.Throws<ArgumentNullException>(() => SubnetPlanner.Plan(parent, null!));
    }

    [Theory]
    [InlineData("127.1")]
    [InlineData("010.0.0.1")]
    [InlineData("192.168.1")]
    [InlineData("1.2.3.4.5")]
    [InlineData("256.0.0.1")]
    [InlineData("192.168.1.-1")]
    [InlineData("192.168.1.01")]
    [InlineData("192.168.1.1 ")]
    public void ParseAddressRejectsInvalidFormats(string address)
    {
        Assert.Throws<ArgumentException>(
            () => Ipv4Subnet.ParseAddress(address));
    }

    [Theory]
    [InlineData("192.168.1.1")]
    [InlineData("127.0.0.1")]
    [InlineData("0.0.0.0")]
    [InlineData("255.255.255.255")]
    public void ParseAddressAcceptsValidFormats(string address)
    {
        ulong parsed = Ipv4Subnet.ParseAddress(address);

        Assert.Equal(address, Ipv4Subnet.FormatAddress(parsed));
    }
    [Theory]
    [InlineData("192.168.1.0", true)]
    [InlineData("192.168.1.255", true)]
    [InlineData("192.168.0.255", false)]
    [InlineData("192.168.2.0", false)]
    public void ContainsHandlesBoundaryAddresses(
        string address,
        bool expected)
    {
        Ipv4Subnet subnet = Ipv4Subnet.Parse("192.168.1.0/24");

        bool result = subnet.Contains(
            Ipv4Subnet.ParseAddress(address));

        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData(
    "0.0.0.0/0",
    "0.0.0.0",
    "0.0.0.1",
    "255.255.255.254",
    "255.255.255.255",
    4294967294UL)]
    [InlineData(
    "192.168.1.10/31",
    "255.255.255.254",
    "192.168.1.10",
    "192.168.1.11",
    "N/A (/31 or /32)",
    2UL)]
    [InlineData(
    "192.168.1.10/32",
    "255.255.255.255",
    "192.168.1.10",
    "192.168.1.10",
    "N/A (/31 or /32)",
    1UL)]
    public void SpecialPrefixesHaveCorrectProperties(
        string cidr,
        string expectedMask,
        string expectedFirst,
        string expectedLast,
        string expectedBroadcast,
        ulong expectedHosts)
    {
        Ipv4Subnet subnet = Ipv4Subnet.Parse(cidr);

        Assert.Equal(expectedMask, subnet.Mask);
        Assert.Equal(
            expectedFirst,
            Ipv4Subnet.FormatAddress(subnet.FirstUsable));
        Assert.Equal(
            expectedLast,
            Ipv4Subnet.FormatAddress(subnet.LastUsable));
        Assert.Equal(expectedBroadcast, subnet.Broadcast);
        Assert.Equal(expectedHosts, subnet.UsableHosts);
    }

    [Theory]
    [InlineData(2, 30)]
    [InlineData(3, 29)]
    [InlineData(6, 29)]
    [InlineData(7, 28)]
    [InlineData(254, 24)]
    [InlineData(255, 23)]
    [InlineData(int.MaxValue, 0)]
    public void PrefixForHostsHandlesCapacityBoundaries(
        int hosts,
        int expectedPrefix)
    {
        int prefix = SubnetPlanner.PrefixForHosts(hosts);

        Assert.Equal(expectedPrefix, prefix);
    }

    [Fact]
    public void PlanAllowsExactCapacity()
    {
        Ipv4Subnet parent = Ipv4Subnet.Parse("192.168.1.0/24");

        IReadOnlyList<SubnetAllocation> allocations =
            SubnetPlanner.Plan(parent, [126, 126]);

        Assert.Equal(2, allocations.Count);
        Assert.Equal("192.168.1.0/25", allocations[0].Subnet.ToString());
        Assert.Equal("192.168.1.128/25", allocations[1].Subnet.ToString());
    }

    [Fact]
    public void PlanRejectsRequestsExceedingExactCapacity()
    {
        Ipv4Subnet parent = Ipv4Subnet.Parse("192.168.1.0/24");

        Assert.Throws<InvalidOperationException>(() => SubnetPlanner.Plan(parent, [126, 126, 1]));
    }

    [Fact]
    public void PlanHandlesHighestIpv4AddressRange()
    {
        Ipv4Subnet parent = Ipv4Subnet.Parse("255.255.255.252/30");

        IReadOnlyList<SubnetAllocation> allocations =
            SubnetPlanner.Plan(parent, [2]);

        SubnetAllocation allocation = Assert.Single(allocations);

        Assert.Equal("255.255.255.252/30", allocation.Subnet.ToString());
        Assert.Equal("255.255.255.255", allocation.Subnet.Broadcast);
        Assert.Equal(2UL, allocation.Subnet.UsableHosts);
        Assert.Equal(2, allocation.RequestedHosts);
    }

    [Fact]
    public void PlanPreservesRequestOrderForEqualSizes()
    {
        Ipv4Subnet parent = Ipv4Subnet.Parse("192.168.10.0/24");

        IReadOnlyList<SubnetAllocation> allocations =
            SubnetPlanner.Plan(parent, [10, 10, 25, 10]);

        Assert.Equal(4, allocations.Count);

        Assert.Equal(
            [3, 1, 2, 4],
            allocations.Select(x => x.RequestNumber));

        Assert.Equal(
            [25, 10, 10, 10],
            allocations.Select(x => x.RequestedHosts));

        Assert.Equal(
            [
                "192.168.10.0/27",
                "192.168.10.32/28",
                "192.168.10.48/28",
                "192.168.10.64/28"
            ],
            allocations.Select(x => x.Subnet.ToString()));
    }

    [Theory]
    [InlineData("192.168.1.0/24", new int[] { 50, 20, 10 })]
    [InlineData("172.16.0.0/22", new int[] { 40, 400, 180 })]
    [InlineData("10.0.0.0/24", new int[] { 126, 126 })]
    [InlineData("255.255.255.252/30", new int[] { 2 })]
    [InlineData("0.0.0.0/0", new int[] { int.MaxValue })]
    public void PlanAllocationsRespectInvariants(
        string parentCidr,
        int[] requestedHosts)
    {
        Ipv4Subnet parent = Ipv4Subnet.Parse(parentCidr);

        IReadOnlyList<SubnetAllocation> allocations =
            SubnetPlanner.Plan(parent, requestedHosts);

        Assert.Equal(requestedHosts.Length, allocations.Count);

        foreach (SubnetAllocation allocation in allocations)
        {
            Ipv4Subnet subnet = allocation.Subnet;

            // The allocation must remain inside the parent network.
            Assert.True(subnet.NetworkAddress >= parent.NetworkAddress);
            Assert.True(subnet.LastAddress <= parent.LastAddress);

            // The allocation must satisfy its original host request.
            Assert.InRange(allocation.RequestNumber, 1, requestedHosts.Length);
            Assert.Equal(
                requestedHosts[allocation.RequestNumber - 1],
                allocation.RequestedHosts);

            Assert.True(
                subnet.UsableHosts >= (ulong)allocation.RequestedHosts);
        }

        SubnetAllocation[] sorted = [.. allocations.OrderBy(x => x.Subnet.NetworkAddress)];

        // No two allocated subnets may overlap.
        for (int index = 1; index < sorted.Length; index++)
        {
            Assert.True(
                sorted[index - 1].Subnet.LastAddress <
                sorted[index].Subnet.NetworkAddress,
                "Subnet allocations must not overlap.");
        }
    }

}
