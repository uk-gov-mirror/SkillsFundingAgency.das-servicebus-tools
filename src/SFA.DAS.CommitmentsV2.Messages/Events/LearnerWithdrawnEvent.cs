namespace SFA.DAS.CommitmentsV2.ExternalHandlers.EventHandlers;

// Matches the temporary inline event in das-commitments until Learning publishes the final contract.
public class LearnerWithdrawnEvent
{
    public Guid LearningKey { get; set; }

    public long ApprenticeshipId { get; set; }

    public DateTime Created { get; set; }

    public DateTime WithdrawnDate { get; set; }

    public int WithdrawnReasonCode { get; set; }
}
