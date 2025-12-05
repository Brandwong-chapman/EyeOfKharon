public static class Layers
{
    public const int Default = 0;
    public const int TransparentFX = 1;
    public const int IgnoreSCRaycast = 2;
    public const int Entity = 3;
    public const int Water = 4;
    public const int UI = 5;
    public const int Hurtbox = 6;
    public const int Hitbox = 7;
    public const int Platform = 8;
    public const int CameraBounds = 9;
    public const int BackgroundSCObjects = 10;

    public static int ToLayerMask(int layer)
    {
        return 1 << layer;
    }
}