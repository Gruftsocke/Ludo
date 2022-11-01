/*********************************************************
 * Dateiname: SpawnSystem.cs
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
namespace SchnabelSoftware.Ludo
{
	/// <summary>
	/// 
	/// </summary>
	[System.Serializable]
	public class SpawnSystem
	{
		[System.Serializable]
		private struct SpawnPointInfo
		{
			public bool isFree;
			public readonly int spawnID;
			public readonly int dollID;

			public SpawnPointInfo(int spawnID, int dollID, bool isFree)
			{
				this.dollID = dollID;
				this.spawnID = spawnID;
				this.isFree = isFree;
			}
		}

		private SpawnPointInfo[] spawnPointInfos;
		private Player player = null;

		public SpawnSystem(Player player)
		{
			this.player = player;
			if (player == null)
				throw new System.Exception("You have not handed over a player.");

			Setup();
		}

		public void Setup()
		{
			spawnPointInfos = new SpawnPointInfo[player.SpawnPointsCount];

			for (int i = 0; i < player.SpawnPointsCount; i++)
			{
				spawnPointInfos[i] = new SpawnPointInfo(i, i, false);
			}
		}
    }
}
