﻿/*
 * Author(s): Isaiah Mann
 * Description: Describes the base behaviour of a tower
 */

[System.Serializable]
public class Tower : Unit {
	public Tower (string type, int health, MapLocation location, string description, int cost, int unlockLevel,
		IWorldController worldController, ITowerController towerController,
		int damage = 0, float cooldown = 0, int range = 0, int attackRadius = 0, string illuminationRadius = "0") : 
	base(type, health, damage, cooldown, range, attackRadius, location, description, worldController) {
		this.Cost = cost;
		this.UnlockLevel = unlockLevel;
		this.IlluminationRadius = illuminationRadius;
		this.towerController = towerController;
	}

	public Tower (string jsonText):base(jsonText){}

	#region JSON Keys

	public const string ILLUMINATION_KEY = "Ilumination Range";
	public const string REFLECTIVITIY_KEY = "Reflectivitiy";
	public const string VARIABLE_KEY = "Variable";

	#endregion

	protected const int ATTACK_STAT_DEFAULT = 0;

	protected ITowerController towerController;
	public int Cost;
	public int ICost {
		get {
			return this.Cost;
		}
	}
	public int UnlockLevel;
	public int IUnlockLevel {
		get {
			return this.UnlockLevel;
		}
	}
	public string IlluminationRadius;
	public int IIlluminationRadius {
		get {
			if (IlluminationRadius == VARIABLE_KEY) {
				return CalculateVariableIlluminationRadius();
			} else {
				return int.Parse(IlluminationRadius);
			}
		}
	}

	// Should calculate illumination radius if the tower has reflectivity
	int CalculateVariableIlluminationRadius () {
		throw new System.NotImplementedException();
	}
		

	public override void Copy (Unit unit) {
		base.Copy(unit);
		try {
			Tower towerData = (Tower) unit;
			this.Cost = towerData.Cost;
			this.UnlockLevel = towerData.UnlockLevel;
			this.IlluminationRadius = towerData.IlluminationRadius;
		} catch {
			UnityEngine.Debug.LogWarningFormat("Unable to fully copy unit. Is not of type {0}", this.GetType());
		}
	}

	public override void DeserializeFromJSON(string jsonText) {
		Tower towerData = UnityEngine.JsonUtility.FromJson<Tower>(jsonText);
		Copy(towerData);
	}

	public override void DeserializeFromJSONAtPath(string jsonPath) {

	}
}