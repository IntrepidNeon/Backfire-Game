using Palmmedia.ReportGenerator.Core.CodeAnalysis;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(VehicleController))]
public class VehicleDriver : MonoBehaviour
{
	public VehicleController vc;

	private Rigidbody _rb;

	private Vector3 _steerTarget;
	private float calculatedSteerAngle;

	Vector3 debugDesire = new();
	Vector3 navMeshTarget = new();
	NavMeshPath path;

	private RaycastHit targetHitInfo;
	private bool targetObstacle;

	private void Awake()
	{
		vc = GetComponent<VehicleController>();
		_rb = GetComponent<Rigidbody>();
	}
	private void FixedUpdate()
	{
		Navigate();
	}

	private void Navigate()
	{

		if (!vc.target)
		{
			vc.throttle = 0;
			vc.brake = Mathf.Clamp01(1 - _rb.velocity.magnitude / 20f);
			vc.desiredSteerAngle = 0;
			return;
		}
		vc.throttle = 1f;
		vc.brake = 0f;

		NavNode targetNode = vc.target.GetComponent<NavNode>();
		if (targetNode)
		{
			float velocityMagnitude = _rb.velocity.magnitude;
			float mag = Mathf.Max(velocityMagnitude / 2f, targetNode.maxPointDist);

			Vector3 desireVector =
				(transform.forward + 1.001f * (targetNode.transform.position - transform.position).normalized).normalized * mag;

			debugDesire = desireVector;

			if ((transform.position - targetNode.transform.position).magnitude < mag)
			{
				vc.target = targetNode.next.transform;
			}

			float minDist = Mathf.Infinity;
			SplineTools.SamplePoint closestPoint = new();

			foreach (SplineTools.SamplePoint point in targetNode.prev.subPoints)
			{
				float dist = (point.position - (transform.position + desireVector)).magnitude;
				if (dist < minDist)
				{
					minDist = dist;
					closestPoint = point;
				}
			}
			closestPoint = SplineTools.ClosestSampleLerped(targetNode.prev.subPoints, transform.position + desireVector);
			_steerTarget = closestPoint.position;

			float maxVelocity = closestPoint.forward.magnitude / 3;
			if (velocityMagnitude > maxVelocity)
			{
				vc.throttle = 0f;
				vc.brake = 1f;
			}
		}


		calculatedSteerAngle = SteerAngleToTarget(_steerTarget - transform.position);
		NavMeshObstacleAvoidance();

		vc.desiredSteerAngle = calculatedSteerAngle;
	}

	public float NormalizeAngle(float angle)
	{
		angle %= 360;


		if (angle > 180)
		{
			angle -= 360;
		}
		else if (angle < -180)
		{
			angle += 360;
		}

		return angle;
	}

	private SplineTools.SamplePoint NavNodePathTarget(NavNode targetNode)
	{
		if (targetNode && targetNode.prev)
		{
			float velocityMagnitude = _rb.velocity.magnitude;
			float seekMagnitude = Mathf.Max(velocityMagnitude / 2f, targetNode.maxPointDist);

			Vector3 desireVector =
				(transform.forward + 1.001f * (targetNode.transform.position - transform.position).normalized).normalized * seekMagnitude;

			if ((transform.position - targetNode.transform.position).magnitude < seekMagnitude)
			{
				vc.target = targetNode.next.transform;
			}

			float minDist = Mathf.Infinity;
			SplineTools.SamplePoint closestPoint = new();
			foreach (SplineTools.SamplePoint point in targetNode.prev.subPoints)
			{
				float dist = (point.position - (transform.position + desireVector)).magnitude;
				if (dist < minDist)
				{
					minDist = dist;
					closestPoint = point;
				}
			}
			return closestPoint;
		}
		return new SplineTools.SamplePoint { position = targetNode.transform.position, forward = new(), radius = 0f };
	}

