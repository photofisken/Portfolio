using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine.Rendering.PostProcessing;

namespace Navigation
{
	[System.Serializable]
	public class RoomVolume
	{
		public Vector3 position = Vector3.zero;
		public Vector3 rotation = Vector3.zero;
		public Vector3 scale = Vector3.one;
	}

	public enum RoomType
	{
		Standard, Corridor
	}

	public enum RoomEnvironment
	{
		Cold, Hot, Freezing
	}

	[ExecuteInEditMode]
	public class Room : MonoBehaviour
	{
		public string roomName = "New Room";
		public RoomType type = RoomType.Standard;
		public RoomEnvironment environment = RoomEnvironment.Cold;
		public bool isRadioactive;
		private string oldName;
		public Color roomColor = Color.white;
		[Space]
		public bool fetchNodesInVolume;
		public bool generateNewColor;
		[Space]
		public List<Node> nodes = new List<Node>();
		public List<Node> interestNodes = new List<Node>();
		public List<Node> exitNodes = new List<Node>();
		public List<RoomVolume> volumes = new List<RoomVolume>();
		[HideInInspector] public List<BoxCollider> spawnedColliders = new List<BoxCollider>();

		[HideInInspector] public bool isWorld;

		[Header("Reflections")]
		[SerializeField] private bool useReflections;
		[SerializeField] private GameObject reflectionProbeObject;
		[SerializeField] private ReflectionProbe reflectionProbe;
		[SerializeField] private ReflectionProbeOptions options;
		[SerializeField] private bool copyScale;

		private void Start()
		{
			if (isWorld)
				return;

			roomColor = Random.ColorHSV(0f, 1f, 0.5f, 1f);

			oldName = transform.name;
			if (volumes.Count < 1)
			{
				RoomVolume volume = new RoomVolume
				{
					scale = new Vector3(5f, 3f, 5f),
					position = new Vector3(0f, 1f, 0f)
				};
				volumes.Add(volume);
			}
			if(Application.isPlaying)
			{
				for(int i = 0; i < volumes.Count; i++)
				{
					GameObject obj = new GameObject();
					obj.name = roomName + " Collider " + i;
					obj.transform.position = transform.position + volumes[i].position;
					obj.transform.rotation = Quaternion.Euler(volumes[i].rotation);
					obj.transform.parent = transform;
					obj.gameObject.layer = LayerMask.NameToLayer("Post Processing Volumes");

					BoxCollider col = obj.AddComponent<BoxCollider>();
					col.size = volumes[i].scale;
					col.isTrigger = true;
					col.tag = "Room";
					spawnedColliders.Add(col);

					NodeManager nodeManager = GameManager.instance.GetNodeManager();
					PostProcessVolume postProcessVolume = obj.AddComponent<PostProcessVolume>();
					postProcessVolume.profile = nodeManager.GetProfile(environment);
					postProcessVolume.priority = Random.Range(nodeManager.profilePriorityMinMax.x, nodeManager.profilePriorityMinMax.y);
					postProcessVolume.blendDistance = nodeManager.profileBlendDistance;
				}
			}
		}

		private void OnDisable()
		{
			if (isWorld)
				return;

			if(oldName == "")
			{
				oldName = "Nav Node";
			}
			transform.name = oldName;
		}

		private void OnDestroy()
		{
			if (isWorld)
				return;

			if (oldName == "")
			{
				oldName = "Nav Node";
			}
			transform.name = oldName;
		}

