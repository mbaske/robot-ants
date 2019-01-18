using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class CamTarget : MonoBehaviour
{
    [SerializeField]
    private bool autoCycle = true;
    [SerializeField]
    private float followDuration = 5f; // if autoCycle
    [SerializeField]
    private float interpolation = 0.1f;

    private Ant[] ants;
    private Ant selected;
    private Coroutine crFollow;
    private float time;

    public void Initialize(Ant[] ants)
    {
        this.ants = ants;
        Follow(ants[0]);
    }

    private void Update()
    {
        if (Input.anyKey)
        {
            if (Input.GetKeyDown(KeyCode.N))
            {
                StopFollowCR();
                SelectNext();
            }
            else if (Input.GetKeyDown(KeyCode.R))
            {
                StopFollowCR();
                Follow(Util.RndItem(ants));
            }
            else if (Input.GetKeyDown(KeyCode.C))
            {
                autoCycle = !autoCycle;
            }
        }

        transform.position = Vector3.Lerp(transform.position, selected.Position, interpolation);
    }

    private void Follow(Ant ant)
    {
        if (ant != selected)
        {
            selected = ant;
            time = Time.time;
        }
        crFollow = StartCoroutine(FollowCR());
    }

    private void SelectNext()
    {
        List<Ant> candidates = new List<Ant>();
        foreach (Ant ant in ants)
        {
            if (ant.Strength > 0.1f)
            {
                candidates.Add(ant);
            }
        }
        if (candidates.Count == 0)
        {
            candidates = new List<Ant>(ants);
        }

        float minDist = Mathf.Infinity;
        Ant closest = selected;
        foreach (Ant ant in candidates)
        {
            float d = Vector3.Distance(transform.position, ant.Position);
            if (ant != selected && d < minDist)
            {
                closest = ant;
                minDist = d;
            }
        }
        Follow(closest);
    }

    private bool NeedToSwitch()
    {
        return Time.time - time > followDuration;
    }

    private void StopFollowCR()
    {
        if (crFollow != null)
        {
            StopCoroutine(crFollow);
        }
    }

    private IEnumerator FollowCR()
    {
        yield return new WaitForSeconds(1f);

        if (autoCycle && NeedToSwitch())
        {
            SelectNext();
        }
        else
        {
            Follow(selected);
        }
    }
}