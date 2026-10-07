using System;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Rigidbody))]
public class PlayerController : MonoBehaviour
{
    [Header("引用")]
    [SerializeField] private PlayerData playerData;
    [SerializeField] private Transform bulletSpawnPoint;

    [Header("视觉（用几何体搭建）")]
    [SerializeField] private Color shipColor = Color.cyan;
    [SerializeField] private float shipSize = 0.5f;

    //组件
    private Rigidbody rb;
    private MeshRenderer renderer;

    //射击计时
    private float lastFireTime;
    //无敌状态
    private bool isInvincible = false;
    private float invincibleTimer = 0f;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        //获取 MeshRenderer
        renderer = GetComponentInChildren<MeshRenderer>();

        //如果没有手动赋值，从GameManager获取
        if (playerData == null && GameManager.Instance != null)
        {
            playerData = GameManager.Instance.PlayerData;
        }

        //创建飞船几何体？
        BuildShip();
    }

    // void Start()
    // {
    //订阅事件
    // GameManager.Instance.OnGameOver += OnGameOver;
    // }

    // void OnDestroy()
    // {
    //取消订阅（防止内存泄漏）
    // 安全取消订阅
    // if (GameManager.Instance != null)
    // {
    // 如果是 UnityEvent
    // if (GameManager.Instance.OnGameOver != null)
    // {
    //     GameManager.Instance.OnGameOver.RemoveListener(OnGameOver);
    // }

    // 如果是 Action（用 += 订阅的）
    // GameManager.Instance.OnGameOver -= OnGameOver;
    // }
    // }

    //改为不使用enabled = false 而是用Update内的检查控制
    void Update()
    {
        //游戏结束时只跳过逻辑，不禁用脚本
        if (GameManager.Instance != null && GameManager.Instance.IsGameOver)
        {
            //不执行移动/射击
            // enabled = false;
            return;
        }
        HandleMovement();
        HandleShooting();
        UpdateInvincible();
    }

    //事件回调
    // void OnGameOver()
    // {
    //     //游戏结束时处理
    //     enabled = false;
    // }

    // ==== 移动控制 =====
    void HandleMovement()
    {
        float moveSpeed = playerData?.MoveSpeed ?? 8f;
        float rotationSpeed = playerData?.RotationSpeed ?? 5f;

        //WASD移动
        float horizontal = Input.GetAxis("Horizontal");
        float vertical = Input.GetAxis("Vertical");

        Vector3 moveDirection = new Vector3(horizontal, 0, vertical).normalized;
        if (moveDirection.magnitude > 0.1f)
        {
            //移动
            Vector3 targetPosition = transform.position + moveDirection * moveSpeed * Time.deltaTime;

            //边界限制（在相机视口内）
            Vector3 viewportPos = Camera.main.WorldToViewportPoint(targetPosition);

            viewportPos.x = Mathf.Clamp(viewportPos.x, 0.05f, 0.95f);
            viewportPos.y = Mathf.Clamp(viewportPos.y, 0.05f, 0.95f);

            targetPosition = Camera.main.ViewportToWorldPoint(viewportPos);
            targetPosition.y = 0;
            rb.MovePosition(targetPosition);

            //旋转指向移动方向
            if (moveDirection != Vector3.zero)
            {
                Quaternion targetRotation = Quaternion.LookRotation(moveDirection);
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
            }
        }
    }

    // =====  射击控制 =====
    void HandleShooting()
    {
        //检测射击输入（空格或鼠标左键）
        if (Input.GetKeyDown(KeyCode.Space) || Input.GetMouseButton(0))
        {
            Fire();
        }
    }

    void Fire()
    {
        float fireRate = playerData?.FireRate ?? 0.2f;
        if (Time.time - lastFireTime < fireRate)
            return;
        lastFireTime = Time.time;
        //触发射击事件 （Day3.4会用到）
        // EventCenter.Broadcast("OnWeaponFired");
        // ✅ 枪口闪光
        // if (bulletSpawnPoint != null)
        // {
        //     EffectPoolManager.Instance?.PlayMuzzleFlash(
        //         bulletSpawnPoint.position,
        //         transform.rotation
        //     );
        // }
        // ✅ 播放射击音效
        // AudioPoolManager.Instance?.PlayShoot();
        //使用对象池生成子弹
        if (BulletPoolManager.Instance != null)
        {
            BulletPoolManager.Instance.GetBullet(bulletSpawnPoint.position, transform.rotation);
        }
        else
        {
            Debug.LogWarning("BulletPoolManager 不存在，使用旧的CreateBullet");
            CreateBullet();//降级方案
        }

        Debug.Log($"射击！");
    }

    void CreateBullet()
    {
        //临时子弹实现，使用几何体
        GameObject bullet = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        bullet.transform.position = bulletSpawnPoint.position;
        bullet.transform.rotation = transform.rotation;
        bullet.transform.localScale = Vector3.one * 0.15f;
        bullet.GetComponent<MeshRenderer>().material.color = Color.cyan;

        //添加子弹脚本
        Bullet bulletScript = bullet.AddComponent<Bullet>();

        //等Day5接入对象池
    }

    void UpdateInvincible()
    {
        if (isInvincible)
        {
            invincibleTimer -= Time.deltaTime;
            //闪烁效果
            if (renderer != null)
            {
                renderer.enabled = Mathf.FloorToInt(invincibleTimer * 10) % 2 == 0;
            }
            if (invincibleTimer <= 0)
            {
                isInvincible = false;
                if (renderer != null)
                {
                    renderer.enabled = true;
                }
            }
        }
    }

    public void BecomeInvincible(float duration)
    {
        isInvincible = true;
        invincibleTimer = duration;
    }

    //碰撞处理
    void OnCollisionEnter(Collision collision)
    {
        if (isInvincible) return;
        if (collision.gameObject.CompareTag("Enemy"))
        {
            //受到伤害
            GameManager.Instance?.TakeDamage(1);
            // ✅ 播放受伤音效
            // AudioPoolManager.Instance?.PlayDamage();

            BecomeInvincible(playerData?.InvincibleTime ?? 1f);

            //简单击退效果
            Vector3 pushDirection = (transform.position - collision.transform.position).normalized;
            rb.AddForce(pushDirection * 5f, ForceMode.Impulse);
        }
    }


    // ==== 飞船构造 =====
    void BuildShip()
    {
        //主身体
        GameObject body = GameObject.CreatePrimitive(PrimitiveType.Capsule);
        body.transform.SetParent(transform);
        body.transform.localPosition = Vector3.zero;
        body.transform.localScale = Vector3.one * shipSize;
        body.GetComponent<MeshRenderer>().material.color = shipColor;

        //机翼：用Cube拼两个小翅膀
        GameObject wingLeft = GameObject.CreatePrimitive(PrimitiveType.Cube);
        wingLeft.transform.SetParent(transform);
        wingLeft.transform.localPosition = new Vector3(-shipSize * 0.8f, 0, 0);
        wingLeft.transform.localScale = new Vector3(0.3f, 0.05f, 0.8f) * shipSize;
        wingLeft.GetComponent<MeshRenderer>().material.color = shipColor;

        GameObject wingRight = GameObject.CreatePrimitive(PrimitiveType.Cube);
        wingRight.transform.SetParent(transform);
        wingRight.transform.localPosition = new Vector3(shipSize * 0.8f, 0, 0);
        wingRight.transform.localScale = new Vector3(0.3f, 0.05f, 0.8f) * shipSize;
        wingRight.GetComponent<MeshRenderer>().material.color = shipColor;

        //驾驶舱：小Sphere
        GameObject cockpit = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        cockpit.transform.SetParent(transform);
        cockpit.transform.localPosition = new Vector3(0, shipSize * 0.3f, shipSize * 0.6f);
        cockpit.transform.localScale = Vector3.one * shipSize * 0.35f;
        cockpit.GetComponent<MeshRenderer>().material.color = Color.white;

        //引擎喷口：小Cyliner
        GameObject engine = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        engine.transform.SetParent(transform);
        engine.transform.localPosition = new Vector3(0, 0, -shipSize * 0.8f);
        engine.transform.localScale = new Vector3(0.3f, 0.1f, 0.3f) * shipSize;
        engine.GetComponent<MeshRenderer>().material.color = new Color(1, 0.5f, 0);//橙色

        //子弹生成点
        if (bulletSpawnPoint == null)
        {
            GameObject spawnPoint = new GameObject("BulletSpawn");
            spawnPoint.transform.SetParent(transform);
            spawnPoint.transform.localPosition = new Vector3(0, 0, shipSize * 1.2f);
            bulletSpawnPoint = spawnPoint.transform;
        }

        //添加碰撞体（自动生成的都有）
        //调整刚体
        rb.mass = 1f;
        rb.useGravity = false;
        rb.constraints = RigidbodyConstraints.FreezeRotation;

        //让子物体不阻挡射线（可选）
        foreach (Transform child in transform)
        {
            child.gameObject.layer = LayerMask.NameToLayer("Player");
        }
        gameObject.layer = LayerMask.NameToLayer("Player");
    }
}