using Procyon.Email.Abstractions;

namespace Procyon.Email.Abstractions.Tests;

public class EmailAddressTests
{
    [Fact]
    public void Constructor_NormalizesAddress()
    {
        var address = new EmailAddress("sender@example.com", "Sender");

        Assert.Equal("sender@example.com", address.Address);
        Assert.Equal("Sender", address.Name);
    }

    [Fact]
    public void Constructor_RejectsInvalidAddress()
    {
        Assert.Throws<EmailValidationException>(() => new EmailAddress("not-an-address"));
    }

    [Fact]
    public void ToString_IncludesDisplayNameWhenPresent()
    {
        var address = new EmailAddress("sender@example.com", "Sender");

        Assert.Equal("Sender <sender@example.com>", address.ToString());
    }
}
