public class Gearbox
{
    public float damage;

    public int actualGear = 1;

    public float[] gearRatios;
    public float finalDriveRatio;

    int numberOfGears;
    public int reverseGear = 0, neutralGear = 1, firstGear = 2, secondGear = 3, thirdGear = 4,
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
        }
    }

    public void ShiftDown()
    {
        if (GearboxBroken())
            return;

        if (actualGear > reverseGear)
        {
            actualGear--;
        }
    }
}