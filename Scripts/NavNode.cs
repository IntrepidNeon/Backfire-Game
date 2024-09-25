using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

[RequireComponent(typeof(SphereCollider))]
public class NavNode : MonoBehaviour
{
	public SphereCollider sphereCollider;
	public NavNode next;
	private void OnValidate()
	{
		sphereCollider = GetComponent<SphereCollider>();
		sphereCollider.isTrigger = true;
	}
	private void OnTriggerEnter(Collider other)
	{
		VehicleController v = other.GetComponentInParent<VehicleController>();
		if (v && v.target == transform)
		{
			v.target = next.transform;
		}
	}
	void OnDrawGizmos()
	{
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
					Handles.DrawLine(pos, node.next.transform.position);
				}
			}
			Handles.DrawWireDisc(pos, Vector3.up, node.sphereCollider.radius);
			Gizmos.DrawSphere(transform.position, node.sphereCollider.radius / 8);

			Handles.Label(pos, "  " + transform.parent.name + "." + transform.name);
		}
	}
}