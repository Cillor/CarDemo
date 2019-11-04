public class Gears
{
    public int numberOfGears;
    public int actual, last;
    public bool isLastGearNeutral;
    public float timeSinceGearWasNeutral;
    public int rear = 0, neutral = 1, first = 2, second = 3, third = 4, fourth = 5, fifth = 6, sixth = 7;

    public Gears(int _actual, int _numberOfGears)
    {
        actual = _actual;
        numberOfGears = _numberOfGears;
    }

    public void GearUp()
    {
        if (actual == rear && CarController.carSpeedInMetersPerSecond < .5f)
        {
            last = actual;
            actual++;
        }
        else if (actual < numberOfGears && actual > rear)
        {
            last = actual;
            actual++;
        }
    }

    public void GearDown()
    {
        if (actual == neutral && CarController.carSpeedInMetersPerSecond < .5f)
        {
            last = actual;
            actual--;
        }
        else if (actual > neutral)
        {
            last = actual;
            actual--;
        }
    }

    public void LastGearInfo(float _deltaTime)
    {
        if (last == neutral)
        {
            isLastGearNeutral = true;
            timeSinceGearWasNeutral += _deltaTime;
        }
        else
        {
            isLastGearNeutral = false;
        }

        if (actual == neutral)
        {
            timeSinceGearWasNeutral = 0;
        }
    }
}
