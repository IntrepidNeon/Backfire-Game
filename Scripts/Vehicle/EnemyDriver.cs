using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AI;

public class EnemyDriver : VehicleDriver
{

	private enum EnemyNavState
	{
		//Default State
		Follow,
		//Defensive State
		Evade,
		Distance,
		//Offensive State
		Charge,
		Broadside
	};

	private EnemyNavState _navState = EnemyNavState.Follow;
	private void FixedUpdate()
	{
		speed = _rb.velocity.magnitude;
		Navigate();
	}
	public void Navigate()
	{
		VehicleController targetController = target.GetComponent<VehicleController>();
		if (targetController)
		{
			//if (1 > (target.position - transform.position).magnitude / 8f || _rb.velocity.magnitude > targetController.Velocity.magnitude)
			//	vc.throttle = 0f;
			Follow(targetController);
			//navPath = new();
			//if (NavMesh.CalculatePath(transform.position, target.position + target.transform.right * 2f + target.transform.forward * 2f, NavMesh.AllAreas, navPath))
			//{
			vc.desiredSteerAngle = SteerAngleToTarget(GetPointAlongPolyLine(4f, navPath.corners) - transform.position);
			//}

		}
	}
	public void ChangeStates()
	{
		switch (Random.Range(0, 3))
		{
			case 0:
				_navState = EnemyNavState.Follow;
				break;
			case 1:
				_navState = EnemyNavState.Charge;
				break;
			case 2:
				_navState = EnemyNavState.Broadside;
				break;
		}
	}

	private bool Follow(VehicleController targetController)
	{
		NavMeshPath path = new();
		float pathLength = Mathf.Infinity;

		if (NavMesh.CalculatePath(transform.position, targetController.transform.position, NavMesh.AllAreas, path))
			pathLength = GetPolyLineLength(path.corners);

		if (path.status == NavMeshPathStatus.PathComplete)
		{
			navPath = path;
		}
		else return false;

		//if we are X meters away, match target's speed. Otherwise accelerate if further and decelerate if closer.
		targetSpeed = pathLength / 6f * targetController.Velocity.magnitude;

		return true;
	}

	private bool BroadSide(VehicleController targetController)
	{
		float
			pathLengthL = Mathf.Infinity,
			pathLengthR = Mathf.Infinity,
			pathLength,
			fVel = Vector3.Dot(_rb.velocity, transform.forward),
			targetFVel = Vector3.Dot(targetController.Velocity, targetController.transform.forward),
			targetRVel = Vector3.Dot(targetController.Velocity, targetController.transform.right);

		NavMeshPath
			pathL = new(),
			pathR = new();

		Vector3
			forwardVector = targetController.transform.forward * 1.5f,
			rightVector = targetController.transform.right * 2f,
			pathTargetL = forwardVector - rightVector,
			pathTargetR = forwardVector + rightVector;


		if (NavMesh.CalculatePath(transform.position, targetController.transform.position + pathTargetL, NavMesh.AllAreas, pathL))
			pathLengthL = GetPolyLineLength(pathL.corners);

		if (NavMesh.CalculatePath(transform.position, targetController.transform.position + pathTargetR, NavMesh.AllAreas, pathR))
			pathLengthR = GetPolyLineLength(pathR.corners);

		if (pathLengthL < pathLengthR && pathL.status == NavMeshPathStatus.PathComplete)
		{
			navPath = pathL;
			pathLength = pathLengthL;
		}
		else if (pathR.status == NavMeshPathStatus.PathComplete)
		{
			navPath = pathR;
			pathLength = pathLengthR;
		}
		else return false;

		//vc.throttle = Mathf.Clamp01(pathLength - fVel);
		//vc.brake = Mathf.Clamp01(-pathLength + fVel);

		targetSpeed = pathLength / 3f * targetController.Velocity.magnitude;

		return true;

	}

	private void OnDrawGizmos()
	{
		Vector3 tVec = target.transform.position - transform.position;

		Vector3 midPos = Vector3.Lerp(target.transform.position, transform.position, 12f / tVec.magnitude);

		Debug.DrawLine(target.transform.position, midPos, Color.green);
		Debug.DrawLine(transform.position, midPos, Color.red);

		if (navPath == null) return;
		for (int i = 1; i < navPath.corners.Length; i++)
		{
			Debug.DrawLine(navPath.corners[i - 1], navPath.corners[i], Color.blue);
		}
	}
}
