using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Monster))]
public class SearchNearby : MonoBehaviour {

	Navigation.NodeManager nodeManager;
	Monster monster;
	bool wait = false;

	Vector3 soundSource = Vector3.zero;
	Navigation.Room soundSourceRoom;
	bool searching = false;
	List<Navigation.Node> roomNodes = new List<Navigation.Node>();
	int amountOfNodes;
	bool doneSearching = true;

    Vector3 searchPosition;

	private void Awake()
	{
		monster = GetComponent<Monster>();
	}

	private void Start()
	{
		// Check if what's required exists
		if (nodeManager == null) { nodeManager = GameManager.instance.GetNodeManager(); }
		if (monster == null) { monster = GameManager.instance.GetMonster(); }
	}

	public void Search()
	{
		if (monster.playerRecentlySeen && !wait)
		{
			monster.movement.GoToDestination(monster.playerLastPos);

			if (Vector3.Distance(monster.transform.position, monster.playerLastPos) <= 0.3f)
			{
				wait = true;
				StartCoroutine(Wait(monster.waitingTime));
				nodeManager.astarPath.theList.Clear();
				monster.playerRecentlySeen = false;
				doneSearching = false;
			}
		}
		else if (!monster.playerRecentlySeen)
		{
			Navigation.Room playerRoom = GameManager.instance.GetPlayer().roomAgent.GetRecentRoom();
			//If the player is in a room after chase, search the interest nodes in that room
			if (!playerRoom.isWorld)
				SearchNearbyX(playerRoom, monster.playerLost);
			else
			{
				monster.playerLost = false;
				Debug.Log("Must have been the wind..");
			}
		}
	}

	public void SetSoundSource(Vector3 dest, Navigation.Room room)
	{
		if (!monster.heardSound)
		{
			monster.heardSound = true;
			soundSource = dest;
			soundSourceRoom = room;
		}
	} 

	public void SearchSoundSource()
	{
		if (monster.heardSound)
		{
			monster.movement.GoToDestination(soundSource);
			bool goToSource = true;							// Temporary example solution to make the monster only do this once. The bool should be set in another script or be solved in another way to prevent setting every tree update

			if (goToSource && !wait)
			{
				monster.movement.GoToDestination(monster.playerLastPos);

				if (Vector3.Distance(monster.transform.position, monster.playerLastPos) <= 0.5f)
				{
					wait = true;
					StartCoroutine(Wait(monster.waitingTime));
					nodeManager.astarPath.theList.Clear();
					goToSource = false;
					doneSearching = false;
					monster.heardSound = false;
				}
			}
			else if (!goToSource)
			{
				//If the sound came from a room, search that room
				if (!soundSourceRoom.isWorld)
					SearchNearbyX(soundSourceRoom, monster.heardSound);
				else
				{
					monster.heardSound = false;
					Debug.Log("Must have been the wind..");
				}
			}
		}
	}

	private void SearchNearbyX(Navigation.Room room, bool condition)
	{
		// Make the list of room nodes in a random order when first initialized
		if (roomNodes.Count == 0 && !doneSearching)
		{
			List<Navigation.Node> nodesInRoom = room.interestNodes;
			while (nodesInRoom.Count > 0)
			{
				int rng = Random.Range(0, nodesInRoom.Count);
				roomNodes.Add(nodesInRoom[rng]);
				nodesInRoom.RemoveAt(rng);
			}
		}

		// Stop searching in the room if it has finished searching all the nodes
		else if (roomNodes.Count == 0 && doneSearching)
		{
			condition = false;
			Debug.Log("Must have been the wind..");
		}

		// Only search a new node if the player isn't seen/chased and if it has reached the last node
		if (nodeManager.end == true && monster.seesPlayer == false && !nodeManager.atStairs)
		{
			nodeManager.SetGoalDestination(roomNodes[0]);
			nodeManager.end = false;
			roomNodes.RemoveAt(0);
		}
	}

	private void LookAround()
	{
		wait = true;
		StartCoroutine(Wait(monster.waitingTime));
		nodeManager.astarPath.theList.Clear();
		monster.playerRecentlySeen = false;
		searching = false;
	}

	private IEnumerator Wait(float waitTime)
	{
		monster.headLookIK.state = HeadLookState.random;
		yield return new WaitForSeconds(waitTime);
		wait = false;
		monster.headLookIK.state = HeadLookState.idle;
	}
}
