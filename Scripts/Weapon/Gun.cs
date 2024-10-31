using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting.Dependencies.Sqlite;
using UnityEngine;

public class Gun : ProjectileWeapon
{
	public int MaxBullets;          // Maximum bullets that can be fired before reloading
	public float FireRate;          // Bullets per second
	public float ReloadTime;        // Time to reload to max ammo in seconds
	public bool Magazine;           // If true, reload all bullets at once and dont keep progress

	private int _bullets;
	private bool _isReloading;
	private bool _isFiring;

	private Coroutine _reloadRoutine;

	public override void Equip()
	{
		base.Equip();
		_isReloading = false;
		_isFiring = true;
		StartCoroutine(FireRoutine());
	}
	public override void Unequip()
	{
		CancelReload();
		base.Unequip();
	}

	private void Update()
	{
		Control();
	}

	private void Control()
	{
		if (!gameObject.activeSelf) return;
		if (Input.GetKey(KeyCode.Mouse0))
		{
			Fire();
		}
		if (Input.GetKey(KeyCode.R))
		{
			Reload();
		}
	}
	public void Fire()
	{
		if (_isFiring) return;

		if (_bullets < 1) { Reload(); return; }

		CancelReload();
		_isFiring = true;
		SpawnProjectile();
		_bullets -= 1;
		StartCoroutine(FireRoutine());
	}

	public void Reload()
	{
		if (_bullets == MaxBullets || Reserve < 1 || _isReloading) return;

		CancelReload();
		_isReloading = true;
		_reloadRoutine = StartCoroutine(ReloadRoutine());

	}
	public void CancelReload()
	{
		_isReloading = false;
		if (_reloadRoutine != null) StopCoroutine(_reloadRoutine);

	}
	public IEnumerator FireRoutine()
	{
		yield return new WaitForSeconds(1 / Mathf.Max(FireRate, Mathf.Epsilon));
		_isFiring = false;
	}
	public IEnumerator ReloadRoutine()
	{
		if (Magazine)
		{
			yield return new WaitForSeconds(ReloadTime);
			_bullets = Mathf.Min(MaxBullets, Reserve);
			Reserve -= _bullets;
		}
		else
		{
			while (_bullets < MaxBullets && Reserve > 0)
			{
				yield return new WaitForSeconds(ReloadTime / MaxBullets);
				_bullets += 1;
				Reserve -= 1;
			}
		}
		_isReloading = false;

		Debug.Log("Reload Complete!");
	}
}
