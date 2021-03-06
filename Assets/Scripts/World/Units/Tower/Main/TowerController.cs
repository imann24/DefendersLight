﻿/*
 * Author(s): Isaiah Mann
 * Description: Controls all the towers in the game
 */

using UnityEngine;
using SimpleJSON;
using System.IO;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.EventSystems;
using System.Text.RegularExpressions;

public class TowerController : UnitController<ITower, Tower, TowerList>, ITowerController {
	public static TowerController Instance;
	public const string TOWER_TAG = "Tower";
	const float DRAG_HEIGHT_OFFSET = 0.5f;
	CameraController gameCamera;

	public GameObject CoreOrbPrefab;
	public GameObject CoreOrbInstance;
	TowerBehaviour potentialPurchaseTower = null;
	MapTileBehaviour previousHighlightedMapTile = null;
	HashSet<TowerBehaviour> activeTowers = new HashSet<TowerBehaviour>();
	Dictionary<TowerType, List<Tower>> towerTemplatesByType = new Dictionary<TowerType, List<Tower>>();
	public Dictionary<TowerType, List<Tower>> ITowerTemplatesByType {
		get {
			return towerTemplatesByType;
		}
	}
	public override void Setup (WorldController worldController, DataController dataController, MapController mapController, string unitTemplateJSONPath) {
		base.Setup(worldController, dataController, mapController, unitTemplateJSONPath);
		towerTemplatesByType = sortTowers(templateUnits.Values.ToArray());
	}

	public void SellTower (Tower tower) {
		worldController.OnSellTower(tower);
	}

	protected override void SubscribeEvents () {
		base.SubscribeEvents ();
		EventController.OnUnitEvent += HandleUnitEvent;
	}

	protected override void UnusbscribeEvents () {
		base.UnusbscribeEvents ();
		EventController.OnUnitEvent -= HandleUnitEvent;
	}

	protected void HandleUnitEvent (string eventName, Unit unit) {
		if (eventName == EventType.TowerDestroyed) {
			_activeUnits.Remove(unit as Tower);
		}
	}
		
	public void PlaceCoreOrb (MapTileBehaviour mapTile) {
		GameObject coreOrb = (GameObject) Instantiate(CoreOrbPrefab);
		CoreOrbInstance = coreOrb;
		CoreOrbBehaviour coreOrbBehaviour = coreOrb.GetComponent<CoreOrbBehaviour>();
		coreOrbBehaviour.Setup(this);
		Tower coreOrbClone = new Tower(templateUnits[CoreOrbBehaviour.CORE_ORB_KEY]);
		coreOrbClone.ResetHealth();
		coreOrbBehaviour.SetTower(coreOrbClone);
		mapTile.PlaceStaticAgent(coreOrbBehaviour, false);
	}

	Dictionary<TowerType, List<Tower>> sortTowers (Tower[] towers) {
		Dictionary<TowerType, List<Tower>> sortedTowers = new Dictionary<TowerType, List<Tower>>();
		for (int i = 0; i < System.Enum.GetNames(typeof(TowerType)).Length; i++) {
			sortedTowers.Add((TowerType)i, new List<Tower>());
		}
		foreach (Tower tower in towers) {
			if (includeTowerInTypes(tower)) {
				sortedTowers[tower.TowerType].Add(tower);
			}
		}
		return sortedTowers;
	}

	bool includeTowerInTypes (Tower tower) {
		return tower.Type != CoreOrbBehaviour.CORE_ORB_KEY;
	}

	public Tower[] GetTowersOfType (TowerType type) {
		List<Tower> towers;
		if (towerTemplatesByType.TryGetValue(type, out towers)) {
			return towers.ToArray();
		} else {
			return new Tower[0];
		}
	}

	public void HandleBeginDragPurchase (PointerEventData dragEvent, TowerPurchasePanel towerPanel) {
		if (potentialPurchaseTower != null) {
			Destroy(potentialPurchaseTower);
		}
		Vector3 startPosition = getDragPosition(dragEvent);
		this.potentialPurchaseTower = GetTowerBehaviourFromTower(towerPanel.GetTower(), startPosition, false);
	}

