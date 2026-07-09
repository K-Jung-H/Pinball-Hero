using UnityEngine;

[CreateAssetMenu(menuName = "Pinball/Ball Factory")]
public class BallFactorySO : ScriptableObject
{
    [SerializeField] private BallPrefabEntry[] prefabEntries;

    public Ball_Base Create(BallType type, Vector3 position, Transform parent)
    {
        Ball_Base prefab = GetPrefab(type);

        if (prefab == null)
        {
            Debug.LogError($"Ball prefab not found. Type: {type}");
            return null;
        }

        return Instantiate(prefab, position, Quaternion.identity, parent);
    }

    private Ball_Base GetPrefab(BallType type)
    {
        for (int i = 0; i < prefabEntries.Length; i++)
        {
            if (prefabEntries[i].type == type)
            {
                return prefabEntries[i].prefab;
            }
        }

        return null;
    }
}


[System.Serializable]
public class BallPrefabEntry
{
    public BallType type;
    public Ball_Base prefab;
}