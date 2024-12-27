using System.Collections.Generic;
using UnityEngine;

namespace VRController.Hands
{
    public class VelocityTracker
    {
        private readonly Queue<Vector3> _positions = new();
        private readonly Queue<float> _timestamps = new();
        private readonly int _maxSamples;
        private readonly float _samplingInterval;
        private float _lastSampleTime;

        private Vector3 _lastPosition;
        private Vector3 _secondLastPosition;

        private float _lastTimeStamp;
        private float _secondLastTimeStamp;
        
        public VelocityTracker(int maxSamples = 5, float samplingInterval = 0.05f)
        {
            _maxSamples = maxSamples;
            _samplingInterval = samplingInterval;
            _lastSampleTime = -1;
        }

        public void TrackPosition(Vector3 position, float currentTime)
        {
            if (_lastSampleTime < 0)
            {
                // First sample initialization
                _lastSampleTime = currentTime;
                _lastPosition = position;
                _lastTimeStamp = currentTime;
                return;
            }

            if (currentTime - _lastSampleTime >= _samplingInterval)
            {
                _secondLastPosition = _lastPosition;
                _secondLastTimeStamp = _lastTimeStamp;

                _lastPosition = position;
                _lastTimeStamp = currentTime;

                if (_positions.Count >= _maxSamples)
                {
                    _positions.Dequeue();
                    _timestamps.Dequeue();
                }

                _positions.Enqueue(position);
                _timestamps.Enqueue(currentTime);

                _lastSampleTime = currentTime;
            }
        }

        public Vector3 GetAverageVelocity()
        {
            if (_positions.Count < 2) return Vector3.zero;

            var totalDisplacement = Vector3.zero;
            float totalTime = 0;

            var previousPosition = _positions.Peek();
            var previousTime = _timestamps.Peek();
            var timestampsArray = _timestamps.ToArray();
            int index = 0;

            foreach (var position in _positions)
            {
                if (index == 0)
                {
                    previousPosition = position;
                    previousTime = timestampsArray[index];
                    index++;
                    continue;
                }

                float deltaTime = timestampsArray[index] - previousTime;
                if (deltaTime > Mathf.Epsilon)
                {
                    totalDisplacement += position - previousPosition;
                    totalTime += deltaTime;
                }

                previousPosition = position;
                previousTime = timestampsArray[index];
                index++;
            }

            return totalTime > 0 ? totalDisplacement / totalTime : Vector3.zero;
        }


        public Vector3 GetWeightedAverageVelocity()
        {
            if (_positions.Count < 2) return Vector3.zero;

            var cumulativeVelocity = Vector3.zero;
            var totalWeight = 0f;

            var previousPosition = _positions.Peek();
            var previousTime = _timestamps.Peek();

            var index = 0;
            var timeStamps = _timestamps.ToArray();
            foreach (var position in _positions)
            {
                if (index++ == 0) continue;

                var currentTime = timeStamps[index - 1];  
                var deltaTime = currentTime - previousTime;

                if (deltaTime > 0)
                {
                    cumulativeVelocity += (position - previousPosition) / deltaTime * deltaTime;
                    totalWeight += deltaTime;
                }

                previousPosition = position;
                previousTime = currentTime;
            }

            return totalWeight > 0 ? cumulativeVelocity / totalWeight : Vector3.zero;
        }


        public Vector3 GetFilteredAverageVelocity(float outlierThreshold = 1.5f)
        {
            if (_positions.Count < 2) return Vector3.zero;

            var filteredVelocity = Vector3.zero;
            var validSamples = 0;

            var previousPosition = _positions.Peek();
            var previousTime = _timestamps.Peek();

            var timeStamps = _timestamps.ToArray();
            var index = 0;
            
            foreach (var currentPosition in _positions)
            {
                if (index++ == 0) continue;

                var currentTime = timeStamps[index - 1]; 
                var deltaTime = currentTime - previousTime;

                if (deltaTime > 0)
                {
                    Vector3 velocity = (currentPosition - previousPosition) / deltaTime;
                    if (velocity.magnitude <= outlierThreshold)
                    {
                        filteredVelocity += velocity;
                        validSamples++;
                    }
                }

                previousPosition = currentPosition;
                previousTime = currentTime;
            }

            return validSamples > 0 ? filteredVelocity / validSamples : Vector3.zero;
        }


        public Vector3 GetCurrentSpeed()
        {
            if (_lastTimeStamp <= _secondLastTimeStamp) return Vector3.zero;

            var deltaTime = _lastTimeStamp - _secondLastTimeStamp;

            return deltaTime > 0 ? (_lastPosition - _secondLastPosition) / deltaTime : Vector3.zero;
        }


        public void Clear()
        {
            _positions.Clear();
            _timestamps.Clear();
            _lastSampleTime = -1;
            _lastPosition = Vector3.zero;
            _secondLastPosition = Vector3.zero;
            _lastTimeStamp = 0;
            _secondLastTimeStamp = 0;
        }

        public Vector3 GetLastVelocity()
        {
            if (_positions.Count < 2) return Vector3.zero;

            Vector3 displacement = _lastPosition - _secondLastPosition;
            float deltaTime = _lastTimeStamp - _secondLastTimeStamp;

            return deltaTime > 0 ? displacement / deltaTime : Vector3.zero;
        }

    }
}