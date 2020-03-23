using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Rendering.PostProcessing;

namespace Navigation
{
	[RequireComponent(typeof(AStarPathFinding))]
	public class NodeManager : MonsterGoal
	{
		[Range(0f, 2f)] public float reachNodeDistance = 0.5f;

		[Header("Lists")]
		public List<Room> rooms = new List<Room>();
		public List<Node> nodes = new List<Node>();
		public List<Node> interestNodes = new List<Node>();
		public List<Node> stairNodes = new List<Node>();
		public List<Node> pathNodes = new List<Node>();
		[SerializeField] List<Node> nodesWithNoRoom = new List<Node>();

		public Stack<Node> pathStack = new Stack<Node>();

		private bool wait;
		private bool enterStair = true;

		[Header("Node Placer")]
		public bool addNodes = true;
		public NodeType type = NodeType.stairs;
		public GameObject prefab;
		[Range(0.1f, 5f)]
		public float fetchRadius = 3f;

		[Space]
		public bool isRoom;

		[Header("Room Environment Post Processing Profiles")]
		[SerializeField] private PostProcessProfile coldProfile;
		[SerializeField] private PostProcessProfile freezingProfile;
		[SerializeField] private PostProcessProfile hotProfile;
		public float profileBlendDistance = 5f;
		public Vector2Int profilePriorityMinMax = new Vector2Int(1, 100);
		public PostProcessProfile GetProfile(RoomEnvironment environment)
		{
			switch(environment)
			{
				case RoomEnvironment.Freezing:
					return freezingProfile;

				case RoomEnvironment.Hot:
					return hotProfile;

				default:
					return coldProfile;
			}
		}

		private Node startNode;
		private Node current;
		[HideInInspector] public AStarPathFinding astarPath;
		[HideInInspector] public bool end = false;
		[HideInInspector] public bool start;
		[HideInInspector] public bool atStairs = true;

		public static bool drawNodeSpheres = true;
		public static bool drawNeighborLines = true;
		public static bool drawNeighborDistance = false;
		public static bool drawFetchDistance = false;

		public static bool drawRoomNames = true;
		public static bool drawRoomVolumes = true;
		public static bool drawRoomLines = false;
		public static bool drawRoomNodes = false;

		private void Awake()
		{
			astarPath = GetComponent<AStarPathFinding>();

			if (interestNodes.Count <= 1) { Debug.LogError("No interest nodes placed. Make sure to change some nodes to the correct type."); }
			if (stairNodes.Count <= 1) { Debug.LogError("No stair nodes placed. Make sure to change some nodes to the correct type."); }
		}

		private void Update()
		{

			// Set all nodes in the path as selected
			if (pathNodes.Count > 0)
			{
				foreach (Node n in pathNodes)
				{
					n.selected = true;
				}
			}
		}

		/// <summary>
		/// Get all NavigationNode components and add to list.
		/// </summary>
		public void GetAllNodes()
		{
			nodes.Clear();
			interestNodes.Clear();
			stairNodes.Clear();
			nodesWithNoRoom.Clear();
			Node[] nodeArray = transform.GetComponentsInChildren<Node>();

			// Add all nodes to list and corresponding type lists
			foreach (Node node in nodeArray)
			{
				nodes.Add(node);

				switch(node.type)
				{
					case NodeType.interest:
						interestNodes.Add(node);
						break;

					case NodeType.stairs:
						stairNodes.Add(node);
						break;
				}
			}
		}

