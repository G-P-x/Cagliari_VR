using System.Collections.Generic;
using UnityEngine;
using System;

namespace Oculus.Interaction
{
    /// <summary>
    /// A Transformer that transforms the target in a free form way for an intuitive
    /// two hand translation, rotation and scale.
    /// </summary>
    public class TwoGrabFreeTransform_y : MonoBehaviour, ITransformer
    {
        // The active rotation for this transformation is tracked because it
        // cannot be derived each frame from the grab point information alone.
        private Quaternion _activeRotation;
        private Vector3 _initialLocalScale;
        private float _initialDistance;
        private float _initialScale = 1.0f;
        private float _activeScale = 1.0f;
        public GameObject myMap;

        private Pose _previousGrabPointA;
        private Pose _previousGrabPointB;
        [Serializable]
        public class TwoGrabFreeConstraints
        {
            [Tooltip("If true then the constraints are relative to the initial scale of the object " +
                     "if false, constraints are absolute with respect to the object's selected axes.")]
            public bool ConstraintsAreRelative;
            public FloatConstraint MinScale;
            public FloatConstraint MaxScale;
            public bool ConstrainXScale = true;
            public bool ConstrainYScale = false;
            public bool ConstrainZScale = false;
        }

        [SerializeField]
        private TwoGrabFreeConstraints _constraints;

        public TwoGrabFreeConstraints Constraints
        {
            get
            {
                return _constraints;
            }

            set
            {
                _constraints = value;
            }
        }

        private IGrabbable _grabbable;

        public void Initialize(IGrabbable grabbable)
        {
            _grabbable = grabbable;
        }

        public void BeginTransform()
        {
            var grabA = _grabbable.GrabPoints[0];
            var grabB = _grabbable.GrabPoints[1];

            // Initialize our transformer rotation
            Vector3 diff = grabB.position - grabA.position;
            _activeRotation = Quaternion.LookRotation(diff, Vector3.up).normalized;
            Debug.LogError("BEGIN activeRotation: " + _activeRotation.eulerAngles);
            _initialDistance = diff.magnitude;
            if (!_constraints.ConstraintsAreRelative)
            {
                _activeScale = _grabbable.Transform.localScale.x;
            }
            _initialScale = _activeScale;
            _initialLocalScale = _grabbable.Transform.localScale / _initialScale;

            _previousGrabPointA = new Pose(grabA.position, grabA.rotation);
            _previousGrabPointB = new Pose(grabB.position, grabB.rotation);
        }

