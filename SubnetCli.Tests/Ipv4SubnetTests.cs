using SubnetCli.Cli;
using SubnetCli.Core;
using Xunit;

namespace SubnetCli.Tests;

public sealed class Ipv4SubnetTests
{
    [Theory]
    [InlineData("192.168.1.100/24", "192.168.1.0/24")]
    [InlineData("172.16.1.20/23", "172.16.0.0/23")]
    [InlineData("10.0.0.5/32", "10.0.0.5/32")]
    public void ParseNormalizesNetworkAddress(
        string input,
        string expected)
    {
        Ipv4Subnet subnet = Ipv4Subnet.Parse(input);

        Assert.Equal(expected, subnet.ToString());
    }

    [Fact]
    public void ParseCalculatesSubnetProperties()
    {
        Ipv4Subnet subnet = Ipv4Subnet.Parse("192.168.1.0/24");

        Assert.Equal(24, subnet.Prefix);
        Assert.Equal("255.255.255.0", subnet.Mask);
        Assert.Equal("192.168.1.255", subnet.Broadcast);
        Assert.Equal(256UL, subnet.TotalAddresses);
        Assert.Equal(254UL, subnet.UsableHosts);
    }

    [Theory]
    [InlineData("192.168.1.0/31", 2UL)]
    [InlineData("192.168.1.1/32", 1UL)]
    public void ParseHandlesSpecialPrefixes(
        string cidr,
        ulong expectedHosts)
    {
        Ipv4Subnet subnet = Ipv4Subnet.Parse(cidr);

        Assert.Equal(expectedHosts, subnet.UsableHosts);
    }

    [Fact]
    public void ContainsReturnsTrueForAddressInsideSubnet()
    {
        Ipv4Subnet subnet = Ipv4Subnet.Parse("192.168.1.0/24");
        ulong address = Ipv4Subnet.ParseAddress("192.168.1.50");

        Assert.True(subnet.Contains(address));
    }

    [Fact]
    public void ContainsReturnsFalseForAddressOutsideSubnet()
    {
        Ipv4Subnet subnet = Ipv4Subnet.Parse("192.168.1.0/24");
        ulong address = Ipv4Subnet.ParseAddress("192.168.2.1");

        Assert.False(subnet.Contains(address));
    }

    [Theory]
    [InlineData("192.168.1.1/33")]
    [InlineData("192.168.1.1/-1")]
    [InlineData("invalid")]
    public void ParseRejectsInvalidCidr(string cidr)
    {
        Assert.Throws<ArgumentException>(() => Ipv4Subnet.Parse(cidr));
    }

    [Fact]
    public void FormatAddressRejectsOutOfRangeAddress()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => Ipv4Subnet.FormatAddress(uint.MaxValue + 1UL));
    }

    [Theory]
    [InlineData("invalid/24")]
    [InlineData("2001:db8::1/64")]
    [InlineData("192.168.1.1")]
    [InlineData("192.168.1.1/24/25")]
    [InlineData("192.168.1.1/33")]
    public void ParseRejectsMalformedCidr(string cidr)
    {
        Assert.Throws<ArgumentException>(
            () => Ipv4Subnet.Parse(cidr));
    }

    [Theory]
    [InlineData("abc")]
    [InlineData("0")]
    [InlineData("-1")]
    public void RunPlanRejectsInvalidHostCount(string hosts)
    {
        int exitCode = CliApplication.Run(
            ["plan", "192.168.1.0/24", hosts]);

        Assert.Equal(1, exitCode);
    }

    [Theory]
    [InlineData("1.2.3.4\0")]
    [InlineData("1\0.2.3.4")]
    [InlineData("1.2.3.12\0")]
    public void ParseAddressRejectsNullCharacters(string address)
    {
        Assert.Throws<ArgumentException>(
            () => Ipv4Subnet.ParseAddress(address));
    }

    [Fact]
    public void ParseRejectsNullCharactersInCidr()
    {
        Assert.Throws<ArgumentException>(
            () => Ipv4Subnet.Parse("1.2.3.4\0/24"));
    }
}
