using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerDriver : VehicleDriver
{
	public void Navigate()
	{
		NavNode targetNode = target.GetComponent<NavNode>();
	}
}
