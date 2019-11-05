using System;

public class Engine
{
    public float RPM;
    float engineRPM;
    public float damage;
    public float RPMLimit = 14001;

    const float extraRPM = 500;

    const float rpmLoss = 2000;
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
            RPM = Helper.Lerp(Math.Abs(engineRPM), Math.Abs(_pilotShaftSpeed),
                                Helper.Clamp(acceleratorInput, .45f, .55f));
            //Concerta essa bagunça aqui
            engineRPM = Helper.Lerp(Math.Abs(engineRPM), Math.Abs(_pilotShaftSpeed),
                                Helper.Clamp(carSpeedMS / 60, 0, .1f));
            //Vc dividiu isso por 60 pra tentar fazer com que o Lerp fosse aumentando
            //de acordo com a velocidade
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
