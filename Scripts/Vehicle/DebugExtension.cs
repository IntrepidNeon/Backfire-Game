using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class DebugExtension
{
	public static void DrawWireCube(Vector3 center, Vector3 halfExtents, Quaternion rotation, Color color)
	{
		Vector3
			right = rotation * Vector3.right,
			up = rotation * Vector3.up,
			forward = rotation * Vector3.forward;

		Vector3[] point = {
			center - (right * halfExtents.x) - (up * halfExtents.y) - (forward * halfExtents.z), //-X-Y-Z
			center + (right * halfExtents.x) + (up * halfExtents.y) + (forward * halfExtents.z), //+X+Y+Z

			center - (right * halfExtents.x) + (up * halfExtents.y) - (forward * halfExtents.z), //-X+Y-Z
			center + (right * halfExtents.x) - (up * halfExtents.y) + (forward * halfExtents.z), //+X-Y+Z

			center - (right * halfExtents.x) + (up * halfExtents.y) + (forward * halfExtents.z), //-X+Y+Z
			center + (right * halfExtents.x) - (up * halfExtents.y) - (forward * halfExtents.z), //+X-Y-Z
			
			center - (right * halfExtents.x) - (up * halfExtents.y) + (forward * halfExtents.z), //-X-Y+Z
			center + (right * halfExtents.x) + (up * halfExtents.y) - (forward * halfExtents.z), //+X+Y-Z
		};
		//Local X Axis
		Debug.DrawLine(point[0], point[5], color);
		Debug.DrawLine(point[2], point[7], color);
		Debug.DrawLine(point[4], point[1], color);
		Debug.DrawLine(point[6], point[3], color);
		//Local Y Axis
		Debug.DrawLine(point[0], point[2], color);
		Debug.DrawLine(point[6], point[4], color);
		Debug.DrawLine(point[3], point[1], color);
		Debug.DrawLine(point[5], point[7], color);
		//Local Z Axis
		Debug.DrawLine(point[0], point[6], color);
		Debug.DrawLine(point[5], point[3], color);
		Debug.DrawLine(point[7], point[1], color);
		Debug.DrawLine(point[2], point[4], color);

		return;
	}
}
