using UnityEngine;

[System.Serializable]
public class EnemySpawnInfo
{
    public EnemyData enemyData;
    public int count = 5;
    public float spawnInterval = 0.5f;
}

[CreateAssetMenu(fileName = "WaveData_", menuName = "Game/Wave Data")]
public class WaveData : ScriptableObject
{
    [Header("波次信息")]
    [SerializeField] private int waveNumber = 1;
    [SerializeField] private EnemySpawnInfo[] enemiesToSpawn;
    [SerializeField] private float waveDuration = 10f;
    [SerializeField] private float nextWaveDelay = 3f;

    public int WaveNumber => waveNumber;
    public EnemySpawnInfo[] EnemiesToSpawn => enemiesToSpawn;
    public float WaveDuration => waveDuration;
    public float NextWaveDelay => nextWaveDelay;
}