using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting.Dependencies.Sqlite;
using UnityEngine;

public class Gun : WeaponController
{
	public GameObject Projec

	public int MaxBullets;          //Number of bullets in magazine
	public float FireRate;          //Bullets per second
	public float ReloadTime;        //Time to reload to max ammo in seconds
	public bool Magazine;           //If true, reload all at once and dont keep progress

	protected int _bullets;
	protected bool _isReloading;
	protected float _reloadProgress;

	public float ReloadProgress { get { return _reloadProgress; } }
	private void FixedUpdate()
	{
		//RELOAD OVER TIME
		if (_isReloading)
		{
			if (ReloadProgress >= 1f)
			{
				_isReloading = false;
				_bullets = MaxBullets;
			}
			else if (!Magazine) _bullets = (int)(MaxBullets * _reloadProgress);
			_reloadProgress += ReloadTime * Time.fixedDeltaTime;
		}
	}

	public override void Equip()
	{
		base.Equip();
	}
	public override void Unequip()
	{
		base.Unequip();
		_isReloading = false;
	}

	public void SetBullets(int newBullets)
	{
		_bullets = newBullets;
	}

	public void Reload()
	{
		if (_bullets == MaxBullets) return;

		_isReloading = true;

		_reloadProgress = Magazine ? 0f : _bullets;
	}
}
