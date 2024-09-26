using Palmmedia.ReportGenerator.Core.CodeAnalysis;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(VehicleController))]
public class VehicleDriver : MonoBehaviour
{
	public Transform target;

	public VehicleController vc;

	private Rigidbody _rb;


	private Vector3 _steerTarget;

	private float recklessness;

	private float _width;

	private RaycastHit obstacleHitInfo;
	private bool obstacleHit;

	private void Awake()
	{
		vc = GetComponent<VehicleController>();
		_rb = GetComponent<Rigidbody>();

		float maxWidth = 0;
		foreach (AxleInfo axle in vc.axleInfos)
		{
			maxWidth = Mathf.Max((axle.leftWheel.collider.transform.position - axle.rightWheel.collider.transform.position).magnitude, maxWidth);
		}

		_width = maxWidth + 0.5f;
	}
	private void FixedUpdate()
	{
		vc.throttle = 1f;
		BasicObstacleAvoidance();
	}

	private void Navigate()
	{
		if (!target)
		{
			vc.throttle = 0;
			vc.brake = Mathf.Clamp01(_rb.velocity.magnitude / 20f);
			vc.desiredSteerAngle = 0;
			return;
		}

	}

	private void BasicObstacleAvoidance()
	{
		const float castDistance = 8f;

		obstacleHitInfo = default;
		RaycastHit[] hits = Physics.BoxCastAll(transform.position, new(_width / 2f, 0.001f, 0.001f), transform.forward, transform.rotation, castDistance, Physics.DefaultRaycastLayers, QueryTriggerInteraction.Ignore);

		float maxDistance = Mathf.Infinity;

		int hitIndex = 0;
		obstacleHit = false;

		for (int i = 0; i < hits.Length; i++)
		{
			if (hits[i].point == Vector3.zero ||
				new Vector2(hits[i].normal.x, hits[i].normal.z).magnitude < hits[i].normal.y ||
				hits[i].transform.IsChildOf(transform.root))
				continue;

			if (hits[i].distance < maxDistance)
			{
				hitIndex = i;
				maxDistance = hits[i].distance;
				obstacleHit = true;
			}
		}

		if (obstacleHit)
		{
			obstacleHitInfo = hits[hitIndex];
			float influence = 1f - Mathf.Clamp01(obstacleHitInfo.distance / maxDistance);
		}

		if (!obstacleHitInfo.Equals(default(RaycastHit)))
		{
			//Vector3 hitCenter = transform.position + transform.InverseTransformDirection(hitInfo.point - transform.position).z * transform.forward;
			//Debug.DrawLine(transform.position, hitCenter, Color.blue);
			//Vector3 offset = transform.right * _width / 2f;
			//Debug.DrawLine(hitCenter - offset, hitCenter + offset, Color.blue);

			/*

			vc.throttle = Mathf.Clamp(vc.throttle, 0f, 1 - Mathf.Clamp01((influence - 0.5f) / 0.5f));

			//Clamp the steering angle toward the direction of the steering target
			float
				angleOffset = influence * vc.maxSteerAngle,
				preferredSign = Mathf.Sign(transform.InverseTransformDirection(_steerTarget - transform.position).x),
				steeringMin = preferredSign > 0f ? 0f + angleOffset : -vc.maxSteerAngle,
				steeringMax = preferredSign > 0f ? vc.maxSteerAngle : 0f - angleOffset;

			vc.desiredSteerAngle = Mathf.Clamp(vc.desiredSteerAngle, steeringMin, steeringMax);*/
		}
	}

	private Vector3 NodeSteeringTarget()
	{
		NavNode targetNode = target.GetComponent<NavNode>();

		Vector3 nextVector, halfVector;

		float position;

		if (targetNode && targetNode.next)
		{
			nextVector = (targetNode.next.transform.position - targetNode.transform.position).normalized;

			halfVector = (transform.forward + nextVector).normalized;

			position = (transform.position - target.position).magnitude - _rb.velocity.magnitude - targetNode.sphereCollider.radius;

			return target.position - halfVector * Mathf.Clamp(position, -16f, 16f);
		}

		else
		{
			return target.position;
		}
	}

	private Vector3 SmartTarget(Vector3 targetPos, Vector3 futureVector, float position)
	{
		Vector3 halfVector = (transform.forward + futureVector).normalized;

		return targetPos - halfVector * Mathf.Clamp(position, -16f, 16f);
	}

	private void OnDrawGizmos()
	{
		if (obstacleHit)
		{
			Color orange = new(1f, 0.5f, 0f);
			Vector3 rayEnd = transform.position + transform.forward * obstacleHitInfo.distance;
			Debug.DrawLine(transform.position, rayEnd, orange);
			Vector3 offset = _width / 2 * transform.right;
			Debug.DrawLine(rayEnd - offset, rayEnd + offset, orange);
			Debug.DrawRay(obstacleHitInfo.point, obstacleHitInfo.normal, Color.blue);
			Gizmos.color = Color.blue;
			Gizmos.DrawSphere(obstacleHitInfo.point, 0.25f);
		}
	}

	/*
	else if (targetBody)
		{
			nextVector = targetBody.velocity;

			halfVector = (transform.forward + nextVector).normalized;

			position = (transform.position - target.position).magnitude - _rb.velocity.magnitude - targetBody.velocity.magnitude;

			return target.position - halfVector * Mathf.Clamp(position, -16f, 16f);
		}
	 */

	/*private void OnTriggerEnter(Collider other)
	{
		if (other.GetComponentInParent<EnemyVC>())
		{
			EnemyVC enemyVC = other.GetComponentInParent<EnemyVC>();
			Transform desiredTarget = vc.desiredSteerAngle < 0 ? targetA : targetB;
			if (enemyVC.VC.target != A || enemyVC.VC.target != B)
			{
				enemyVC.VC.target = FilterTarget(enemyVC, desiredTarget);
			}
		}
	}
	/*private Transform FilterTarget(EnemyVC e, Transform t)
	{
		Transform o;
		if (A == null && t == targetA)
		{
			A = e;
			o = targetA;
		}
		else if (B == null && t == targetB)
		{
			B = e;
			o = targetB;
		}
		else
		{
			o = vc.transform;
		}
		e.navLock = true;
		return o;
	}
	private void OnTriggerExit(Collider other)
	{
		if (other.GetComponentInParent<EnemyVC>())
		{
			EnemyVC enemyVC = other.GetComponentInParent<EnemyVC>();
			if (enemyVC == A) { A = null; }
			if (enemyVC == B) { B = null; }
			enemyVC.navLock = false;
			other.GetComponentInParent<EnemyVC>().VC.target = transform;
		}
	}*/
}
