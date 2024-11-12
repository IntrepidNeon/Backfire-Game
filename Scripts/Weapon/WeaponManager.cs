using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeaponManager : MonoBehaviour
{
	public Gun MainGunPrefab, SpecialGunPrefab;

	[System.NonSerialized]
	public Gun MainGun, SpecialGun;

	[System.NonSerialized]
	public WeaponController ActiveWeapon;

	private void OnEnable()
	{
		if (SpecialGun)
		{
			SpecialGun.OnReloadStart += SpecialReloadStart;
		}
	}
	private void OnDisable()
	{
		if (SpecialGun)
		{
			SpecialGun.OnReloadStart -= SpecialReloadStart;
		}
	}

	private void Update()
	{
		Control();
	}

	private void SpecialReloadStart()
	{
        if (MainGun) EquipWeapon(MainGun);
		if (!SpecialGun.IsReloading) SpecialGun.ReloadRoutine = StartCoroutine(SpecialGun.ReloadIEnumerator());
	}

	private void Awake()
	{
		if (SpecialGunPrefab)
		{
			SpecialGun = Instantiate(SpecialGunPrefab, transform);
			SpecialGun.Unequip();

			SpecialReloadStart();
		}

		if (MainGunPrefab)
		{
			MainGun = Instantiate(MainGunPrefab, transform);
			MainGun.Unequip();
		}
		EquipWeapon(MainGun);
	}

	public void EquipWeapon(WeaponController equipWeapon)
	{
		if (equipWeapon == SpecialGun && SpecialGun.Bullets < 1) return;
		if (ActiveWeapon != equipWeapon) ActiveWeapon?.Unequip();
		else return;

		equipWeapon.Equip();
		ActiveWeapon = equipWeapon;
	}

	private void Control()
	{
		if (ActiveWeapon is Gun)
		{
			Gun activeGun = (Gun)ActiveWeapon;
			if (Input.GetKey(KeyCode.Mouse0))
			{
				activeGun.Fire();
			}
			if (Input.GetKey(KeyCode.R))
			{
				activeGun.Reload();
			}
		}

		if (Input.GetKey(KeyCode.Alpha1))
		{
			EquipWeapon(MainGun);
		}
		if (Input.GetKey(KeyCode.Alpha2))
		{
			EquipWeapon(SpecialGun);
		}
	}


}
