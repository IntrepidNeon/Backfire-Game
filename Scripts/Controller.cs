using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using static UnityEngine.GraphicsBuffer;

public class Controller : MonoBehaviour
{
	public Vector2 maxTurnAngle = new(15,45);

	private Vector3 lookAngle;

	private Ray aimRay;

	[Header("Camera Settings")]
	public GameObject cameraParent;
	public Camera gameCamera;
	public float rotSmoothSpeed = 0.01f;
	public float posSmoothSpeed = 0.1f;

	// Start is called before the first frame update
	private void Start()
	{
		cameraParent.transform.SetPositionAndRotation(transform.position, transform.rotation);
	}

	// Update is called once per frame
	void Update()
	{
		Look();
	}
	void Look()
	{
		Vector3 targetPos = transform.position;
		Vector3 smoothPos = Vector3.Lerp(cameraParent.transform.position, targetPos, posSmoothSpeed);
		Quaternion targetRot = transform.rotation;
		Quaternion smoothRot = Quaternion.Slerp(cameraParent.transform.rotation, targetRot, rotSmoothSpeed);

		cameraParent.transform.SetPositionAndRotation(smoothPos, smoothRot);

		Vector3 mousePos = Input.mousePosition;
		Vector3 mouseOffset = new(mousePos.x - Screen.width / 2, mousePos.y - Screen.height / 2, 0f);

		Debug.Log("Screen Width = " + Screen.width + " Screen Height = " + Screen.height + " Mouse Position = " + mousePos);

		lookAngle = new Vector3(
			mouseOffset.y * -2f / Screen.height * maxTurnAngle.x,
			mouseOffset.x * 2f / Screen.width * maxTurnAngle.y);

		gameCamera.transform.localEulerAngles = lookAngle ;

		aimRay = gameCamera.ScreenPointToRay(mousePos);
	}
}
