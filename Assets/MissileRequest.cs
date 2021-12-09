[System.Serializable]
public struct MissileRequest
{
    public int target;
    public NetControllers.Position position;
    public float[] rotation;

    public MissileRequest(int target, NetControllers.Position position, float[] rotation)
    {
        this.target = target;
        this.position = position;
        this.rotation = rotation;
    }
}