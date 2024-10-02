using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class SplineTools
{
	public struct SamplePoint
	{
		public Vector3 position;
		public Vector3 forward;
		public float radius;
	}

	public static Vector3 CatmullRomPosition(float t, Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3)
	{
		Vector3 a = 2f * p1;
		Vector3 b = p2 - p0;
		Vector3 c = 2f * p0 - 5f * p1 + 4f * p2 - p3;
		Vector3 d = -p0 + 3f * p1 - 3f * p2 + p3;

		Vector3 pos = 0.5f * (a + (b * t) + (c * t * t) + (d * t * t * t));

		return pos;
	}
	public static Vector3 CatmullRomDirection(float t, Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3)
	{
		return 0.5f * (3 * t * t * (3 * p1 + p3 - 3 * p2 - p0) + 2 * t * (4 * p2 + 2 * p0 - 5 * p1 - p3) + p2 - p0);
	}
}
