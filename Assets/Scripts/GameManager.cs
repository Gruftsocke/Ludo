/*********************************************************
 * Dateiname: GameManager.cs
 * Projekt  : SchnabelSoftware.Ludo
 * Datum    : 14.08.2022
 *
 * Author   : Daniel Schnabel
 * E-Mail   : info@schnabel-software.de
 *
 * Zweck    : 
 *
 * © Copyright by Katersoft 2009-2022
 */
using UnityEngine;

namespace SchnabelSoftware.Ludo
{
    /// <summary>
	/// 
	/// </summary>
	public class GameManager : MonoBehaviour
	{
        #region Singleton
        private static GameManager current = null;
		public static GameManager Current => current;
		#endregion // End of Singleton
		[Header("Players")]
		[SerializeField] private Player[] players = null;

		[Header("Layer Mask")]
		[SerializeField] private LayerMask dollLayerMask = 0;

		[Header("Path Systems")]
		[SerializeField] private PathSystem pathSystem = null;

		public int PathLength => pathSystem != null ? pathSystem.Length : 0;

        private void Awake()
		{
			current = this;
		}

		private void Start()
		{

		}

		private DollController currentDoll = null;
		private int nextSteps = 0;

		private void Update()
		{
			if (Input.GetKeyDown(KeyCode.Alpha6) && !currentDoll)
			{
				nextSteps = 6;
			}
			else if (Input.GetKeyDown(KeyCode.Alpha1) && !currentDoll)
            {
                nextSteps = 38;
            }
            else if (Input.GetKeyDown(KeyCode.Space) && !currentDoll)
			{
				// Virtual dice which gives us the number for the next steps.
				nextSteps = Random.Range(1, 7);
                Debug.Log($"Next try is {nextSteps} steps.");
            }
			else if (Input.GetMouseButtonDown(0) && nextSteps > 0)
			{
				Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

				if (Physics.Raycast(ray, out RaycastHit hit, 500f, dollLayerMask, QueryTriggerInteraction.Ignore))
				{
					//Debug.Log(hit.collider.name);
					currentDoll = hit.collider.gameObject.GetComponent<DollController>();
                }
			}
			else if (Input.GetMouseButtonUp(0) && currentDoll)
			{
				if (nextSteps == 6)
				{
					if (currentDoll.IsHome)
					{
						// Place the game doll on the starting point of the waypoint system.
						currentDoll.GoToStart();
                        // Debug.Log($"{currentDoll.name} go to start.");
                    }
					else
					{
						// The game doll can move forward six steps on the waypoint system.
						currentDoll.MoveTo(nextSteps); 
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
                        currentDoll.MoveTo(nextSteps);
                        //Debug.Log($"{currentDoll.name} moving {nextSteps} steps.");
                    }
                }
				
				currentDoll = null;
				nextSteps = 0;
            }
		}

		private bool CheckForDoll(Vector3 worldPosition)
		{
            Ray ray = new Ray(worldPosition + (Vector3.up * 2f), Vector3.down);
            if (Physics.Raycast(ray, out RaycastHit hit, 5f, dollLayerMask))
            {
                if (hit.collider.GetComponent<DollController>() != null)
                {
                    //Debug.Log($"Dollname: {dollTest.name}");
                    return false;
                }
            }

            return true;
        }

		public bool IsWaypointFree(int index)
		{
			if (pathSystem != null)
				return CheckForDoll(pathSystem.GetPositionAt(index));

			return true;
		}

		public bool TryGetGameDollFromWaypointAt(int waypointIndex, out DollController other)
		{
			other = null;

			if (pathSystem != null)
			{
				Ray ray = new Ray(pathSystem.GetPositionAt(waypointIndex) + (Vector3.up * 2f), Vector3.down);
				if (Physics.Raycast(ray, out RaycastHit hit, 5f, dollLayerMask))
				{
					if (hit.collider.TryGetComponent(out other))
					{
						//Debug.Log($"Dollname: {dollTest.name}");
						
						return true;
					}
				}
			}

            return false;
        }


		public int GetWaypointIndexFrom(Transform entryPoint)
		{
			if (pathSystem != null)
				return pathSystem.GetWaypointIndexFrom(entryPoint);

			return 0;
		}

        public Vector3 GetWaypointPositionAt(int index)
        {
			if (pathSystem != null)
				return pathSystem.GetPositionAt(index);

            return Vector3.zero;
        }

		public void ResetGame()
		{
			foreach (var player in players)
			{
				player.ResetGameDolls();
			}
		}
    }
}