		/// <summary>
		/// Clear all nodes then fetch them again.
		/// </summary>
		public void FetchNeighbors()
		{
			GetAllNodes();
			ClearNeighbors();

			// For all nodes
			foreach (Node nodeA in nodes)
			{
				// Iterate all nodes
				foreach (Node nodeB in nodes)
				{
					if (nodeB.type == NodeType.stairs)
					{
						nodeB.stairCost = 1000;
					}

					// Determine what floor the node is on
					if (nodeB.transform.position.y > 1)
					{
						nodeB.floor = 2;
					}
					else if (nodeB.transform.position.y < -1)
					{
						nodeB.floor = 0;
					}
					else
					{
						nodeB.floor = 1;
					}

					// If they are not the same node
					if (nodeA != nodeB)
					{
						// Add edges between neighbors if within radius
						float distance = Vector3.Distance(nodeA.transform.position, nodeB.transform.position);
						if (distance < nodeA.fetchRadius)
						{
							nodeA.AddEdge(nodeA, nodeB);
						}
					}
				}

				// For all neighbors
				foreach (Node nodeB in nodeA.GetNeighbors())
				{
					if (nodeB == null)
					{
						nodeA.RemoveEdge(nodeA, nodeB);
						break;
					}
				}
			}
		}

		/// <summary>
		/// Clear all neighbors on all nodes.
		/// </summary>
		public void ClearNeighbors()
		{
			foreach (Node n in nodes)
			{
				if (n.edges != null)
				{
					n.edges.Clear();
				}
			}
		}

		public void FetchRooms()
		{
			Room[] rooms = FindObjectsOfType<Room>();
			this.rooms.Clear();

			foreach (Room room in rooms)
			{
				room.FetchNodesInRoom();
				this.rooms.Add(room);
			}
		}

		/// <summary>
		/// Removes all neighboring edges and removes the node from the nodes list.
		/// </summary>
		/// <param name="node">The node to remove.</param>
		public void RemoveNode(Node node)
		{
			List<Node> neighbors = node.GetNeighbors();

			foreach (Node neighbor in neighbors)
			{
				neighbor.RemoveEdge(neighbor, node);
			}

			node.ClearEdges();

			//nodes.Remove(node);
		}

		/// <summary>
		/// Get the node closest to a position
		/// </summary>
		/// <param name="position">The position to compare with</param>
		/// <param name="scale">A vector with which to scale the coordinate system. Default: (1, 1, 1)</param>
		/// <returns>The node that is closest to the position</returns>
		public Node GetClosestNode(Vector3 position, Vector3 scale = default(Vector3))
		{
			if (scale == default(Vector3)) {
				scale = Vector3.one;
			}

			Node currentNode = nodes[0];
			float bestDist = Mathf.Infinity;

			// For all nodes, find closest node
			foreach (Node n in nodes)
			{
				Vector3 nodePos = n.transform.position;
				float dist = Vector3.Distance(position, Vector3.Scale(nodePos, scale));

				if (dist <= bestDist)
				{
					bestDist = dist;
					currentNode = n;
				}
			}
			return currentNode;
		}

        /// <summary>
        /// Get the enabled node closest to a position
        /// </summary>
        /// <param name="position">The position to compare with</param>
        /// <param name="scale">A vector with which to scale the coordinate system. Default: (1, 1, 1)</param>
        /// <returns>The node that is closest to the position</returns>
        public Node GetClosestEnabledNode(Vector3 position, Vector3 scale = default(Vector3))
        {
            if (scale == default(Vector3))
            {
                scale = Vector3.one;
            }

            Node currentNode = nodes[0];
            float bestDist = Mathf.Infinity;

            // For all nodes, find closest node
            foreach (Node n in nodes)
            {
                if (!n.enabled) { continue; }

                Vector3 nodePos = n.transform.position;
                float dist = Vector3.Distance(position, Vector3.Scale(nodePos, scale));

                if (dist <= bestDist)
                {
                    bestDist = dist;
                    currentNode = n;
                }
            }
            return currentNode;
        }