	Vector3 getPointerWorldPosition (PointerEventData pointerEvent) {
		Vector3 dragPosition = pointerEvent.position;
		dragPosition.z -= Camera.main.transform.position.z;
		return Camera.main.ScreenToWorldPoint(dragPosition);
	}

	Vector3 getTilePosition (Vector3 pointerPosition) {
		RaycastHit hit;
		if (Physics.Raycast(pointerPosition, gameCamera.ICameraDirection, out hit)) {
			return hit.point;
		} else {
			return Vector3.zero;
		}
	}

	public TowerBehaviour GetTowerBehaviourFromTower (Tower tower, Vector3 startPosition, bool shouldStartActive = false) {
		TowerBehaviour potentialPurchaseTower = SpawnTower(tower, startPosition);
		potentialPurchaseTower.Setup(this);
		potentialPurchaseTower.ToggleColliders(shouldStartActive);
		Tower towerClone = new Tower(tower);
		potentialPurchaseTower.SetTower(towerClone);
		potentialPurchaseTower.ToggleActive(shouldStartActive);
		potentialPurchaseTower.OnSpawn();
		if (towerClone.Type.Equals(Tower.CORE_ORB)) {
			CoreOrbInstance = potentialPurchaseTower.gameObject;
		}
		return potentialPurchaseTower;
	}

	public void SpawnTower (Tower tower) {
		tower.SetController(worldController);
		MapTileBehaviour tile = mapController.GetTileFromLocation(tower.Location);
		TowerBehaviour towerBehaviour = GetTowerBehaviourFromTower(tower, tile.GetWorldPosition(), shouldStartActive:true);
		tile.PlaceStaticAgent(towerBehaviour, shouldPlaySound:false);
		towerBehaviour.ToggleActive(true);
	}

	public TowerBehaviour GetPrefab (Tower tower) {
		return loadPrefab(FileUtil.CreatePath(TOWER_TAG, PREFABS_DIR, tower.Type)) as TowerBehaviour;
	}
		
	TowerBehaviour SpawnTower (Tower tower, Vector3 startingPosition) {
		ActiveObjectBehaviour behaviour;
		if (TryGetActiveObject(tower.IType, startingPosition, out behaviour)) {
			return behaviour as TowerBehaviour;
		} else {
			TowerBehaviour prefabInResources = GetPrefab(tower);
			if (prefabInResources) {
				return Instantiate(prefabInResources);
			} else {
				return Instantiate(worldController.GetTowerPrefab(tower.TowerType)).GetComponent<TowerBehaviour>();
			}
		}
	}
		
	Vector3 getDragPosition (PointerEventData dragEvent) {
		Vector3 pointerPosition = getPointerWorldPosition(dragEvent);
		Vector3 towerPosition = getTilePosition(pointerPosition);
		float angle = gameCamera.ICameraAngleRad;
		Vector3 offset = new Vector3(
			0,
			Mathf.Cos(angle) * DRAG_HEIGHT_OFFSET,
			-Mathf.Sin(angle) * DRAG_HEIGHT_OFFSET
		);
		return towerPosition + offset;
	}

	public void HandleDragPurchase (PointerEventData dragEvent, TowerPurchasePanel towerPanel) {
		potentialPurchaseTower.transform.position = getDragPosition(dragEvent);
		HighlightSpotToPlace(potentialPurchaseTower.transform.position);
	}

	void HighlightSpotToPlace (Vector3 dragPosition) {
		RaycastHit hit;
		if (Physics.Raycast(dragPosition, gameCamera.ICameraDirection, out hit)) {
			MapTileBehaviour mapTile;
			if (hit.collider != null && (mapTile = hit.collider.GetComponent<MapTileBehaviour>()) != null) {
				if (previousHighlightedMapTile) {
					previousHighlightedMapTile.Unhighlight();
				}
				if (!mapTile.HasAgent()) {
					mapTile.HightlightToPlace(potentialPurchaseTower.GetComponent<StaticAgentBehaviour>());
					previousHighlightedMapTile = mapTile;
				}
			}
		}
	}

