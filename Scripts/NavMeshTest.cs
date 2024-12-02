using Unity.AI.Navigation;
using UnityEngine;
using UnityEngine.AI;
#if UNITY_EDITOR
using UnityEditor;
#endif

[RequireComponent(typeof(NavMeshSurface))]
public class NavMeshTest : MonoBehaviour
{
	public Transform startPos, endPos;

	public float distance = 1.0f;

	private NavMeshSurface _nms;

	private void Awake()
	{
		_nms = GetComponent<NavMeshSurface>();
	}

#if UNITY_EDITOR
	private void OnDrawGizmos()
	{
		NavMeshPath path = new();
		if (startPos && endPos)
		{
			if (NavMesh.CalculatePath(startPos.position, endPos.position, NavMesh.AllAreas, path))
			{

				float pathDistance = 0f;

				for (int i = 1; i < path.corners.Length; i++)
				{
					pathDistance += (path.corners[i] - path.corners[i - 1]).magnitude;

				}

				DistanceDraw(path.corners, Mathf.Clamp(distance, 0f, pathDistance));
			}
		}
	}

	private void DistanceDraw(Vector3[] points, float distance)
	{
		bool transitionComplete = false;
		for (int i = 0; i < points.Length - 1; i++)
		{
			if (!transitionComplete)
			{
				float segmentLength = (points[i + 1] - points[i]).magnitude;
				if (segmentLength > distance)
				{
					Vector3 transitionPoint = Vector3.Lerp(points[i], points[i + 1], distance / segmentLength);
					Debug.DrawLine(points[i], transitionPoint, Color.green);
					Debug.DrawLine(transitionPoint, points[i + 1], Color.red);
					transitionComplete = true;
				}
				else
				{
					Debug.DrawLine(points[i], points[i + 1], Color.green);
					distance -= segmentLength;
				}
			}
			else
			{
				Debug.DrawLine(points[i], points[i + 1], Color.red);
			}
			
			Handles.Label(points[i], "" + i);
		}
	}
#endif

}



/*for (int i = 0; i < prunedCorners.Count - 1; i++)
					{
						Debug.DrawLine(prunedCorners[i], prunedCorners[i + 1], Color.yellow);
						Handles.Label(prunedCorners[i], "" + i);
					}
					for (int i = 0; i < subPoints.Length - 1; i++)
					{
						Debug.DrawLine(subPoints[i], subPoints[i + 1], Color.white);
						Handles.Label(subPoints[i], "" + i);
					}*/

/*int resolution = 4;

				Vector3 p0 = Vector3.zero, p1 = Vector3.zero, p2 = Vector3.zero, p3 = Vector3.zero;

				List<Vector3> prunedCorners = new();
				bool skip = false;
				for (int i = 0; i < path.corners.Length; i++)
				{
					if (skip) { skip = false; continue; }

					if (i < path.corners.Length - 1 && i > 0)
					{
						if ((path.corners[i + 1] - path.corners[i]).magnitude < 4f)
						{
							prunedCorners.Add((path.corners[i + 1] + path.corners[i]) / 2f);
							skip = true;
							continue;
						}
						//if (Vector3.Dot((path.corners[i] - path.corners[i - 1]).normalized, (path.corners[i + 1] - path.corners[i]).normalized) > 0.999f) { continue; }

					}
					prunedCorners.Add(path.corners[i]);
				}
				Vector3[] subPoints = new Vector3[resolution * (prunedCorners.Count - 1)];

				if (prunedCorners.Count > 2)
				{
					for (int i = 0; i < prunedCorners.Count - 1; i++)
					{
						bool isFirst = i < 1;
						bool isLast = i > prunedCorners.Count - 3;

						//The 4 points we need to form a spline between p1 and p2
						p0 = isFirst ? 2 * prunedCorners[i] - prunedCorners[i + 1] : prunedCorners[i - 1];
						p1 = prunedCorners[i];
						p2 = prunedCorners[i + 1];
						p3 = isLast ? 2 * prunedCorners[i + 1] - prunedCorners[i] : prunedCorners[i + 2];

						for (int j = 0; j < resolution; j++)
						{
							//Which t position are we at?
							float t = (float)j / resolution;

							//Find the coordinate between the end points with a Catmull-Rom spline
							subPoints[j + i * resolution] = SplineTools.CatmullRomPosition(t, p0, p1, p2, p3);
						}

					}*/