        /// <summary>
        /// Creates a new path from a position to a node
        /// </summary>
        /// <param name="position">The position of the agent wanting to travel to the node</param>
        /// <param name="destinationNode">The node to travel to</param>
        /// <returns>The first node the agent should travel to, or null if the path is rejected.</returns>
        public Node StartPath(Vector3 position, Node destinationNode)
		{
            // Scale the y-axis times 10 so that we choose a node on the same floor.
			Node startNode = GetClosestNode(position, new Vector3(1f, 10f, 1f));
			List<Node> path = new List<Node>();

			//Debug.Log("Trying path from " + startNode + " to " + destinationNode, destinationNode);

			if (startNode.floor == destinationNode.floor)
			{
				//Debug.Log("Same floor.");
				path = astarPath.GetPath(startNode, destinationNode);
                if (path == null) {
                    return null;
                }

				if (GetPathComplexity(path) > 0)
				{
					//Debug.Log("Path too complex.");
					path = null;
				}
			}
			else
			{
				//Debug.Log("Finding staircase path...");
				path = GetStaircasePath(startNode, destinationNode);
			}

			// Could not find a non-complex path to destination. Give up.
			if (path == null)
			{
				return null;
			}

			pathStack = CreateStack(path);

			foreach (var node in pathStack)
			{
				node.selected = true;
			}

			return startNode;
		}

		/// <summary>
		/// Use staircase nodes to create a path that leads to a node on another floor.
		/// </summary>
		/// <param name="startNode">The node to originate the path from.</param>
		/// <param name="destinationNode">The node to set the destination to.</param>
		/// <returns>A list of nodes representing the path, with the closest node first.</returns>
		private List<Node> GetStaircasePath(Node startNode, Node destinationNode)
		{
			List<Node> currentPath = new List<Node>();

			Node currentOriginNode = startNode;

			int iterations = 0;

			// Get the path to the closest staircase node that leads towards the floor we want, and the path to its exit.
			do
			{
				Node staircaseExit = null;
				bool up = currentOriginNode.floor < destinationNode.floor;
				Node staircase = GetClosestStaircase(currentOriginNode, up, out staircaseExit);

				if (staircase == null || staircaseExit == null) {
					// No staircases at this floor going in the right direction.
					if (staircase == null) {
						//Debug.Log("staircase is null.");
					}
					if (staircaseExit == null) {
						//Debug.Log("staircase exit is null.");
					}
					return null;
				}

				List<Node> pathToStaircase = astarPath.GetPath(currentOriginNode, staircase);
				List<Node> staircasePath = astarPath.GetPath(staircase, staircaseExit);

				if (pathToStaircase == null || staircasePath == null || GetPathComplexity(pathToStaircase) > 0) {
                    // Creating path to staircase didn't work.
                    if (pathToStaircase == null) {
                        //Debug.Log("path to staircase is null.", staircase);
                    } else if (GetPathComplexity(pathToStaircase) > 0) {
                        //Debug.Log("path to staircase too complex.");
                    }
					if (staircasePath == null) {
						//Debug.Log("staircase path is null.", staircaseExit);
					}
					return null;
				}

				currentPath.AddRange(pathToStaircase);
				currentPath.AddRange(staircasePath);

				currentOriginNode = staircaseExit;

				iterations++;
			} while (currentOriginNode.floor != destinationNode.floor); // If the stair exit is not on the right floor, repeat.

			//Debug.Log("Finding staircase path iterations: " + iterations);

			// We're finally on the right floor, now get the path from the stair exit to the destination.
			List<Node> correctFloorPath = astarPath.GetPath(currentOriginNode, destinationNode);

			// If the path is not zero-complexity, give up.
			if (correctFloorPath == null || GetPathComplexity(correctFloorPath) > 0)
			{
				//Debug.Log("Got to the correct floor, but no further.");
				return null;
			}
			else
			{
				currentPath.AddRange(correctFloorPath);
			}

			return currentPath;
		}