	private float SteerAngleToTarget(Vector3 dir)
	{
		Quaternion worldWishDir, localWishDir;
		worldWishDir = Quaternion.LookRotation(dir);
		localWishDir = worldWishDir * Quaternion.Inverse(transform.rotation);
		return Mathf.Clamp(NormalizeAngle(localWishDir.eulerAngles.y), -vc.maxSteerAngle, vc.maxSteerAngle);
	}

	private void NavMeshObstacleAvoidance()
	{
		float velocityMagnitude = _rb.velocity.magnitude;
		Vector3 forwardHalfExtents = new(0.75f, 0.05f, 0.05f);
		targetObstacle = ObstacleBoxCast(
			transform.position,
			forwardHalfExtents,
			Quaternion.AngleAxis(calculatedSteerAngle, transform.up) * transform.forward,
			transform.rotation,
			Mathf.Clamp(3 * velocityMagnitude + 1.5f, 0f, (vc.target.position - transform.position).magnitude),
			out targetHitInfo);

		if (targetObstacle)
		{
			path = new();
			if (NavMesh.CalculatePath(transform.position, vc.target.position, NavMesh.AllAreas, path))
			{
				float targetDist = 4f;
				Vector3 targetPoint = Vector3.zero;
				for (int i = 0; i < path.corners.Length - 1; i++)
				{
					float segmentLength = (path.corners[i + 1] - path.corners[i]).magnitude;
					Debug.Log(segmentLength);
					if (segmentLength > targetDist)
					{
						targetPoint = Vector3.Lerp(path.corners[i], path.corners[i + 1], targetDist / segmentLength);
						break;
					}
					else
					{
						targetDist -= segmentLength;
					}
				}
				calculatedSteerAngle = SteerAngleToTarget(targetPoint - transform.position);
				navMeshTarget = targetPoint;
			}
		}
	}

	public bool ObstacleBoxCast(Vector3 center, Vector3 halfExtents, Vector3 direction, Quaternion orientation, float distance, out RaycastHit hitInfo)
	{
		hitInfo = default;
		bool obstacleHit = false;

		RaycastHit[] hits = Physics.BoxCastAll(center, halfExtents, direction, orientation, distance, Physics.DefaultRaycastLayers, QueryTriggerInteraction.Ignore);

		float maxDistance = Mathf.Infinity;

		foreach (RaycastHit hit in hits)
		{
			if (hit.point == Vector3.zero ||
				new Vector2(hit.normal.x, hit.normal.z).magnitude < hit.normal.y || // If the normal points more upward than sideways, it probably isnt relevant
				hit.transform.IsChildOf(transform.root))
				continue;

			if (hit.distance < maxDistance)
			{
				maxDistance = hit.distance;
				obstacleHit = true;
				hitInfo = hit;
			}
		}

		return obstacleHit;

	}

	private void OnDrawGizmos()
	{
		//VisualizeSteering();

		Debug.DrawLine(transform.position, transform.position + debugDesire, Color.red);

		Color orange = new(1f, 0.5f, 0f);

		if (_rb)
		{
			//Debug.DrawLine(transform.position, transform.position + transform.forward * _rb.velocity.magnitude, Color.magenta);


			if (targetObstacle)
			{
				Gizmos.color = Color.red;
				Gizmos.DrawSphere(navMeshTarget, 0.25f);
				if (path != null)
				{
					for (int i = 0; i < path.corners.Length - 1; i++)
					{
						Debug.DrawLine(path.corners[i], path.corners[i + 1], Color.green);
						Handles.Label(path.corners[i], "" + i);

					}
				}
				Gizmos.color = orange;

				Debug.DrawLine(targetHitInfo.point, transform.position, orange);
				Debug.DrawLine(targetHitInfo.point, targetHitInfo.point + targetHitInfo.normal, orange);
				Gizmos.DrawSphere(targetHitInfo.point, 0.25f);
			}
		}


		Gizmos.color = Color.red;
		Gizmos.DrawSphere(_steerTarget, 0.25f);

	}
}


