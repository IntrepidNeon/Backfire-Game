using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GasEngine : Damagable
{
	public AnimationCurve TorqueCurve;

	[Range(0, 1)]
	public float throttle;

	private float _rpm;
	public float RPM { get { return _rpm; } set { _rpm = value; } }

	new private void Awake()
	{
		base.Awake();
	}

	public float EvaluateTorque()
	{
		return Mathf.Max(TorqueCurve.Evaluate(_rpm), 0f) * throttle;
	}
}
