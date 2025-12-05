using UnityEngine;
[CreateAssetMenu(menuName = "SubLevelDoor")]

public class SubLevelDoor : ScriptableObject
{
    [Header("Scene To Load")]
    public string sceneName;

    [Header("Hold Requirements")]
    public float holdUpTime = 0.75f; 
}
