using UnityEngine;

public class TestSpawner : MonoBehaviour
{
    [SerializeField] private EnemyData enemyData;
    [SerializeField] private float spawnInterval = 2f;
    [SerializeField] private float spawnRadius = 6f;
    [SerializeField] private int maxEnemies = 5;

    private int _spawnedCount = 0;

    void Start()
    {
        InvokeRepeating(nameof(SpawnEnemy), 1f, spawnInterval);
    }

    void SpawnEnemy()
    {
        if (_spawnedCount >= maxEnemies)
        {
            CancelInvoke(nameof(SpawnEnemy));
            Debug.Log($"🚫 已达上限 {maxEnemies}，停止生成");
            return;
        }

        // ✅ 检查池管理器
        if (EnemyPoolManager.Instance == null)
        {
            Debug.LogWarning("⚠️ EnemyPoolManager 不存在");
            return;
        }

        // ✅ 检查 EnemyData
        if (enemyData == null)
        {
            Debug.LogWarning("⚠️ EnemyData 未配置");
            return;
        }

        // ✅ 生成敌人（随机位置）
        Vector2 random = Random.insideUnitCircle * spawnRadius;
        Vector3 pos = new Vector3(random.x, 0, random.y);

        EnemyPoolManager.Instance.GetEnemy(pos, Quaternion.identity, enemyData);

        _spawnedCount++;
        Debug.Log($"🐛 生成敌人 {_spawnedCount}/{maxEnemies} 位置：{pos}");
    }
}