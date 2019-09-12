using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AntiRollBars : MonoBehaviour
{
    private Rigidbody rb;

    public WheelCollider frontWheelL, frontWheelR, backWheelL, backWheelR;
    public float antiRoll;

    private void Start()
    {
        rb = GetComponent<Rigidbody>();
    }
    private void FixedUpdate()
    {
        AntiRollBar(frontWheelL, frontWheelR);
        AntiRollBar(backWheelL, backWheelR);
    }

    void AntiRollBar(WheelCollider wheelL, WheelCollider wheelR)
    {
        WheelHit hit;
        float travelL = 1f;
        float travelR = 1f;

        bool groundedL = wheelL.GetGroundHit(out hit);
        if (groundedL)
            travelL = (-wheelL.transform.InverseTransformPoint(hit.point).y - wheelL.radius) / wheelL.suspensionDistance;

        bool groundedR = wheelR.GetGroundHit(out hit);
        if (groundedR)
            travelR = (-wheelR.transform.InverseTransformPoint(hit.point).y - wheelR.radius) / wheelR.suspensionDistance;

        float antiRollForce = (travelL - travelR) * antiRoll;

        if (groundedL)
            rb.AddForceAtPosition(wheelL.transform.up * -antiRollForce,
                   wheelL.transform.position);
        if (groundedR)
            rb.AddForceAtPosition(wheelR.transform.up * antiRollForce,
                   wheelR.transform.position);
    }
}
