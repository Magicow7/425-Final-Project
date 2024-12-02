using System;
using System.Collections;
using System.Collections.Generic;
using Stat;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UI;
using Utils.Singleton;

namespace Combat
{
    [SingletonAttribute(SingletonCreationMode.Auto, false)]
    public class Player : SingletonMonoBehaviour<Player>, IDamageable
    {
        public bool TakeDamage(float damage)
        {
            if (PlayerStats.Instance != null && !PlayerStats.Instance.Health.TrySpendResource(damage))
            {
                PlayerStats.Instance.Health.Value = 0;
                Death();
                return false;
            }
            
            SoundManager.PlaySound(SoundManager.Sound.PlayerHit);

            return true;
        }

        public void Death()
        {
            TextUpdates.Instance.ShowDeathScreen();
            // Player HP too low, cannot take another hit.
            SoundManager.PlaySound(SoundManager.Sound.PlayerDeath);
        }
    }
}