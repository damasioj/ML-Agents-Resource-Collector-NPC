# Master's Thesis: Resource Collector Simulation

This project is being developed for a Master's thesis that studies the implementation of behaviours in subjects using Machine Learning. The Resource Collector project attempts to simulate the behaviour of "workers" tasked with collecting resources from their surroundings and bringing them to a centralized location. 

The environment will have multiple targets to represent different kinds of resources and a goal where to return the resources. The agent must be flexible and not require retraining when task requirements (amounts and type of resources needed) change or when presented new targets.

# Unity ML-Agents SDK

The project uses Unity ML-Agents to train agents.

# Goals

The following are the goals of this project:

- [x] Collecting a resource and bringing it to the goal
- [x] Collecting multiple different resources and bringing it to the goal

# Requirements

To run this environment, you will need:
- Python 3.6.1 or greater
	- mlagents version 0.16.1
- Unity 2019.2.0f1 or greater

# Current limitations / issues

## Data Association

At the moment one of the bigger limitations being faced is the lack of assocation in the data that is sent to the model. All observations of the environment are bundled together into a single array and then submitted. It's presumed that the value locations in the array are fixed and this is how the model eventually learns the meaning of each one through training. This causes problems when associating values to an object in the environment. For example, the model may have the information of a tree (its ID, location, and resource ID), but it won't be able to associate the relevant values (location, resource ID) to the parent (tree ID). The model is essentially training "in the dark" and becomes dependant on the value variations between training sessions.
