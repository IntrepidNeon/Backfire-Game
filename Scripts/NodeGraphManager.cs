using UnityEngine;

public class NodeGraphManager : MonoBehaviour
{
	private SplineSample[] samplePoints = new SplineSample[0];
	private NavNode[] navNode = new NavNode[0];

	private void OnDrawGizmos()
	{
		Validate();
	}
	public void Validate()
	{
		navNode = GetComponentsInChildren<NavNode>();
		if (samplePoints.Length != navNode.Length)
		{
			samplePoints = new SplineSample[navNode.Length];
		}


		for (int i = 0; i < navNode.Length; i++)
		{
			SplineSample dummy = navNode[i].GetSample(0f);
			if (samplePoints[i] == null ||
				samplePoints[i].forward != dummy.forward ||
				samplePoints[i].position != navNode[i].transform.position ||
				!navNode[i].next)
			{
				if (navNode[i].prev) navNode[i].prev.ValidatePoints();
				navNode[i].ValidatePoints();
				Debug.Log("Updated NavNode " + i);
			}
			samplePoints[i] = navNode[i].headSample;
		}
	}
	private void Start()
	{
		Validate();
	}
}
