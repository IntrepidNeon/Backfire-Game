using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody),typeof(BoxCollider))]
public class Projectile : MonoBehaviour
{
	private Rigidbody _rb;
	private BoxCollider _boxCollider;

	public Rigidbody Rigidbody { get { return _rb; } }

	private void Awake()
	{
		_rb = GetComponent<Rigidbody>();
		_boxCollider = GetComponent<BoxCollider>();
	}

}
