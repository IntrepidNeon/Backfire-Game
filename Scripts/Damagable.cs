using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Damagable : MonoBehaviour
{
	public int MaxHealth = 100;

	private int _health;

	public int Health
	{
		get { return _health; }
		set { _health = Mathf.Clamp(value, 0, MaxHealth); }
	}

	internal void Awake()
	{
		_health = MaxHealth;
	}
}
