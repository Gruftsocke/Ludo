/*********************************************************
 * Dateiname: SimpleRotate.cs
 * Projekt  : SchnabelSoftware.
 * Datum    : 
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
	public class SimpleRotate : MonoBehaviour
	{
		[SerializeField] private float rotatePerSecond = .15f;

		private const float fullCircle = 360f;

		private void Update()
		{
			transform.Rotate(Vector3.up, rotatePerSecond * (Time.deltaTime * fullCircle));
		}
	}
}
