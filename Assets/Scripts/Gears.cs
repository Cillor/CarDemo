public class Gears
{
    public int numberOfGears;
    public int actual;
    public int rear = 0, neutral = 1, first = 2, second = 3, third = 4, fourth = 5, fifth = 6, sixth = 7;

    public Gears(int _actual, int _numberOfGears)
    {
        actual = _actual;
        numberOfGears = _numberOfGears;
    }

    public void GearUp()
    {
        if (actual == rear && CarController.carSpeed < .5f)
        {
            actual++;
        }
        else if (actual < numberOfGears && actual > rear)
        {
            actual++;
        }
    }

    public void GearDown()
    {
        if (actual == neutral && CarController.carSpeed < .5f)
        {
            actual--;
        }
        else if (actual > neutral)
        {
            actual--;
        }
    }
}
