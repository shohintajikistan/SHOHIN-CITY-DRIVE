using UnityEngine;
using UnityEngine.UI;

namespace SHOHIN.CityDrive
{
    [RequireComponent(typeof(Rigidbody))]
    public sealed class VehicleController : MonoBehaviour
    {
        [Header("Wheel colliders")]
        [SerializeField] private WheelCollider frontLeft;
        [SerializeField] private WheelCollider frontRight;
        [SerializeField] private WheelCollider rearLeft;
        [SerializeField] private WheelCollider rearRight;

        [Header("Wheel visuals")]
        [SerializeField] private Transform frontLeftVisual;
        [SerializeField] private Transform frontRightVisual;
        [SerializeField] private Transform rearLeftVisual;
        [SerializeField] private Transform rearRightVisual;

        [Header("Vehicle tuning")]
        [SerializeField, Min(100f)] private float motorTorque = 950f;
        [SerializeField, Min(100f)] private float brakeTorque = 2600f;
        [SerializeField, Range(15f, 42f)] private float maxSteerAngle = 30f;
        [SerializeField, Min(20f)] private float maxSpeedKph = 220f;
        [SerializeField] private float downforce = 45f;
        [SerializeField] private float steeringResponse = 95f;

        [Header("HUD")]
        [SerializeField] private Text speedText;

        private Rigidbody body;
        private float steeringInput;
        private float currentSteerAngle;
        private bool throttle;
        private bool braking;
        private bool handbrake;

        public float SpeedKph => body == null ? 0f : body.linearVelocity.magnitude * 3.6f;

        private void Awake()
        {
            body = GetComponent<Rigidbody>();
            body.mass = 1450f;
            body.centerOfMass = new Vector3(0f, 0.18f, 0.05f);
            body.interpolation = RigidbodyInterpolation.Interpolate;
            body.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
        }

        private void Update()
        {
            if (speedText != null)
                speedText.text = Mathf.RoundToInt(SpeedKph).ToString("000") + " KM/H";

            UpdateWheelVisual(frontLeft, frontLeftVisual);
            UpdateWheelVisual(frontRight, frontRightVisual);
            UpdateWheelVisual(rearLeft, rearLeftVisual);
            UpdateWheelVisual(rearRight, rearRightVisual);
        }

        public void SetThrottle(bool pressed) => throttle = pressed;
        public void SetBrake(bool pressed) => braking = pressed;
        public void SetHandbrake(bool pressed) => handbrake = pressed;

        public void SetSteering(float value, bool pressed)
        {
            steeringInput = pressed ? Mathf.Clamp(value, -1f, 1f) : 0f;
        }

        private void FixedUpdate()
        {
            float speedSteerLimit = Mathf.Lerp(maxSteerAngle, maxSteerAngle * 0.45f, Mathf.Clamp01(SpeedKph / 160f));
            currentSteerAngle = Mathf.MoveTowards(
                currentSteerAngle,
                steeringInput * speedSteerLimit,
                steeringResponse * Time.fixedDeltaTime);

            float speedFactor = Mathf.Clamp01(1f - SpeedKph / maxSpeedKph);
            bool reverse = braking && SpeedKph < 2.5f && !throttle;
            float driveTorque = throttle && !braking
                ? motorTorque * speedFactor
                : reverse ? -motorTorque * 0.55f : 0f;
            float serviceBrake = braking && !reverse ? brakeTorque : 0f;

            frontLeft.steerAngle = currentSteerAngle;
            frontRight.steerAngle = currentSteerAngle;

            rearLeft.motorTorque = driveTorque * 0.5f;
            rearRight.motorTorque = driveTorque * 0.5f;

            frontLeft.brakeTorque = serviceBrake;
            frontRight.brakeTorque = serviceBrake;
            rearLeft.brakeTorque = handbrake ? brakeTorque * 1.35f : serviceBrake;
            rearRight.brakeTorque = handbrake ? brakeTorque * 1.35f : serviceBrake;

            body.AddForce(-transform.up * (downforce * body.linearVelocity.magnitude), ForceMode.Force);
        }

        private static void UpdateWheelVisual(WheelCollider wheel, Transform visual)
        {
            if (wheel == null || visual == null) return;
            wheel.GetWorldPose(out Vector3 position, out Quaternion rotation);
            visual.SetPositionAndRotation(position, rotation * Quaternion.Euler(0f, 0f, 90f));
        }
    }
}
