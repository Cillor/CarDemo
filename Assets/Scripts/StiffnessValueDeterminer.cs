using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StiffnessValueDeterminer : MonoBehaviour
{
    WheelCollider wheel;

    void Start()
    {
        wheel = GetComponent<WheelCollider>();
    }

    void FixedUpdate()
    {
        if (!CarController.isHandbrakePressed)
        {
            WheelHit hit;
            if (wheel.GetGroundHit(out hit))
            {
                WheelFrictionCurve fFriction = wheel.forwardFriction;
                fFriction.stiffness = hit.collider.material.staticFriction - DetecSlip();
                wheel.forwardFriction = fFriction;
                WheelFrictionCurve sFriction = wheel.sidewaysFriction;
                sFriction.stiffness = hit.collider.material.staticFriction - DetecSlip();
                wheel.sidewaysFriction = sFriction;
            }
        }
    }

    float DetecSlip()
    {
        WheelHit hit;
        if (wheel.GetGroundHit(out hit))
        {
            if (hit.forwardSlip > 0.7f)
            {
                Debug.Log("Acceleration Slip");
                return hit.forwardSlip;
            }
            if (hit.forwardSlip < -0.5f)
            {
                Debug.Log("Braking Slip");
                return Mathf.Abs(hit.forwardSlip);
            }
            return 0;
        }
        return 0;
    }
}
