using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public abstract class ProjectileWeapon : WeaponController
{
	public float LaunchSpeed;           // Velocity of projectile on spawn
	public Projectile ProjectilePrefab;       // Projectile Prefab
	public Transform ProjectileSpawn;   // Used for projectile spawn location and direciton

	[System.NonSerialized]
	public Vector3 VelocityOffset;

	public void SpawnProjectile()
	{
		Projectile newProjectile = Instantiate(ProjectilePrefab, ProjectileSpawn.position, ProjectileSpawn.rotation, null);
		newProjectile.parent = this;

		GetNetVelocity(out Vector3 netVelocity, out Vector3 netAngularVelocity);

		newProjectile.Rigidbody.AddForce(newProjectile.transform.forward * LaunchSpeed + netVelocity, ForceMode.VelocityChange);

		//EditorApplication.isPaused = true;
	}

	public void GetNetVelocity(out Vector3 netVelocity, out Vector3 netAngularVelocity)
	{
		netVelocity = Vector3.zero;
		netAngularVelocity = Vector3.zero;

		Transform rbFetch = transform;
		Rigidbody rb;
		while (true)
		{
			rb = rbFetch.GetComponent<Rigidbody>();
			if (rb)
			{
				netVelocity += rb.velocity;
				netAngularVelocity += rb.angularVelocity;
			}
			if (rbFetch.parent == null) break;
			rbFetch = rbFetch.parent;
		}
	}
}
