public class BXDBox : System.IComparable<BXDBox>
{
    BXDVector3 minPoint;
    BXDVector3 maxPoint;

    public BXDBox(double passMinX, double passMinY, double passMinZ,
        double passMaxX, double passMaxY, double passMaxZ)
    {
        minPoint = new BXDVector3(passMinX, passMinY, passMinZ);
        maxPoint = new BXDVector3(passMaxX, passMaxY, passMaxZ);
    }

    public BXDBox(BXDVector3 passMinPoint, BXDVector3 passMaxPoint)
    {
        minPoint = passMinPoint;
        maxPoint = passMaxPoint;
    }

    public float GetPossibleRadius()
    {
        return minPoint.Copy().Subtract(maxPoint).Magnitude();  
    }

    public int CompareTo(BXDBox other)
    {
        float thisRadius = GetPossibleRadius();
        float otherRadius = other.GetPossibleRadius();

        if (thisRadius > otherRadius)
        {
            return 1;
        }
        else if (thisRadius < otherRadius)
        {
            return -1;
        }
        else
        {
            return 0;
        }
    }
}

