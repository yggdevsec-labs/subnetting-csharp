using System.Buffers.Binary;
using System.Globalization;
using System.Net;

namespace SubnetCli.Core;

/// <summary>Represents an IPv4 CIDR block as unsigned integer bounds.</summary>
public sealed class Ipv4Subnet
{
    private const ulong MaxIpv4 = uint.MaxValue;

    public ulong NetworkAddress { get; }
    public int Prefix { get; }
    public ulong TotalAddresses => 1UL << (32 - Prefix);
    public ulong LastAddress => NetworkAddress + TotalAddresses - 1;
    public ulong UsableHosts => Prefix switch
    {
        32 => 1,
        31 => 2,
        _ => TotalAddresses - 2
    };
    public ulong FirstUsable => Prefix >= 31 ? NetworkAddress : NetworkAddress + 1;
    public ulong LastUsable => Prefix >= 31 ? LastAddress : LastAddress - 1;
    public string Mask => FormatAddress(MaskForPrefix(Prefix));
    public string Broadcast => Prefix <= 30 ? FormatAddress(LastAddress) : "N/A (/31 or /32)";

    private Ipv4Subnet(ulong networkAddress, int prefix)
    {
        NetworkAddress = networkAddress;
        Prefix = prefix;
    }

    public static Ipv4Subnet Parse(string cidr)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(cidr);

        string[] parts = cidr.Split('/', StringSplitOptions.TrimEntries);
        if (parts.Length != 2 || !int.TryParse(parts[1], out int prefix))
        {
            throw new ArgumentException("Expected IPv4 CIDR, for example 172.16.0.0/22.");
        }

        return FromAddress(ParseAddress(parts[0]), prefix);
    }

    public static Ipv4Subnet FromAddress(ulong address, int prefix)
    {
        if (address > MaxIpv4 || prefix is < 0 or > 32)
        {
            throw new ArgumentException("IPv4 address or prefix is out of range.");
        }

        return new Ipv4Subnet(address & MaskForPrefix(prefix), prefix);
    }

    public static ulong ParseAddress(string text)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(text);

        string[] octets = text.Split('.');

        if (octets.Length != 4)
        {
            throw new ArgumentException($"Invalid IPv4 address: {text}");
        }

        ulong address = 0;

        foreach (string octet in octets)
        {
            if (octet.Length is < 1 or > 3 || (octet.Length > 1 && octet[0] == '0') ||
                octet.Any(static c => c is < '0' or > '9') ||
                !byte.TryParse(octet, NumberStyles.None, CultureInfo.InvariantCulture, out byte value))
            {
                throw new ArgumentException($"Invalid IPv4 address: {text}");
            }

            address = (address << 8) | value;
        }

        return address;
    }

    public static string FormatAddress(ulong address)
    {
        ArgumentOutOfRangeException.ThrowIfGreaterThan(address, MaxIpv4);

        byte[] bytes = new byte[4];
        BinaryPrimitives.WriteUInt32BigEndian(bytes, (uint)address);
        return new IPAddress(bytes).ToString();
    }

    public bool Contains(ulong address)
    {
        return address >= NetworkAddress && address <= LastAddress;
    }

    public override string ToString()
    {
        return $"{FormatAddress(NetworkAddress)}/{Prefix}";
    }

    private static ulong MaskForPrefix(int prefix)
    {
        return prefix == 0
        ? 0UL
        : (MaxIpv4 << (32 - prefix)) & MaxIpv4;
    }
}

