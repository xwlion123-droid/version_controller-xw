using UnityEngine;

[CreateAssetMenu(fileName = "PlayerData_", menuName = "Game/Player Data")]
public class PlayerData : ScriptableObject
{
    [Header("移动属性")]
    [SerializeField] private float moveSpeed = 8f;
    [SerializeField] private float rotationSpeed = 5f;

    [Header("生存属性")]
    [SerializeField] private int maxHealth = 3;
    [SerializeField] private float invincibleTime = 5f;

    [Header("射击游戏")]
    [SerializeField] private float fireRate = 0.2f;
    [SerializeField] private int bulletDamage = 1;

    //只读属性
    public float MoveSpeed => moveSpeed;
    public float RotationSpeed => rotationSpeed;
    public int MaxHealth => maxHealth;
    public float InvincibleTime => invincibleTime;
    public float FireRate => fireRate;
    public int BulletDamage => bulletDamage;
}

