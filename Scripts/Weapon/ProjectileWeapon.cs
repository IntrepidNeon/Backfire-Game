using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class ProjectileWeapon : WeaponController
{
	public int Reserve;                 // Quantity of uses
	public float LaunchSpeed;           // Velocity of projectile on spawn
	public Projectile ProjectilePrefab;       // Projectile Prefab
	public Transform ProjectileSpawn;   // Used for projectile spawn location and direciton

	[System.NonSerialized]
	public Vector3 VelocityOffset;

	public void SpawnProjectile()
	{
		Projectile newProjectile = Instantiate(ProjectilePrefab, ProjectileSpawn.position, ProjectileSpawn.rotation, null);
		newProjectile.Rigidbody.AddForce(newProjectile.transform.forward * LaunchSpeed + VelocityOffset, ForceMode.VelocityChange);
	}

	public void ProjectileHit()
	{

	}
}