	public void AddActiveTower (TowerBehaviour tower) {
		if (!activeTowers.Contains(tower)) {
			activeTowers.Add(tower);
			_activeUnits.Add(tower.ITower);
		}
	}

	public void RemoveActiveTower (TowerBehaviour tower) {
		if (activeTowers.Contains(tower)) {
			activeTowers.Remove(tower);
			_activeUnits.Remove(tower.ITower);
		}
	}

	public void RefreshIlluminations () {
		// These need to be calculated last (after getting all the other lights)
		List<TowerBehaviour> reflectiveTowers = new List<TowerBehaviour>();
		foreach (TowerBehaviour tower in activeTowers) {
			if (tower.IsReflective) {
				reflectiveTowers.Add(tower);
			}
			else if (tower.HasIllumination) {
				worldController.SendIlluminationToMap(tower, onTowerPlace:true);
			}
		}
		foreach (TowerBehaviour tower in reflectiveTowers) {
			worldController.SendIlluminationToMap(tower, onTowerPlace:true);
		}
	}
		
    public void compareTowerLevels() {
        foreach (Tower tower in templateUnits.Values) {
            if (tower.UnlockLevel == dataController.PlayerLevel) {
				EventController.Event(EventType.TowerUnlocked);
                TowerUnlockedScreen.textToDisplay = "Tower Unlocked.";
            }
        }
    }

    public override void HandleObjectDestroyed (ActiveObjectBehaviour activeObject) {
		base.HandleObjectDestroyed (activeObject);
		activeTowers.Remove(activeObject as TowerBehaviour);
	}

	public void HandleEndDragPurchase (PointerEventData dragEvent, TowerPurchasePanel towerPanel) {
		if (previousHighlightedMapTile && previousHighlightedMapTile.CanPlaceTower()) {
			previousHighlightedMapTile.PlaceStaticAgent(potentialPurchaseTower);
			towerPanel.OnPurchased();
		} else {
			HandleObjectDestroyed(potentialPurchaseTower);
			if (previousHighlightedMapTile) {
				previousHighlightedMapTile.Unhighlight();
				previousHighlightedMapTile = null;
			}
		}
		potentialPurchaseTower = null;
	}

	public void HealAllTowers (){
		for (int i = 0; i < activeTowers.Count; i++) {
			int TowerMaxHealth = activeTowers.ElementAt (i).IMaxHealth;

			activeTowers.ElementAt (i).Heal (TowerMaxHealth);
		}
	}

    public void DestroyAllTowers() {
        
		for (int i = 0; i < activeTowers.Count; i++) {
			TowerBehaviour tower = activeTowers.ElementAt(i) as TowerBehaviour;
			if (!(tower is CoreOrbBehaviour)) {
				tower.Destroy();
			}
		}
		activeTowers.Clear();
		activeTowers.Add(CoreOrbInstance.GetComponent<CoreOrbBehaviour>());
    }

    public void ToggleGodMode() {
        for (int i = 0; i < activeTowers.Count; i++) {
            activeTowers.ElementAt(i).isInvulnerable = !activeTowers.ElementAt(i).isInvulnerable;
        }
    }

	public Tower[] GetAll() {
		throw new System.NotImplementedException();
	}

	protected override void SetReferences() {
		if (SingletonUtil.TryInit(ref Instance, this, gameObject)) {
			_activeUnits = new List<Tower>();
		} else {
			Destroy(gameObject);
		}
	}

	protected override void FetchReferences () {
		base.FetchReferences ();
		gameCamera = CameraController.Instance;
	}

	protected override void CleanupReferences () {
		base.CleanupReferences ();
		Instance = null;
	}

	public Sprite GetTowerSprite (string towerKey) {
		// Remove special characters from tower key (to produce filename)
		Regex rgx = new Regex("[^a-zA-Z0-9 -]");
		towerKey = rgx.Replace(towerKey, "");
		return Resources.Load<Sprite>(Path.Combine(TOWER_TAG, (Path.Combine(SPRITES_DIR, towerKey.ToLower().Replace(" ", string.Empty)))));
	}
}
