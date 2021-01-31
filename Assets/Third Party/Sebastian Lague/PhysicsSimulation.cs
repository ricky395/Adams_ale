
// Author: Sebastian Lague
// Extracted from: https://www.youtube.com/watch?v=SnhfcdtGM2E

using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class PhysicsSimulation : MonoBehaviour {

    public int maxIterations = 1000;
    SimulatedBody[] simulatedBodies;

    public Vector2 forceMinMax;
    public float forceAngleInDegrees;
    public bool randomizeForceAngle;

    List<Rigidbody> generatedRigidbodies;
    List<Collider> generatedColliders;
    //List<Collider> deletedColliders;

    [ContextMenu("Run Simulation")]
    public void RunSimulation()
    {
        AutoGenerateComponents();
        simulatedBodies = FindObjectsOfType<Rigidbody>().Select(rb => new SimulatedBody(rb, rb.transform.IsChildOf(transform))).ToArray();
    
        // Add force to bodies
        foreach (SimulatedBody body in simulatedBodies)
        {
            if (body.isChild)
            {
                float randomForceAmount = Random.Range(forceMinMax.x, forceMinMax.y);
                float forceAngle = ((randomizeForceAngle) ? Random.Range(0, 360f) : forceAngleInDegrees) * Mathf.Deg2Rad;
                Vector3 forceDir = new Vector3(Mathf.Sin(forceAngle), 0, Mathf.Cos(forceAngle));
                body.rigidbody.AddForce(forceDir * randomForceAmount, ForceMode.Impulse);
            }
        }

        // Run simulation for maxIteration frames, or until all child rigidbodies are sleeping
        Physics.autoSimulation = false;
        for (int i = 0; i < maxIterations; i++)
        {
            Physics.Simulate(Time.fixedDeltaTime);
            if (simulatedBodies.All(body => body.rigidbody.IsSleeping() || !body.isChild))
            {
                break;
            }
        }
        Physics.autoSimulation = true;

        // Reset bodies which are not child objects of the transform to which this script is attached
        foreach (SimulatedBody body in simulatedBodies)
        {
            if (!body.isChild)
            {
                body.Reset();
            }
        }

        RestoreComponents();

    }

    // Automatically add rigidbody and box collider to object if it doesn't already have
    void AutoGenerateComponents()
    {
        generatedRigidbodies = new List<Rigidbody>();
        generatedColliders = new List<Collider>();
        //deletedColliders = new List<Collider>();

        foreach (Transform child in transform)
        {
            if (!child.GetComponent<Rigidbody>())
            {
                generatedRigidbodies.Add(child.gameObject.AddComponent<Rigidbody>());
            }

            Collider collider = child.GetComponent<Collider>();
            if (collider)
			{
                //deletedColliders.Add(collider);
            }
            else
            {
                MeshCollider meshCollider = child.gameObject.AddComponent<MeshCollider>();
                Mesh m = child.gameObject.GetComponent<MeshFilter>().sharedMesh;
                (meshCollider as MeshCollider).convex = true;
                if (m != null)
                    (meshCollider as MeshCollider).sharedMesh = m;

                generatedColliders.Add(meshCollider);
            }            
        }
    }

    // Restore the components which were changed at start of simulation
    void RestoreComponents()
    {
        foreach (Rigidbody rb in generatedRigidbodies)
        {
            DestroyImmediate(rb);
        }
        foreach (Collider c in generatedColliders)
		{
			DestroyImmediate(c);
		}
    }

    [ContextMenu("Reset")]
    public void ResetAllBodies()
    {
        if (simulatedBodies != null)
        {
            foreach (SimulatedBody body in simulatedBodies)
            {
                body.Reset();
            }
        }
    }

    struct SimulatedBody
    {
        public readonly Rigidbody rigidbody;
        public readonly bool isChild;
        readonly Vector3 originalPosition;
        readonly Quaternion originalRotation;
        readonly Transform transform;

        public SimulatedBody(Rigidbody rigidbody, bool isChild)
        {
            this.rigidbody = rigidbody;
            this.isChild = isChild;
            transform = rigidbody.transform;
            originalPosition = rigidbody.position;
            originalRotation = rigidbody.rotation;
        }

        public void Reset()
        {
            transform.position = originalPosition;
            transform.rotation = originalRotation;
            if (rigidbody != null)
            {
                rigidbody.velocity = Vector3.zero;
                rigidbody.angularVelocity = Vector3.zero;
            }
        }
    }
}
