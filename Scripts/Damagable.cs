using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Damagable : MonoBehaviour
{
	public int MaxHealth = 100;

	private int _health;
	private bool _alive;

	public event Action OnHealthEmpty;

	public bool IsAlive { get { return _alive; } }

	public int Health
	{
		get { return _health; }
		set { _health = Mathf.Clamp(value, 0, MaxHealth); HealthEvents(); }
	}

	private void HealthEvents()
	{
		if (_health <= 0 && _alive)
		{
			OnHealthEmpty?.Invoke();
			_alive = false;
			Debug.Log(gameObject + " died");
		}
		if (Health > 0 && !_alive) _alive = true;
	}

	internal void Awake()
	{
		_health = MaxHealth;
		_alive = true;
	}
}
