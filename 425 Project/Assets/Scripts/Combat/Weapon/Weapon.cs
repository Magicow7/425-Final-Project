using Stat;
using UnityEngine;

namespace Combat.Weapon
{
    public abstract class Weapon : MonoBehaviour
    {
        [SerializeField] protected float _manaPerShot = 20;

        [SerializeField] protected float _damage = 10;


        private void LateUpdate()
        {
            if (!PlayerStats.Instance)
            {
                return;
            }

            if (Input.GetButtonDown("Fire1"))
            {
                FireStart();
            }

            if (Input.GetButtonUp("Fire1"))
            {
                FireEnd();
            }

            if (Input.GetButton("Fire1"))
            {
                if (PlayerStats.Instance.Mana.Value > _manaPerShot)
                {
                    if (Fire())
                    {
                        PlayerStats.Instance.Mana.Value -= _manaPerShot;
                    }
                }
                else
                {
                    FireEnd();
                }
            }
        }

        public abstract void SetStats();

        protected abstract bool Fire();

        protected virtual void FireStart() { }
        protected virtual void FireEnd() { }
    }
}