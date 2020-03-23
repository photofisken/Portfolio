using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

[RequireComponent(typeof(Monster))]
public class Roam : MonoBehaviour
{
    Monster monster;
	Navigation.NodeManager nodeManager;
	[SerializeField] Stack<Navigation.Node> roomNodes = new Stack<Navigation.Node>();

    [SerializeField] List<Navigation.Room> roomExceptions = new List<Navigation.Room>();

    Navigation.Room currentRoom;

	private void Awake()
	{
		monster = GetComponent<Monster>();
	}

	private void Start()
	{
		if (nodeManager == null) { nodeManager = monster.navigation.nodeManager; }
	}

    /// <summary>
    /// Get number of nodes left to search in the current room.
    /// </summary>
    /// <returns>An integer</returns>
    public int NodesToSearch()
    {
        return roomNodes.Count;
    }

    /// <summary>
    /// Clear all interest nodes to search next.
    /// </summary>
    public void ClearNodes()
    {
        roomNodes.Clear();
    }

    /// <summary>
    /// Start searching all the interest points in a room.
    /// </summary>
    /// <param name="room">The room to search</param>
    /// <returns>True if at least one of the interest points are accessible, False otherwise.</returns>
    public bool SearchRoom(Navigation.Room room)
    {
        SetRoom(room);

        return TryNextRoomNode();
    }

	/// <summary>
	/// Try searching a random room
	/// </summary>
    /// <returns>False if no accessible rooms were found, True otherwise.</returns>
	public bool SearchRandomRoom()
    {
        List<Navigation.Room> shuffledRooms = GetShuffledRooms();
        bool success = false;

        while (!success && shuffledRooms.Count > 0)
        {
            Navigation.Room room = shuffledRooms[0];    // Get the topmost room
            shuffledRooms.RemoveAt(0);                  // Pop the room to not search it again
            success = SearchRoom(room);                 // Try searching the room
        }                                               // If we failed, try the next room in the list

        return success;                                 // If all searches failed, this will return false
    }

	/// <summary>
	/// Make the monster Look around
	/// </summary>
    /// <param name="lookTime">If >0f, calls <code>StopLookingAround()</code> after this many seconds.</param>
    public void LookAround(float lookTime = 0f)
    {
        if (lookTime > 0f)
            Invoke("StopLookingAround", lookTime);  // stop looking around after lookTime
        
        monster.headLookIK.state = HeadLookState.random;
    }

	/// <summary>
	/// Stop the monster from looking around.
	/// </summary>
    public void StopLookingAround()
    {
        monster.headLookIK.state = HeadLookState.idle;
    }

    private List<Navigation.Room> GetShuffledRooms()
    {
        Navigation.Room[] exceptions;
        
        if (currentRoom != null)
        {
            roomExceptions.Add(currentRoom);
            exceptions = roomExceptions.ToArray();
            roomExceptions.Remove(currentRoom);
        }
        else
            exceptions = roomExceptions.ToArray();

        return nodeManager.ShuffledRooms(exceptions);
    }

	/// <summary>
	/// Get a new random room from the Node Manager
	/// </summary>
    /// <remarks>
    /// Chooses between all rooms except the rooms in the exception list 
    /// and the previously roamed room.
    /// </remarks>
    /// <returns>A room</returns>
    private Navigation.Room GetRandomRoom()
    {
        Navigation.Room[] exceptions;

        if (currentRoom != null)
        {
            roomExceptions.Add(currentRoom);
            exceptions = roomExceptions.ToArray();
            roomExceptions.Remove(currentRoom);
        }
        else
            exceptions = roomExceptions.ToArray();

        return nodeManager.RandomRoom(exceptions);
    }

    /// <summary>
    /// Set a room for the monster to explore next.
    /// </summary>
    /// <param name="room">The room to explore next.</param>
    public void SetRoom(Navigation.Room room)
    {
        currentRoom = room;
        roomNodes = nodeManager.CreateStack(nodeManager.ShuffleInterestNodes(currentRoom));
    }

    /// <summary>
    /// Try going to the first accessible node in the room.
    /// </summary>
    /// <returns>False if no accessible nodes were found, true othewise.</returns>
    public bool TryNextRoomNode()
    {
        bool success = false;
        while (!success && roomNodes.Count > 0)
            success = GoToNextRoomNode();

        return success;
    }

	/// <summary>
	/// Get the next node in the current room and walk to it.
	/// </summary>
    /// <returns>False if the path to the room node is rejected, True otherwise.</returns>
    private bool GoToNextRoomNode()
    {
        Navigation.Node node = roomNodes.Pop();
        
        return monster.navigation.StartPath(node);
    }
}
