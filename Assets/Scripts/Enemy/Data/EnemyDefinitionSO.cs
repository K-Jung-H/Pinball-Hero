using UnityEngine;

[CreateAssetMenu(fileName = "EnemyDefinition", menuName = "Pinball/Enemy/Definition")]
public class EnemyDefinitionSO : ScriptableObject
{
    [SerializeField] private Enemy_Base enemyPrefab;
    [SerializeField] private int maxHp;
    [SerializeField] private int damage;
    [SerializeField] private int cellWidth;
    [SerializeField] private int cellHeight;
    [SerializeField] private float moveSpeed;

    public Enemy_Base EnemyPrefab => enemyPrefab;
    public int MaxHp => maxHp;
    public int Damage => damage;
    public float Speed => moveSpeed;
    public int CellWidth => cellWidth;
    public int CellHeight => cellHeight;
    public int CellArea => Mathf.Max(0, cellWidth) * Mathf.Max(0, cellHeight);
    public int ExperienceReward => CellArea;
}
