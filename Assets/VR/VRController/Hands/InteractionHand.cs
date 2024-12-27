using System.Collections;
using System.Linq;
using UnityEngine;

namespace VRController.Hands
{
    public class InteractionHand : VRHand
    {
        [SerializeField] private float overlapDistance = 0.1f;
        [SerializeField] private InteractionHand otherHand;
        [SerializeField] private Transform handAnchor;
        [SerializeField] private LayerMask grabLayer;

        [SerializeField] private float magnetDuration = 0.5f;
        [SerializeField] private AudioSource throwAudioSource;
        [SerializeField] private AudioSource pickupAudioSource;
        
        [SerializeField] private float soundVelocityThreshold = 2.0f;
        [SerializeField] private GameObject handObject;
        [SerializeField] private float lowSpeedThreshold = 0.1f;

        [SerializeField] private float velocitySampleInterval = 0.024f;
        [SerializeField] private int velocitySamples = 10;
        
        private VelocityTracker _velocityTracker;
        private bool _trackingVelocity;
        
        private GameObject _inHandObject;
        private readonly Collider[] _colliders = new Collider[1];

        private float _blend;
        private float _lastBlend;
        private float _blendBackTime;

        private void Start()
        {
            _velocityTracker = new VelocityTracker(velocitySamples, velocitySampleInterval);
            _trackingVelocity = false;
        }

        protected override void OnEnable()
        {
            base.OnEnable();
            animator = handObject.GetComponentInChildren<Animator>();
        }

        private void SwitchHands() => _velocityTracker.Clear();

        private void HandleVelocityTrackerClear(Vector3 velocity)
        {
            if (velocity.magnitude < lowSpeedThreshold) _velocityTracker.Clear();
        }

        private void ClearVelocity()
        {
            _inHandObject = null;
            _velocityTracker.Clear();
        }

        private void FixedUpdate()
        {
            if (!_inHandObject && !_trackingVelocity) return;
            
            if (!_inHandObject)
            {
                ClearVelocity();
                return;
            }
            
            _velocityTracker.TrackPosition(_inHandObject.transform.position, Time.time);
            HandleVelocityTrackerClear(_velocityTracker.GetAverageVelocity());
        }

        protected override void OnInteraction()
        {
            if (UnityEngine.Physics.OverlapBoxNonAlloc(handAnchor.transform.position, Vector3.one * overlapDistance, _colliders,
                    handAnchor.transform.rotation, grabLayer) <= 0) return;

            var ballCollider = _colliders.FirstOrDefault();

            if (ballCollider) _inHandObject = ballCollider.gameObject;
            else
            {
                Debug.LogError("Found collider on ball layer with no baseball script attached.");
                return;
            }
            base.OnInteraction();
            
            var rb = ballCollider.GetComponent<Rigidbody>();
            if (!rb.isKinematic) rb.velocity = Vector3.zero;
            
            ballCollider.enabled = false;
            otherHand.SwitchHands();
            PlayHapticImpulse(0.15f, 0.05f);
            
            StartCoroutine(MagnetBallToHand(rb.gameObject));
        }
        
        private IEnumerator MagnetBallToHand(GameObject ball)
        {
            var rb = ball.GetComponent<Rigidbody>();
            rb.isKinematic = false;
            rb.useGravity = false;
            rb.velocity = Vector3.zero;
            rb.isKinematic = true;

            rb.GetComponent<Collider>().enabled = false;
            
            var elapsedTime = 0f;
            var initialPosition = ball.transform.position;
            var initialRotation = ball.transform.rotation;

            SFXManager.PlayRandomCatch(pickupAudioSource);
            PlayHapticImpulse(0.35f, magnetDuration);
            while (elapsedTime < magnetDuration)
            {
                elapsedTime += Time.deltaTime;
                var t = elapsedTime / magnetDuration;

                ball.transform.position = Vector3.Lerp(initialPosition, handAnchor.position, t);
                ball.transform.rotation = Quaternion.Slerp(initialRotation, handAnchor.rotation, t);

                yield return null;
            }
            
            if (!_inHandObject)

            ball.transform.SetParent(handAnchor);
            ball.transform.position = handAnchor.position;
            rb.transform.localPosition = Vector3.zero;
            ball.transform.rotation = handAnchor.rotation;

            rb.isKinematic = false;
            rb.useGravity = false;
            rb.velocity = Vector3.zero;
            rb.isKinematic = true;

            _inHandObject = ball;
            _trackingVelocity = true;
        }

        protected override void OnInteractionCancelled()
        {
            base.OnInteractionCancelled();
            if (_inHandObject == null) return;
            var ball = _inHandObject;
            _inHandObject.transform.parent = null;
            _inHandObject = null;
            
            StopCoroutine(MagnetBallToHand(ball));
            
            var velocity = _velocityTracker.GetAverageVelocity();
            _velocityTracker.Clear();

            SFXManager.PlayRandomThrow(throwAudioSource, Mathf.Lerp(0, 1,
                velocity.magnitude / soundVelocityThreshold));
            
            PlayHapticImpulse(0.1f, 0.1f);
            var rb = ball.GetComponent<Rigidbody>();
            rb.isKinematic = false;
            rb.GetComponent<Collider>().enabled = true;
            
            // throws the object, make sure that the object is not kinematic
            rb.velocity = velocity;
        }
    }
}