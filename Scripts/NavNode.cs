using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

[RequireComponent(typeof(SphereCollider))]
public class NavNode : MonoBehaviour
{
	public SphereCollider sphereCollider;

	public bool autoResolution = false;
	public int resolution = 16;
	public float width = 1f;

	[System.NonSerialized]
	public NavNode prev;
	public NavNode next;

	public SplineSample headSample = new();

	[System.NonSerialized]
	public float maxPointDist;

	private void OnValidate()
	{
		sphereCollider = GetComponent<SphereCollider>();
		sphereCollider.isTrigger = true;
		sphereCollider.radius = width / 2f;
		if (next) next.prev = this;
		if (prev) prev.ValidatePoints();
		ValidatePoints();
	}
	private void OnTriggerEnter(Collider other)
	{
		VehicleDriver driver = other.transform.root.GetComponentInChildren<VehicleDriver>();

		if (driver && driver.target == transform) { driver.target = next.transform; Debug.Log("driver collision"); }
	}

	void OnDrawGizmos()
	{
		NavNode node;
		Vector3 pos = transform.position;
		if (transform.GetComponent<NavNode>() != null)
		{
			node = transform.GetComponent<NavNode>();

			Handles.color = Color.magenta;
			if (node.next != null)
			{
				Vector3 nextPos = node.next.transform.position;
				if (pos != nextPos)
				{
					//Handles.DrawLine(pos, node.next.transform.position);
				}
			}

			Vector3
				prevRight = new(),
				prevLeft = new();



			SplineSample current = headSample;

			for (int i = 0; i <= resolution; i++)
			{
				if (current == null)
				{
					break;
				}
				Vector3
					pointRight = Vector3.Cross(Vector3.up, current.forward.normalized),
					currentLeft = current.position - pointRight * current.radius,
					currentRight = current.position + pointRight * current.radius;

				float x = Mathf.Clamp01((current.forward.magnitude - 12f) / 16f);
				Color weightedColor = new Color(-2 * Mathf.Abs(x) + 1, -2 * Mathf.Abs(x - 0.5f) + 1, -2 * Mathf.Abs(x - 1f) + 1);
				Gizmos.color = weightedColor;

				if (i == 0) Gizmos.DrawSphere(transform.position, node.sphereCollider.radius);
				
				if (i > 0)
				{
					Gizmos.DrawLine(prevRight, currentRight);
					Gizmos.DrawLine(prevLeft, currentLeft);

					Gizmos.DrawLine(prevRight, current.position);
					Gizmos.DrawLine(prevLeft, current.position);
				}
				current = current.next;
				prevRight = currentRight;
				prevLeft = currentLeft;
			}
			//Handles.DrawWireDisc(pos, Vector3.up, node.sphereCollider.radius);
			Handles.Label(pos, "  " + transform.parent.name + "." + transform.name);
		}
	}



	public void ValidatePoints()
	{
		SplineSample oldSample = GetSample(0f);

		headSample.position = oldSample.position;
		headSample.forward = oldSample.forward;
		headSample.radius = oldSample.radius;

		if (!prev) headSample.prev = null;
		if (!next || resolution < 1) { headSample.next = null; return; }

		oldSample = headSample;

		for (int i = 1; i < resolution; i++)
		{
			float t = (float)i / (resolution);
			SplineSample newSample = GetSample(t);
			oldSample.next = newSample;
			newSample.prev = oldSample;
			oldSample = oldSample.next;
		}
		oldSample.next = next.headSample;
	}
	public SplineSample GetSample(float t)
	{
		if (!next) { return new SplineSample { position = transform.position }; }

		bool isFirst = !prev;
		bool isLast = !next.next;

		Vector3
			p0 = isFirst ? 2 * transform.position - next.transform.position : prev.transform.position,
			p1 = transform.position,
			p2 = next.transform.position,
			p3 = isLast ? 2 * next.transform.position - transform.position : next.next.transform.position;

		return new SplineSample
		{
			position = SplineTools.CatmullRomPosition(t, p0, p1, p2, p3),
			forward = SplineTools.CatmullRomDirection(t, p0, p1, p2, p3),
			radius = Mathf.Lerp(width / 2f, next.width / 2f, t)
		};
	}


}

