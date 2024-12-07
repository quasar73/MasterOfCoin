namespace Lib.MessageBroker.Contracts
{
    public enum AckType
    {
        AckOnlyOnSuccess,
        AckOnFailure,
        RequeueOnFailure,
        RepublishOnFailure
    }
}
