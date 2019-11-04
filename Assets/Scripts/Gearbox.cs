public class Gearbox
{
    public float shaftDriveRatio;
    public float damage;

    public int actualGear = 1;

    float[] gearRatios;
    float finalDriveRatio;

    int numberOfGears;
    public int rearGear = 0, neutralGear = 1, firstGear = 2, secondGear = 3, thirdGear = 4,
        fourthGear = 5, fifthGear = 6, sixthGear = 7;

    public Gearbox(float[] _gearRatios, float _finalDriveRatio)
    {
        gearRatios = _gearRatios;
        finalDriveRatio = _finalDriveRatio;
        numberOfGears = _gearRatios.Length;
    }

    bool GearboxBroken()
    {
        bool gearboxBroken = damage > 100;
        if (gearboxBroken)
        {
            return true;
        }
        return false;
    }

    public void ShiftUp()
    {
        if (GearboxBroken())
            return;

        if (actualGear < numberOfGears - 1)
        {
            actualGear++;
            shaftDriveRatio = gearRatios[actualGear] * finalDriveRatio;
        }
    }

    public void ShiftDown()
    {
        if (GearboxBroken())
            return;

        if (actualGear > rearGear)
        {
            actualGear--;
            shaftDriveRatio = gearRatios[actualGear] * finalDriveRatio;
        }
    }
}