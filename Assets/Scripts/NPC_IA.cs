using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class NPC_IA : MonoBehaviour
{
    private NavMeshAgent npc;
    private float speed = 0.9f;
    public GameObject goal;
    public bool move = false;

    public void SetNavMeshAgent(NavMeshAgent newNpc)
    {
        npc = newNpc;
    }

    public void SetGoal(GameObject newGoal)
    {
        goal = newGoal;
    }

    public void SetSpeed(float newSpeed)
    {
        speed = newSpeed;
        if (npc != null)
        {
            npc.speed = speed;
        }
    }

    public bool CheckArrived()
    {
        float distance = Vector3.Distance(npc.transform.position, goal.transform.position);
        if (distance <= 0.85f)
        {
            Debug.Log("Arrived at goal: " + goal.name);
            move = false;
            return true;
        }
        return false;
    }

    void Awake()
    {
        if (npc == null)
        {
            npc = GetComponent<NavMeshAgent>();
        }
    }

    void Update()
    {
        if (move)
        {
            npc.speed = speed;
            npc.SetDestination(goal.transform.position);
        }
    }
}
