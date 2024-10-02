using UnityEngine;
using System.Collections;
using static UnityEditor.PlayerSettings;
using UnityEditor;

//Interpolation between points with a Catmull-Rom spline
public class CatmullRomSpline : MonoBehaviour
{
	//Has to be at least 4 points
	private Transform[] controlPointsList;
	//Are we making a line or a loop?
	public bool isLooping = true;

	//Display without having to press play
	void OnDrawGizmos()
	{
		controlPointsList = transform.GetComponentsInChildren<Transform>();
		Gizmos.color = Color.white;

		//Draw the Catmull-Rom spline between the points
		for (int i = 0; i < controlPointsList.Length; i++)
		{
			Handles.Label(controlPointsList[i].position, "  " + transform.name + "." + i);
			Gizmos.DrawSphere(controlPointsList[i].position, 0.25f);
			/*
			//Cant draw between the endpoints
			//Neither do we need to draw from the second to the last endpoint
			//...if we are not making a looping line
			if ((i == 0 || i == controlPointsList.Length - 2 || i == controlPointsList.Length - 1) && !isLooping)
			{
				continue;
			}*/

			//FUCK THE RULES!!!

			DisplayCatmullRomSpline(i);
		}
	}

	//Display a spline between 2 points derived with the Catmull-Rom spline algorithm
	void DisplayCatmullRomSpline(int pos)
	{
		

		if (controlPointsList.Length < 2 || pos > controlPointsList.Length - 2) return;

		bool isFirst = pos < 1;
		bool isLast = pos > controlPointsList.Length - 3;

		//The 4 points we need to form a spline between p1 and p2
		Vector3 p0 = isFirst ? 2 * controlPointsList[pos].position - controlPointsList[pos + 1].position : controlPointsList[pos - 1].position;
		Vector3 p1 = controlPointsList[pos].position;
		Vector3 p2 = controlPointsList[pos + 1].position;
		Vector3 p3 = isLast ? 2 * controlPointsList[pos + 1].position - controlPointsList[pos].position : controlPointsList[pos + 2].position;

		//The start position of the line
		Vector3 lastPos = p1;

		//The spline's resolution
		//Make sure it's is adding up to 1, so 0.3 will give a gap, but 0.2 will work
		float resolution = 0.05f;

		//How many times should we loop?
		int loops = Mathf.FloorToInt(1f / resolution);

		for (int i = 1; i <= loops; i++)
		{
			//Which t position are we at?
			float t = i * resolution;

			//Find the coordinate between the end points with a Catmull-Rom spline
			Vector3 newPos = GetCatmullRomPosition(t, p0, p1, p2, p3);

			//Draw this line segment
			Debug.DrawLine(lastPos, newPos);
			//Debug.DrawLine(newPos, newPos + GetCatmullRomDirection(t, p0, p1, p2, p3).normalized, Color.red);

			//Save this pos so we can draw the next line segment
			lastPos = newPos;
		}
	}

	//Returns a position between 4 Vector3 with Catmull-Rom spline algorithm
	//http://www.iquilezles.org/www/articles/minispline/minispline.htm
	Vector3 GetCatmullRomPosition(float t, Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3)
	{
		//The coefficients of the cubic polynomial (except the 0.5f * which I added later for performance)
		Vector3 a = 2f * p1;
		Vector3 b = p2 - p0;
		Vector3 c = 2f * p0 - 5f * p1 + 4f * p2 - p3;
		Vector3 d = -p0 + 3f * p1 - 3f * p2 + p3;

		//The cubic polynomial: a + b * t + c * t^2 + d * t^3
		Vector3 pos = 0.5f * (a + (b * t) + (c * t * t) + (d * t * t * t));

		return pos;
	}
	Vector3 GetCatmullRomDirection(float t, Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3)
	{
		return 0.5f * (3 * t * t * (3 * p1 + p3 - 3 * p2 - p0) + 2 * t * (4 * p2 + 2 * p0 - 5 * p1 - p3) + p2 - p0);
	}
}