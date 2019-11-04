using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public enum TractionType { AWD, RWD, FWD }

public class CarController : MonoBehaviour
{
    private const int carMiddlePosition = 0;
    public static bool isHandbrakePressed;
    public static float carSpeedInMetersPerSecond, engineRPM;
    private WheelCollider[] wheels;
    private float steeringWheelInput, acceleratorInput, brakeInput, clutchInput;
    private float gearDriveRatio;
    private float wheelRPM;
    private Rigidbody rb;
    private float wheelsTotalRadius;
    private const float transmissionEfficiency = 0.7f;
    private float outputEngineForce;
    private float normalDrag;
    private bool isAcceleratorPressed, isClutchPressed, gearChangeAllowed;
    private const float revolutionsFactor = 60;
    private const float slipForwardFriction = 0.3f, slipSidewayFriction = 0.42f;

    [HideInInspector] public Gears gear;

    public float maxSteerAngle = 30;
    public GameObject frontWheelShape, backWheelShape;
    [Space] public AnimationCurve engineTorque;
    public float engineIdle, engineRPMLimit;
    public float brakeTorque, handBrakeTorque, slowingDownTorque, slowingDownDrag;
    [Range(0, 1)] public float frontBrakeBias;
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
        Clutch();
        Accelerate();
        CalculateCarSpeed();
        TractionControl();
        UpdateWheelPoses();
    }

    void InitializeWheelShapes()
    {
        foreach (WheelCollider wheel in wheels)
        {
            if (frontWheelShape && backWheelShape)
            {
                bool isFrontWheels = wheel.transform.localPosition.z > carMiddlePosition;
                if (isFrontWheels)
                {
                    GameObject inSceneWheelShape = GameObject.Instantiate(frontWheelShape);
                    inSceneWheelShape.transform.parent = wheel.transform;

                    bool isRightWheel = wheel.transform.localPosition.x > carMiddlePosition;
                    if (isRightWheel)
                        inSceneWheelShape.transform.localScale = new Vector3(inSceneWheelShape.transform.localScale.x * -1f, inSceneWheelShape.transform.localScale.y, inSceneWheelShape.transform.localScale.z);
                }
                else
                {
                    GameObject inSceneWheelShape = GameObject.Instantiate(backWheelShape);
                    inSceneWheelShape.transform.parent = wheel.transform;
                    bool isRightWheel = wheel.transform.localPosition.x > carMiddlePosition;
                    if (isRightWheel)
                        inSceneWheelShape.transform.localScale = new Vector3(inSceneWheelShape.transform.localScale.x * -1f, inSceneWheelShape.transform.localScale.y, inSceneWheelShape.transform.localScale.z);
                }
            }
        }
    }

    void GetInput()
    {
        steeringWheelInput = Input.GetAxis("SteeringWheel");
        acceleratorInput = Input.GetAxis("Accelerator");

        if (FuelConsumption.fuelInTank < 0)
            acceleratorInput = 0;

        brakeInput = Input.GetAxis("Brake");
        isHandbrakePressed = Input.GetButton("HandBrake");
        isClutchPressed = Input.GetButton("Clutch");

        if (acceleratorInput > 0)
            isAcceleratorPressed = true;
        else
            isAcceleratorPressed = false;
    }

    bool isLastGearNeutral;
    void GearChange()
    {
        if (gearChangeAllowed)
        {
            if (Input.GetButtonDown("GearUp"))
            {
                gear.GearUp();

                gearDriveRatio = gearRatio[gear.actual] * finalDriveRatio;
            }

            if (Input.GetButtonDown("GearDown"))
            {
                gear.GearDown();

                gearDriveRatio = gearRatio[gear.actual] * finalDriveRatio;
            }
        }

        gear.LastGearInfo(Time.deltaTime);
    }

    float timeSinceGearWasNeutral;
    float freeAndLockedRevvInterpolationTime;
    public float CalculateEngineRPM(int _desiredGear)
    {
        if (isClutchPressed)
            return ClutchDisk(_desiredGear);

        if (_desiredGear == gear.neutral)
        {
            return FreeRevvEngineRPM();
        }
        else
        {
            if (gear.isLastGearNeutral)
            {
                return FreeAndLockedRevvInterpolation(_desiredGear);
            }
            else
            {
                freeAndLockedRevvInterpolationTime = 0;
            }
            return LockedRevvEngineRPM(_desiredGear);
        }
    }

    float freeEngineRPM;
    float FreeRevvEngineRPM()
    {
        float maxRPMAllowed = engineRPMLimit + 1000;

        if (freeEngineRPM > engineRPMLimit)
            freeEngineRPM -= 600;

        if (isAcceleratorPressed)
        {
            freeEngineRPM += 6000f * Time.deltaTime;
        }
        else
        {
            freeEngineRPM -= 2000f * Time.deltaTime;
        }

        freeEngineRPM = Mathf.Clamp(freeEngineRPM, 0, maxRPMAllowed);

        return freeEngineRPM;
    }

    float LockedRevvEngineRPM(int _desiredGear)
    {
        float desiredGearDriveRatio = gearRatio[_desiredGear] * finalDriveRatio;
        wheelRPM = carSpeedInMetersPerSecond / wheelsTotalRadius;
        float lockedEngineRPM = wheelRPM * desiredGearDriveRatio * revolutionsFactor / 2 * Mathf.PI;
        lockedEngineRPM = Mathf.Abs(lockedEngineRPM);
        if (lockedEngineRPM < engineIdle && (_desiredGear == 2 || _desiredGear == 0) && FuelConsumption.fuelInTank > 0)
        {
            lockedEngineRPM = engineIdle;
        }

        return lockedEngineRPM;
    }

    float ClutchDisk(int _desiredGear)
    {
        if (acceleratorInput > .5f)
        {
            if (engineRPM < engineRPMLimit)
                return FreeRevvEngineRPM();

            return LockedRevvEngineRPM(_desiredGear);
        }
        else
            return LockedRevvEngineRPM(_desiredGear);
    }

    float FreeAndLockedRevvInterpolation(int _desiredGear)
    {
        if (gear.timeSinceGearWasNeutral < .5f)
        {
            return FreeRevvEngineRPM();
        }
        else
        {
            freeAndLockedRevvInterpolationTime += Time.deltaTime / 3.5f;
            freeAndLockedRevvInterpolationTime = Mathf.Clamp(freeAndLockedRevvInterpolationTime, 0, 1);
            return Mathf.Lerp(FreeRevvEngineRPM(), LockedRevvEngineRPM(_desiredGear), freeAndLockedRevvInterpolationTime);
        }
    }

    void Steer()
    {
        float steeringAngle = steeringWheelInput * maxSteerAngle;

        foreach (WheelCollider wheel in wheels)
        {
            bool isFrontWheels = wheel.transform.localPosition.z > carMiddlePosition;
            if (isFrontWheels)
                wheel.steerAngle = steeringAngle;
        }
    }

    void Brake()
    {
        if (isHandbrakePressed)
        {
            foreach (WheelCollider wheel in wheels)
            {
                bool isBackWheels = wheel.transform.localPosition.z < carMiddlePosition;
                if (isBackWheels)
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
                float frontAxleBrakeTorque = brakeTorque * frontBrakeBias;
                float individualFrontBrakeTorque = frontAxleBrakeTorque / 2;
                float rearBrakeBias = 1 - frontBrakeBias;
                float rearAxleBrakeTorque = brakeTorque * rearBrakeBias;
                float individualRearBrakeTorque = rearAxleBrakeTorque / 2;

                bool isFrontWheels = wheel.transform.localPosition.z > carMiddlePosition;
                if (isFrontWheels)
                {
                    wheel.brakeTorque = individualFrontBrakeTorque * brakeInput;
                }
                else
                {
                    wheel.brakeTorque = individualRearBrakeTorque * brakeInput;
                }
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
                    wheel.motorTorque = outputEngineForce * acceleratorInput * Helper.Normalize(gear.actual - 1) * clutchInput;
                }
                break;
            case TractionType.RWD:
                foreach (WheelCollider wheel in wheels)
                {
                    bool isBackWheels = wheel.transform.localPosition.z < carMiddlePosition;
                    if (isBackWheels)
                        wheel.motorTorque = outputEngineForce * acceleratorInput * Helper.Normalize(gear.actual - 1) * clutchInput;
                }
                break;
            case TractionType.FWD:
                foreach (WheelCollider wheel in wheels)
                {
                    bool isFrontWheels = wheel.transform.localPosition.z > carMiddlePosition;
                    if (isFrontWheels)
                        wheel.motorTorque = outputEngineForce * acceleratorInput * Helper.Normalize(gear.actual - 1) * clutchInput;
                }
                break;
        }
    }

    void Clutch()
    {
        if (isClutchPressed)
        {
            clutchInput = 0;
            gearChangeAllowed = true;
        }
        else
        {
            clutchInput = 1;
            gearChangeAllowed = false;
        }
    }

    void CalculateCarSpeed()
    {
        float sidewaysSpeed = rb.velocity.x;
        float forwardSpeed = rb.velocity.z;
        carSpeedInMetersPerSecond = Mathf.Sqrt(sidewaysSpeed * sidewaysSpeed + forwardSpeed * forwardSpeed);
    }

    void Engine()
    {
        engineRPM = CalculateEngineRPM(gear.actual);
        const float minimumEngineRPMMultiplier = .8f;
        const float maximumEngineRPMMultiplier = 1f;
        engineRPM *= Mathf.Lerp(minimumEngineRPMMultiplier, maximumEngineRPMMultiplier, acceleratorInput);

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
        if (!isAcceleratorPressed && carSpeedInMetersPerSecond > 1f)
        {
            rb.drag = slowingDownDrag;
        }
        else
        {
            rb.drag = normalDrag;
        }
    }

    void TractionControl()
    {
        foreach (WheelCollider wheel in wheels)
        {
            float carSpeedInMetersPerMinutes = carSpeedInMetersPerSecond * 60;
            float wheelCircunference = 2 * Mathf.PI * wheel.radius;
            float theoreticalWheelRPM = carSpeedInMetersPerMinutes / wheelCircunference;
            const float wheelRPMValueModifier = 500;
            float acceptableWheelRPM = theoreticalWheelRPM + wheelRPMValueModifier;

            bool isWheelRPMHigherThanAcceptableRPM = Mathf.Abs(wheel.rpm) > acceptableWheelRPM;
            if (isWheelRPMHigherThanAcceptableRPM)
                wheel.motorTorque = 0;
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