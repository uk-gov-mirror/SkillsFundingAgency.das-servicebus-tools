namespace SFA.DAS.Learning.Types;

public class LearningPausedEvent
{
    public long ApprenticeshipId { get; set; }

    public DateTime PauseDate { get; set; }

    public Guid LearningKey { get; set; }

    public DateTime Created { get; set; }
}
