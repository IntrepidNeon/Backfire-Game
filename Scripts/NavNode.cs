using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;

[RequireComponent(typeof(SphereCollider))]
public class NavNode : MonoBehaviour
{
	public SphereCollider sphereCollider;

	public int resolution = 16;
	public float width = 1f;

	[System.NonSerialized]
	public NavNode prev;
	public NavNode next;

	public SplineTools.SamplePoint[] subPoints = new SplineTools.SamplePoint[0];

	public struct TargetInfo
	{
		public Vector3 position;    // Target Location
		public NavNode node;        // Target Node
		public bool forward;        // Direction of travel
		public int index;           // Index of the current sub point
	}

	[System.NonSerialized]
	public float maxPointDist;

	private void OnValidate()
	{
		sphereCollider = GetComponent<SphereCollider>();
		sphereCollider.isTrigger = true;
		next.prev = this;
		ValidatePoints();
	}
	private void OnTriggerEnter(Collider other)
	{
		/*VehicleController v = other.GetComponentInParent<VehicleController>();
		if (v && v.target == transform)
		{
			v.target = next.transform;
		}*/
	}
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
	void OnDrawGizmos()
	{
		ValidatePoints();
		Gizmos.color = Color.magenta;
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
			//Vector3 prevPoint = transform.position;

			Vector3
				prevRight = new(),
				prevLeft = new();
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
			//Handles.DrawWireDisc(pos, Vector3.up, node.sphereCollider.radius);
			Gizmos.DrawSphere(transform.position, node.sphereCollider.radius / 8);

			if (next) Handles.Label(pos, "  " + transform.parent.name + "." + transform.name + " : " + subPoints[0].forward.magnitude);
			else Handles.Label(pos, "  " + transform.parent.name + "." + transform.name);

		}
	}

	private void ValidatePoints()
	{
		if (!next)
		{
			subPoints = new SplineTools.SamplePoint[0];
			return;
		}

		bool isFirst = !prev;
		bool isLast = !next.next;

		subPoints = new SplineTools.SamplePoint[resolution + 1];

		subPoints[0].position = transform.position;
		subPoints[^1].position = next.transform.position;

		Vector3
			p0 = isFirst ? 2 * transform.position - next.transform.position : prev.transform.position,
			p1 = transform.position,
			p2 = next.transform.position,
			p3 = isLast ? 2 * next.transform.position - transform.position : next.next.transform.position;

		for (int i = 0; i < subPoints.Length; i++)
		{
			float t = (float)i / (subPoints.Length - 1);
			subPoints[i] = new SplineTools.SamplePoint
			{
				position = SplineTools.CatmullRomPosition(t, p0, p1, p2, p3),
				forward = SplineTools.CatmullRomDirection(t, p0, p1, p2, p3),
				radius = Mathf.Lerp(width / 2f, next.width / 2f, t)
			};
		}

		float maxDist = 0f;

		for (int i = 0; i < subPoints.Length - 1; i++)
		{
			float dist = (subPoints[i + 1].position - subPoints[i].position).magnitude;
			if (dist > maxDist) maxDist = dist;
		}

		maxPointDist = maxDist;

	}


}