		/// <summary>
		/// Gets a random room-node with a zero complexity path to a node.
		/// </summary>
		/// <param name="originNode">The node to compare the path to the room with</param>
		/// <param name="maxDistance">The maximum distance that a room node can have from the origin. Default: <code>float.MaxValue</code></param>
		/// <param name="exceptions">Rooms to not return. Default: <code>null</code></param>
		/// <returns>A room-node, or null if none were found</returns>
		public Room RandomZeroComplexityRoom(Node originNode, float maxDistance = float.MaxValue, params Room[] exceptions)
		{
			// All rooms that are further away than maxDistance are excluded
			List<Room> farRooms = GetFarRooms(originNode, maxDistance);
			farRooms.AddRange(exceptions);

			List<Room> roomPool = ShuffledRooms(farRooms.ToArray());

			while (roomPool.Count > 0)
			{
				Node roomNode = roomPool[0].GetComponent<Node>();
				if (originNode.floor == roomNode.floor && GetPathComplexity(astarPath.GetPath(originNode, roomNode)) <= 0) {
					// When a zero-complexity room is found, return it instantly.
					return roomPool[0];
				} else {
					// Otherwise pop and continue searching.
					roomPool.RemoveAt(0);
				}
			}

			return null;    // No zero-complexity rooms were found.
        }

		/// <summary>
		/// Gets all room-nodes that are further away than <code>distance</code>
		/// </summary>
		/// <param name="distance">The distance threshold</param>
		/// <returns>A list of rooms</returns>
		public List<Room> GetFarRooms(Node originNode, float distance)
		{
			List<Room> ret = new List<Room>();
			foreach (Room room in rooms) {
				var collider = room.spawnedColliders[0];

				Vector3 closestPoint = collider.ClosestPointOnBounds(originNode.transform.position);

				if (Vector3.Distance(closestPoint, originNode.transform.position) > distance) {
					ret.Add(room);
				}
			}

			return ret;
		}

		/// <summary>
		/// Check how many times a path switches floors.
		/// </summary>
		/// <param name="path">The path to check.</param>
		/// <returns>How many times the path changes floors.</returns>
		private int GetPathComplexity(List<Node> path)
		{
			if (path.Count < 2) { return 0; }

			int changes = 0;
			int currentFloor = path[0].floor;

			for (int i = 1; i < path.Count; i++)
			{
				if (path[i].floor != currentFloor)
				{
					changes++;
					currentFloor = path[i].floor;
				}
			}
			
			return changes;
		}

		/// <summary>
		/// Deselect and remove all nodes in the path.
		/// </summary>
		public void OnFinishedPath()
		{
			while (pathStack.Count > 0)
			{
				Node node = pathStack.Pop();
				node.selected = false;
			}
		}

        public bool HasPath()
        {
            return pathStack.Count > 0;
        }

		/// <summary>
		/// Pops the path stack.
		/// </summary>
		/// <returns>The top of the path stack. Null if empty.</returns>
		public Node PopPath()
		{
			if (pathStack.Count > 0)
			{
				Node node = pathStack.Pop();
				return node;
			}
			return null;
		}

		/// <summary>
		/// Get a random room.
		/// </summary>
		/// <param name="exceptions">Array of rooms that may not be chosen.</param>
		/// <returns>A Room object.</returns>
		public Room RandomRoom(params Room[] exceptions)
		{
			Room room = null;
			int rnd = 0;

			// Create a separate copy of the room list and remove all the exceptions from it
			List<Room> roomPool = ApplyExceptions(rooms, exceptions);

			// Get a random room from the room list
			if (roomPool.Count > 0) {
				rnd = UnityEngine.Random.Range(0, roomPool.Count);
				room = roomPool[rnd];
			}

			return room;
		}

		/// <summary>
		/// Creates a new list of shuffled rooms.
		/// </summary>
		/// <param name="exceptions">Rooms not to include in the returned list.</param>
		/// <returns></returns>
		public List<Room> ShuffledRooms(params Room[] exceptions)
		{
			List<Room> roomPool = ApplyExceptions(rooms, exceptions);
			List<Room> shuffledRooms = new List<Room>();
			while (roomPool.Count > 0)
			{
				int rnd = UnityEngine.Random.Range(0, roomPool.Count);
				shuffledRooms.Add(roomPool[rnd]);
				roomPool.RemoveAt(rnd);
			}

			return shuffledRooms;
		}

