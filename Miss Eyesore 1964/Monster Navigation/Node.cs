using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Navigation
{
	public enum NodeType
	{
		standard, interest, stairs, exit
	}

	public class Node : MonoBehaviour
	{
		[Header("Fetch Neighbors")]
		public float fetchRadius = 3f;
		public NodeType type;

		public List<Edge> edges = new List<Edge>();
		public Node parent;
		public float gCost;
		public float hCost;
		public float stairCost = 0;
		public int floor;

		public bool selected;
		public bool enabled = true;

		public float fCost
		{
			get { return gCost + hCost; }
		}

        /// <summary>
        /// The edge created in Edge is saved in a list of edges on the node, to be used as neighbours.
        /// </summary>
        /// <param name="a">Node A</param>
        /// <param name="b">Node B</param>
        public void AddEdge(Node a, Node b)
		{
			Edge edge = new Edge(a, b);
			if (!a.IsInList(a, b))
			{
				a.edges.Add(edge);
			}
			if (!b.IsInList(a, b))
			{
				b.edges.Add(edge);
			}
		}

        /// <summary>
        /// Check if there already is an edge between node a and b. Returns true or false.
        /// </summary>
        /// <param name="a">Node A</param>
        /// <param name="b">Node B</param>
        /// <returns>True if there is an edge. False otherwise.</returns>
        public bool IsInList(Node a, Node b)
		{
			foreach(Edge edge in edges)
			{
				if (edge.IsAnEdge(a, b))
					return true;
			}
			return false;
		}

        /// <summary>
        /// Get all the neighbours saved on the node
        /// </summary>
        /// <returns>The list of neighbours.</returns>
		public List<Node> GetNeighbors()
		{
			List<Node> neighbors = new List<Node>();

			// If disabled, pretend we don't have neighbors.
			if (!enabled) {
				return neighbors;
			}

			foreach(Edge edge in edges)
			{
				neighbors.Add(edge.GetNeighbor(this));
			}

			return neighbors;
		}

		public float DistanceToNeighbor(Node a)
		{
			foreach(Edge edge in edges)
			{
				if(edge.Compare(new Edge(a, this)))
				{
					return edge.distance;
				}
			}
			return -1f;
		}

		public void RemoveEdge(Node a, Node b)
		{
			List<Edge> edgesToRemove = new List<Edge>();

			foreach (Edge edge in edges)
			{
				if(edge.GetNeighbor(a) == b)
				{
					edgesToRemove.Add(edge);
				}
				if (edge.GetNeighbor(b) == a)
				{
					edgesToRemove.Add(edge);
				}
			}

			foreach (Edge edge in edgesToRemove) { edges.Remove(edge); }
		}

		public void ClearEdges()
		{
			edges.Clear();
		}

		private void OnDrawGizmos()
		{
			if (NodeManager.drawNodeSpheres)
			{
				if (type == NodeType.standard)
					Gizmos.color = Color.white;
				else if (type == NodeType.interest)
					Gizmos.color = Color.green;
				else if (type == NodeType.stairs)
					Gizmos.color = Color.yellow;
				else
					Gizmos.color = Color.red;

				if (selected)
					Gizmos.color = Color.blue;
				if (!enabled) {
					Gizmos.color = Color.grey;
				}

				Gizmos.DrawSphere(transform.position, 0.2f);
			}

			if (NodeManager.drawNeighborLines && enabled)
			{
				Gizmos.color = Color.red;
				if (edges == null)
				{
					return;
				}
				foreach (Edge edge in edges)
				{
					if (edge.nodes[0] == null || edge.nodes[1] == null)
					{
						edges.Remove(edge);
						break;
					}

					if (!edge.nodes[0].enabled || !edge.nodes[1].enabled) {
						continue;
					}

					if (edge.nodes[0].selected && edge.nodes[1].selected)
						Gizmos.color = Color.blue;
					else
						Gizmos.color = Color.red;

					Gizmos.DrawLine(edge.nodes[0].transform.position, edge.nodes[1].transform.position);
				}
			}
		}
#if UNITY_EDITOR
		private void OnDrawGizmosSelected()
		{
			if(NodeManager.drawNeighborDistance)
			{
				foreach(Edge edge in edges)
				{
					Vector3 position = Vector3.Lerp(edge.nodes[0].transform.position, edge.nodes[1].transform.position, 0.5f);
					float distance = Mathf.Round(edge.distance * 100f) / 100f;
					Handles.Label(position, distance.ToString());
				}
			}
			if (NodeManager.drawFetchDistance)
			{
				Gizmos.color = Color.blue;
				Gizmos.DrawWireSphere(transform.position, fetchRadius);
			}
		}
#endif
	}
}