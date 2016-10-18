﻿/*
 * Author(s): Isaiah Mann
 * Description: Denotes an enemy spawn point
 */

using UnityEngine;

public class EnemySpawnPoint : MannBehaviour {
	public static System.Collections.Generic.Dictionary<Direction, EnemySpawnPoint> SpawnPoints = new System.Collections.Generic.Dictionary<Direction, EnemySpawnPoint>();

	public Direction Location;

	public MapLocation MapLocation {
		get {
			if (currentSpawnPoint) {
				return currentSpawnPoint.GetLocation();
			} else {
				return null;
			}
		}
	}
	public MapTileBehaviour Tile {
		get {
			return currentSpawnPoint;
		}
	}
	MapTileBehaviour currentSpawnPoint;
	MapTileBehaviour[] currentPath;
	MapQuadrant quadrant;

	public static Vector3 GetPosition (Direction spawnLocation) {
		EnemySpawnPoint spawnPoint;
		if (SpawnPoints.TryGetValue(spawnLocation, out spawnPoint)) {
			return spawnPoint.GetPosition();
		} else {
			Debug.LogWarningFormat("Dictionary does not contains direction {0}. Returning zero vector", spawnLocation);
			return Vector3.zero;
		}
	}

	public Vector3 GetPosition () {
		return transform.position;
	}

	public void Setup (MapQuadrant quadrant) {
		this.quadrant = quadrant;
	}

	public void ChooseStartingTile () {
		MapTileBehaviour[] tiles = MapController.Instance.GetQudrantEdges(quadrant);
		currentSpawnPoint = tiles[Random.Range(0, tiles.Length)];
	}

	public void SetPath (MapTileBehaviour[] path) {
		this.currentPath = path;
		Debug.Log(currentPath.Length);
		foreach (MapTileBehaviour tile in path) {
			tile.Highlight();
		}
	}

	public void SpawnEnemies (EnemyBehaviour[] enemies) {
		foreach (EnemyBehaviour enemy in enemies) {
			currentSpawnPoint.PositionMobileAgent(enemy);
		}
	}
		
	protected override void FetchReferences () {
		// NOTHING
	}

	protected override void SetReferences () {
		if (SpawnPoints.ContainsKey(Location)) {
			Debug.LogWarningFormat("Dictionary already contains direction {0}. Spawn Point {1} will not be available", Location, gameObject);
		} else {
			SpawnPoints.Add(Location, this);
		}
	}

	protected override void HandleNamedEvent (string eventName) {
		// NOTHING
	}

	protected override void CleanupReferences () {
		if (SpawnPoints.ContainsValue(this) && SpawnPoints.ContainsKey(Location)) {
			SpawnPoints.Remove(Location);
		}
	}
}
