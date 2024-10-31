using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class WeaponController : MonoBehaviour
{
	public float Damage; //Damage Per Hit

	public virtual void Equip()
	{
		gameObject.SetActive(true);
	}
	public virtual void Unequip()
	{
		gameObject.SetActive(false);
	}

}
