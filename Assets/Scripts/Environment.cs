using NUnit.Framework;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class Environment : MonoBehaviour
{
    protected List<Agent> Agents;
    private List<Agent> ActiveAgents;

    private List<ItemPickup> Coins;
    private List<ItemPickup> Bombs;
    private List<Explosion> Explosions;
    private int[,] Area;

    public void New_round()
    {
        Bombs = new List<ItemPickup>();
        Explosions = new List<Explosion>();
        //...
    }
    public virtual (int[,], List<ItemPickup>, List<Agent>) BuildArea()
    {
        throw new NotImplementedException();
    }
}

class BombRLeWorld : Environment
{
    public BombRLeWorld(List<Agent> Agents)
    {
        
    }
    private void Setup_agents(List<Agent> Agents)
    {
        foreach (Agent agent in Agents){
            this.Agents.Add(agent);
        }
    }
    public override (int[,], List<ItemPickup>, List<Agent>) BuildArea()
    {
        int INDESTRUCTIBLE = -1;
        int FREE = 0;
        int DESTRUCTABLE = 1;

        int ROW = 10, COL = 18;
        int[,] Area = new int[ROW, COL];

        return base.BuildArea();
    }
}
