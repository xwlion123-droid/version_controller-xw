using UnityEngine;

[CreateAssetMenu(fileName = "EnemyData_", menuName = "Game/Enemy Data")]
public class EnemyData : ScriptableObject
{
    [Header("基础属性")]
    [SerializeField] private string enemyName = "敌人";
    [SerializeField] private float health = 5f;
    [SerializeField] private float speed = 3f;
    [SerializeField] private int scoreValue = 10;
    [SerializeField] private float damage = 1f;

    [Header("物理属性")]
    [SerializeField] private float size = 1f;
    [SerializeField] private Color color = Color.red;

    public string EnemyName => enemyName;
    public float Health => health;
    public float Speed => speed;
    public int ScoreValue => scoreValue;
    public float Damage => damage;
    public float Size => size;
    public Color Color => color;
}