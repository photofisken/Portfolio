using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Navigation
{
	public class MonsterGoal : MonoBehaviour
	{
		private Node goal;

		public void SetGoalDestination(Node newGoal)
		{
			goal = newGoal;
		}

		public Node GetGoalDestination()
		{
			return goal;
		}
	}
}