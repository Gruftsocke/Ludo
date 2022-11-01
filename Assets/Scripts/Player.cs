/*********************************************************
 * Dateiname: Player.cs
 * Projekt  : SchnabelSoftware.Ludo
 * Datum    : 16.08.2022
 *
 * Author   : Daniel Schnabel
 * E-Mail   : info@schnabel-software.de
 *
 * Zweck    : 
 *
 * © Copyright by Schnabel-Software 2009-2022
 */
using UnityEngine;

namespace SchnabelSoftware.Ludo
{
    /// <summary>
	/// 
	/// </summary>
	public class Player : MonoBehaviour
	{
		[Header("Prefabs")]
		[SerializeField] private DollController dollPrefab = null;

		[Header("Spawn- and Endpoints")]
		[SerializeField] private Transform[] spawnPoints = null;
		[SerializeField] private Transform[] endPoints = null;

		[Header("Entry Point")]
		[SerializeField] private Transform entryPoint = null;

		[Header("Layer Masks")]
		[SerializeField] private LayerMask dollLayerMask = 0;

        //private SpawnSystem spawnSystem = null;
		private DollController[] dolls = null;
		private int entryPointIndex = 0;
		private int goalIndex = 0;

        public DollController DollPrefab => dollPrefab;
		public Transform[] SpawnPoints => spawnPoints;
		public Transform[] EndPoints => endPoints;
		public Transform EntryPoint => entryPoint;
		public int SpawnPointsCount => spawnPoints.Length;
        public int EndPointsCount => endPoints.Length;
		public int EntryPointIndex => entryPointIndex;
		public int EndPointCount => goalIndex + 1;

		private void Awake()
		{
			//spawnSystem = new SpawnSystem(this);
			// Spielpuppen
			dolls = new DollController[spawnPoints.Length];

			for (int i = 0; i < spawnPoints.Length; i++)
			{
				dolls[i] = Instantiate(dollPrefab, GetSpawnPointAt(i), Quaternion.identity);
				dolls[i].transform.parent = transform;
				dolls[i].player = this;
				dolls[i].name = name.Replace("Player", "Doll") + i.ToString();
            }

			goalIndex = endPoints.Length - 1;
		}

		private void Start()
		{
			entryPointIndex = GameManager.Current.GetWaypointIndexFrom(entryPoint);
		}

		public void ResetGameDolls()
		{
            for (int i = 0; i < spawnPoints.Length; i++)
            {
				dolls[i].transform.position = GetSpawnPointAt(i);
				dolls[i].GoToHome();
            }

            goalIndex = endPoints.Length - 1;
        }

		public Vector3 GetSpawnPointAt(int index)
        {
            if (index >= 0 && index < spawnPoints.Length)
                return spawnPoints[index].position;

            return Vector3.zero;
        }

		public void SetHomePositionWhere(DollController doll)
		{
			if (doll == null)
				return;

			for (int i = 0; i < dolls.Length; i++)
			{
				if (dolls[i] != doll)
					continue;

				doll.transform.position = GetSpawnPointAt(i);
				break;
            }
		}

		public bool IsWaypointFree(Vector3 worldPosition)
		{
			Ray ray = new Ray(worldPosition + (Vector3.up * 2f), Vector3.down);
			if (Physics.Raycast(ray, out RaycastHit hit, 5f, dollLayerMask))
			{
				if (hit.collider.TryGetComponent(out DollController dollTest))
				{
					//Debug.Log($"Dollname: {dollTest.name}");
					return false;
				}
			}

			return true;
		}

        public Vector3 GetEndPointPositionAt(int endPointIndex)
        {
			if (endPointIndex >= 0 && endPointIndex < endPoints.Length)
			{
				return endPoints[endPointIndex].position;
			}

            return Vector3.zero;
        }
		/// <summary>
		/// Prüft ob der Weg zum Ziel frei ist.
		/// Hier wird das eigentliche Ziel nicht geprüft!
		/// </summary>
		/// <param name="endPointIndex">Zielindex</param>
		/// <returns>Ist wahr, wenn der Weg frei ist.</returns>
        public bool IsWayToEndPointClear(int endPointIndex, int startFromIndex)
        {
            if (endPointIndex >= 0 && endPointIndex <= goalIndex)
            {
				for (int i = startFromIndex; i < endPointIndex; i++)
				{
					bool status = IsWaypointFree(endPoints[i].position);
					Debug.Log($"Status: {status} index: {i}/{endPointIndex}");
                    if (!status)
						return false;
				}
			}

            return true;
        }

		public bool GameDollHasReachedTheGoal(int endPointIndex)
		{
			if (endPointIndex == goalIndex)
			{
				goalIndex--;
				if (goalIndex < 0)
					goalIndex = 0;

				return true;
			}

			return false;
		}
		/// <summary>
		/// Prüft ob der Spieler alle Figuren an das Ziel gebracht hat.
		/// </summary>
		/// <returns>Ist wahr, wenn der Spieler alle Figuren an das Ziel gebracht hat.</returns>
		public bool IsFinish()
		{
			bool result = false;

			foreach (var doll in dolls)
			{
				if (!doll.IsFinish)
					return false;

				result = true;
			}

			return result;
		}
    }
}
