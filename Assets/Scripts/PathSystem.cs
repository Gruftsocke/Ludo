/*********************************************************
 * Dateiname: PathSystem.cs
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
using System.Collections.Generic;
using UnityEngine;

namespace SchnabelSoftware.Ludo
{
	/// <summary>
	/// 
	/// </summary>
	[System.Serializable]
	public class PathSystem
	{
		[SerializeField] private List<Transform> paths = new List<Transform>();

		public int Length => paths.Count;

		public Vector3 GetPositionAt(int index)
		{
			int id = Mathf.Clamp(index, 0, paths.Count - 1);
			return paths[id].position;
		}

        public int GetWaypointIndexFrom(Transform waypoint)
        {
            if (waypoint == null || Length == 0)
                return 0;

			for (int i = 0; i < Length; i++)
			{
				if (paths[i] == waypoint)
					return i;
			}

			return 0;
        }
    }
}
