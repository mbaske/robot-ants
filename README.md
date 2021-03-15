## Robot Ants

This is a reinforcement learning demo made with [Unity Machine Learning Agents](https://github.com/Unity-Technologies/ml-agents).  
It was insprired by ants following pheromone trails. The robot ants have an energy level which decreases over time. As long as its energy is above zero, each ant leaves a slowly evaporating trail, with individual trail point strengths matching the current energy level. Ants have to balance their behaviour between walking large distances and recharging their batteries at power-ups, spread randomly across the terrain.  
The ants are controlled by two reinforcement learning agents that were trained sequentially with PPO:  
The lower-tier agent ("walker") controls leg rotations and observes the ant's physics state as well as given a target direction. Training the policy was aided by imitation learning, with demonstrations generated from an oscillator driven heuristic. The agent is rewarded for its velocity in the target direction and penalized for facing away from it.  
The upper-tier agent ("searcher") observes the ant's energy level and surroundings (trails, power-ups, other ants' energies) using a [grid sensor](https://github.com/mbaske/grid-sensor). The policy's single output value serves as the target direction for the "walker" agent. The agent is rewarded for its current energy level. A waypoint queue stores a given number of agent positions over time. The squared distance between the first and last queued positions is divided by the maximum squared distance the agent can travel, the resulting ratio serves as an additional reward.
<br/><br/>
Ant Design: Ergin3D (modified) via [turbosquid](https://www.turbosquid.com/FullPreview/Index.cfm/ID/1339233).
