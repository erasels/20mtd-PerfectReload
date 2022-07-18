using flanne;
using UnityEngine;
using UnityEngine.Events;
using static PerfectReload.PRMechanic;

namespace PerfectReload.listeners {
    public class ChainSuccessManager : MonoBehaviour {
        private UnityEvent rEvent;
        private void OnReload() {
            if (!jankLoaded) {
                ChainSuccess = 0;
            } else {
                ChainSuccess++;
            }
        }

        private void Start() {
            PlayerController componentInParent = transform.GetComponentInParent<PlayerController>();
            rEvent = componentInParent.ammo.OnReload;
            rEvent.AddListener(new UnityAction(OnReload));
        }

        private void OnDestroy() {
            rEvent.RemoveListener(new UnityAction(OnReload));
        }
    }
}
