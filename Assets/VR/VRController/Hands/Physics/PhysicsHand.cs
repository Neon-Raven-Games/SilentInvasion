using System;
using UnityEngine;

namespace VRController.Hands.Physics
{
    public class PhysicsHand : MonoBehaviour
    {
        [SerializeField] private Transform target;
        [SerializeField] private float positionSpeed = 1.5f;
        [SerializeField] private float rotationSpeed = 1.5f;

        private Rigidbody _rb;

        private Quaternion _deltaRotation;
        private Vector3 _angularDisplacement;
        private Vector3 _angularVelocity;
        
        private void Start()
        {
            _rb = GetComponent<Rigidbody>();
        }

        private void Update()
        {
            if (target.localPosition == Vector3.zero) return;
            // _rb.MovePosition(Vector3.Lerp(transform.position, target.position, positionSpeed * Time.fixedDeltaTime));
            _rb.MoveRotation(Quaternion.Slerp(transform.rotation, target.rotation, // * _rotationOffsetQuat,
                rotationSpeed * Time.fixedDeltaTime));
        }

        private void FixedUpdate()
        {
            if (target.localPosition == Vector3.zero) return;
            _rb.velocity = (target.position - transform.position) / Time.fixedDeltaTime;
        }
    }
}