		/// <summary>
		/// Creates a new list of rooms that does not include the rooms in the exceptions array.
		/// </summary>
		/// <param name="exceptions">The rooms to exclude</param>
		/// <returns>A list of rooms</returns>
		private List<Room> ApplyExceptions(List<Room> originalList, Room[] exceptions)
		{
			List<Room> roomPool = new List<Room>(originalList);
			if (exceptions != null)
			{
				foreach (Room r in exceptions)
				{
					roomPool.Remove(r);
				}
			}

			return roomPool;
		}

		/// <summary>
		/// Get a list of all interestNodes in a given room in a random order.
		/// </summary>
		/// <param name="room">Room to get nodes from.</param>
		/// <returns>List of interestNodes in a random order.</returns>
		public List<Node> ShuffleInterestNodes(Room room)
		{
			List<Node> shuffledNodes = new List<Node>();

			// For all interestNodes
			while (room.interestNodes.Count > 0)
			{
				int rng = UnityEngine.Random.Range(0, room.interestNodes.Count);    // Get a random node from the list
				shuffledNodes.Add(room.interestNodes[rng]);     // Add that node to the shuffledNodes list
				room.interestNodes.RemoveAt(rng);   // Remove the node from the original list, ensures that it won't be selected again
			}

			room.interestNodes = shuffledNodes; // Replace interestNodes with shuffledNodes

			return shuffledNodes;
		}

		/// <summary>
		/// Create a stack of nodes from a list of nodes.
		/// </summary>
		/// <param name="nodes">List of nodes to convert into a stack.</param>
		/// <returns>Stack of nodes.</returns>
		public Stack<Node> CreateStack(List<Node> nodes)
		{
			Stack<Node> stack = new Stack<Node>();

			for (int i = 0; i < nodes.Count; i++)
			{
				stack.Push(nodes[nodes.Count - i - 1]);
			}

			return stack;
		}

		/// <summary>
		/// Gets the closest staircase node.
		/// </summary>
		/// <param name="origin">The node to start searching from</param>
		/// <param name="up">The direction of the desired staircase. False is down, True is up.</param>
		/// <param name="stairExitNode">An out-parameter that is set to the exit node of the staircase node.</param>
		/// <returns>A staircase node, or null if none were found.</returns>
		private Node GetClosestStaircase(Node origin, bool up, out Node stairExitNode)
		{
			Node currentNode = null;
			stairExitNode = null;
			float bestDist = Mathf.Infinity;

			// For all nodes, find closest node
			foreach (Node stairNode in stairNodes)
			{
				Vector3 nodePos = stairNode.transform.position;
				float dist = Vector3.Distance(origin.transform.position, nodePos);
				Node exit = GetStairExit(stairNode, up);
				if (exit == null) {
					continue;		// the proper staircase exit couldn't be found
				}

				if (dist <= bestDist) // if the stair node is shortest distance...
				{
					bestDist = dist;
					stairExitNode = exit;
					currentNode = stairNode;                // this is the new best node.
				}
			}

			return currentNode;
		}

		/// <summary>
		/// Get the closest staircase node on a different floor.
		/// </summary>
		/// <param name="startStair">The staircase node to measure from.</param>
		/// <param name="up">The direction of the desired stair exit. True is up, False is down.</param>
		/// <returns>The closest staircase node, or <code>null</code> if none are found.</returns>
		public Node GetStairExit(Node startStair, bool up)
		{
			float bestDist = Mathf.Infinity;
			Node currentStairs = null;
			Vector3 startPos = startStair.transform.position;

			if (startStair.type != NodeType.stairs)
			{
				return null;
			}

			// For all stairNodes, find closest node to the destination
			foreach (Node stairNode in stairNodes)
			{
				Vector3 nodePos = stairNode.transform.position;

				float dist = Vector3.Distance(startPos, nodePos);
				bool isUp = stairNode.floor > startStair.floor;

				// If it has a better distance, does not share the same floor, and is in the desired direction.
				if (dist <= bestDist && startStair.floor != stairNode.floor && up == isUp)
				{
					bestDist = dist;
					currentStairs = stairNode;  // We've found a new best stair node.
				}
			}
			return currentStairs;
		}