//CODE GRAVEYARD
/*void VisualizeSteering()
		{
			Debug.DrawLine(transform.position, transform.position + Quaternion.AngleAxis(_steeringClampMin, transform.up) * transform.forward * 4f, Color.blue);
			Debug.DrawLine(transform.position, transform.position + Quaternion.AngleAxis(_steeringClampMax, transform.up) * transform.forward * 4f, Color.red);
		}*/

/*if (leftObstacle)
			{
				Debug.DrawLine(leftHitInfo.point, transform.position, Color.blue);
				Debug.DrawLine(leftHitInfo.point, leftHitInfo.point + leftHitInfo.normal, orange);
				Gizmos.DrawSphere(leftHitInfo.point, 0.25f);
			}
			if (rightObstacle)
			{
				Debug.DrawLine(rightHitInfo.point, transform.position, Color.red);
				Debug.DrawLine(rightHitInfo.point, rightHitInfo.point + rightHitInfo.normal, orange);
				Gizmos.DrawSphere(rightHitInfo.point, 0.25f);
			}*/

/*
	private void BasicObstacleAvoidance()
	{

		//Vector3 magicVector = (vc.target.transform.position - transform.position + transform.forward).normalized;

		float velocityMagnitude = _rb.velocity.magnitude;

		Vector3 forwardHalfExtents = new(0.75f, 0.05f, 0.05f);

		forwardObstacle = ObstacleBoxCast(
			transform.position,
			forwardHalfExtents,
			transform.forward,
			transform.rotation,
			velocityMagnitude,
			out forwardHitInfo);

		float forwardDist = forwardObstacle ? (forwardHitInfo.point - transform.position).magnitude : velocityMagnitude;

		Vector3 sidewaysHalfExtents = new(0.05f, 0.05f, forwardDist / 2f);

		leftObstacle = ObstacleBoxCast(
			transform.position + transform.forward * forwardDist / 2f + transform.right * (sidewaysHalfExtents.x + 0.1f),
			new(0.05f, 0.05f, forwardDist / 2f),
			-transform.right,
			transform.rotation,
			velocityMagnitude,
			out leftHitInfo);

		rightObstacle = ObstacleBoxCast(
			transform.position + transform.forward * forwardDist / 2f - transform.right * (sidewaysHalfExtents.x + 0.1f),
			new(0.05f, 0.05f, forwardDist / 2f),
			transform.right,
			transform.rotation,
			velocityMagnitude,
			out rightHitInfo);

		if (leftObstacle || rightObstacle)
		{
			float
			leftDist = leftObstacle ? leftHitInfo.distance : velocityMagnitude,
			rightDist = rightObstacle ? rightHitInfo.distance : velocityMagnitude;

			Vector3
				leftAvoidanceVector = -transform.right,
				rightAvoidanceVector = transform.right;

			if (leftObstacle)
			{
				leftAvoidanceVector = (transform.forward - Vector3.Dot(transform.forward, leftHitInfo.normal) * leftHitInfo.normal).normalized;
			}
			if (rightObstacle)
			{
				rightAvoidanceVector = (transform.forward - Vector3.Dot(transform.forward, rightHitInfo.normal) * rightHitInfo.normal).normalized;
			}

			float
				leftAvoidanceAngle = SteerAngleToTarget(leftAvoidanceVector),
				rightAvoidanceAngle = SteerAngleToTarget(rightAvoidanceVector);

			float
				leftInfluence = 1 - (leftDist / velocityMagnitude),
				rightInfluence = 1 - (rightDist / velocityMagnitude);

			_steeringClampMin = Mathf.LerpUnclamped(-vc.maxSteerAngle, leftAvoidanceAngle, leftInfluence);
			_steeringClampMax = Mathf.LerpUnclamped(vc.maxSteerAngle, rightAvoidanceAngle, rightInfluence);

			if (_steeringClampMin > _steeringClampMax)
			{
				float aveClamp = (_steeringClampMin + _steeringClampMax) / 2f;
				_steeringClampMin = aveClamp;
				_steeringClampMax = aveClamp;
			}

		}
		calculatedSteerAngle = Mathf.Clamp(calculatedSteerAngle, _steeringClampMin, _steeringClampMax);
	}*/

