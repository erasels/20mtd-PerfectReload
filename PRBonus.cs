using flanne;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace PerfectReload {
    public class PRBonus : MonoBehaviour {
        [SerializeField]
        private float damageBonus = 0.5f;
        [SerializeField]
        private float duration = 1.5f;

        private StatsHolder stats;
        private List<float> timers = new List<float>();

        //This stacks, baby!
        private void OnPerfectReload() {
            stats[StatType.BulletDamage].AddMultiplierBonus(damageBonus);
            timers.Add(duration);
        }

        private void Start() {
            PlayerController componentInParent = transform.GetComponentInParent<PlayerController>();
            stats = componentInParent.stats;
            PRConstants.plugin.OnPerfectReload.AddListener(new UnityAction(OnPerfectReload));
        }

        private void OnDestroy() {
            PRConstants.plugin.OnPerfectReload.RemoveListener(new UnityAction(OnPerfectReload));
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

