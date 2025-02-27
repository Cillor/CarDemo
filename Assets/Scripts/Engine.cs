﻿using System;

public class Engine
{
    public float RPM;
    float engineRPM;
    public float damage;
    public float RPMLimit = 14001;

    const float extraRPM = 500;

    const float rpmLoss = 2000;
    private const int carTopSpeed = 86;
    private const float clutchFriction = 5f;
    bool acceleratorPressed, engineEngagedWithWheels;
    float acceleratorInput;
    float deltaTime;
    float carSpeedMS;

    Gearbox gearbox;

    public Engine(Gearbox _gearbox)
    {
        gearbox = _gearbox;
    }
    public void UserInput(float _acceleratorInput, bool _acceleratorPressed, bool _engineEngagedWithWheels)
    {
        acceleratorInput = _acceleratorInput;
        acceleratorPressed = _acceleratorPressed;
        engineEngagedWithWheels = _engineEngagedWithWheels;
    }

    public void UpdateEngine(float _carSpeedMS, float _pilotShaftSpeed, float _deltaTime)
    {
        carSpeedMS = _carSpeedMS;
        EngineDamage();
        bool engineBroken = damage > 100;
        if (engineBroken)
        {
            RPM = 0;
            return;
        }

        deltaTime = _deltaTime;
        RPM = GetCarRPM(_pilotShaftSpeed);
    }

    public void EngineDamage()
    {
        if (RPM > RPMLimit)
        {
            float overshootedRPM = (RPM - RPMLimit) / 1000;
            float damageToAplly = (float)Math.Pow(2, 0.5 * overshootedRPM);

            damage += damageToAplly * deltaTime;

            damage = Helper.Clamp(damage, 0, 101);
        }
    }

    public float GetCarRPM(float _pilotShaftSpeed)
    {
        float maxRPMAllowed = acceleratorInput * RPMLimit + extraRPM;

        if (acceleratorPressed)
        {
            if (engineRPM < maxRPMAllowed)
                engineRPM += 5000 * EngineShaftFriction(maxRPMAllowed) * deltaTime;
        }
        else
        {
            engineRPM -= 2000 * EngineShaftInertia(maxRPMAllowed) * deltaTime;
            engineRPM = engineRPM < 0 ? 0 : engineRPM;
        }

        if (engineRPM > RPMLimit)
            engineRPM -= 200;

        if (engineEngagedWithWheels)
        {
            engineRPM = Helper.Lerp(Math.Abs(engineRPM), Math.Abs(_pilotShaftSpeed),
                                Helper.Clamp(carSpeedMS / carTopSpeed, clutchFriction / 500, clutchFriction / 100));
            return Helper.Lerp(Math.Abs(engineRPM), Math.Abs(_pilotShaftSpeed),
                                Helper.Clamp(acceleratorInput, .45f, .55f));
        }
        else
            return engineRPM;
    }

    float EngineShaftFriction(float _maxRPM)
    {
        float rpmPercent = RPM / _maxRPM;

        float xValue = Math.Abs(10 * rpmPercent + .2f);
        float vOut = Convert.ToSingle(Math.Log10(xValue));
        vOut = Math.Abs(vOut);

        vOut = Helper.Clamp(vOut, .4f, 1f);
        return vOut;
    }

    float EngineShaftInertia(float _maxRPM)
    {
        float rpmPercent = RPM / _maxRPM;

        float xValue = Math.Abs(1 / (5 * rpmPercent));
        float vOut = Convert.ToSingle((Math.Log10(xValue) + 1) / 2);
        vOut = Math.Abs(vOut);
        return vOut;
    }
}
