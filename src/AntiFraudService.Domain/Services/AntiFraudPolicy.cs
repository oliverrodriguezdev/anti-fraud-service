namespace AntiFraudService.Domain.Services;

public class AntiFraudPolicy
{
    public string ValidateTransaction(decimal value, decimal dailyTotal)
    {
        if (value > 2000)
            return "rejected";
        if (dailyTotal + value > 20000)
            return "rejected";
        return "approved";
    }
} 