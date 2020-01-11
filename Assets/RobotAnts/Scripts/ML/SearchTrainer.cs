using UnityEngine;

public class SearchTrainer : Searcher
{
    public override void CollectObservations()
    {
        base.CollectObservations();

        // Need to balance between exploring new territory and repowering:
        // Penalize proximity to trails.
        AddReward(trailStrength * -0.1f);
        if (Energy < 0)
        {
            // Penalize low energy.
            AddReward(Energy * 0.25f);
        }
        
        // Penalize proximity to obstacles.
        AddReward(cumlObstacleProximity * -0.25f);

        // Reward walking forward.
        AddReward(0.2f - Mathf.Abs(walker.WalkDirection) * 0.2f);

        if (body.transform.up.y < 0)
        {
            // Fell over.
            AgentReset();
        }
    }
}
