using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class DraggableObject : MonoBehaviour
{
    [SerializeField]
    private Transform wallTransform, offsetTransform;

    [SerializeField]
    private float followingSpeed = 50f;

    protected Rigidbody mRigidbody;

    private const int MaxNumberOfPositions = 8;
    private Queue<Vector3> velocities = new Queue<Vector3>(MaxNumberOfPositions);
    private Queue<Vector3> angularVelocities = new Queue<Vector3>(MaxNumberOfPositions);

    private Vector3 startPosition;
    private Quaternion starRotation;

    private Vector3? lastPosition = null;

    private Transform controllerTransform = null;

    public bool IsDragged
    {
        get
        {
            return controllerTransform != null;
        }
    }

    protected virtual void Awake()
    {
        mRigidbody = GetComponent<Rigidbody>();

        startPosition = mRigidbody.position;
        starRotation = mRigidbody.rotation;
    }

    private void FixedUpdate()
    {
        if (controllerTransform != null)
        {
            if (lastPosition.HasValue)
            {
                UpdatePosition(controllerTransform.position, followingSpeed);
                UpdateRotation(controllerTransform.rotation * Quaternion.Euler(Vector3.right * 90f), followingSpeed);

                Vector3 velocity = (mRigidbody.position - lastPosition.Value) / Time.deltaTime;
                
                velocities.Enqueue(velocity);
                if (velocities.Count > MaxNumberOfPositions)
                    velocities.Dequeue();

                Vector3 angularVelocity = SteamVR_Controller.Input((int)controllerTransform.GetComponent<SteamVR_TrackedObject>().index).angularVelocity;

                angularVelocities.Enqueue(angularVelocity);
                if (angularVelocities.Count > MaxNumberOfPositions)
                    angularVelocities.Dequeue();
            }

            lastPosition = mRigidbody.position;
        }
    }

    protected virtual void UpdatePosition(Vector3 targetPosition, float followingSpeed)
    {
        float zPositionOffset = offsetTransform.position.z - transform.position.z;
        if (controllerTransform.position.z + zPositionOffset > wallTransform.position.z)
            targetPosition.z = wallTransform.position.z - zPositionOffset;

        mRigidbody.position = Vector3.Lerp(mRigidbody.position, targetPosition, Time.deltaTime * followingSpeed);
    }
    
    protected virtual void UpdateRotation(Quaternion targetRotation, float followingSpeed)
    {
        mRigidbody.rotation = Quaternion.Lerp(mRigidbody.rotation, targetRotation, Time.deltaTime * followingSpeed);
    }

    public void StartDragging(Transform targetTransform)
    {
        GetComponent<Rigidbody>().isKinematic = true;

        velocities.Clear();

        controllerTransform = targetTransform;
    }

    public void StopDragging()
    {
        controllerTransform = null;

        Vector3 releaseVelocity = Vector3.zero;

        foreach (Vector3 velocity in velocities)
        {
            releaseVelocity += velocity;
        }

        if (velocities.Count > 0)
            releaseVelocity /= velocities.Count;

        Vector3 releaseAngularVelocity = Vector3.zero;

        foreach (Vector3 angularVelocity in angularVelocities)
        {
            releaseAngularVelocity += angularVelocity;
        }

        if (angularVelocities.Count > 0)
            releaseAngularVelocity /= angularVelocities.Count;

        mRigidbody.isKinematic = false;
        mRigidbody.velocity = releaseVelocity;
        mRigidbody.angularVelocity = releaseAngularVelocity;
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.collider.tag == "Floor")
        {
            ResetPosition();
        }
    }

    private void ResetPosition()
    {
        mRigidbody.position = startPosition;
        mRigidbody.rotation = starRotation;

        mRigidbody.velocity = Vector3.zero;
        mRigidbody.angularVelocity = Vector3.zero;
    }
}
