using UnityEngine;
using DopamineCleaner.Enemies;

namespace DopamineCleaner.Weapons
{
    public class OrbitSatelliteContact : MonoBehaviour
    {
        private OrbitWeapon parentWeapon;

        public void Initialize(OrbitWeapon weapon)
        {
            parentWeapon = weapon;
        }

        private void OnTriggerStay2D(Collider2D other)
        {
            if (other.TryGetComponent<Enemy>(out var enemy))
            {
                parentWeapon?.OnSatelliteHit(enemy);
            }
        }
    }
}