		private void Update()
		{
			if (Application.isEditor)
			{
				if (isWorld)
					return;

				if (transform.name != roomName)
				{
					transform.name = roomName;
				}

				if (fetchNodesInVolume)
				{
					FetchNodesInRoom();
					fetchNodesInVolume = false;
				}

				if (generateNewColor)
				{
					GenerateNewColor();
					generateNewColor = false;
				}

#if UNITY_EDITOR
				if (useReflections && volumes.Count > 0)
				{
					if (reflectionProbeObject == null)
					{
						if (transform.childCount != 0 && transform.GetChild(0).GetComponent<ReflectionProbe>() != null)
						{
							reflectionProbeObject = transform.GetChild(0).gameObject;
							reflectionProbe = reflectionProbeObject.GetComponent<ReflectionProbe>();
						}
						else
						{
							reflectionProbeObject = new GameObject(roomName + " Reflection Probe");
							reflectionProbeObject.transform.parent = transform;
							reflectionProbe = reflectionProbeObject.AddComponent<ReflectionProbe>();
							reflectionProbeObject.isStatic = true;
						}
					}
					if(reflectionProbe == null && transform.childCount != 0)
					{
						reflectionProbe = GetComponent<ReflectionProbe>();
					}

					reflectionProbeObject.transform.localPosition = volumes[0].position;
					reflectionProbeObject.transform.localRotation = Quaternion.Euler(volumes[0].rotation);

					if(reflectionProbe != null && copyScale)
					{
						reflectionProbe.size = volumes[0].scale;
					}
					if (reflectionProbe != null && options != null)
					{
						reflectionProbe.boxProjection = options.boxProjection;
						reflectionProbe.blendDistance = options.blendDistance;
						reflectionProbe.resolution = options.resolution;
						reflectionProbe.hdr = options.hdr;
						reflectionProbe.shadowDistance = options.shadowDistance;
						reflectionProbe.nearClipPlane = options.nearPlane;
						reflectionProbe.farClipPlane = options.farPlane;
					}
				}
#endif
			}
		}

		public void FetchNodesInRoom()
		{
			if (isWorld)
				return;

			nodes.Clear();
			interestNodes.Clear();
			exitNodes.Clear();
			Node[] allNodes = FindObjectsOfType<Node>();

			foreach(Node node in allNodes)
			{
				foreach(RoomVolume volume in volumes)
				{
					Vector3 relativePosition = node.transform.position - (transform.position + volume.position);
					Vector3 p = Quaternion.Euler(-volume.rotation) * relativePosition;
					if (
						(p.x <= volume.scale.x / 2f && p.x >= -volume.scale.x / 2f) &&
						(p.y <= volume.scale.y / 2f && p.y >= -volume.scale.y / 2f) &&
						(p.z <= volume.scale.z / 2f && p.z >= -volume.scale.z / 2f) && 
						!nodes.Contains(node))
					{
						nodes.Add(node);

						if(node.type == NodeType.interest)
						{
							interestNodes.Add(node);
						}

						if (node.type == NodeType.exit) {
							exitNodes.Add(node);
						}
						break;
					}
				}
			}
		}

		private void GenerateNewColor()
		{
			roomColor = Random.ColorHSV(0f, 1f, 0.5f, 1f);
		}

#if UNITY_EDITOR
		private void OnDrawGizmosSelected()
		{
			if (NodeManager.drawRoomVolumes)
			{
				Gizmos.matrix = Matrix4x4.identity;
				Gizmos.color = roomColor;
				Gizmos.color = new Color(Gizmos.color.r, Gizmos.color.g, Gizmos.color.b, 0.3f);
				Gizmos.DrawSphere(transform.position, 0.3f);
				foreach (RoomVolume volume in volumes)
				{
					Matrix4x4 rotationMatrix = Matrix4x4.TRS(transform.position + volume.position, Quaternion.Euler(volume.rotation), volume.scale);
					Gizmos.matrix = rotationMatrix;
					Gizmos.DrawWireCube(Vector3.zero, Vector3.one);
					Gizmos.DrawCube(Vector3.zero, Vector3.one);
				}
			}

			Gizmos.color = new Color(Gizmos.color.r, Gizmos.color.g, Gizmos.color.b, 1f);

			if (NodeManager.drawRoomLines || NodeManager.drawRoomNodes)
			{
				foreach (Node node in nodes)
				{
					if (NodeManager.drawRoomLines)
					{
						Gizmos.color = roomColor;
						Gizmos.matrix = Matrix4x4.identity;
						Gizmos.DrawLine(transform.position + Vector3.up, node.transform.position);
					}

					if (NodeManager.drawRoomNodes)
					{
						Gizmos.color = roomColor;
						Matrix4x4 rotationMatrix = Matrix4x4.TRS(node.transform.position, node.transform.rotation, new Vector3(1f, 0f, 1f));
						Gizmos.matrix = rotationMatrix;
						Gizmos.DrawWireSphere(Vector3.zero, 0.5f);
					}
				}
			}
			if (NodeManager.drawRoomNames && volumes.Count > 0)
			{
				GUIStyle style = new GUIStyle();
				style.normal.textColor = roomColor;
				style.alignment = TextAnchor.MiddleCenter;
				style.fontSize = 20;
				Handles.Label(transform.position + volumes[0].position, roomName, style);
			}
		}
#endif
	}
}