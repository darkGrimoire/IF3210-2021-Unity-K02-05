using UnityEngine;
using UnityEngine.UI;
using Mirror;

public class TankShooting : NetworkBehaviour
{
    public int m_PlayerNumber = 1;              // Used to identify the different players.
    public Rigidbody m_Shell;                   // Prefab of the shell.
    public Rigidbody m_BigShell;                // Prefab big shell
    public Rigidbody m_SmallShell;               // Prefab Small Shell
    public Transform m_FireTransform;           // A child of the tank where the shells are spawned.
    public Transform m_SummonTransform;         // A child of the tank where the summons are spawned.
    public Transform m_FireSmallTransform;      // A child
    public Slider m_AimSlider;                  // A child of the tank that displays the current launch force.
    public AudioSource m_ShootingAudio;         // Reference to the audio source used to play the shooting audio. NB: different to the movement audio source.
    public AudioSource m_ChangeAudio;
    public AudioClip m_ChargingClip;            // Audio that plays when each shot is charging up.
    public AudioClip m_FireClip;                // Audio that plays when each shot is fired.
    public AudioClip m_ChangeClip;
    public float m_MinLaunchForce = 15f;        // The force given to the shell if the fire button is not held.
    public float m_MaxLaunchForce = 30f;        // The force given to the shell if the fire button is held for the max charge time.
    public float m_MaxChargeTime = 0.75f;       // How long the shell can charge for before it is fired at max force.


    private ObjectPooler objectPooler = ObjectPooler.Instance;
    private string m_FireButton;                // The input axis that is used for launching shells.
    private int m_SumSummon = 0;
    private float m_CurrentLaunchForce;         // The force that will be given to the shell when the fire button is released.
    private float m_ChargeSpeed;                // How fast the launch force increases, based on the max charge time.
    private bool m_Fired;                       // Whether or not the shell has been launched with this button press.
    private int m_TypeBullet = 0;
    private TankCash tankCash;


    private void OnEnable()
    {
        // When the tank is turned on, reset the launch force and the UI
        m_CurrentLaunchForce = m_MinLaunchForce;
        m_AimSlider.value = m_MinLaunchForce;
    }


    private void Start ()
    {
        // The fire axis is based on the player number.
        m_FireButton = "Fire1";

        // The rate that the launch force charges up is the range of possible forces by the max charge time.
        m_ChargeSpeed = (m_MaxLaunchForce - m_MinLaunchForce) / m_MaxChargeTime;

        tankCash = GetComponent<TankCash>();
    }


    private void Update ()
    {
        // movement for local player
        if (!isLocalPlayer) return;

        // change bullet type
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            m_TypeBullet = (m_TypeBullet + 1) % 3;
            m_ChangeAudio.clip = m_ChangeClip;
            m_ChangeAudio.volume = PlayerPrefs.GetFloat("volMusic");
            m_ChangeAudio.Play();
            Debug.Log("ganti");
        }

