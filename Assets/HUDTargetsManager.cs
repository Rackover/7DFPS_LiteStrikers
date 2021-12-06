using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HUDTargetsManager : MonoBehaviour
{
    [SerializeField] private HUDTarget targetExample;


    List<HUDTarget> activeTargets = new List<HUDTarget>();
    Queue<HUDTarget> pooledTargets = new Queue<HUDTarget>();


    // Update is called once per frame
    void Update()
    {
        // ADd missing targets
        while(activeTargets.Count < Game.i.Players.Count-1)
        {
            List<Player> playersWithoutTarget = new List<Player>(Game.i.Players);
            playersWithoutTarget.RemoveAll(o => o.IsLocal || activeTargets.Find(t => t.Player == o) != null);

            foreach(var player in playersWithoutTarget)
            {
                var target = GetTarget();
                target.Player = player;
                target.gameObject.SetActive(true);

                activeTargets.Add(target);
            }
        }

        // Remove extra targets
        while(activeTargets.Count != 0 && activeTargets.Count > Game.i.Players.Count-1)
        {
            List<HUDTarget> targets = new List<HUDTarget>(activeTargets);
            foreach (var target in targets)
            {
                if (target.Player == null)
                {
                    RemoveTarget(target);
                }
            }
        }
    }

    HUDTarget GetTarget()
    {
        if (pooledTargets.Count > 0)
        {
            return pooledTargets.Dequeue();
        }
        else
        {
            return Instantiate(targetExample, transform);
        }
    }

    void RemoveTarget(HUDTarget target)
    {
        activeTargets.Remove(target);
        target.gameObject.SetActive(false);
        pooledTargets.Enqueue(target);
    }
}
