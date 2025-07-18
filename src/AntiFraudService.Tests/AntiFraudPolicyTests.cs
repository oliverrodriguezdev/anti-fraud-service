using Xunit;
using AntiFraudService.Domain.Services;

public class AntiFraudPolicyTests
{
    [Fact]
    public void Rejects_Transaction_Over_Individual_Limit()
    {
        var policy = new AntiFraudPolicy();
        var result = policy.ValidateTransaction(2500, 0);
        Assert.Equal("rejected", result);
    }

    [Fact]
    public void Rejects_Transaction_Over_Daily_Limit()
    {
        var policy = new AntiFraudPolicy();
        var result = policy.ValidateTransaction(1500, 19000);
        Assert.Equal("rejected", result);
    }

    [Fact]
    public void Approves_Valid_Transaction()
    {
        var policy = new AntiFraudPolicy();
        var result = policy.ValidateTransaction(2000, 18000);
        Assert.Equal("approved", result);
    }
} 