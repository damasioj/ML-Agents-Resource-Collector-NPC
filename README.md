# Master's Thesis: Resource Collector Simulation

This project is being developed for a Master's thesis that studies the implementation of behaviours in subjects using Machine Learning. The Resource Collector project attempts to simulate the behaviour of "workers" tasked with collecting resources from their surroundings and bringing them to a centralized location. 

The environment will have multiple targets to represent different kinds of resources, a goal to bring the resources known as the "resource depot", and at least two agents.

# Unity ML-Agents SDK

The project uses Unity ML-Agents to train agents.

# Goals

The following are the goals of this project:

- [x] 1 Agent: collecting a resource and bringing it to the goal
- [x] 1 Agent: collecting multiple different resources and bringing it to the goal
- [ ] 2 Agents: collecting multiple different resources and bringing it to the goal

# Requirements

To run this environment, you will need:
- Python 3.6.1 or greater
	- mlagents version 1.0.2 
- Unity 2019.2.0f1 or greater

# Current limitations / issues

## Data Association

At the moment one of the bigger limitations being faced is the lack of assocation in the data that is sent to the model. All observations of the environment are bundled together into a single array and then submitted. It's presumed that the value locations in the array are fixed and this is how the model eventually learns the meaning of each one through training. This causes problems when associating values to an object in the environment. For example, the model may have the information of a tree (its ID, location, and resource ID), but it won't be able to associate the relevant values (location, resource ID) to the parent (tree ID). The model is essentially training "in the dark" and becomes dependant on the value variations between training sessions.

## Model Flexibility

### Flexible targets and goals

As mentioned with issues in associating the data to specific objects, this creates another issue which is the model's flexibility when presented with more data. As the model "understands" the meaning of a value based on its location in the input array, the addition or removal of data from that array would require it to be trained all over again. There are ways to overcome this issue, which is usually by fixing the amount of information a model can have. For example, perhaps instead of giving information of all the objects in an area (which varies based on the agent's location), we will only provide the information of the closest object to the agent. This causes the input array to always have information of only one environment object. This could also be achieved through raycasting by providing the object that the agent "sees", and providing a default value if it doesn't see anything.

### Flexible requirements

It's still complex to solve issues where the goal is essentially the same but the requirements change. For instance, we may have a Builder agent that is currently tasked to build a house. We feed him the data that to build the house it requires 5 wood and 2 stone resources. We also provide information of the locations to gather these resources. After building the house, he's tasked with building a tower, which requires 5 wood, 1 stone, and 2 steel. The goal is the same: build a structure, but the requirements have now changed and include "steel". In this scenario, the Builder agent would need to have an input value for every kind of possible resource that the goals may require. Again, it's possible to overcome this by fixating the input data, but even by reducing the input data through OOP and using base classes this may be highly impractical/complicated in a actual game scenario. 

