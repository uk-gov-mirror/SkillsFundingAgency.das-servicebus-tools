namespace SFA.DAS.CommitmentsV2.ExternalHandlers.LearningEvents;

public class LearningResumedEvent
{
    public Guid LearningKey { get; set; }
    public long ApprenticeshipId { get; set; }
    public DateTime Created { get; set; }
    public DateTime ResumeDate { get; set; }
}
