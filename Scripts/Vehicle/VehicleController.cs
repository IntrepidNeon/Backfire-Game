using JetBrains.Annotations;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.Scripting;
[RequireComponent(typeof(Rigidbody))]
public class VehicleController : MonoBehaviour
{
	public float StartVelocity;

	[Header("Drive")]
	public float driveTorqueClamp = 200;
	[Range(0, 1)]
	public float throttle;

	[Header("Brake")]
	public float brakeTorqueClamp = 800;
	[Range(0, 1)]
	public float brake;
	public float parkTorqueClamp = 800;
	[Range(0, 1)]
	public float park;

	[Header("Steer")]
	[Range(0f, 90f)]
	public float maxSteerAngle = 15;
	[Range(0f, 1f)]
	public float steeringSmoothness = 0.5f;

	internal float desiredSteerAngle;
	internal float desireAngle;

	private Rigidbody _rb;
	private float _steerAngle;

	public Transform target;

	public AxleInfo[] axleInfos;

	private void Awake()
	{
		_rb = GetComponent<Rigidbody>();
	}
	internal void FixedUpdate()
	{
		Navigate();
		UpdateWheels();
	}
	private void Start()
	{
		_rb.velocity = transform.forward * StartVelocity;
	}
	internal void UpdateWheels()
	{
		int motorCount = axleInfos.Count(axleInfo => axleInfo.motor);

		//throttle = Mathf.Max(Mathf.Clamp01((45 - Mathf.Abs(desireAngle)) / 45), 8 - _rb.velocity.magnitude);
		//throttle = Mathf.Clamp01(throttle);
		brake = Mathf.Clamp01(brake);

		float angleDiff = desiredSteerAngle - _steerAngle;
		if (steeringSmoothness <= 0) _steerAngle = desiredSteerAngle;

		else _steerAngle += Mathf.Sign(angleDiff) * Mathf.Min(maxSteerAngle * Time.fixedDeltaTime / steeringSmoothness, Mathf.Abs(angleDiff));

		float
			wheelBrakeTorque = brake * brakeTorqueClamp / axleInfos.Length / 2f,
			wheelDriveTorque = throttle * Mathf.Max(driveTorqueClamp, 0f) / motorCount / 2f,
			wheelParkTorque = park * parkTorqueClamp / motorCount;

		//Debug.Log(_rb.velocity.magnitude * 3.6f + " KPH");

		foreach (AxleInfo axleInfo in axleInfos)
		{
			Wheel leftWheel, rightWheel;
			leftWheel = axleInfo.leftWheel;
			rightWheel = axleInfo.rightWheel;
			if (axleInfo.steer)
			{
				leftWheel.collider.steerAngle = _steerAngle;
				rightWheel.collider.steerAngle = _steerAngle;
			}
			if (axleInfo.motor)
			{
				leftWheel.collider.motorTorque = wheelDriveTorque;
				rightWheel.collider.motorTorque = wheelDriveTorque;
			}
			leftWheel.collider.brakeTorque = wheelBrakeTorque + (axleInfo.park ? wheelParkTorque : 0);
			rightWheel.collider.brakeTorque = wheelBrakeTorque + (axleInfo.park ? wheelParkTorque : 0);

			leftWheel.collider.GetWorldPose(out Vector3 pos, out Quaternion quat);
			leftWheel.mesh.transform.SetPositionAndRotation(pos, quat);
			rightWheel.collider.GetWorldPose(out pos, out quat);
			rightWheel.mesh.transform.SetPositionAndRotation(pos, quat);
		}
	}

	internal void KeepUpright()
	{
		float relativeAngle = NormalizeAngle(transform.localEulerAngles.z);

		float force = -Mathf.Max(Mathf.Abs(Mathf.Abs(relativeAngle) - 30f) / -30f + 1, 0) * 2f;

		Debug.Log(force);

		_rb.AddRelativeTorque(0f, 0f, force, ForceMode.Acceleration);
	}

	public float NormalizeAngle(float angle)
	{
		return angle > 180 ? angle - 360 : angle;
	}

	public static Vector3 EvaluateBezier(Vector3 A, Vector3 B, Vector3 C, Vector3 D, float t)
	{
		Vector3
			AB = Vector3.Lerp(A, B, t),
			BC = Vector3.Lerp(B, C, t),
			CD = Vector3.Lerp(C, D, t),

			ABC = Vector3.Lerp(AB, BC, t),
			BCD = Vector3.Lerp(BC, CD, t);

		return Vector3.Lerp(ABC, BCD, t);
	}
	internal void Navigate()
	{
		if (!target) return;
		Quaternion worldWishDir, localWishDir;
		worldWishDir = Quaternion.LookRotation(NavigationBezier() - transform.position);
		localWishDir = worldWishDir * Quaternion.Inverse(transform.rotation);
		desireAngle = NormalizeAngle(localWishDir.eulerAngles.y);
		desiredSteerAngle = Mathf.Clamp(desireAngle, -maxSteerAngle, maxSteerAngle);
	}

	public Vector3 NavigationBezier()
	{
		NavNode targetNode = target.GetComponent<NavNode>();

		if (targetNode && _rb)
		{
			float sign = Mathf.Sign(Vector3.Dot(transform.forward, targetNode.transform.position - transform.position));

			Vector3 nextVector = (targetNode.next.transform.position - targetNode.transform.position).normalized;

			Vector3 halfVector = (transform.forward + nextVector).normalized;

			float offset = (transform.position - target.position).magnitude - _rb.velocity.magnitude - targetNode.sphereCollider.radius;

			return target.position - halfVector * Mathf.Clamp(offset, -16f, 16f);
		}
		return Vector3.zero;

	}

	void OnDrawGizmos()
	{
		if (target)
		{
			Debug.DrawLine(transform.position, NavigationBezier(), Color.red);

			float halfDistance = (transform.position - target.position).magnitude / 2f;

			//Debug.DrawLine(transform.position, transform.position + transform.forward * halfDistance, Color.yellow);

			NavNode targetNode = target.GetComponent<NavNode>();

			if (targetNode)
			{
				float sign = Mathf.Sign(Vector3.Dot(transform.forward, targetNode.transform.position - transform.position));

				Gizmos.color = Color.red;
				Gizmos.DrawSphere(NavigationBezier(), 0.5f);

				Vector3 nextVector = (targetNode.next.transform.position - targetNode.transform.position).normalized;

				Vector3 halfVector = (transform.forward + nextVector).normalized;

				//Debug.DrawLine(transform.position, transform.position + nextVector.normalized * 4f, Color.green);
				//Debug.DrawLine(transform.position, transform.position + transform.forward * 4f, Color.green);
				Debug.DrawLine(transform.position, transform.position - halfVector * 4f, Color.cyan);

				Debug.DrawLine(NavigationBezier(), target.position, Color.yellow);
			}


		}
	}
}

[System.Serializable]
public class AxleInfo
{
	public Wheel leftWheel, rightWheel;
	public bool motor, steer, park;

	public float AverageRPM { get { return (leftWheel.collider.rpm + rightWheel.collider.rpm) / 2f; } }
}
[System.Serializable]
public class Wheel
{
	public WheelCollider collider;
	public GameObject mesh;
}
