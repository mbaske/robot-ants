using Unity.Collections;
using Unity.Mathematics;
using Unity.Jobs;
using Unity.Burst;
using UnityEngine;

public class DetectTrails
{
    [BurstCompile]
    private struct DetectTrailsJob : IJob
    {
        [ReadOnly]
        public NativeArray<Color32> colors;
        [ReadOnly]
        public float scale;
        [ReadOnly]
        public int length;
        [ReadOnly]
        public int radius;
        [ReadOnly]
        public int px;
        [ReadOnly]
        public int py;
        [WriteOnly]
        public NativeArray<float> result;

        public void Execute()
        {
            // The search area.
            int x0 = math.max(0, px - radius);
            int y0 = math.max(0, py - radius);
            int x1 = math.min(length - 1, px + radius);
            int y1 = math.min(length - 1, py + radius);
            int rSqr = radius * radius;

            // Calculate trail centroid and average trail point strength.

            float xAvg = 0;
            float yAvg = 0;
            float sAvg = 0;
            float thresh = 0.001f;
            int i = 0;

            // Scale trail point deltas with relative strength values.
            // -> High differentiation between point strengths,  
            // resulting in a better sense of direction.

            float min = 1f;
            float max = 0f;
            for (int x = x0; x <= x1; x++)
            {
                for (int y = y0; y <= y1; y++)
                {
                    int dx = px - x;
                    int dy = py - y;
                    if (dx * dx + dy * dy < rSqr)
                    {
                        float s = (255 - colors[y * length + x].r) / 255f;
                        if (s > thresh)
                        {
                            min = math.min(min, s);
                            max = math.max(max, s);
                        }
                    }
                }
            }
            if (max > thresh)
            {
                float div = max - min;
                for (int x = x0; x <= x1; x++)
                {
                    for (int y = y0; y <= y1; y++)
                    {
                        int dx = px - x;
                        int dy = py - y;
                        if (dx * dx + dy * dy < rSqr)
                        {
                            float s = (255 - colors[y * length + x].r) / 255f;
                            if (s > thresh)
                            {
                                sAvg += s;
                                s -= min;
                                s /= div;
                                s *= scale;
                                xAvg += dx * s;
                                yAvg += dy * s;
                                i++;
                            }
                        }
                    }
                }
            }

            // Alt: Scale trail point deltas with absolute strength values.
            // -> Low differentiation between point strengths, but faster.

            // for (int x = x0; x <= x1; x++)
            // {
            //     for (int y = y0; y <= y1; y++)
            //     {
            //         int dx = px - x;
            //         int dy = py - y;
            //         if (dx * dx + dy * dy < rSqr)
            //         {
            //             float s = (255 - colors[y * length + x].r) / 255f;
            //             if (s > thresh)
            //             {
            //                 sAvg += s;
            //                 s *= scale;
            //                 xAvg += dx * s;
            //                 yAvg += dy * s;
            //                 i++;
            //             }
            //         }
            //     }
            // }

            float n = i > 0 ? (float)i : 1f;
            result[0] = xAvg / n;
            result[1] = yAvg / n;
            result[2] = sAvg / n;
        }
    }

    private DetectTrailsJob job;
    private JobHandle jobHandle;
    private bool hasJob;

    public void ScheduleJob(Terrain terrain, Searcher agent, float radius, NativeArray<float> result)
    {
        Vector2 uv = agent.UVCoord;
        job = new DetectTrailsJob()
        {
            scale = 1f / terrain.Scale,
            colors = terrain.Colors,
            length = terrain.Length,
            radius = Mathf.RoundToInt(radius * terrain.Scale),
            px = Mathf.RoundToInt(uv.x * terrain.Length),
            py = Mathf.RoundToInt(uv.y * terrain.Length),
            result = result
        };
        jobHandle = job.Schedule();
        hasJob = true;
    }

    public void CompleteJob(Searcher agent)
    {
        if (hasJob)
        {
            jobHandle.Complete();
            agent.OnDetectionResult(job.result);
            hasJob = false;
        }
    }

    public void Dispose(NativeArray<float> result)
    {
        jobHandle.Complete();
        result.Dispose();
    }
}
