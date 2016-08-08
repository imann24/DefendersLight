﻿/*
 * Author(s): Isaiah Mann
 * Description: Describes the behaviour of a missile or projectile launched by an active object
 */

using UnityEngine;
using System.Collections;

public class ProjectileBehaviour : MobileAgentBehaviour {
	ActiveObjectBehaviour _target;

	protected override void SetReferences() {
		base.SetReferences();
	}

	protected override void FetchReferences() {
		
	}

	protected override void CleanupReferences () {
		
	}

	protected override void HandleNamedEvent (string eventName) {
		
	}

	public void SetTarget (ActiveObjectBehaviour target) {
		this._target = target;
		StartCoroutine(MoveTo(target.gameObject));
	}

	public override ActiveObjectBehaviour SelectTarget () {
		return this._target;
	}

	public override void MoveTo (MapLocation location) {
		
	}
		
	public override void Attack (ActiveObjectBehaviour activeAgent) {
		base.Attack (activeAgent);
	}

	void OnTriggerEnter(Collider other) {
		if (_target == null) {
			return;
		}

		if (other.gameObject == this._target.gameObject) {
			
			Attack(this._target);
			Destroy(gameObject);
		}
	}
}