        // Summon
        if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            CmdSummon(tankCash.getCash());
        }

        // The slider should have a default value of the minimum launch force.
        m_AimSlider.value = m_MinLaunchForce;

        // If the max force has been exceeded and the shell hasn't yet been launched...
        if (m_CurrentLaunchForce >= m_MaxLaunchForce && !m_Fired)
        {
            // ... use the max force and launch the shell.
            m_CurrentLaunchForce = m_MaxLaunchForce;
            CmdFire(m_TypeBullet, tankCash.getCash());
        }
        // Otherwise, if the fire button has just started being pressed...
        else if (Input.GetButtonDown (m_FireButton))
        {
            // ... reset the fired flag and reset the launch force.
            m_Fired = false;
            m_CurrentLaunchForce = m_MinLaunchForce;

            // Change the clip to the charging clip and start it playing.
            m_ShootingAudio.clip = m_ChargingClip;
            m_ShootingAudio.volume = PlayerPrefs.GetFloat("volMusic");
            m_ShootingAudio.Play ();
        }
        // Otherwise, if the fire button is being held and the shell hasn't been launched yet...
        else if (Input.GetButton (m_FireButton) && !m_Fired)
        {
            // Increment the launch force and update the slider.
            m_CurrentLaunchForce += m_ChargeSpeed * Time.deltaTime;

            m_AimSlider.value = m_CurrentLaunchForce;
        }
        // Otherwise, if the fire button is released and the shell hasn't been launched yet...
        else if (Input.GetButtonUp (m_FireButton) && !m_Fired)
        {
            // ... launch the shell.
            CmdFire (m_TypeBullet, tankCash.getCash());
        }
    }

    // this is called on the server
    [Command]
    void CmdFire(int type, int amount)
    {
        // Create an instance of the shell and store a reference to it's rigidbody.
        if (type == 0){
            if (amount < 500) return;
            Rigidbody shellInstance =
                Instantiate(m_Shell, m_FireTransform.position, m_FireTransform.rotation) as Rigidbody;

            // Set the shell's velocity to the launch force in the fire position's forward direction.
            shellInstance.velocity = m_CurrentLaunchForce * m_FireTransform.forward;

            // Spawn shell on server
            NetworkServer.Spawn(shellInstance.gameObject);

            RpcOnFire();
            tankCash.RpcSubtract(500);
        }
        else if (type == 1){
            if (amount < 1000) return;
            Rigidbody shellInstance =
                Instantiate(m_BigShell, m_FireTransform.position, m_FireTransform.rotation) as Rigidbody;

            // Set the shell's velocity to the launch force in the fire position's forward direction.
            shellInstance.velocity = m_CurrentLaunchForce * m_FireTransform.forward * 2 / 3;

            // Spawn shell on server
            NetworkServer.Spawn(shellInstance.gameObject);

            RpcOnFire();
            tankCash.RpcSubtract(1000);
        }
        else if (type == 2){
            if (amount < 50) return;
            Rigidbody shellInstance =
                Instantiate(m_SmallShell, m_FireSmallTransform.position, m_FireSmallTransform.rotation) as Rigidbody;

            // Set the shell's velocity to the launch force in the fire position's forward direction.
            shellInstance.velocity = m_CurrentLaunchForce * m_FireTransform.forward * 3 / 2;

            // Spawn shell on server
            NetworkServer.Spawn(shellInstance.gameObject);

            RpcOnFire();
            tankCash.RpcSubtract(250);
        }
    }

    [Command]
    void CmdSummon(int amount){
        if (amount < 3000) return;
        Vector3 position = m_SummonTransform.position;
        GameObject spawnedObj = objectPooler.SpawnFromPool("Character", position, Quaternion.identity);
        Debug.Log(m_PlayerNumber);
        spawnedObj.GetComponent<CharacterMovement>().setPlayerNumber(m_PlayerNumber);
        spawnedObj.GetComponent<CharacterMovement>().setEnemy();
        NetworkServer.Spawn(spawnedObj);

        // SetPlayerNumber, SetEnemy
        
        RpcOnSummon();

        tankCash.RpcSubtract(3000);
    }

    // this is called on the tank that fired for all observers
    [ClientRpc]
    void RpcOnFire()
    {
        Fire();
    }

    void RpcOnSummon()
    {
        Summon();
    }
    [Client]
    private void Fire ()
    {
        // Set the fired flag so only Fire is only called once.
        m_Fired = true;
        Debug.Log(m_TypeBullet);
        //// Create an instance of the shell and store a reference to it's rigidbody.
        //Rigidbody shellInstance =
        //    Instantiate (m_Shell, m_FireTransform.position, m_FireTransform.rotation) as Rigidbody;

        //// Set the shell's velocity to the launch force in the fire position's forward direction.
        //shellInstance.velocity = m_CurrentLaunchForce * m_FireTransform.forward;

        // Change the clip to the firing clip and play it.
        m_ShootingAudio.clip = m_FireClip;
        m_ShootingAudio.volume = PlayerPrefs.GetFloat("volMusic");
        m_ShootingAudio.Play ();

        // Reset the launch force.  This is a precaution in case of missing button events.
        m_CurrentLaunchForce = m_MinLaunchForce;
    }

    private void Summon ()
    {
        m_SumSummon = m_SumSummon + 1;
    }
}
