## Robot Ants - [Video](https://www.youtube.com/watch?v=xeYvA6Ag0Hg)

<img src="images/banner.png" align="middle" width="1920"/>

This is a reinforcement learning demo made with [Unity Machine Learning Agents](https://github.com/Unity-Technologies/ml-agents) [v0.12](https://github.com/Unity-Technologies/ml-agents/releases/tag/0.12.0). It was insprired by ants following pheromone trails. The robot ants have an energy level which gradually decreases from plus one to minus one. They create slowly evaporating trails for as long as that level is greater than zero. Trail point strengths match the bots' current energy. Each ant has to balance its behaviour between exploring new ground and recharging its battery at power-ups, which are spread randomly across the terrain.

The ants are controlled by two reinforcement learning agents that were trained sequentially with PPO.

The lower-tier agent ("walker") observes the bot's movement and body inclination, in addition to its leg positions and relative velocities of the feet. Ground distances are measured for each foot and the body center. It receives walking directions from the "searcher" agent - a normalized angle, relative to the bot's current heading on the XZ plane. Model outputs are 36 continuous values for controlling the rotations of two ball joints per leg. For training the ant to walk, rewards are set in proportion to its velocity towards a random target direction and for facing that direction.

The upper-tier agent ("searcher") observes the bot's energy level, the average trail strength in its vicinity and the direction towards the densest concentration of nearby trail points. The model's single output value serves as the walking direction for the "walker" agent. For training the desired behaviour, energy levels below zero and proximity to trails are penalized. The agent has to choose the least unfavourable action, which depends on the current energy level. As long as that level is high enough, ants will avoid trails and explore new terrain. Once it drops too low, they start following detected trails until they find a power-up.

Ant Design: Ergin3D (modified) via [turbosquid](https://www.turbosquid.com/FullPreview/Index.cfm/ID/1339233).

Dependencies:  
https://github.com/Unity-Technologies/ml-agents/releases/tag/0.12.0
