using System;

public class Engine
{
    public float RPM;
    float engineRPM;
    public float damage;
    public float RPMLimit = 14001;

    const float extraRPM = 500;

    const float rpmLoss = 2000;
    bool acceleratorPressed, clutchEngaged;
    float acceleratorInput;
    float deltaTime;

    Gearbox gearbox;

    public Engine(Gearbox _gearbox)
    {
        gearbox = _gearbox;
    }
    public void UserInput(float _acceleratorInput, bool _acceleratorPressed, bool _clutchPressed)
    {
        acceleratorInput = _acceleratorInput;
        acceleratorPressed = _acceleratorPressed;
        clutchEngaged = !_clutchPressed;
    }

    public void UpdateEngine(float _pilotShaftSpeed, float _deltaTime)
    {
        bool engineBroken = damage > 100;
        if (engineBroken)
        {
            return;
        }

        deltaTime = _deltaTime;
        Crankshaft(_pilotShaftSpeed);
    }

    void Crankshaft(float _pilotShaftSpeed)
    {
        float maxRPMAllowed = acceleratorInput * RPMLimit + extraRPM;

        if (acceleratorPressed)
        {
            if (engineRPM < maxRPMAllowed)
                engineRPM += 5000 * EngineShaftFriction(maxRPMAllowed) * deltaTime;
            /* if (engineRPM > RPMLimit)
                engineRPM -= 2000 * EngineShaftInertia(maxRPMAllowed) * deltaTime; */
        }
        else
        {
            engineRPM -= 2000 * EngineShaftInertia(maxRPMAllowed) * deltaTime;
            engineRPM = engineRPM < 0 ? 0 : engineRPM;
        }

        if (engineRPM > RPMLimit)
            engineRPM -= 200;

        if (clutchEngaged)
        {
            RPM = Helper.Lerp(Math.Abs(engineRPM), Math.Abs(_pilotShaftSpeed),
                                Helper.Clamp(acceleratorInput, .45f, .55f));
        }
        else
            RPM = engineRPM;
    }

    float EngineShaftFriction(float _maxRPM)
    {
        float rpmPercent = RPM / _maxRPM;

        float xValue = Math.Abs(10 * rpmPercent + .2f);
        float vOut = Convert.ToSingle(Math.Log10(xValue));
        vOut = Math.Abs(vOut);
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
