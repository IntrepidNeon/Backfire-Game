using System;
using System.Collections;
using UnityEngine;
using Random = UnityEngine.Random;

namespace DefaultNamespace
{
	public class WeaponController : MonoBehaviour
	{
		public WeaponOld weapon;
		public Camera gameCamera;

		public TrailRenderer bulletTrail;

		private int bulletsLeft;
		private float shootCooldown, reloadCooldown;

		private void Start()
		{
			bulletsLeft = weapon.magazineSize;
		}

		private void FixedUpdate()
		{
			shootCooldown = Mathf.Max(0, shootCooldown - Time.fixedDeltaTime);
			reloadCooldown = Mathf.Max(0, reloadCooldown - Time.fixedDeltaTime);
			CheckInputs();
		}

		private void CheckInputs()
		{
			if (Input.GetAxis("Fire1") > 0 && shootCooldown == 0 && reloadCooldown == 0)
			{
				if (bulletsLeft > 0) { Shoot(); } else { Reload(); }
			}
		}

		private void Shoot()
		{
			shootCooldown = weapon.firerate;
			
			StartCoroutine(PlaySoundOnce(weapon.fireSound));

			Ray aimRay = gameCamera.ScreenPointToRay(Input.mousePosition);
			Vector3 mouseWorldPosition = gameCamera.ScreenToWorldPoint(Input.mousePosition);
			mouseWorldPosition.z = 0f;

			TrailRenderer trail = Instantiate(bulletTrail, mouseWorldPosition, Quaternion.identity);
			if (Physics.Raycast(aimRay.origin, aimRay.direction, out RaycastHit rayHit, 64))
			{
				StartCoroutine(SpawnTrail(trail, rayHit.point));
				//TODO DAMAGE HERE
			}
			else
			{
				StartCoroutine(SpawnTrail(trail, aimRay.GetPoint(10)));
			}

			StartCoroutine(Shake());

			bulletsLeft--;
		}

		private void Reload()
		{
			reloadCooldown = weapon.reloadTime;
			bulletsLeft = weapon.magazineSize;

			StartCoroutine(PlaySoundOnce(weapon.reloadSound));
		}

		private IEnumerator SpawnTrail(TrailRenderer Trail, Vector3 HitPoint)
		{
			Vector3 startPosition = gameCamera.transform.position;
			float distance = Vector3.Distance(startPosition, HitPoint);
			float remainingDistance = distance;

			while (remainingDistance > 0)
			{
				Trail.transform.position = Vector3.Lerp(startPosition, HitPoint, 1 - (remainingDistance / distance));

				remainingDistance -= Time.fixedDeltaTime * 10;

				yield return null;
			}
			Trail.transform.position = HitPoint;

			Destroy(Trail.gameObject);
		}

		private IEnumerator Shake()
		{
			Quaternion originalRot = gameCamera.transform.localRotation;

			float elapsed = 0.0f;
			while (elapsed < 0.3)
			{
				float x = Random.Range(-1f, 1f) * 0.2f;
				float y = Random.Range(-1f, 1f) * 0.2f;

				gameCamera.transform.localRotation *= Quaternion.Euler(x, y, 0);

				elapsed += Time.fixedDeltaTime;

				yield return null;
			}

			gameCamera.transform.localRotation = originalRot;
		}

		// ReSharper disable Unity.PerformanceAnalysis
		private IEnumerator PlaySoundOnce(AudioClip clip)
		{
			AudioSource audio = transform.parent.gameObject.AddComponent(typeof(AudioSource)) as AudioSource;
			audio.clip = clip;
			audio.Play();
			
			do yield return null; while(audio.isPlaying);
			
			Destroy(audio);
		}
	}
}
