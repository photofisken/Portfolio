using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Navigation
{
	[RequireComponent(typeof(NodeManager))]
	public class AStarPathFinding : MonoBehaviour
	{
		public List<Node> theList = new List<Node>();
		
        public List<Node> GetPath(Node startPos, Node endPos)
        {
            Node startNode = startPos;
            Node targetNode = endPos;

            List<Node> openSet = new List<Node>();
            HashSet<Node> closedSet = new HashSet<Node>();

            openSet.Add(startNode);

            while (openSet.Count > 0)
            {
                Node currentNode = openSet[0];
                for (int i = 1; i < openSet.Count; i++)
                {
                    if (openSet[i].fCost < currentNode.fCost || openSet[i].fCost == currentNode.fCost && openSet[i].hCost < currentNode.hCost)
                    {
                        currentNode = openSet[i];
                    }
                }
                openSet.Remove(currentNode);
                closedSet.Add(currentNode);

                // If A* reaches the end node, make the path in a list of nodes
                if (currentNode == null)
                {
                    Debug.LogError("currentNode.node == null");
                    return null;
                }
                if (targetNode == null)
                {
                    Debug.LogError("targetNode.node == null");
                    return null;
                }
                if (currentNode == targetNode)
                {
                    return RetracePath(startNode, targetNode);
                }

                foreach (Node neighbor in currentNode.GetNeighbors())
                {
                    if (closedSet.Contains(neighbor))
                    {
                        continue;
                    }


                    float newMovementCostToNeighbor = currentNode.gCost + currentNode.DistanceToNeighbor(neighbor) + currentNode.stairCost;
                    float extraYCost = Mathf.Abs(neighbor.transform.position.y - targetNode.transform.position.y) * 10f; // Makes the AI prefer walking on the same floor instead of taking stairs, since going in y-axis is extra costly.

                    if (newMovementCostToNeighbor < neighbor.gCost || !openSet.Contains(neighbor))
                    {
                        neighbor.gCost = newMovementCostToNeighbor;
                        neighbor.hCost = GetDistance(neighbor, targetNode) + extraYCost;
                        neighbor.parent = currentNode;

                        if (!openSet.Contains(neighbor))
                        {
                            openSet.Add(neighbor);
                        }
                    }
                }
            }

            //Debug.LogError("AStarPath failed to produce a path.");
            return null;
        }

        /// <summary>
        /// Retrace path from endNode to startNode.
        /// </summary>
        /// <param name="startNode">Start Node</param>
        /// <param name="endNode">End Node</param>
        /// <returns>List of nodes from start to end</returns>
        public List<Node> RetracePath(Node startNode, Node endNode)
		{
			List<Node> path = new List<Node>();
			Node currentNode = endNode;

			while (currentNode != startNode)
			{
				path.Add(currentNode);
				currentNode = currentNode.parent;
			}
			path.Reverse();

			return path;
		}

		private float GetDistance(Node nodeA, Node nodeB)
		{
			return Vector3.Distance(nodeA.transform.position, nodeB.transform.position);
		}
		private float GetYDistance(Node nodeA, Node nodeB)
		{
			return Mathf.Abs(nodeA.transform.position.y - nodeB.transform.position.y);
		}

	}
}