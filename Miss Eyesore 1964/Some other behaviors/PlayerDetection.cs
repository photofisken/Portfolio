using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

[RequireComponent(typeof(Monster))]
public class PlayerDetection : MonoBehaviour
{
	private Monster monster;
	private Player player;

    [Tooltip("Layers to be treated as vision obstacles to the monster")]
    [SerializeField] private LayerMask stuffInTheWay;

    public Vector3 lastPlayerPosition = Vector3.zero;

	private float dotProduct;

	private void OnEnable()
	{
		GameManager.InitEvent += Init;
	}

	private void OnDisable()
	{
		GameManager.InitEvent -= Init;
	}

	private void Awake()
	{
		monster = GetComponent<Monster>();
	}

	private void Init()
	{
		player = GameManager.instance.GetPlayer();
		if (player == null) { Debug.LogError("Player reference missing on PlayerDetection"); }
	}

    /// <summary>
    /// Get whether the monster is currently seeing the player
    /// </summary>
    /// <remarks>Also sets a new value to <code>lastPlayerPosition</code> if the player is seen.</remarks>
    /// <returns>True if the player is seen, False otherwise.</returns>
	public bool Detection(bool isChasing)
	{
		// Get position of monster's head and the player position
		Vector3 playerPosition = GameManager.instance.GetCamera().transform.position;
		Vector3 monsterHeadPosition = monster.head.position;

		// Get vector towards player Position and forward vector
		Vector3 playerVector = playerPosition - monsterHeadPosition;

		Vector3 forwardVector = Vector3.ProjectOnPlane(monster.head.forward, Vector3.up);

		forwardVector = transform.InverseTransformDirection(forwardVector);
		forwardVector.z = Mathf.Clamp01(forwardVector.z);
		forwardVector = transform.TransformDirection(forwardVector);

		// Check if player is in sight
		dotProduct = Vector3.Dot(forwardVector.normalized, playerVector.normalized);
        bool stuffInTheWay = StuffInTheWay(playerPosition, forwardVector, isChasing);

        // If nothing is in the way of the linecast to the player, and the player is in the monster's vision - return detected == true;
		if (!stuffInTheWay && dotProduct >= monster.visionWidth)
		{
			Debug.DrawLine(monster.head.position, playerPosition, Color.red);
            lastPlayerPosition = playerPosition;
			return true;
		}
		else if (!stuffInTheWay && Vector3.Distance(playerPosition, monsterHeadPosition) <= monster.visionRadius)
		{
			Debug.DrawLine(monster.head.position, playerPosition, Color.red);
            lastPlayerPosition = playerPosition;
			return true;
		}
		else
		{
			return false;
		}
	}

	private bool StuffInTheWay(Vector3 playerPosition, Vector3 forwardVector, bool isChasing)
	{
		// Get the mask for player and walls and invert them
		RaycastHit hit;

		// Does the ray intersect player or wall
		LayerMask mask;
		if (isChasing)
		{
            // If the player is chased, only walls will hide him/her
			mask = stuffInTheWay | (1 << LayerMask.NameToLayer("Player"));
		}
		else
		{
            // If the player isn't actively chased, the player will not be seen in hiding places as well as behind walls.
			mask = stuffInTheWay | (1 << LayerMask.NameToLayer("Hiding Object")) | (1 << LayerMask.NameToLayer("Player"));
		}

		if (Physics.Linecast(monster.head.position, playerPosition, out hit, mask.value))
		{
			if (((1 << hit.transform.gameObject.layer) & stuffInTheWay) != 0)
			{
				Debug.DrawLine(monster.head.position, hit.point, Color.yellow);
				return true;
			}
			else if (hit.transform.gameObject.layer == LayerMask.NameToLayer("Player"))
			{
				Debug.DrawLine(monster.head.position, GameManager.instance.GetCamera().transform.position, Color.blue);
				return false;
			}
			else
			{
				Debug.DrawLine(monster.head.position, monster.head.position + monster.head.forward);
				return true;
			}
		}
		else
		{
			Debug.DrawLine(monster.head.position, forwardVector * 10, Color.white);
			//Debug.Log("Did not Hit");
			return true;
		}
	}

#if UNITY_EDITOR
    // The vision gizmo that doesn't work
    private void OnDrawGizmosSelected()
	{
		if (Application.isEditor && Application.isPlaying)
		{
			GUIStyle style = new GUIStyle();
			style.alignment = TextAnchor.MiddleCenter;
			style.fontSize = 20;
			Handles.Label(transform.position, (Mathf.Round(dotProduct * 100f)/100f).ToString(), style);
			/*
			float angle = Mathf.Rad2Deg * Mathf.Acos(1f - monster.visionWidth);
			float angleOffset = 360f / coneResolution;

			
			for (int i = 0; i < coneResolution; i++)
			{
				
				Vector3 forwardVector = Vector3.ProjectOnPlane(monster.head.forward, Vector3.up);

				forwardVector = transform.InverseTransformDirection(forwardVector);
				forwardVector.z = Mathf.Clamp01(forwardVector.z);
				forwardVector = transform.TransformDirection(forwardVector);

				forwardVector = Quaternion.Euler(new Vector3(0f, angle, 0f)) * forwardVector;
				forwardVector = Quaternion.Euler(new Vector3(angleOffset * i, 0f, 0f)) * forwardVector;

				//forwardVector = Quaternion.LookRotation(Vector3.ProjectOnPlane(monster.head.forward, Vector3.up), Vector3.up) * forwardVector;
				//forwardVector = Quaternion.Euler(new Vector3(0f, -90f, 0f)) * forwardVector;

				Gizmos.color = Color.cyan;

				Gizmos.DrawLine(monster.head.position, monster.head.position + forwardVector * coneLength);
			}
			*/

			// Flat WireSphere Matrix
			//Matrix4x4 flatMatrix = Matrix4x4.TRS(transform.position, transform.rotation, new Vector3(1f, 0f, 1f));
			//Gizmos.matrix = flatMatrix;
			//Gizmos.color = Color.white;
			Gizmos.DrawWireSphere(transform.position, monster.visionRadius);
			//Gizmos.matrix = Matrix4x4.identity;
		}
	}
#endif
}
