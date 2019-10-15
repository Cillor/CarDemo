using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public enum TractionType { AWD, RWD, FWD }

public class CarController : MonoBehaviour
{
    public static bool isHandbrakePressed;
    public static float carSpeed, engineRPM;
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
    float realSteeringSensitivity, realAcceleratorSensitivity;
    [HideInInspector] public Gears gear;

    public bool isKeyboardAndMouseEnabled;
    public float userSteeringSensitivity;
    public float userAcceleratorSensitivity;
    public float maxSteerAngle = 30;
    public GameObject frontWheelShape, backWheelShape;
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
        normalDrag = rb.drag;
        gear = new Gears(1, gearRatio.Length - 1);

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
        foreach (WheelCollider wheel in wheels)
        {
            if (frontWheelShape && backWheelShape)
            {
                if (wheel.transform.localPosition.z > 0)
                {
                    GameObject inSceneWheelShape = GameObject.Instantiate(frontWheelShape);
                    inSceneWheelShape.transform.parent = wheel.transform;
                    if (wheel.transform.localPosition.x > 0f)
                        inSceneWheelShape.transform.localScale = new Vector3(inSceneWheelShape.transform.localScale.x * -1f, inSceneWheelShape.transform.localScale.y, inSceneWheelShape.transform.localScale.z);
                }
                else
                {
                    GameObject inSceneWheelShape = GameObject.Instantiate(backWheelShape);
                    inSceneWheelShape.transform.parent = wheel.transform;
                    if (wheel.transform.localPosition.x > 0f)
                        inSceneWheelShape.transform.localScale = new Vector3(inSceneWheelShape.transform.localScale.x * -1f, inSceneWheelShape.transform.localScale.y, inSceneWheelShape.transform.localScale.z);
                }
            }
        }
    }

    void GetInput()
    {
        if (isKeyboardAndMouseEnabled)
        {
            GetMouseSteeringInput();
            GetMouseAcceleratorInput();
        }
        else
        {
            steeringWheelInput = Input.GetAxis("SteeringWheel");
            acceleratorInput = Input.GetAxis("Accelerator");
        }

        if (FuelConsumption.fuelInTank < 0)
            acceleratorInput = 0;

        brakeInput = Input.GetAxis("Brake");
        isHandbrakePressed = Input.GetButton("HandBrake");

        if (acceleratorInput > 0)
            isAcceleratorPressed = true;
        else
            isAcceleratorPressed = false;
    }

    void GetMouseSteeringInput()
    {
        realSteeringSensitivity = userSteeringSensitivity / 100;
        if (Input.GetMouseButton(1))
        {
            steeringWheelInput = 0;
        }
        else
        {
            steeringWheelInput += Input.GetAxis("Mouse X") * realSteeringSensitivity;
            steeringWheelInput = Mathf.Clamp(steeringWheelInput, -1, 1);
        }
    }

    void GetMouseAcceleratorInput()
    {
        realAcceleratorSensitivity = userAcceleratorSensitivity / 100;
        if (!Input.GetMouseButton(0))
        {
            acceleratorInput += Input.GetAxis("Mouse Y") * realAcceleratorSensitivity;
            acceleratorInput = Mathf.Clamp(acceleratorInput, 0, 1);
        }
    }
    void GearChange()
    {
        if (Input.GetButtonDown("GearUp"))
        {
            gear.GearUp();

            gearDriveRatio = gearRatio[gear.actual] * finalDriveRatio;
        }

        if (Input.GetButtonDown("GearDown"))
        {
            if (CalculateEngineRPM(gear.actual - 1) > engineRPMLimit)
                return;

            gear.GearDown();

            gearDriveRatio = gearRatio[gear.actual] * finalDriveRatio;
        }
    }

    public float CalculateEngineRPM(int _desiredGear)
    {
        float desiredGearDriveRatio = gearRatio[_desiredGear] * finalDriveRatio;
        wheelRPM = carSpeed / wheelsTotalRadius;
        float newEngineRPM = wheelRPM * desiredGearDriveRatio * revolutionsFactor / 2 * Mathf.PI;
        newEngineRPM = Mathf.Abs(newEngineRPM);
        if (newEngineRPM < engineIdle && (_desiredGear == 2 || _desiredGear == 0) && FuelConsumption.fuelInTank > 0)
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
                    wheel.motorTorque = outputEngineForce * acceleratorInput;
                }
                break;
            case TractionType.RWD:
                foreach (WheelCollider wheel in wheels)
                {
                    if (wheel.transform.localPosition.z < 0)
                        wheel.motorTorque = outputEngineForce * acceleratorInput;
                }
                break;
            case TractionType.FWD:
                foreach (WheelCollider wheel in wheels)
                {
                    if (wheel.transform.localPosition.z > 0)
                        wheel.motorTorque = outputEngineForce * acceleratorInput;
                }
                break;
        }

        foreach (WheelCollider wheel in wheels)
        {
            if (wheel.rpm > 2500 || wheel.rpm < -2000)
                wheel.motorTorque = 0;
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
        engineRPM = CalculateEngineRPM(gear.actual);
        engineRPM *= Mathf.Lerp(.8f, 1f, acceleratorInput);

        outputEngineForce = engineTorque.Evaluate(engineRPM) * gearDriveRatio * transmissionEfficiency / wheelsTotalRadius;

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
            if (frontWheelShape && backWheelShape)
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