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

	//Returns closest point along the segment created by samplepoints using interpolated values
	public static SamplePoint ClosestSampleLerped(SamplePoint[] samplePoint, Vector3 point)
	{
		SamplePoint closestPoint = new();
		float closestDistanceSqr = Mathf.Infinity;
		for (int i = 0; i < samplePoint.Length - 1; i++)
		{
			SamplePoint start = samplePoint[i];
			SamplePoint end = samplePoint[i + 1];

			Vector3 dir = end.position - start.position;
			float sqrLen = dir.sqrMagnitude;

			SamplePoint closestSegmentPoint;
			if (sqrLen == 0f)
			{
				closestSegmentPoint = start;
			}
			else
			{
				float t = Mathf.Clamp01(Vector3.Dot(point - start.position, dir) / sqrLen);

				closestSegmentPoint = new SamplePoint
				{
					position = Vector3.Lerp(start.position, end.position, t),
					forward = Vector3.Lerp(start.forward, end.forward, t),
					radius = Mathf.Lerp(start.radius, end.radius, t)
				};
			}
			float sqrDist = (point - closestSegmentPoint.position).sqrMagnitude;
			if (sqrDist < closestDistanceSqr)
			{
				closestDistanceSqr = sqrDist;
				closestPoint = closestSegmentPoint;
			}
		}
		return closestPoint;
	}
	//Returns closest sample point to position (less smooth but less expensive than closestSampleLerped.)
	public static SamplePoint ClosestSample(SamplePoint[] samplePoint, Vector3 point)
	{
		SamplePoint closestPoint = new();
		float closestDistanceSqr = Mathf.Infinity;

		foreach (SamplePoint sample in samplePoint)
		{
			float sqrDist = (point - sample.position).sqrMagnitude;
			if (sqrDist < closestDistanceSqr)
			{
				closestDistanceSqr = sqrDist;
				closestPoint = sample;
			}
		}
		return closestPoint;
	}
}

public class SplineSample
{
	public SplineSample prev, next;
	public Vector3 position;
	public Vector3 forward;
	public float radius;

	public static SplineSample ClosestSample(SplineSample head, Vector3 position, int range)
	{
		if (range == 0) return head;

		SplineSample closestSample = head, currentSample = head;
		float closestDistanceSqr = Mathf.Infinity;
		for (int i = 0; i < Mathf.Abs(range); i++)
		{
			if (currentSample == null) break;
			float sqrDist = (position - currentSample.position).sqrMagnitude;
			if (sqrDist < closestDistanceSqr)
			{
				closestDistanceSqr = sqrDist;
				closestSample = currentSample;
			}
			currentSample = range > 0 ? currentSample.next : currentSample.prev;
		}
		return closestSample;
	}

	public static SplineSample ClosestSampleLerped(SplineSample head, Vector3 position, int range)
	{
		if (range == 0) return head;

		SplineSample
			currentSample = range > 0 ? head : head.prev,
			closestStart = head,
			closestEnd = head;

		float
			closestDistanceSqr = Mathf.Infinity,
			closestT = 0f;

		for (int i = 0; i < Mathf.Abs(range); i++)
		{
			if (currentSample == null || currentSample.next == null) break;

			SplineSample start = currentSample;
			SplineSample end = currentSample.next;

			Vector3
				dir = end.position - start.position,
				closestSegmentPosition;

			float sqrLen = dir.sqrMagnitude, t = 0f;

			if (sqrLen == 0f)
			{
				closestSegmentPosition = start.position;
			}
			else
			{
				t = Mathf.Clamp01(Vector3.Dot(position - start.position, dir) / sqrLen);

				closestSegmentPosition = Vector3.Lerp(start.position, end.position, t);
			}
			float sqrDist = (position - closestSegmentPosition).sqrMagnitude;
			if (sqrDist < closestDistanceSqr)
			{
				closestDistanceSqr = sqrDist;
				closestStart = start;
				closestEnd = end;
				closestT = t;
			}
			currentSample = range > 0 ? currentSample.next : currentSample.prev;
		}
		return new SplineSample
		{
			prev = closestStart,
			next = closestEnd,
			position = Vector3.Lerp(closestStart.position, closestEnd.position, closestT),
			forward = Vector3.Lerp(closestStart.forward, closestEnd.forward, closestT),
			radius = Mathf.Lerp(closestStart.radius, closestEnd.radius, closestT)
		};
	}

	//Creates a sample at a point that is "dist" away from the head, links to but not from original segment.
	public static SplineSample TraverseSampleLerped(SplineSample head, float dist)
	{
		if (dist == 0f) return head;

		SplineSample
			currentSample = head,
			nextSample = head;
		float remainder = Mathf.Abs(dist);
		while (currentSample != null && nextSample != null)
		{
			nextSample = dist > 0 ? currentSample.next : currentSample.prev;

			float sampleDist = (nextSample.position - currentSample.position).magnitude;

			if (sampleDist > remainder)
			{
				float t = remainder / sampleDist;
				return new SplineSample
				{
					prev = currentSample,
					next = nextSample,
					position = Vector3.Lerp(currentSample.position, nextSample.position, t),
					forward = Vector3.Lerp(currentSample.forward, nextSample.forward, t),
					radius = Mathf.Lerp(currentSample.radius, nextSample.radius, t)
				};
			}
			else
			{
				remainder -= sampleDist;
			}
			currentSample = dist > 0 ? currentSample.next : currentSample.prev;
		}
		return currentSample;
	}
	//Checks if a specified point is associated with a segment starting at "head" with length "range".
	public static bool LinkedToSegment(SplineSample sample, SplineSample head, int range)
	{
		SplineSample currentSample = head.next;
		for (int i = 1; i < Mathf.Abs(range) - 1; i++)
		{
			if (currentSample == sample.prev || currentSample == sample.next || currentSample == sample) return true;
			currentSample = range > 0 ? currentSample.next : currentSample.prev;
		}
		return false;
	}
}