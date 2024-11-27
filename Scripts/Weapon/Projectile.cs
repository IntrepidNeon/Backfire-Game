using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody),typeof(BoxCollider))]
public class Projectile : MonoBehaviour
{
	private Rigidbody _rb;
	private TrailRenderer _trailRenderer;


	[System.NonSerialized]
	public ProjectileWeapon parent;

	public Rigidbody Rigidbody { get { return _rb; } }

	private void Awake()
	{
		_rb = GetComponent<Rigidbody>();
		_trailRenderer = GetComponent<TrailRenderer>();

		Destroy(gameObject, 5f);
	}

	private void OnCollisionEnter(Collision collision)
	{
		if(collision.transform.root != parent.transform.root)
		{
			Damagable receiver = collision.gameObject.GetComponent<Damagable>();
			if (receiver)
			{
				parent.DoDamage(receiver);
				Destroy(gameObject);
			}
		}
	}

}