/*if (targetObstacle || forwardObstacle)
		{
			float
			targetDist = targetObstacle ? targetHitInfo.distance : velocityMagnitude,
			forwardDist = forwardObstacle ? forwardHitInfo.distance : velocityMagnitude,
			aveDist = (targetDist + forwardDist) / 2f;

			Vector3
				averageNormal = targetHitInfo.normal * targetDist + forwardHitInfo.normal * forwardDist,
				avoidanceVector = (transform.forward - Vector3.Dot(transform.forward, averageNormal) * averageNormal).normalized;

			float avoidanceAngle = SteerAngleToTarget(avoidanceVector);

			float influence = Mathf.Clamp01(1f - aveDist / velocityMagnitude);
			Debug.Log(influence);
			_steeringClampMin = avoidanceAngle < 0 ? -vc.maxSteerAngle : Mathf.Lerp(-vc.maxSteerAngle, avoidanceAngle, influence);
			_steeringClampMax = avoidanceAngle > 0 ? vc.maxSteerAngle : Mathf.Lerp(vc.maxSteerAngle, avoidanceAngle, influence);
			//_steeringClampMin = avoidanceAngle < 0 ? -vc.maxSteerAngle : avoidanceAngle;
			//_steeringClampMax = avoidanceAngle > 0 ? vc.maxSteerAngle : avoidanceAngle;
		}*/

/*public bool ObstacleSweepTest(Vector3 direction, float distance, out RaycastHit hitInfo)
{
	hitInfo = default;
	bool obstacleHit = false;

	RaycastHit[] hits = _rb.SweepTestAll(direction, distance, QueryTriggerInteraction.Ignore);

	float maxDistance = Mathf.Infinity;

	foreach (RaycastHit hit in hits)
	{
		if (new Vector2(hit.normal.x, hit.normal.z).magnitude < hit.normal.y || // If the normal points more upward than sideways, it probably isnt relevant
			hit.transform.IsChildOf(transform.root))
			continue;

		if (hit.distance < maxDistance)
		{
			maxDistance = hit.distance;
			obstacleHit = true;
			hitInfo = hit;
		}
	}

	return obstacleHit;

}*/

/*Vector3
				averageNormal =
				(leftObstacle ? leftHitInfo.normal * leftDist : Vector3.zero) +
				(forwardObstacle ? forwardHitInfo.normal * forwardDist : Vector3.zero) +
				(rightObstacle ? rightHitInfo.normal * rightDist : Vector3.zero),
				avoidanceVector = (transform.forward - Vector3.Dot(transform.forward, averageNormal) * averageNormal).normalized;

			float avoidanceAngle = SteerAngleToTarget(avoidanceVector);

			//float influence = Mathf.Clamp01(1f - aveDist / velocityMagnitude);
			//Debug.Log(influence);
			//_steeringClampMin = avoidanceAngle < 0 ? -vc.maxSteerAngle : Mathf.Lerp(-vc.maxSteerAngle, avoidanceAngle, influence);
			//_steeringClampMax = avoidanceAngle > 0 ? vc.maxSteerAngle : Mathf.Lerp(vc.maxSteerAngle, avoidanceAngle, influence);
			//_steeringClampMin = avoidanceAngle < 0 ? -vc.maxSteerAngle : avoidanceAngle;
			//_steeringClampMax = avoidanceAngle > 0 ? vc.maxSteerAngle : avoidanceAngle;

			float
				leftInfluence = 1 - ((leftDist - velocityMagnitude / 4f) / velocityMagnitude),
				forwardInfluence = forwardDist / velocityMagnitude / 4f,
				rightInfluence = 1 - ((rightDist - velocityMagnitude / 4f) / velocityMagnitude);

			_steeringClampMin = Mathf.Lerp(-vc.maxSteerAngle, vc.maxSteerAngle, Mathf.Clamp01(leftInfluence - forwardInfluence));
			_steeringClampMax = Mathf.Lerp(vc.maxSteerAngle, -vc.maxSteerAngle, Mathf.Clamp01(rightInfluence - forwardInfluence));

			if (_steeringClampMin > _steeringClampMax)
			{
				float aveClamp = (_steeringClampMin + _steeringClampMax) / 2f;
				_steeringClampMin = aveClamp;
				_steeringClampMax = aveClamp;
			}*/

