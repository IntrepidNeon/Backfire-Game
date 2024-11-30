using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ShopMenuScript : MonoBehaviour
{
	public int RevolverDamage;
	public int DrumGunDamage;
	public int DrumGunMaxBullets;
	public int PlayerMaxHealth;
	private int _cash;

	public int Cash
	{
		get { return _cash; }
		set { _cash = 0; }
	}

	public void PlayGame()
	{
		SceneManager.LoadScene(1);
	}

	public void ReturnToMenu()
	{
		SceneManager.LoadScene(0);
	}
}