//CODE GRAVEYARD

//Array Based Points
/*
 * public SplineTools.SamplePoint GetSample(float t)
	{
		if (!next) { return new(); }

		bool isFirst = !prev;
		bool isLast = !next.next;

		Vector3
			p0 = isFirst ? 2 * transform.position - next.transform.position : prev.transform.position,
			p1 = transform.position,
			p2 = next.transform.position,
			p3 = isLast ? 2 * next.transform.position - transform.position : next.next.transform.position;

		return new SplineTools.SamplePoint
		{
			position = SplineTools.CatmullRomPosition(t, p0, p1, p2, p3),
			forward = SplineTools.CatmullRomDirection(t, p0, p1, p2, p3),
			radius = Mathf.Lerp(width / 2f, next.width / 2f, t)
		};
	}
 public SplineTools.SamplePoint[] subPoints = new SplineTools.SamplePoint[0];

for (int i = 0; i < subPoints.Length; i++)
			{
				Vector3
					pointRight = Vector3.Cross(Vector3.up, subPoints[i].forward.normalized),
					currentLeft = subPoints[i].position - pointRight * subPoints[i].radius,
					currentRight = subPoints[i].position + pointRight * subPoints[i].radius;

				float x = Mathf.Clamp01((subPoints[i].forward.magnitude - 12f) / 16f);
				Color weightedColor = new Color(-2 * Mathf.Abs(x) + 1, -2 * Mathf.Abs(x - 0.5f) + 1, -2 * Mathf.Abs(x - 1f) + 1);

				if (i > 0)
				{
					Debug.DrawLine(prevRight, currentRight, weightedColor);
					Debug.DrawLine(prevLeft, currentLeft, weightedColor);

					Debug.DrawLine(prevRight, subPoints[i].position, weightedColor);
					Debug.DrawLine(prevLeft, subPoints[i].position, weightedColor);
				}

				prevRight = currentRight;
				prevLeft = currentLeft;
			}

public void ValidatePoints()
	{
		if (autoResolution) resolution = (int)(next.transform.position - transform.position).magnitude;

		if (!next || resolution < 1)
		{
			subPoints = new SplineTools.SamplePoint[1];
			return;
		}

		subPoints = new SplineTools.SamplePoint[resolution + 1];

		for (int i = 0; i < subPoints.Length; i++)
		{
			float t = (float)i / (subPoints.Length - 1);
			subPoints[i] = GetSample(t);
		}

		float maxDist = 0f;

		for (int i = 0; i < subPoints.Length - 1; i++)
		{
			float dist = (subPoints[i + 1].position - subPoints[i].position).magnitude;
			if (dist > maxDist) maxDist = dist;
		}
		maxPointDist = maxDist;
	}
 */


/*public TargetInfo AdvanceTarget(TargetInfo currentInfo)
{
	Vector3 newPosition;
	NavNode newNode = currentInfo.node;
	int newIndex;

	if (currentInfo.forward)
	{
		newIndex = currentInfo.index + 1;

		if (newIndex > newNode.subPoints.Length - 1)
		{
			newNode = next;
			newIndex = 1;

		}
	}
	else
	{
		newIndex = currentInfo.index - 1;

		if (newIndex < 1)
		{
			newNode = prev;
			newIndex = prev.subPoints.Length - 1;
		}

	}
	newPosition = newNode.subPoints[newIndex];

	return new TargetInfo { position = newPosition, node = newNode, forward = currentInfo.forward, index = newIndex };
}*/

/*VehicleController v = other.GetComponentInParent<VehicleController>();
		if (v && v.target == transform)
		{
			v.target = next.transform;
		}*/