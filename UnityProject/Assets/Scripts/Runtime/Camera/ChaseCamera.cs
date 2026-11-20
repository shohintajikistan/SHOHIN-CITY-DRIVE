using UnityEngine;

namespace SHOHIN.CityDrive
{
    public sealed class ChaseCamera : MonoBehaviour
    {
        [SerializeField] private Transform target;
        [SerializeField] private Vector3 offset = new Vector3(0f, 4.2f, -8.5f);
        [SerializeField, Range(1f, 20f)] private float positionSmooth = 7f;
        [SerializeField, Range(1f, 20f)] private float rotationSmooth = 8f;
        [SerializeField] private float lookAhead = 2.5f;

        public void SetTarget(Transform value) => target = value;

        private void LateUpdate()
        {
            if (target == null) return;
            Vector3 desiredPosition = target.TransformPoint(offset);
            transform.position = Vector3.Lerp(transform.position, desiredPosition, Time.deltaTime * positionSmooth);
            Vector3 lookPoint = target.position + target.forward * lookAhead + Vector3.up * 1.1f;
            Quaternion desiredRotation = Quaternion.LookRotation(lookPoint - transform.position, Vector3.up);
            transform.rotation = Quaternion.Slerp(transform.rotation, desiredRotation, Time.deltaTime * rotationSmooth);
        }
    }
}
