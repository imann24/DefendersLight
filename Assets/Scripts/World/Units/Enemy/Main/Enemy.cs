﻿/*
 * Author(s): Isaiah Mann
 * Description: Represents a basic enemy
 */

using UnityEngine;
using System.Collections.Generic;

[System.Serializable]
public class Enemy : Unit {
	#region JSON Keys

	static Dictionary<string, UnityEngine.Sprite> enemySprites = new Dictionary<string, UnityEngine.Sprite>();
	public const string REWARDS_KEY = "Death Rewards";
	const string GRUNT_TYPE = "Undead";

	#endregion

	[System.NonSerialized]
	Sprite sprite;

	public RewardAmount DeathReward;
    public float Speed;
    public int Agression;
	public int Frequency;
	public int Quantity;
	public string FlavorText;
	public int ManaReward;
	public int XPReward;
	public int WaveSpawnedAt;

	public RewardAmount IDeathReward {
		get {
			return new RewardAmount(ManaReward, XPReward);
		}
	}

	public bool IsGruntType {
		get {
			return IType == GRUNT_TYPE;
		}
	}

	public Enemy (string type, int health, int damage, float cooldown, int range, MapLocation location, string description, RewardAmount deathReward,
		IWorldController worldController, float speed, int agro) : 
	base(type, health, damage, cooldown, range, location, description, worldController) {
		this.DeathReward = deathReward;
        this.Speed = speed;
		this.Agression = agro;
	}

	public Enemy (string jsonText):base(jsonText){}

	public Enemy (Enemy fromClone):base(fromClone){}

	public override void Copy (Unit unit) {
		base.Copy(unit);
		try {
			Enemy enemyData = (Enemy) unit;
			this.DeathReward = enemyData.DeathReward;
			this.Speed = enemyData.Speed;
			this.Agression = enemyData.Agression;
			this.Frequency = enemyData.Frequency;
			this.Quantity = enemyData.Quantity;
			this.FlavorText = enemyData.FlavorText;
			this.ManaReward = enemyData.ManaReward;
			this.XPReward = enemyData.XPReward;
			this.WaveSpawnedAt = enemyData.WaveSpawnedAt;
		} catch {
			UnityEngine.Debug.LogWarningFormat("Unable to fully copy unit. Is not of type {0}", this.GetType());
		}
	}

	public override void DeserializeFromJSON(string jsonText) {
		Enemy enemyData = UnityEngine.JsonUtility.FromJson<Enemy>(jsonText);
		Copy(enemyData);
	}

	public override void DeserializeFromJSONAtPath(string jsonPath) {

	}

	public override Sprite GetSprite () {
		Sprite sprite;
		if (enemySprites.TryGetValue(Type, out sprite)) {
			return sprite;
		} else if (WorldController.Instance){ 
			sprite = WorldController.Instance.GetEnemySprite(Type);
			enemySprites.Add(Type, sprite);
			return sprite;
		} else {
			return null;
		}
	}

	public override string GetDescription () {
		return string.Format("{0}\n\n{1}", FlavorText, Description);
	}

}
