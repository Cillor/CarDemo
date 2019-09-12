using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public enum TractionType { AWD, RWD, FWD }

public class CarController : MonoBehaviour
{
    public static bool isHandbrakePressed;
    private WheelCollider[] wheels;
    private float steeringWheelInput, acceleratorInput, brakeInput;
    private float gearDriveRatio;
    private float wheelRPM;
    private Rigidbody rb;
    private float wheelsTotalRadius;
    private float transmissionEfficiency = 0.7f;
    private float outputEngineForce;
    private float normalDrag;
    private bool isAcceleratorPressed;
    private float revolutionsFactor = 60;
    private float slipForwardFriction = 0.3f, slipSidewayFriction = 0.42f;
    [HideInInspector] public int actualGear;
    [HideInInspector] public float carSpeed, engineRPM;

    public float maxSteerAngle = 30;
    public GameObject wheelShape;
    [Space] public AnimationCurve engineTorque;
    public float engineIdle, engineRPMLimit;
    public float brakeTorque, handBrakeTorque, slowingDownTorque, slowingDownDrag;
    public float finalDriveRatio;
    public float[] gearRatio;

    [Space]
    public TractionType tractionType;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        wheels = GetComponentsInChildren<WheelCollider>();
        wheelsTotalRadius = wheels[0].radius;
        actualGear = 1;
        normalDrag = rb.drag;

        InitializeWheelShapes();
    }

    void Update()
    {
        GetInput();
        GearChange();
    }

    void FixedUpdate()
    {
        Steer();
        Brake();
        Engine();
        Accelerate();
        CalculateCarSpeed();
        UpdateWheelPoses();
    }

    void InitializeWheelShapes()
    {
        for (int i = 0; i < wheels.Length; ++i)
        {
            var wheel = wheels[i];

            if (wheelShape != null)
            {
                var ws = GameObject.Instantiate(wheelShape);
                ws.transform.parent = wheel.transform;

                if (wheel.transform.localPosition.x < 0f)
                    ws.transform.localScale = new Vector3(ws.transform.localScale.x * -1f, ws.transform.localScale.y, ws.transform.localScale.z);
            }
        }
    }

    void GetInput()
    {
        steeringWheelInput = Input.GetAxis("SteeringWheel");
        acceleratorInput = Input.GetAxis("Accelerator");
        brakeInput = Input.GetAxis("Brake");
        isHandbrakePressed = Input.GetButton("HandBrake");
        isAcceleratorPressed = Input.GetButton("Accelerator");
    }

    void GearChange()
    {
        if (Input.GetButtonDown("GearUp"))
        {
            if (actualGear == 0 && carSpeed < .5f)
            {
                actualGear++;
            }
            else if (actualGear < gearRatio.Length - 1 && actualGear > 0)
            {
                actualGear++;
            }

            gearDriveRatio = gearRatio[actualGear] * finalDriveRatio;
        }

        if (Input.GetButtonDown("GearDown"))
        {
            if (CalculateEngineRPM(actualGear - 1) > engineRPMLimit)
                return;

            if (actualGear == 1 && carSpeed < .5f)
            {
                actualGear--;
            }
            else if (actualGear > 1)
            {
                actualGear--;
            }

            gearDriveRatio = gearRatio[actualGear] * finalDriveRatio;
        }
    }

    float CalculateEngineRPM(int _desiredGear)
    {
        float desiredGearDriveRatio = gearRatio[_desiredGear] * finalDriveRatio;
        wheelRPM = carSpeed / wheelsTotalRadius;
        float newEngineRPM = wheelRPM * desiredGearDriveRatio * revolutionsFactor / 2 * Mathf.PI;
        newEngineRPM = Mathf.Abs(newEngineRPM);
        if (newEngineRPM < engineIdle && (_desiredGear == 2 || _desiredGear == 0))
        {
            newEngineRPM = engineIdle;
        }

        return newEngineRPM;
    }

    void Steer()
    {
        float steeringAngle = steeringWheelInput * maxSteerAngle;

        foreach (WheelCollider wheel in wheels)
        {
            if (wheel.transform.localPosition.z > 0)
                wheel.steerAngle = steeringAngle;
        }
    }

    void Brake()
    {
        if (isHandbrakePressed)
        {
            foreach (WheelCollider wheel in wheels)
            {
                if (wheel.transform.localPosition.z < 0)
                {
                    WheelFrictionCurve fFriction = wheel.forwardFriction;
                    fFriction.stiffness = slipForwardFriction;
                    wheel.forwardFriction = fFriction;
                    WheelFrictionCurve sFriction = wheel.sidewaysFriction;
                    sFriction.stiffness = slipSidewayFriction;
                    wheel.sidewaysFriction = sFriction;

                    wheel.brakeTorque = handBrakeTorque;
                    wheel.motorTorque = 0;
                }
            }
        }
        else
        {
            foreach (WheelCollider wheel in wheels)
            {
                wheel.brakeTorque = brakeTorque * brakeInput;
            }
        }
    }

    void Accelerate()
    {
        if (LapTimer.countdownStart > 0)
            return;

        switch (tractionType)
        {
            case TractionType.AWD:
                foreach (WheelCollider wheel in wheels)
                {
                    wheel.motorTorque = outputEngineForce;
                }
                break;
            case TractionType.RWD:
                foreach (WheelCollider wheel in wheels)
                {
                    if (wheel.transform.localPosition.z < 0)
                        wheel.motorTorque = outputEngineForce;
                }
                break;
            case TractionType.FWD:
                foreach (WheelCollider wheel in wheels)
                {
                    if (wheel.transform.localPosition.z > 0)
                        wheel.motorTorque = outputEngineForce;
                }
                break;
        }
    }

    void CalculateCarSpeed()
    {
        float xSpeed = rb.velocity.x;
        float zSpeed = rb.velocity.z;
        carSpeed = Mathf.Sqrt(xSpeed * xSpeed + zSpeed * zSpeed);
    }

    void Engine()
    {
        engineRPM = CalculateEngineRPM(actualGear);
        engineRPM *= Mathf.Lerp(.2f, 1f, Input.GetAxis("Accelerator"));

        outputEngineForce = engineTorque.Evaluate(engineRPM) * gearDriveRatio * transmissionEfficiency / wheelsTotalRadius;
        outputEngineForce *= acceleratorInput;

        LimitEngineRPM();
        DeccelerateCar();
    }

    void LimitEngineRPM()
    {
        if (engineRPM > engineRPMLimit)
        {
            outputEngineForce = 0;
            if (brakeInput == 0)
            {
                foreach (WheelCollider wheel in wheels)
                {
                    wheel.brakeTorque = slowingDownTorque;
                }
            }
            return;
        }
    }

    void DeccelerateCar()
    {
        if (!isAcceleratorPressed && carSpeed > 1f)
        {
            rb.drag = slowingDownDrag;
        }
        else
        {
            rb.drag = normalDrag;
        }
    }

    void UpdateWheelPoses()
    {
        foreach (WheelCollider wheel in wheels)
        {
            if (wheelShape)
            {
                Quaternion q;
                Vector3 p;
                wheel.GetWorldPose(out p, out q);

                Transform shapeTransform = wheel.transform.GetChild(0);
                shapeTransform.position = p;
                shapeTransform.rotation = q;
            }
        }
    }
}