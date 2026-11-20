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

        [Header("HUD")]
        [SerializeField] private Text speedText;

        private Rigidbody body;
        private float steering;
        private bool throttle;
        private bool braking;
        private bool handbrake;

        public float SpeedKph => body == null ? 0f : body.linearVelocity.magnitude * 3.6f;

        private void Awake()
        {
            body = GetComponent<Rigidbody>();
            body.mass = 1450f;
            body.centerOfMass = new Vector3(0f, -0.42f, 0.05f);
            body.interpolation = RigidbodyInterpolation.Interpolate;
            body.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
        }

        private void Update()
        {
            float keyboardSteer = Input.GetAxisRaw("Horizontal");
            steering = Mathf.Abs(keyboardSteer) > 0.01f ? keyboardSteer : steering;
            throttle |= Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.UpArrow);
            braking |= Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.DownArrow);
            handbrake |= Input.GetKey(KeyCode.Space);

            if (speedText != null)
                speedText.text = Mathf.RoundToInt(SpeedKph).ToString("000") + " KM/H";

            UpdateWheelVisual(frontLeft, frontLeftVisual);
            UpdateWheelVisual(frontRight, frontRightVisual);
            UpdateWheelVisual(rearLeft, rearLeftVisual);
            UpdateWheelVisual(rearRight, rearRightVisual);

            // Keyboard inputs are sampled each frame; touch buttons set their own values.
            if (!Input.GetKey(KeyCode.W) && !Input.GetKey(KeyCode.UpArrow) && !TouchThrottle)
                throttle = false;
            if (!Input.GetKey(KeyCode.S) && !Input.GetKey(KeyCode.DownArrow) && !TouchBrake)
                braking = false;
            if (!Input.GetKey(KeyCode.Space) && !TouchHandbrake)
                handbrake = false;
            if (!TouchSteering)
                steering = keyboardSteer;
        }

        private bool TouchThrottle { get; set; }
        private bool TouchBrake { get; set; }
        private bool TouchHandbrake { get; set; }
        private bool TouchSteering { get; set; }

        public void SetThrottle(bool pressed)
        {
            TouchThrottle = pressed;
            throttle = pressed || Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.UpArrow);
        }

        public void SetBrake(bool pressed)
        {
            TouchBrake = pressed;
            braking = pressed || Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.DownArrow);
        }

        public void SetHandbrake(bool pressed)
        {
            TouchHandbrake = pressed;
            handbrake = pressed || Input.GetKey(KeyCode.Space);
        }

        public void SetSteering(float value, bool pressed)
        {
            TouchSteering = pressed;
            if (pressed) steering = Mathf.Clamp(value, -1f, 1f);
            else steering = Input.GetAxisRaw("Horizontal");
        }

        private void FixedUpdate()
        {
            float speedFactor = Mathf.Clamp01(1f - SpeedKph / maxSpeedKph);
            float torque = throttle && !braking ? motorTorque * speedFactor : 0f;
            float brake = braking ? brakeTorque : 0f;

            frontLeft.steerAngle = steering * maxSteerAngle;
            frontRight.steerAngle = steering * maxSteerAngle;

            rearLeft.motorTorque = torque * 0.5f;
            rearRight.motorTorque = torque * 0.5f;

            frontLeft.brakeTorque = brake;
            frontRight.brakeTorque = brake;
            rearLeft.brakeTorque = handbrake ? brakeTorque * 1.35f : brake;
            rearRight.brakeTorque = handbrake ? brakeTorque * 1.35f : brake;

            body.AddForce(-transform.up * (downforce * body.linearVelocity.magnitude), ForceMode.Force);
        }

        private static void UpdateWheelVisual(WheelCollider wheel, Transform visual)
        {
            if (wheel == null || visual == null) return;
            wheel.GetWorldPose(out Vector3 position, out Quaternion rotation);
            visual.SetPositionAndRotation(position, rotation);
        }
    }
}
