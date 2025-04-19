// interface for defining basic points for each different enemy category
public interface IPoints
{
    public int GetBasePoints();
    public int GetTotalShotsDelivered();
    public float GetTotalChasedTime();
}