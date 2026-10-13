namespace WorldCupPlanner.Domain;

public class Journey
{
    public int Number { get; }

    private Journey(int number = 0)
    {
        Number = number;
    }

    public static Journey CreateInstance(int number = 0)
    {
        return new Journey(number);
    }
}
