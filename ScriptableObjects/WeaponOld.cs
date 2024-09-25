using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[CreateAssetMenu(fileName = "NewWeapon", menuName = "Weapon")]
public class WeaponOld : ScriptableObject
{
	public int damage = 10;
	public int magazineSize = 16;

	public float firerate = 0.25f;
	public float reloadTime = 3f;

	public AudioClip fireSound;
	public AudioClip reloadSound;
}