		#region OLD_STUFF
		/*
				/// <summary>
				/// Set the goal destination for the monster.agent and then instruct it to go there
				/// </summary>
				private void SetMonsterDestination()
				{
					if (astarPath.theList.Count == 0)
					{
						// Generate a new destination and set it as goal in Roam script
						end = false;
						//astarPath.theList.Clear();                                                                      // Clear the list in case it isn't clean already ato make space for a new path

						// Set start as closest to monster.agent and goal to end and create a list/path
						startNode = GetStartNode();
						Node endNode = GetGoalDestination();

						if (endNode.floor == startNode.floor)
						{
							atStairs = false;
							astarPath.FindPath(startNode, endNode);
							pathNodes = astarPath.theList;
							start = true;                                                                               // The first node in the list is the start nodes
							//Debug.Log("Going to the node! (Without using stairs)");
						}

						// if the monster wants to go to another floor
						else if (endNode.floor != startNode.floor && enterStair)												// Enter stairs at top/bottom
						{
							pathNodes.Clear();
							//Debug.Log("I need to take the stairs, the node is on another level");
							atStairs = true;
							Node newNode = GetStairEnter(endNode, startNode);
							astarPath.FindPath(startNode, newNode);
							pathNodes = astarPath.theList;
							start = true;
							enterStair = false;
						}
						else if (endNode.floor != startNode.floor && !enterStair)										// Exit stairs at top/bottom
						{
							pathNodes.Clear();
							//Debug.Log("I now choose to go to the end of the stairs next");
							Node newNode2 = GetStairExit(startNode);
							astarPath.FindPath(startNode, newNode2);
							pathNodes = astarPath.theList;
							start = true;
							enterStair = true;																	
						}

					}
					GoToNextNode();
				}

				/// <summary>
				/// If it hasn't reached the end goal, make the next node in the list the target for the monster.agent
				/// </summary>
				private void GoToNextNode()
				{
					if (startNode == GetGoalDestination())																// In case the starting point and end point is the same
					{
						//Debug.Log("Start");
						Vector3 newDest = startNode.transform.position;
						monster.GoToDestination(newDest);
						end = true;
					}
					else
					{
						if (pathNodes.Count > 0 && !start)                                                              // Only remove the first object if the start node has been used
						{
							//Debug.Log("Deletes the first object!");
							pathNodes[0].selected = false;
							current = pathNodes[0];
							pathNodes.Remove(pathNodes[0]);
						}

						if (pathNodes.Count == 0)
						{
							end = true;
						}
						else
						{
							//Debug.Log("Move!");
							Node current = pathNodes[0];
							Vector3 newDest = current.transform.position;
							monster.GoToDestination(newDest);
							start = false;
						}
					}
				}
		*/
		/// <summary>
		/// Generate a new random position as a goal for the monster.agent 
		/// </summary>
		/// <returns>Return target if path is found</returns>
		/*private Node NewRndMonsterDestination()
		{
			if (interestNodes.Count > 0)
			{
				int rnd = UnityEngine.Random.Range(0, interestNodes.Count);
				Node target = interestNodes[rnd];

				return target;
			}
			else
			{
				//Debug.Log("Monster isn't interested!");
				return null;
			}
		}*/

