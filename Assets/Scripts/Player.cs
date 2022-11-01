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
using System.Collections;
using UnityEngine;

namespace SchnabelSoftware.Ludo
{
    /// <summary>
	/// 
	/// </summary>
	public class Player : MonoBehaviour
	{
		[Header("Prefabs")]
		[SerializeField] protected DollController dollPrefab = null;

		[Header("Spawn- and Endpoints")]
		[SerializeField] protected Transform[] spawnPoints = null;
		[SerializeField] protected Transform[] endPoints = null;

		[Header("Entry Point")]
		[SerializeField] protected Transform entryPoint = null;

		[Header("Layer Masks")]
		[SerializeField] protected LayerMask dollLayerMask = 0;

        //private SpawnSystem spawnSystem = null;
		protected DollController[] dolls = null;
		protected DollController currentDoll = null;
		protected int entryPointIndex = 0;
		protected int goalIndex = 0;
		protected int diceNumber = 0;
		protected int numberOfDiceTries = 3;
		protected Coroutine nextPlayerCO = null;
		protected bool isMoveFinish = false;

        public DollController DollPrefab => dollPrefab;
		public Transform[] SpawnPoints => spawnPoints;
		public Transform[] EndPoints => endPoints;
		public Transform EntryPoint => entryPoint;
		public int SpawnPointsCount => spawnPoints.Length;
        public int EndPointsCount => endPoints.Length;
		public int EntryPointIndex => entryPointIndex;
		public int EndPointCount => goalIndex + 1;
		public Color TeamColor => dolls[0].TeamColor;

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

		private bool AllAtHome()
		{
			foreach (var doll in dolls)
			{
				if (!doll.IsHome && !doll.IsFinish)
					return false;
			}

			return true;
		}

		public void ResetGameDolls()
		{
            for (int i = 0; i < spawnPoints.Length; i++)
            {
				dolls[i].transform.position = GetSpawnPointAt(i);
				dolls[i].GoToHome();
            }

            goalIndex = endPoints.Length - 1;
			numberOfDiceTries = 3;
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
				numberOfDiceTries = AllAtHome() ? 3 : 1;
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

        protected bool IsStartingPointFree()
        {
            foreach (var doll in dolls)
            {
                if (doll == currentDoll)
                    continue;

                if (doll.IsAtStartingPoint)
                    return false;
            }

            return true;
        }

        protected bool IsStartingPointFree(out DollController dollAtStartingPoint)
        {
            dollAtStartingPoint = null;

            foreach (var doll in dolls)
            {
                if (doll == currentDoll)
                    continue;

                if (doll.IsAtStartingPoint)
                {
                    dollAtStartingPoint = doll;
                    return false;
                }
            }

            return true;
        }

        public void MakeNextMove()
		{
			if (Input.GetKeyDown(KeyCode.Space) && !currentDoll)
			{
				if (numberOfDiceTries == 0)
					return;

				diceNumber = GameManager.Current.GetNextDiceNumber();
				--numberOfDiceTries;

				if (diceNumber == 6 && numberOfDiceTries == 0)
				{
					numberOfDiceTries = 1;
				}

				if (AllAtHome() && numberOfDiceTries == 0)
					isMoveFinish = true;
				else
					isMoveFinish = false;
            }
			else if (Input.GetMouseButtonDown(0) && diceNumber > 0)
			{
				SelectPuppet();
			}
			else if (Input.GetMouseButtonUp(0) && currentDoll)
			{
				GoTo();
			}

            if (isMoveFinish && nextPlayerCO == null && numberOfDiceTries == 0)
            {
                nextPlayerCO = StartCoroutine(SwitchToNextPlayer());
            }
        }

		private IEnumerator SwitchToNextPlayer()
		{
			yield return new WaitForSeconds(2f);
            
            // Switch to the next player
            GameManager.Current.GoToNextPlayer();
            numberOfDiceTries = AllAtHome() ? 3 : 1;

			nextPlayerCO = null;
        }

		public virtual void SelectPuppet()
		{
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

			if (Physics.Raycast(ray, out RaycastHit hit, 500f, dollLayerMask, QueryTriggerInteraction.Ignore))
			{
				if (hit.collider.gameObject.TryGetComponent(out currentDoll))
				{
					if (currentDoll.player != this || currentDoll.IsFinish)
					{
						currentDoll = null;
					}
				}
			}
		}

		public virtual void GoTo()
		{
			if (diceNumber == 6)
			{
				if (currentDoll.IsHome)
				{
					if (IsStartingPointFree())
					{
						// Place the game doll on the starting point of the waypoint system.
						currentDoll.GoToStart();
                        numberOfDiceTries = 1;
                    }
					else
					{
						Debug.Log("You must first release the starting point. Another doll blocks this point.");
						return;
					}
					// Debug.Log($"{currentDoll.name} go to start.");
				}
				else
				{
					// The game doll can move forward six steps on the waypoint system.
					currentDoll.MoveTo(diceNumber);
					//Debug.Log($"{currentDoll.name} moving (6) steps.");
				}
			}
			else
			{
				if (!currentDoll.IsHome && !currentDoll.IsFinish)
				{
					// The game doll is not at home
					// and it can move forward on the waypoint system,
					// which gives the dice in number.
					currentDoll.MoveTo(diceNumber);
					
                    //Debug.Log($"{currentDoll.name} moving {nextSteps} steps.");
                }
				isMoveFinish = true;
                //if (nextPlayerCO == null && numberOfDiceTries == 0)
                //{
                //    nextPlayerCO = StartCoroutine(SwitchToNextPlayer());
                //}
            }

			currentDoll = null;
			diceNumber = 0;
			Debug.Log("Tries: " + numberOfDiceTries);
		}
    }
}
