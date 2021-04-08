using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using Mirror;

public class CharacterMovement : NetworkBehaviour
{
    public float animSpeed = 1.5f;				// アニメーション再生速度設定
    public float lookSmoother = 3.0f;			// a smoothing setting for camera motion
    public bool useCurves = true;				// Mecanimでカーブ調整を使うか設定する
    public float useCurvesHeight = 0.5f;		// カーブ補正の有効高さ（地面をすり抜けやすい時には大きくする）

    public LayerMask m_TankMask;
    public float m_ExplosionRadius = 3f;
    public float forwardSpeed = 7.0f;
    public float backwardSpeed = 2.0f;
    public float rotateSpeed = 2.0f;
    public float jumpPower = 3.0f;
    private CapsuleCollider col;
    private Rigidbody rb;
    private Vector3 velocity;
    private float orgColHight;
    private Vector3 orgVectColCenter;
    private Animator anim;							// キャラにアタッチされるアニメーターへの参照
    private AnimatorStateInfo currentBaseState;			// base layerで使われる、アニメーターの現在の状態の参照
    private float v = 1;
    private bool isDamaged = true;
    [SyncVar(hook = nameof(updateSpeed))] private float speed = 1.5f;
    [SyncVar(hook = nameof(updateKick))] private bool kick = false;


    private GameObject cameraObject;    // メインカメラへの参照

    static int idleState = Animator.StringToHash("Base Layer.Idle");
    static int locoState = Animator.StringToHash("Base Layer.Locomotion");

    Transform player;
    //PlayerHealth playerHealth;
    int playerNumber = 1;
    //EnemyHealth enemyHealth;
    UnityEngine.AI.NavMeshAgent nav;

    void Start()
    {
        anim = GetComponent<Animator>();
        col = GetComponent<CapsuleCollider>();
        rb = GetComponent<Rigidbody>();
        orgColHight = col.height;
        orgVectColCenter = col.center;
    }

    public void setPlayerNumber(int pnum)
    {
        playerNumber = pnum;
    }

    public void setEnemy()
    {
        //Cari game object with tag player
        GameObject[] res = GameObject.FindGameObjectsWithTag("Player");
        int playerIndex = 0;
        int j;
        for (j = 0; j < res.Length; j++)
        {
            if (res[j].GetComponent<TankController>().m_PlayerNumber == playerNumber)
            {
                playerIndex = j;
                break;
            }
        }
        float minDist = 1000000f;
        int minIndex = 0;
        int i;
        for (i = 0; i < res.Length; i++)
        {
            if (res[i].GetComponent<TankController>().m_PlayerNumber != playerNumber)
            {
                float dist = 0;
                dist = ((res[i].transform.position.x - res[playerIndex].transform.position.x) * (res[i].transform.position.x - res[playerIndex].transform.position.x)) +
                ((res[i].transform.position.y - res[playerIndex].transform.position.y) * (res[i].transform.position.x - res[playerIndex].transform.position.y)) +
                ((res[i].transform.position.z - res[playerIndex].transform.position.z) * (res[i].transform.position.z - res[playerIndex].transform.position.z));
                if (minDist > dist)
                {
                    minDist = dist;
                    minIndex = i;
                }
            }
        }
        player = res[minIndex].transform;
    }


    /*void FixedUpdate ()
    {
        anim.SetFloat ("Speed", v);							// Animator側で設定している"Speed"パラメタにvを渡す
        anim.SetFloat ("Direction", h); 						// Animator側で設定している"Direction"パラメタにhを渡す
        anim.speed = animSpeed;								// Animatorのモーション再生速度に animSpeedを設定する
        currentBaseState = anim.GetCurrentAnimatorStateInfo (0);	// 参照用のステート変数にBase Layer (0)の現在のステートを設定する
        rb.useGravity = true;//ジャンプ中に重力を切るので、それ以外は重力の影響を受けるようにする

        if (currentBaseState.nameHash == locoState) {
            if (useCurves) {
                resetCollider ();
            }
        }
        else if (currentBaseState.nameHash == idleState) {
            if (useCurves) {
                resetCollider ();
            }
        }
    }*/

    void resetCollider()
    {
        col.height = orgColHight;
        col.center = orgVectColCenter;
    }



    void Awake()
    {

        //Mendapatkan componen reference
        //playerHealth = player.GetComponent<PlayerHealth>();
        //enemyHealth = GetComponent<EnemyHealth>();
        nav = GetComponent<UnityEngine.AI.NavMeshAgent>();
    }

    IEnumerator doDamage()
    {
        Collider[] colliders = Physics.OverlapSphere(transform.position, m_ExplosionRadius, m_TankMask);

        // Go through all the colliders...
        for (int i = 0; i < colliders.Length; i++)
        {
            // ... and find their rigidbody.
            Rigidbody targetRigidbody = colliders[i].GetComponent<Rigidbody>();

            // If they don't have a rigidbody, go on to the next collider.
            if (!targetRigidbody)
                continue;

            // Find the TankHealth script associated with the rigidbody.
            TankHealth targetHealth = targetRigidbody.GetComponent<TankHealth>();

            // If there is no TankHealth script attached to the gameobject, go on to the next collider.
            if (!targetHealth)
                continue;

            TankController targetTank = targetRigidbody.GetComponent<TankController>();

            if (targetTank.m_PlayerNumber == playerNumber)
                continue;

            // Calculate the amount of damage the target should take based on it's distance from the shell.
            float damage = 20f;

            // Deal this damage to the tank.
            targetHealth.RpcTakeDamage(damage);
            break;
        }
        yield return new WaitForSeconds(1.0f);
        isDamaged = true;
    }

    [ServerCallback]
    void FixedUpdate()
    {
        //Pindah ke player position
        //if (enemyHealth.currentHealth > 0 && playerHealth.currentHealth> 0)
        //{
        anim.SetFloat("Speed", speed);
        anim.SetBool("Kick", kick);
        if (useCurves)
        {
            resetCollider();
        }
        if (player == null) return;
        nav.SetDestination(player.position);
        if (nav.remainingDistance < 3.0f)
        {
            speed = 0f;
            kick = true;
            if (isDamaged)
            {
                isDamaged = false;
                StartCoroutine(doDamage());
            }
        }
        else
        {
            speed = 1.5f;
            kick = false;
        }
        //}
        /*else //Stop moving
        {
            nav.enabled = false;
        }*/
    }

    [Client]
    void updateSpeed(float _, float speed)
    {
        anim.SetFloat("Speed", speed);
        if (useCurves)
        {
            resetCollider();
        }
    }

    [Client]
    void updateKick(bool _, bool kick)
    {
        anim.SetBool("Kick", kick);
        if (useCurves)
        {
            resetCollider();
        }
    }
}