		/// <summary>
		/// Check if the monster.agent has reached it's goal, if so, wait
		/// </summary>
		//private void CheckReachGoal()
		//{
		// MY FAILED EXPERIMENT
		/*
		if (pathNodes.Count > 0)
		{
			if (Vector3.Distance(monster.transform.position, pathNodes[0].transform.position) > current.DistanceToNeighbor(pathNodes[0]))
			{
				Debug.Log("Blocked! New path!");
				end = true;
				astarPath.theList.Clear();
				SetMonsterDestination();
			}
		}

		*/
		/*
		if (monster.GetRemainingDistance() <= 0.3f)
		{
			//monster.movement.wait = true;
			wait = true;
			StartCoroutine(Wait(monster.waitingTime));
		}
		if (monster.GetRemainingDistance() <= 0.3f && atStairs && !wait)
		{
			wait = true;
			StartCoroutine(Wait(monster.waitingTime));
		}

	}

	private void GoalReached()
	{
		StartCoroutine(Wait(monster.waitingTime));
	}

	private bool HasReachedGoal()
	{
		return monster.GetRemainingDistance() <= 0.6f;
	}

	/// <summary>
	/// Find the node closest to the monster.agent (start node)
	/// </summary>
	/// <returns>Starting node</returns>
	public Node GetStartNode()
	{
		Node currentNode = nodes[0];
		Vector3 monsterPos = monster.transform.position;
		float bestDist = Mathf.Infinity;

		// For all nodes, find closest node
		foreach (Node n in nodes)
		{
			Vector3 nodePos = n.transform.position;
			float dist = Vector3.Distance(monsterPos, nodePos);

			if (dist <= bestDist)
			{
				bestDist = dist;
				currentNode = n;
			}
		}
		return currentNode;
	}

	public Node GetStairEnter(Node dest, Node startPos)
	{
		float bestDist = Mathf.Infinity;
		Node currentStairs = stairNodes[0];
		Vector3 destPos = dest.transform.position;
		Vector3 startPosition = dest.transform.position;

		// For all stairNodes, find closest node to the destination
		foreach (Node stairNode in stairNodes)
		{
			Vector3 nodePos = stairNode.transform.position;


			float dist = Vector3.Distance(destPos, nodePos);
			*/
		/*if (dist <= bestDist && stairNode.floor != dest.floor && stairNode.floor == startPos.floor && startPosition != nodePos)
		{
			bestDist = dist;
			currentStairs = stairNode;
		}*/
		/*
		// If it's the closest stairs to the end destination AND if the stairs start on the same floor monster is on 
		// AND if the end of the stairs is a floor closer to end dest.
		if ((dist <= bestDist)  &&  
			(stairNode.floor == GetStartNode().floor) && 
			(Mathf.Abs(dest.floor - GetStairExit(stairNode).floor) < Mathf.Abs(dest.floor - stairNode.floor)))
		{
			bestDist = dist;
			currentStairs = stairNode;
		}

	}
	return currentStairs;
}

public Node GetStairExit(Node startStair)
{
	float bestDist = Mathf.Infinity;
	Node currentStairs = stairNodes[0];
	Vector3 startPos = startStair.transform.position;

	// For all stairNodes, find closest node to the destination
	foreach (Node stairNode in stairNodes)
	{
		Vector3 nodePos = stairNode.transform.position;

		float dist = Vector3.Distance(startPos, nodePos);

		if (dist <= bestDist && nodePos != startPos)
		{
			bestDist = dist;
			currentStairs = stairNode;
		}
	}
	return currentStairs;
}

private float GetDistance(Node nodeA, Node nodeB)
{
	return Vector3.Distance(nodeA.transform.position, nodeB.transform.position);
}

// Change the monster.agent's goal after waiting
private IEnumerator Wait(float waitTime)
{
	if (end && !atStairs)
	{
		SetMonsterDestination();
		monster.SetStopped(true);
		monster.movement.state = MonsterMoveState.idle;
		if (!monster.seesPlayer)
		{
			monster.headLookIK.state = HeadLookState.looking;
			yield return new WaitForSeconds(waitTime);
			monster.headLookIK.state = HeadLookState.idle;
		}
		monster.SetStopped(false);
		wait = false;
		//monster.movement.wait = false;
	}
	else if (end && atStairs)
	{
		SetMonsterDestination();
		wait = false;
	}
	else
	{
		GoToNextNode();
		wait = false;
		//monster.movement.wait = false;
	}
}
*/
		#endregion
	}
}