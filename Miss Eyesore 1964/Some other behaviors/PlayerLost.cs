using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Monster))]
public class PlayerLost : MonoBehaviour {

	private Monster monster;
	private Player player;
	private Navigation.NodeManager nodeManager;

    private Vector3 playerLost = Vector3.zero;

	private void Awake()
	{
		monster = GetComponent<Monster>();
	}

	private void Start()
	{
		// Check if what's required exists
		if (nodeManager == null) { nodeManager = GameManager.instance.GetNodeManager(); }
		if (monster == null) { monster = GameManager.instance.GetMonster(); }
		if (player == null) { player = GameManager.instance.GetPlayer(); }
	}

	public void LostPlayer()
	{
		monster.playerLost = true;
        monster.playerLastPos = player.transform.position - new Vector3(0f, player.movement.characterController.height / 2f, 0f);
        monster.headLookIK.DetachTarget();
        monster.movement.GoToDestination(monster.playerLastPos);
	}

    // Get the position of where the player was when monster lost sight of him/her
    public Vector3 GetLastPlayerPos()
    {
        return player.transform.position - new Vector3(0f, player.movement.characterController.height / 2f, 0f);
    }

    public bool IsArrived()
    {
        return Vector3.Distance(monster.playerLastPos, monster.transform.position) < 0.5f;
    }

    void OnDrawGizmos()
    {
        if (Application.isEditor && Application.isPlaying)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawSphere(monster.playerLastPos, 0.2f);
        }
    }
}
