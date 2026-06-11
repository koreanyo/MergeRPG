using UnityEngine;

public static class SessionData
{
    public static bool IsCleared    { get; set; }
    public static int  ClearedWaves { get; set; }
    public static int  TotalWaves   { get; set; }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    static void Reset()
    {
        IsCleared    = false;
        ClearedWaves = 0;
        TotalWaves   = 0;
    }
}
