using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Navigation;

[RequireComponent(typeof(Monster))]
public class MonsterNavigation : MonoBehaviour
{
    Monster monster;
	[HideInInspector] public NodeManager nodeManager;
	Room currentRoom;
	Node currentPathNode;
	Node goalNode;
	[SerializeField] float nodeArrivalDistance = 0.5f;

	private void Awake()
	{
		monster = GetComponent<Monster>();
		nodeManager = FindObjectOfType<NodeManager>();
		currentPathNode = null;
	}

	private void Start()
	{
		if (nodeManager == null)
		{
			nodeManager = FindObjectOfType<NodeManager>();
			Debug.LogWarning("NodeManager not assigned on MonsterNavigation. Finding...");
		}
	}

	public Node GetCurrentDestinationNode() 
	{
		return currentPathNode;
	}

	/// <summary>
	/// Is the current path node the last node in the path?
	/// </summary>
	/// <returns>True if current node is the goal.</returns>
	public bool IsLastNode()
	{
		return currentPathNode == goalNode;
	}

	/// <summary>
	/// Are we at the next node in the path?
	/// </summary>
	/// <returns>True if the distance to the next node is smaller than nodeArrivalDistance.</returns>
	public bool IsArrivedAtNode()
	{
		float distance = 0f;

		if (currentPathNode != null)
		{
			distance = Vector3.Distance(currentPathNode.transform.position, transform.position);
			return distance < nodeArrivalDistance;
		}

		return false;
	}

    public bool HasPath() 
	{
        return nodeManager.HasPath();
    }

    /// <summary>
    /// Go to the next node in the current path.
    /// </summary>
	public void GoToNextNode()
	{
		currentPathNode.selected = false;
		currentPathNode = nodeManager.PopPath();
		GoToNode(currentPathNode);
	}

	/// <summary>
	/// Set the node as the destination.
	/// </summary>
	/// <param name="node">Node to walk towards.</param>
	public void GoToNode(Node node)
	{
		monster.movement.GoToDestination(node.transform.position);
	}

    /// <summary>
    /// Create a path from the agent position to a node, and start walking to the first node.
    /// </summary>
    /// <param name="node">The goal node.</param>
    /// <returns>False if the path is rejected, true otherwise.</returns>
    public bool StartPath(Node node)
    {
        Node firstNode = nodeManager.StartPath(transform.position, node);
        if (firstNode == null)
            return false;

        currentPathNode = firstNode;
        goalNode = node;

        GoToNode(currentPathNode);

        return true;
    }

    /// <summary>
    /// Create a path to the node closest to a Vector3 position.
    /// </summary>
    /// <param name="destination">The target position</param>
    /// <returns>False if the path is rejected, true otherwise.</returns>
    public bool StartPath(Vector3 destination)
    {
        Node goal = nodeManager.GetClosestEnabledNode(destination);

        return StartPath(goal);
    }
}