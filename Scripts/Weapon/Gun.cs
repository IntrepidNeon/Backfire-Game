using System;
using System.Collections;
using UnityEngine;

public class Gun : ProjectileWeapon
{
	public int MaxBullets;          // Maximum bullets that can be fired before reloading
	public int Bullets;             // Number of bullets currently in gun
	public float FireRate;          // Bullets per second
	public float ReloadTime;        // Time to reload to max ammo in seconds
	public bool Magazine;           // If true, reload all bullets at once and dont keep progress
	public bool Automatic;          // Whether holding down the fire button continuously fires bullets
	public AudioClip shootSFX;      // SFX for gunshot
	public AudioSource audioSource; // Audio source to play gunshots

	public event Action OnReloadStart;

	public Coroutine ReloadRoutine, FireRoutine;

	public bool IsReloading { get { return _isReloading; } }

	private bool _isReloading;
	private bool _isFiring;

    private void Start()
    {
		audioSource = GetComponent<AudioSource>();
    }

    public override void Equip()
	{
		CancelReload();

		base.Equip();

		_isFiring = true;
		StartCoroutine(FireIEnumerator());
	}
	public override void Unequip()
	{
		CancelReload();

		base.Unequip();
	}
	public void Fire()
	{
		if (!gameObject.activeSelf) return;

		if (_isFiring) return;

		if (Bullets < 1) { Reload(); return; }

		CancelReload();

		SpawnProjectile();
		Bullets -= 1;
		StartCoroutine(FireIEnumerator());
		audioSource.PlayOneShot(shootSFX, .7f);
		_isFiring = true;

	}

	public void Reload()
	{
		if (!gameObject.activeSelf) return;

		if (Bullets == MaxBullets || _isReloading /*|| Reserve < 1*/) return;

		if (Magazine)
		{
			//Reserve += Bullets;
			Bullets = 0;
		}
		CancelReload();
		ReloadRoutine = StartCoroutine(ReloadIEnumerator());
		_isReloading = true;
		OnReloadStart?.Invoke();

	}
	public void CancelReload()
	{
		if (!gameObject.activeSelf) return;
		_isReloading = false;
		if (ReloadRoutine != null) StopCoroutine(ReloadRoutine);
	}

	public IEnumerator FireIEnumerator()
	{
		yield return new WaitForSeconds(1 / Mathf.Max(FireRate, Mathf.Epsilon));
		_isFiring = false;
	}
	public IEnumerator ReloadIEnumerator()
	{
		if (Magazine)
		{
			yield return new WaitForSeconds(ReloadTime);
			Bullets = MaxBullets;// Mathf.Min(MaxBullets, Reserve);
								 //Reserve -= Bullets;
		}
		else
		{
			while (Bullets < MaxBullets)// && Reserve > 0)
			{
				yield return new WaitForSeconds(ReloadTime / MaxBullets);
				Bullets += 1;
				//Reserve -= 1;
			}
		}
		_isReloading = false;
	}
}
