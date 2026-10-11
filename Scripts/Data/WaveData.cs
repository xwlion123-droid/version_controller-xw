//using System.Collections.Generic;
//using UnityEngine;
///// <summary>
///// 单个敌人的生成配置
///// </summary>
//[System.Serializable]
//public class EnemySpawnConfig
//{
//    [Tooltip("敌人数据")]
//    public EnemyData enemyData;

//    [Tooltip("数量")]
//    [Min(1)] public int count = 5;

//    [Tooltip("生成间隔(秒)")]
//    [Min(0.1f)] public float spawnInterval = 0.5f;

//    [Tooltip("生成位置模式")]
//    public SpawnPositionMode positionMode = SpawnPositionMode.RandomEdge;

//    [Tooltip("生成位置半径（用于Random模式）")]
//    public float spawnRadius = 8f;
//}

///// <summary>
///// 生成位置模式
///// </summary>
//public enum SpawnPositionMode
//{
//    RandomEdge,//屏幕边缘随机
//    RandomAround,//玩家周围随机
//    FixedPosition,//固定位置
//}

///// <summary>
///// 波次数据 - 工业级
///// </summary>
//[CreateAssetMenu(fileName = "WaveData_", menuName = "Game/Wave Data")]
//public class WaveData : ScriptableObject
//{
//    [Header("波次信息")]
//    [Tooltip("波次编号")]
//    [SerializeField] private int waveNumber = 1;

//    [Tooltip("波次名称（用于UI显示）")]
//    [SerializeField] private string waveName = "第1波";

//    [Header("敌人配置")]
//    [SerializeField] private List<EnemySpawnConfig> enemiesToSpawn = new List<EnemySpawnConfig>();

//    [Header("时间配置")]
//    [Tooltip("波次准备时间（玩家准备时间）")]
//    [SerializeField] private float prepareDuration = 3f;

//    [Tooltip("波次持续时间（超时自动结束）")]
//    [SerializeField] private float waveDuration = 30f;

//    [Tooltip("波次完成后的休息时间")]
//    [SerializeField] private float nextWaveDelay = 5f;

//    [Header("难度配置")]
//    [Tooltip("血量倍率（用于难度曲线）")]
//    [Range(1f, 10f)] public float healthMultiolier = 1f;

//    [Tooltip("速度倍率")]
//    [Range(1f, 5f)] public float speedMultiplier = 1f;

//    [Tooltip("分数倍率")]
//    [Range(1f, 10f)] public float scoreMultiplier = 1f;

//    //只读属性
//    public int WaveNumber => waveNumber;
//    public string WaveName => waveName;
//    public List<EnemySpawnConfig> EnemiesToSpawn => enemiesToSpawn;
//    public float WaveDuration => waveDuration;
//    public float NextWaveDelay => nextWaveDelay;
//    public float PrepareDuration => prepareDuration;

//    /// <summary>
//    /// 获取本波敌人数量
//    /// </summary>
//    /// <returns></returns>
//    public int GetTotalEnemyCount()
//    {
//        int total = 0;
//        foreach (var config in enemiesToSpawn)
//        {
//            total += config.count;
//        }
//        return total;
//    }
//}


//using UnityEngine;

//[System.Serializable]
//public class EnemySpawnInfo
//{
//    public EnemyData enemyData;
//    public int count = 5;
//    public float spawnInterval = 0.5f;
//}

//[CreateAssetMenu(fileName = "WaveData_", menuName = "Game/Wave Data")]
//public class WaveData : ScriptableObject
//{
//    [Header("波次信息")]
//    [SerializeField] private int waveNumber = 1;
//    [SerializeField] private EnemySpawnInfo[] enemiesToSpawn;
//    [SerializeField] private float waveDuration = 10f;
//    [SerializeField] private float nextWaveDelay = 3f;

//    public int WaveNumber => waveNumber;
//    public EnemySpawnInfo[] EnemiesToSpawn => enemiesToSpawn;
//    public float WaveDuration => waveDuration;
//    public float NextWaveDelay => nextWaveDelay;
//}