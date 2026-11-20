using UnityEngine;
using UnityEngine.EventSystems;

namespace SHOHIN.CityDrive
{
    public sealed class HoldButton : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IPointerExitHandler
    {
        public enum ActionType { Accelerate, Brake, Handbrake, SteerLeft, SteerRight }

        [SerializeField] private ActionType action;
        [SerializeField] private VehicleController vehicle;

        public void Configure(ActionType type, VehicleController target)
        {
            action = type;
            vehicle = target;
        }

        public void OnPointerDown(PointerEventData eventData) => Apply(true);
        public void OnPointerUp(PointerEventData eventData) => Apply(false);
        public void OnPointerExit(PointerEventData eventData) => Apply(false);

        private void Apply(bool pressed)
        {
            if (vehicle == null) vehicle = FindFirstObjectByType<VehicleController>();
            if (vehicle == null) return;

            switch (action)
            {
                case ActionType.Accelerate: vehicle.SetThrottle(pressed); break;
                case ActionType.Brake: vehicle.SetBrake(pressed); break;
                case ActionType.Handbrake: vehicle.SetHandbrake(pressed); break;
                case ActionType.SteerLeft: vehicle.SetSteering(-1f, pressed); break;
                case ActionType.SteerRight: vehicle.SetSteering(1f, pressed); break;
            }
        }
    }
}
