namespace SubnetCli.Core;

public sealed record SubnetAllocation(int RequestNumber, int RequestedHosts, Ipv4Subnet Subnet);

/// <summary>Allocate aligned CIDR blocks, largest host requirement first.</summary>
public static class SubnetPlanner
{
    public static int PrefixForHosts(int hosts)
    {
        if (hosts < 1)
        {
            throw new ArgumentException("Host count must be at least 1.");
        }

        ulong required = (ulong)hosts + 2;
        for (int hostBits = 2; hostBits < 32; hostBits++)
        {
            if ((1UL << hostBits) >= required)
            {
                return 32 - hostBits;
            }
        }

        return 0;
    }

    public static IReadOnlyList<SubnetAllocation> Plan(Ipv4Subnet parent, IEnumerable<int> requestedHosts)
    {
        ArgumentNullException.ThrowIfNull(parent);
        ArgumentNullException.ThrowIfNull(requestedHosts);

        var requests = requestedHosts
            .Select((hosts, index) => new { Hosts = hosts, Number = index + 1 })
            .OrderByDescending(item => item.Hosts)
            .ThenBy(item => item.Number)
            .ToArray();

        if (requests.Length == 0)
        {
            throw new ArgumentException("Supply at least one host count.");
        }

        List<SubnetAllocation> allocations = [];
        ulong cursor = parent.NetworkAddress;
        ulong limit = parent.LastAddress + 1;

        foreach (var request in requests)
        {
            int prefix = PrefixForHosts(request.Hosts);
            ulong blockSize = 1UL << (32 - prefix);
            ulong alignedStart = ((cursor + blockSize - 1) / blockSize) * blockSize;

            if (alignedStart + blockSize > limit)
            {
                throw new InvalidOperationException(
                    $"Not enough space in {parent}: request #{request.Number} " +
                    $"({request.Hosts} hosts) needs a /{prefix} subnet.");
            }

            Ipv4Subnet child = Ipv4Subnet.FromAddress(alignedStart, prefix);
            allocations.Add(new SubnetAllocation(request.Number, request.Hosts, child));
            cursor = alignedStart + blockSize;
        }

        return allocations;
    }
}
