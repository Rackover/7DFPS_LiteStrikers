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
        var playersWhoCouldNeedTargets = Game.i.Players.FindAll(o => !o.IsLocal);

        // ADd missing targets
        if(activeTargets.Count < playersWhoCouldNeedTargets.Count)
        {
            Debug.Log($"Adding missing targets!");
            List<Player> playersWithoutTarget = new List<Player>(playersWhoCouldNeedTargets);
            playersWithoutTarget.RemoveAll(o => activeTargets.Find(t => t.Player == o) != null);

            foreach(var player in playersWithoutTarget)
            {
                var target = GetTarget();
                target.Player = player;
                target.gameObject.SetActive(true);

                activeTargets.Add(target);
                Debug.Log($"Adding target " + target + " to active targets");
            }
        }

        // Remove extra targets
        // IF but it should be a while...
        if (activeTargets.Count > playersWhoCouldNeedTargets.Count)
        {
            var targets = activeTargets.ToArray();
            foreach (var target in targets)
            {
                if (target.Player == null)
                {
                    RemoveTarget(target);
                    Debug.Log($"Removing target " + target + " from active targets");
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
            return Instantiate(targetExample, targetExample.transform.parent);
        }
    }

    void RemoveTarget(HUDTarget target)
    {
        if (target == null)
        {
            activeTargets.RemoveAll(o => o == null);
            return;
        }

        activeTargets.Remove(target);
        target.gameObject.SetActive(false);
        pooledTargets.Enqueue(target);
    }
}
