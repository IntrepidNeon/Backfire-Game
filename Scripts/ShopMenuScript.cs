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

	public void PlayGame()
	{
		SceneManager.LoadScene(1);
	}

	public void ReturnToMenu()
	{
		SceneManager.LoadScene(0);
	}
}
