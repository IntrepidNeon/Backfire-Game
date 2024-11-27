using UnityEngine;

public abstract class WeaponController : MonoBehaviour
{
	public int Damage; //Damage Per Hit

	public virtual void Equip()
	{
		gameObject.SetActive(true);
	}
	public virtual void Unequip()
	{
		gameObject.SetActive(false);
	}
	public void DoDamage(Damagable receiver)
	{
		receiver.Health -= Damage;
	}

}