//targetObstacle = ObstacleSweepTest(SteeringForward, velocityMagnitude, out targetHitInfo);

//forwardObstacle = ObstacleSweepTest(transform.forward, velocityMagnitude, out forwardHitInfo);

//leftObstacle = ObstacleSweepTest(Quaternion.AngleAxis(-45f, Vector3.up) * transform.rotation * Vector3.forward, velocityMagnitude, out leftHitInfo);
//rightObstacle = ObstacleSweepTest(Quaternion.AngleAxis(45f, Vector3.up) * transform.rotation * Vector3.forward, velocityMagnitude, out rightHitInfo);

//Debug.DrawLine(transform.position, transform.position + _rb.velocity);
/*if (targetObstacle)
{
	Vector3 rayEnd = transform.position + SteeringForward * targetHitInfo.distance;
	Debug.DrawLine(transform.position, rayEnd, orange);
	//DebugExtension.DrawWireCube(rayEnd + transform.up * 0.5f, new(0.5f, 0.5f, 2f), transform.rotation, Color.white);

	Debug.DrawRay(targetHitInfo.point, targetHitInfo.normal, Color.cyan);
}
else
{
	Vector3 rayEnd = transform.position + SteeringForward * _rb.velocity.magnitude;
	Debug.DrawLine(transform.position, rayEnd, Color.black);
}
if (forwardObstacle)
{
	rayEnd = transform.position + transform.forward * forwardHitInfo.distance;
	Debug.DrawLine(transform.position, rayEnd, orange);

	Debug.DrawRay(forwardHitInfo.point, forwardHitInfo.normal, Color.cyan);
}
else
{
	rayEnd = transform.position + transform.forward * _rb.velocity.magnitude;
	Debug.DrawLine(transform.position, rayEnd, Color.black);
}
if (leftObstacle)
{
	rayEnd = transform.position + (Quaternion.AngleAxis(-45f, Vector3.up) * transform.rotation * Vector3.forward) * leftHitInfo.distance;
	Debug.DrawLine(transform.position, rayEnd, orange);
}
else
{
	rayEnd = transform.position + (Quaternion.AngleAxis(-45f, Vector3.up) * transform.rotation * Vector3.forward) * _rb.velocity.magnitude;
	Debug.DrawLine(transform.position, rayEnd, Color.black);
}
if (rightObstacle)
{
	rayEnd = transform.position + (Quaternion.AngleAxis(45f, Vector3.up) * transform.rotation * Vector3.forward) * rightHitInfo.distance;
	Debug.DrawLine(transform.position, rayEnd, orange);
}
else
{
	rayEnd = transform.position + (Quaternion.AngleAxis(45f, Vector3.up) * transform.rotation * Vector3.forward) * _rb.velocity.magnitude;
	Debug.DrawLine(transform.position, rayEnd, Color.black);
}*/

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

/*
private Vector3 NodeSteeringTarget()
	{
		NavNode targetNode = vc.target.GetComponent<NavNode>();

		Vector3 nextVector, halfVector;

		float position;

		if (targetNode && targetNode.next)
		{
			nextVector = (targetNode.next.transform.position - targetNode.transform.position).normalized;

			halfVector = (transform.forward + nextVector).normalized;

			position = (transform.position - vc.target.position).magnitude - _rb.velocity.magnitude;

			return vc.target.position - halfVector * Mathf.Clamp(position, -16f, 16f);
		}

		else
		{
			return vc.target.position;
		}
	}*/