using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

[RequireComponent(typeof(VehicleController))]
public class EnemyVC : MonoBehaviour
{
	public VehicleController VC;
	public bool navLock = false;

	public Transform frontLeft, frontRight;

	public List<EnemyVC> ally;

	private void Awake()
	{
		VC = GetComponent<VehicleController>();
	}
	private void FixedUpdate()
	{
		//VC.throttle = VC.driveTorqueClamp * Mathf.Clamp(0.5f + (VC.target.position - transform.position).magnitude / 10, 0, 1);
		Navigate();
	}
	public void Navigate()
	{
		foreach (EnemyVC eVC in ally)
		{
			Quaternion dir = Quaternion.LookRotation(eVC.transform.position - transform.position) * Quaternion.Inverse(transform.rotation);

			Vector3 vector = dir * Vector3.forward;

			//Debug.Log(name + " " + vector);

		}
	}

	private void OnTriggerEnter(Collider other)
	{
		EnemyVC eVC;
		if (eVC = other.GetComponentInParent<EnemyVC>())
		{
			if (!ally.Contains(eVC))
			{
				ally.Add(eVC);
			}
		}
	}
	private void OnTriggerExit(Collider other)
	{
		EnemyVC eVC;
		if (eVC = other.GetComponentInParent<EnemyVC>())
		{
			if (ally.Contains(eVC))
			{
				ally.Remove(eVC);
			}
		}
	}
}
