namespace TechGearAuction.Application.Common.Exceptions;

public class BannedUserException : Exception
{
    public string BanReason { get; }
    public int ViolationCount { get; }
    public bool CanAppeal { get; }

    public BannedUserException(string banReason, int violationCount, bool canAppeal = true) 
        : base("Your account has been banned.")
    {
        BanReason = banReason;
        ViolationCount = violationCount;
        CanAppeal = canAppeal;
    }
}

