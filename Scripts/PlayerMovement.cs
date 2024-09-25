using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
	public Transform[] AnchorPoints;
	public int DefaultAnchor;

	public float TransitionTime = 1.0f;
	public float PullDistance = 0.5f;

	public float Sensitivity = 0.6f;

	private int _currentAnchor;
	private int _newAnchor;

	private float _transitionProgress = 1f;

	private Vector2 lookAngle;

	// Start is called before the first frame update
	void Start()
	{
		_currentAnchor = Mathf.Clamp(DefaultAnchor, 0, AnchorPoints.Length - 1);
		_newAnchor = _currentAnchor;

		transform.SetPositionAndRotation(AnchorPoints[_currentAnchor].position, AnchorPoints[_currentAnchor].rotation);
	}

	// Update is called once per frame
	void Update()
	{
		Cursor.visible = false;

		if (Input.GetKey(KeyCode.D))
		{
			AnchorNext();
		}
		if (Input.GetKey(KeyCode.A))
		{
			AnchorPrev();
		}

		TransitionAnchor();

		Look();
	}

	private void Look()
	{
		lookAngle = new Vector2(
			Mathf.Clamp(lookAngle.x - Input.GetAxis("Mouse Y") * Sensitivity, -90, 90),
			Mathf.Clamp(lookAngle.y + Input.GetAxis("Mouse X") * Sensitivity, -75, 75));

		if (_transitionProgress < 1) return;

		transform.rotation = AddLook(AnchorPoints[_currentAnchor]);
	}

	private Quaternion AddLook(Transform anchor)
	{
		return Quaternion.AngleAxis(lookAngle.y, anchor.up)
			* Quaternion.AngleAxis(lookAngle.x, anchor.right)
			* anchor.rotation;
	}

	private void AnchorNext()
	{
		if (_transitionProgress < 1) return;

		if (_newAnchor < AnchorPoints.Length - 1)
		{
			_newAnchor++;
			AnchorChange();
		}
	}
	private void AnchorPrev()
	{
		if (_transitionProgress < 1) return;

		if (_newAnchor > 0)
		{
			_newAnchor--;
			AnchorChange();
		}
	}
	private void AnchorChange()
	{
		_transitionProgress = 0f;
	}

	private void TransitionAnchor()
	{
		if (_transitionProgress < 1f)
		{
			_transitionProgress = Mathf.Clamp01(_transitionProgress + Time.deltaTime / TransitionTime);
			transform.SetPositionAndRotation(
				Vector3.Lerp(AnchorPoints[_currentAnchor].position, AnchorPoints[_newAnchor].position, _transitionProgress)
				- Mathf.Sin(Mathf.PI * _transitionProgress) * PullDistance * Vector3.Lerp(AnchorPoints[_currentAnchor].forward, AnchorPoints[_newAnchor].forward, _transitionProgress),
				Quaternion.Slerp(AddLook(AnchorPoints[_currentAnchor]), AddLook(AnchorPoints[_newAnchor]), _transitionProgress));
			if (_transitionProgress >= 1f)
			{
				_currentAnchor = _newAnchor;
			}
		}
	}
}
