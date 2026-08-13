namespace SFA.DAS.Learning.Types;

public class LearningResumedEvent
{
    public Guid LearningKey { get; set; }
    public long ApprenticeshipId { get; set; }
    public DateTime Created { get; set; }
    public DateTime ResumeDate { get; set; }
}
