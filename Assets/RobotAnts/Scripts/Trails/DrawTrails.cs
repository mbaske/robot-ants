using Unity.Collections;
using Unity.Mathematics;
using Unity.Jobs;
using Unity.Burst;
using UnityEngine;

public class DrawTrails
{
    [BurstCompile]
    private struct DrawTrailsJob : IJob
    {
        [ReadOnly]
        public int length;
        public NativeArray<Color32> colors;
        [ReadOnly]
        public NativeList<TrailPoint> trailPoints;

        public void Execute()
        {
            for (int i = 0, n = trailPoints.Length; i < n; i++)
            {
                TrailPoint p = trailPoints[i];
                ushort x = (ushort)math.floor(length * p.uv.x);
                ushort y = (ushort)math.floor(length * p.uv.y);
                // Inv. channel -> smaller value represents higher point strength.
                ushort a = colors[y * length + x].r;
                ushort b = (ushort)(255 - math.round(255 * p.strength));
                // Keep a if it's clearly stronger than b.
                // TODO threshold (64) should be derived from life time.
                byte col = (byte)(a + 64 < b ? a : b);
                colors[y * length + x] = new Color32(col, col, col, 255);
            }
        }
    }

    private JobHandle jobHandle;

    public void ScheduleJob(Terrain terrain, NativeList<TrailPoint> trailPoints)
    {
        DrawTrailsJob job = new DrawTrailsJob()
        {
            length = terrain.Length,
            colors = terrain.Colors,
            trailPoints = trailPoints,
        };
        jobHandle = job.Schedule();
    }

    public void CompleteJob(Terrain terrain)
    {
        jobHandle.Complete();
        terrain.UpdateTexture();
    }

    public void Dispose(NativeList<TrailPoint> trailPoints)
    {
        jobHandle.Complete();
        trailPoints.Dispose();
    }
}
