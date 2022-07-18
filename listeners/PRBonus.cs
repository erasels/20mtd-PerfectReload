using flanne;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace PerfectReload.listeners {
    public class PRBonus : MonoBehaviour {
        [SerializeField]
        private float damageBonus = 0.5f;
        [SerializeField]
        private float duration = 1.25f;

        private StatsHolder stats;
        private List<float> timers = new List<float>();

        //This stacks, baby!
        private void OnPerfectReload() {
            stats[StatType.BulletDamage].AddMultiplierBonus(damageBonus);
            //Stack duration on chain success to a max of 5 seconds
            timers.Add(duration + (Math.Min(0.25f * PRMechanic.ChainSuccess, 3.75f)));
        }

        private void Start() {
            PlayerController componentInParent = transform.GetComponentInParent<PlayerController>();
            stats = componentInParent.stats;
            PRMechanic.OnPerfectReload.AddListener(new UnityAction(OnPerfectReload));
        }

        private void OnDestroy() {
            PRMechanic.OnPerfectReload.RemoveListener(new UnityAction(OnPerfectReload));
            timers.Clear();
        }

        private void Update() {
            for (int i = timers.Count - 1; i >= 0; i--) {
                if (timers[i] > 0f) {
                    timers[i] -= Time.deltaTime;
                } else {
                    stats[StatType.BulletDamage].AddMultiplierBonus(-1f * damageBonus);
                    timers.RemoveAt(i);
                }
            }
        }
    }
}

