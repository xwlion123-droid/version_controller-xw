using UnityEngine;

[CreateAssetMenu(fileName = "BulletData_", menuName = "Game/Bullet Data")]
public class BulletData : ScriptableObject
{
    [SerializeField] private float speed = 20f;
    [SerializeField] private float lifeTime = 3f;
    [SerializeField] private int damage = 1;
    [SerializeField] private Color color = Color.cyan;

    public float Speed => speed;
    public float LifeTime => lifeTime;
    public int Damage => damage;
    public Color Color => color;
}