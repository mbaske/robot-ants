using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MLAgents;
using UnityEngine.UI;

public class SoccerAcademy : Academy
{

 
    public Brain brainStriker;
    public Brain brainGoalie;
    public Material redMaterial;
    public Material blueMaterial;
    public float spawnAreaMarginMultiplier;
    public float gravityMultiplier = 1;
    public bool randomizePlayersTeamForTraining = false;
    public int maxAgentSteps;
    public int stepgen;
    public int GEN;
    public float agentRunSpeed;
    public float agentRotationSpeed;
    public float strikerPunish; //if opponents scores, the striker gets this neg reward (-1)
    public float strikerReward; //if team scores a goal they get a reward (+1)
    public float goaliePunish; //if opponents score, goalie gets this neg reward (-1)
    public float goalieReward; //if team scores, goalie gets this reward (currently 0...no reward. can play with this later)
    public float a;
    void Start()
    {
         Monitor.SetActive(true);
        Physics.gravity *= gravityMultiplier; //for soccer a multiplier of 3 looks good
        stepgen = 0;
        GEN = 0;
    }
    public override void AcademyReset()
    {
    }

    public override void AcademyStep()
    {
    stepgen = stepgen +1;
    if (stepgen == 2000){
        GEN = GEN +1;
        stepgen = 0;
    }

    Monitor.Log("Generation: ", GEN.ToString());


    }

}
