using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class EnemyDriver : VehicleDriver
{

	private enum EnemyNavState { Follow, Charge, Broadside};
	private void FixedUpdate()
	{
		Navigate();
	}
	public void Navigate()
	{
		vc.throttle = 1f;
		VehicleController targetController = target.GetComponent<VehicleController>();
		if (targetController)
		{
			if( 1 > (target.position - transform.position).magnitude / 8f || _rb.velocity.magnitude > targetController.Velocity.magnitude)
			vc.throttle = 0f;
			navPath = new();
			if (NavMesh.CalculatePath(transform.position, target.position + target.transform.right * 2f + target.transform.forward * 2f, NavMesh.AllAreas, navPath))
			{
				vc.desiredSteerAngle = SteerAngleToTarget(GetPointAlongPolyLine(4f, navPath.corners) - transform.position);
			}

		}
	}

	private void OnDrawGizmos()
	{
		Vector3 tVec = target.transform.position - transform.position;

		Vector3 midPos = Vector3.Lerp(target.transform.position, transform.position, 12f / tVec.magnitude);

		Debug.DrawLine(target.transform.position, midPos,Color.green);
		Debug.DrawLine(transform.position, midPos,Color.red);
	}
}