        public void UpdateTransform()
        {
            var grabA = _grabbable.GrabPoints[0];
            var grabB = _grabbable.GrabPoints[1];
            var targetTransform = _grabbable.Transform;

            // Use the centroid of our grabs as the transformation center
            Vector3 initialCenter = Vector3.Lerp(_previousGrabPointA.position, _previousGrabPointB.position, 0.5f);
            Vector3 targetCenter = Vector3.Lerp(grabA.position, grabB.position, 0.5f);

            // Our transformer rotation is based off our previously saved rotation
            Quaternion initialRotation = _activeRotation;

            // The base rotation is based on the delta in vector rotation between grab points
            Vector3 initialVector = _previousGrabPointB.position - _previousGrabPointA.position;
            Debug.LogError("initialVector: " + initialVector);
            Vector3 targetVector = grabB.position - grabA.position;
            Debug.LogError("targetVector: " + targetVector);
            // (Vector3 initialVector, Vector3 targetVector) = ComputeVectors(grabA, grabB);
            Quaternion baseRotation = Quaternion.FromToRotation(initialVector, targetVector);
            Debug.LogError("baseRotation: " + baseRotation.eulerAngles);

            // Any local grab point rotation contributes 50% of its rotation to the final transformation
            // If both grab points rotate the same amount locally, the final result is a 1-1 rotation
            Quaternion deltaA = grabA.rotation * Quaternion.Inverse(_previousGrabPointA.rotation);
            Quaternion halfDeltaA = Quaternion.Slerp(Quaternion.identity, deltaA, 0.5f);

            Quaternion deltaB = grabB.rotation * Quaternion.Inverse(_previousGrabPointB.rotation);
            Quaternion halfDeltaB = Quaternion.Slerp(Quaternion.identity, deltaB, 0.5f);

            // Apply all the rotation deltas (provo a metterlo qua la rotazione solo su y)
            // float x = baseRotation.eulerAngles.x * halfDeltaA.eulerAngles.x * halfDeltaB.eulerAngles.x * initialRotation.eulerAngles.x;
            // float y = baseRotation.eulerAngles.y * halfDeltaA.eulerAngles.y * halfDeltaB.eulerAngles.y * initialRotation.eulerAngles.y;
            // float z = baseRotation.eulerAngles.z * halfDeltaA.eulerAngles.z * halfDeltaB.eulerAngles.z * initialRotation.eulerAngles.z;
            // Quaternion baseTargetRotation = Quaternion.Euler(x, y, z);
            Quaternion baseTargetRotation = baseRotation * halfDeltaA * halfDeltaB * initialRotation;
            Debug.LogError("baseTargetRotation: " + baseTargetRotation.eulerAngles);

            // Normalize the rotation
            Vector3 upDirection = baseTargetRotation * Vector3.up;
            Quaternion targetRotation = Quaternion.LookRotation(targetVector, upDirection).normalized;
            Debug.LogError("targetRotation: " + targetRotation.eulerAngles);

            // Save this target rotation as our active rotation state for future updates
            _activeRotation = targetRotation;

            // Scale logic
            float activeDistance = targetVector.magnitude;
            if(Mathf.Abs(activeDistance) < 0.0001f) activeDistance = 0.0001f;

            float scalePercentage = activeDistance / _initialDistance;

            float previousScale = _activeScale;
            _activeScale = _initialScale * scalePercentage;

            var nextScale = _activeScale * _initialLocalScale;
            if (_constraints.MinScale.Constrain)
            {
                float scalar = 1f;
                if (_constraints.ConstrainXScale)
                {
                    scalar = Mathf.Max(scalar, _constraints.MinScale.Value / nextScale.x);
                }
                if (_constraints.ConstrainYScale)
                {
                    scalar = Mathf.Max(scalar, _constraints.MinScale.Value / nextScale.y);
                }
                if (_constraints.ConstrainZScale)
                {
                    scalar = Mathf.Max(scalar, _constraints.MinScale.Value / nextScale.z);
                }
                nextScale *= scalar;
            }
            if (_constraints.MaxScale.Constrain)
            {
                float scalar = 1f;
                if (_constraints.ConstrainXScale)
                {
                    scalar = Mathf.Min(scalar, _constraints.MaxScale.Value / nextScale.x);
                }
                if (_constraints.ConstrainYScale)
                {
                    scalar = Mathf.Min(scalar, _constraints.MaxScale.Value / nextScale.y);
                }
                if (_constraints.ConstrainZScale)
                {
                    scalar = Mathf.Min(scalar, _constraints.MaxScale.Value / nextScale.z);
                }
                nextScale *= scalar;
            }
            _activeScale = nextScale.x / _initialLocalScale.x;

            // Apply the positional delta initialCenter -> targetCenter and the
            // rotational delta initialRotation -> targetRotation to the target transform
            Vector3 worldOffsetFromCenter = targetTransform.position - initialCenter;

            Vector3 offsetInTargetSpace = Quaternion.Inverse(initialRotation) * worldOffsetFromCenter;
            offsetInTargetSpace /= previousScale;

            Quaternion rotationInTargetSpace = Quaternion.Inverse(initialRotation) * targetTransform.rotation;

            targetTransform.position = (targetRotation * (_activeScale * offsetInTargetSpace)) + targetCenter;
            targetTransform.rotation = targetRotation * rotationInTargetSpace;
            targetTransform.localScale = nextScale;

            _previousGrabPointA = new Pose(grabA.position, grabA.rotation);
            _previousGrabPointB = new Pose(grabB.position, grabB.rotation);
        }

        public void MarkAsBaseScale()
        {
            _activeScale = 1.0f;
        }

        public void EndTransform() { }

        #region Inject

        public void InjectOptionalConstraints(TwoGrabFreeConstraints constraints)
        {
            _constraints = constraints;
        }

        #endregion

        private (Vector3 initialVector, Vector3 targetVector) ComputeVectors(Pose grabA, Pose grabB)
        {
            Vector3 initialVector = new Vector3(myMap.transform.position.x, _previousGrabPointB.position.y - _previousGrabPointA.position.y, myMap.transform.position.z);
            Vector3 targetVector = new Vector3(myMap.transform.position.x, grabB.position.y - grabA.position.y, myMap.transform.position.z);
            return (initialVector, targetVector);
        }
    }
    
}


