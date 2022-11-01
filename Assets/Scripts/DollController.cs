/*********************************************************
 * Dateiname: DollController.cs
 * Projekt  : SchnabelSoftware.Ludo
 * Datum    : 14.08.2022
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
	public class DollController : MonoBehaviour
	{
        [Header("Doll Properties")]
        [SerializeField] private GameObject ring = null;

        //[Header("Layer Masks")]
        //[SerializeField] private LayerMask dollLayerMask = 0;

        [HideInInspector]
		public Player player = null;

		private int currentWaypointIndex = -1;
		private int maxWaypointSteps = 0;
		private int currentWaypointSteps = 0;
		private bool isHome = true;
		private bool isFinish = false;
		private bool isAtStartingPoint = false;

		private int startIndex = 0;
		private int stepsToMove = 0;
		private int currentMoveStep = 0;
		private float currentTime = 0f;
		private float delay = .5f;
		private bool movement = false;
		private Vector3 nextWP = Vector3.zero;
		private bool isEndGame = false;
		private bool running = false;
		private bool checkIsFinish = false;
		private DollController otherDoll = null;

        public bool IsHome => isHome;
		public bool IsFinish => isFinish;
		public bool IsAtStartingPoint => isAtStartingPoint;
		public Color TeamColor { get; private set; }

		private void Awake()
		{
			TeamColor = transform.Find("Model/Doll").GetComponent<MeshRenderer>().material.color;
		}

		public void GoToStart()
		{
			currentWaypointSteps = 0;
			currentWaypointIndex = player.EntryPointIndex;
			maxWaypointSteps = GameManager.Current.PathLength;
			transform.position = GameManager.Current.GetWaypointPositionAt(currentWaypointIndex);
			isHome = false;
			isFinish = false;
			isEndGame = false;
            checkIsFinish = false;
			isAtStartingPoint = true;

            if (GameManager.Current.TryGetGameDollFromWaypointAt(currentWaypointIndex, out DollController otherDoll))
            {
                if (otherDoll.player != player)
                {
                    // The opposing player's doll is sent home.
                    otherDoll.GoToHome();
                }
            }
        }

		public void GoToHome()
		{
            currentWaypointSteps = 0;
			currentWaypointIndex = -1;
			currentMoveStep = 0;
			currentTime = 0f;

			stepsToMove = 0;
			startIndex = 0;

            running = false;
            movement = false;
			checkIsFinish = false;

            nextWP = Vector3.zero;

            isHome = true;
            isFinish = false;
            isEndGame = false;
            isAtStartingPoint = false;

            player.SetHomePositionWhere(this);
        }

		public void MoveTo(int steps)
		{
			int tempSteps = currentWaypointSteps + steps;
			if (tempSteps < maxWaypointSteps)
			{
				if (!running)
				{
					int goalIndex = GetNextWaypointIndex(currentWaypointIndex + steps - 1);

                    if (GameManager.Current.TryGetGameDollFromWaypointAt(goalIndex, out otherDoll))
					{
						if (otherDoll.player != player)
						{
							startIndex = currentWaypointIndex;
							stepsToMove = steps;

							currentWaypointSteps = tempSteps;
							running = true;

							// The opposing player's doll is sent home.
							otherDoll.GoToHome();
                        }
                    }
					else
					{
                        startIndex = currentWaypointIndex;
                        stepsToMove = steps;

                        currentWaypointSteps = tempSteps;
                        running = true;
                    }

					if (isAtStartingPoint)
						isAtStartingPoint = false;

					Debug.Log($"Current Index {currentWaypointIndex} Goal Index {goalIndex} other {otherDoll}");
				}

            }
			else // End points
			{
				if (!running)
				{
					int endCount = maxWaypointSteps + player.EndPointCount;

					if (tempSteps < endCount)
					{
						int endPointIndex = (tempSteps - maxWaypointSteps);
						//Debug.Log("Go to index: " + endPointIndex);
						if (player.IsWaypointFree(player.GetEndPointPositionAt(endPointIndex)))
						{
							int startFrom = (currentWaypointSteps < maxWaypointSteps) ? 0 : currentWaypointIndex + 1;
							if (player.IsWayToEndPointClear(endPointIndex, startFrom))
							{
                                //Debug.Log($"We go home! start from {startFrom}");
								startIndex = currentWaypointIndex;
								stepsToMove = steps;
								currentWaypointSteps = tempSteps;
								running = true;
                            }
							else
							{
								Debug.Log("The way is blocked!");
							}
						}
						else
						{
							Debug.Log("Your place is blocked!");
						}
					}
					else
					{
						Debug.Log("Your dice count is too high.");
					}
				}
			}
		}

		private int GetNextWaypointIndex(int currentIndex)
		{
            int tempIndex = currentIndex + 1;

			if (tempIndex >= maxWaypointSteps)
				tempIndex -= maxWaypointSteps;

			return tempIndex;
        }

		private void Update()
		{
			if (running && !isFinish)
			{
				if (currentMoveStep < stepsToMove)
				{
					if (movement)
					{
						currentTime += Time.deltaTime / delay;

						if (currentTime <= 1f)
						{
                            transform.position = Vector3.MoveTowards(transform.position, nextWP, currentTime * Time.deltaTime * 5f);
						}
						else
						{
							currentMoveStep++;
							currentTime = 0f;
							movement = false;

                            int currentSteps = ((currentWaypointSteps - stepsToMove) + currentMoveStep);
                            isEndGame = (currentSteps + 1) >= maxWaypointSteps;
							
							if (isEndGame && checkIsFinish)
							{
								isFinish = player.GameDollHasReachedTheGoal(currentWaypointIndex);
							}
	
							//Debug.Log($"Steps: {currentSteps} ,end game: {isEndGame} , finish: {isFinish} , current index: {currentWaypointIndex}");
                        }
					}
					else
					{
						if (isEndGame)
						{
                            currentWaypointIndex = (((currentWaypointSteps - stepsToMove) + currentMoveStep) - (maxWaypointSteps - 1));
                            nextWP = player.GetEndPointPositionAt(currentWaypointIndex);
							checkIsFinish = true;
							
							//Debug.Log($"We go home to index {currentWaypointIndex}");
						}
						else
						{
                            currentWaypointIndex = GetNextWaypointIndex(startIndex);

                            nextWP = GameManager.Current.GetWaypointPositionAt(currentWaypointIndex);
                            startIndex = currentWaypointIndex;
                        }

                        movement = true;
                        ring?.SetActive(true);
                    }
				}
				else
				{
					running = false;
                    movement = false;
					currentMoveStep = 0;

					ring?.SetActive(false);

					if (otherDoll != null)
					{
						otherDoll.GoToHome();
						otherDoll = null;
					}
                }
			}
        } 
	}
}
