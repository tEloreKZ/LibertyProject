namespace ProjectLiberty.Abstractions.Entities;

/// <summary>
/// Vector3 structure.
/// </summary>
public struct Vector3
{
    public float X { get; set; }
    public float Y { get; set; }
    public float Z { get; set; }
    
    public Vector3(float x, float y, float z)
    {
        X = x;
        Y = y;
        Z = z;
    }
    
    public static Vector3 Zero => new Vector3(0, 0, 0);
}
