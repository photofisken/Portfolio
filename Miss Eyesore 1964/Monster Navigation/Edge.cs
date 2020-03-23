using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Navigation
{
	[System.Serializable]
	public class Edge
	{
		public Node[] nodes = new Node[2];
		public float distance;

		/// <summary>
		/// Create an edge between A and B and store the distance.
		/// </summary>
		/// <param name="a">Node A</param>
		/// <param name="b">Node B</param>
		public Edge(Node a, Node b)
		{
			nodes[0] = a;
			nodes[1] = b;
			distance = Vector3.Distance(a.transform.position, b.transform.position);
		}

		/// <summary>
		/// Checks if there is an edge between A and B.
		/// </summary>
		/// <param name="a">Node A</param>
		/// <param name="b">Node B</param>
		/// <returns>True if there is an edge. False otherwise.</returns>
		public bool IsAnEdge(Node a, Node b)
		{
			if (nodes[0] == a && nodes[1] == b)
				return true;
			else if (nodes[1] == a && nodes[0] == b)
				return true;
			else
				return false;
		}

		/// <summary>
		/// Get the other node in the edge.
		/// </summary>
		/// <param name="a">The node not to return.</param>
		/// <returns>The other node. Null if there is none.</returns>
		public Node GetNeighbor(Node a)
		{
			if (nodes[0] == a)
				return nodes[1];
			else if (nodes[1] == a)
				return nodes[0];
			else
				return null;
		}
	}

	public static class EdgeExtensions {
		/// <summary>
		/// Compare edge a with edge b.
		/// </summary>
		/// <param name="a">The first edge</param>
		/// <param name="b">The edge to compare with</param>
		/// <returns>True if the edges are equal, false otherwise.</returns>
		public static bool Compare(this Edge a, Edge b) {
			return (a.nodes [0] == b.nodes [0] && a.nodes [1] == b.nodes [1])
				|| (a.nodes [0] == b.nodes [1] && a.nodes [1] == b.nodes [0]);
		}
	}
}