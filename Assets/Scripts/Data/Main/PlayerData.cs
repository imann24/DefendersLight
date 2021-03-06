﻿/*
 * Author(s): Isaiah Mann
 * Description: Stores data about the player
 */

[System.Serializable]
public class PlayerData : IPlayerData {

	const int DEFAULT_XP = 0;
	const int DEFAULT_LEVEL = 1;
	const int DEFAULT_HIGHEST_WAVE = 0;

	MathEquation xpEquation;

	int _xp;
	int _level;
	int _highestWave;
	string _filePath;
	public int IXP {
		get {
			return _xp;
		}
	}
	public int ILevel{
		get {
			return _level;
		}
	}
	public int IXPForLevel {
		get {
			if (xpEquation != null) {
				return xpEquation.CalculateAsInt(_level);
			} else {
				return 0;
			}
		}
	}

	public int IHighestWave {
		get {
			return _highestWave;
		}
	}
	public string IFilePath {
		get {
			return _filePath;
		}
		set {
			setFilePath(value);
		}
	}

	public PlayerData (string filePath) {
		setFilePath(filePath);
		Reset();
	}

	public PlayerData (string filePath, int xp, int level, int highestWave) {
		setFilePath(filePath);
		this._xp = xp;
		this._level = level;
		this._highestWave = highestWave;
	}

	public void Reset () {
		this._xp = DEFAULT_XP;
		this._level = DEFAULT_LEVEL;
		this._highestWave = DEFAULT_HIGHEST_WAVE;
	}

	public bool HasDataToSave () {
		return !(this._xp == DEFAULT_XP &&
			this._level == DEFAULT_LEVEL &&
			this._highestWave == DEFAULT_HIGHEST_WAVE);
	}
		
	public void SetXPEquation (MathEquation equation) {
		this.xpEquation = equation;
	}

	public void EarnXP (int xpEarned) {
		this._xp += xpEarned;
	}

    // Returns new player level
    public int LevelUp () {
		if (ReadyToLevelUp()) {
			this._xp -= IXPForLevel;
			this._level++;
			LevelUpRewardScreenController.Instance.toggleScreen();
        	TowerController.Instance.compareTowerLevels();
        }	
		return this._level;
	}

	public void LevelUpCheat() {
        this._level++;
		this._xp = 0;
        TowerController.Instance.compareTowerLevels();
        LevelUpRewardScreenController.Instance.toggleScreen();

    }

	public bool ReadyToLevelUp () {
		return IXPForLevel <= this._xp;
	}

	public bool NewHighestWave (int waveReached) {
		return waveReached > this._highestWave;
	}

	public void UpdateHighestWave (int waveReached) {
		this._highestWave = waveReached;
	}

	void setFilePath (string filePath) {
		this._filePath = filePath;
	}
}
