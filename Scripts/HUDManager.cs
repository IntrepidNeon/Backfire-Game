using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class HUDManager : MonoBehaviour
{
    [SerializeField]
    private WeaponManager wm;
    [SerializeField]
    private Damagable damagable;

    [SerializeField]
    private Slider healthBarAmount;
    [SerializeField]
    private TMP_Text timeText;
    [SerializeField]
    private TMP_Text ammoText;

    private float seconds;
    private int minutes;

    private void AddTime()
    {
        seconds += Time.deltaTime;
        timeText.text = minutes.ToString("00") + ":" + ((int)seconds).ToString("00");
        if (seconds >= 60)
        {
            ++minutes;
            seconds = 0;
        }
    }

    private void AmmoCount()
    {
        if (wm.MainGun.isActiveAndEnabled)
        {
            ammoText.text = wm.MainGun.Bullets + "/" + wm.MainGun.MaxBullets;
        }
        else if (wm.SpecialGun.isActiveAndEnabled)
        {
            ammoText.text = wm.SpecialGun.Bullets + "/" + wm.SpecialGun.MaxBullets;
        }
    }

    private void HealthUpdate()
    {
        healthBarAmount.value = damagable.Health;
    }

    private void Start()
    {
        healthBarAmount.maxValue = damagable.MaxHealth;
        healthBarAmount.value = healthBarAmount.maxValue;
    }

    private void Update()
    {
        AddTime();
        AmmoCount();
        HealthUpdate();
    }
}
