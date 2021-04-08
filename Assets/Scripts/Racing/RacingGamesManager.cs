using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class RacingGamesManager : MonoBehaviour
{
    public float m_StartDelay = 0f;             // The delay between the start of RoundStarting and RoundPlaying phases.
    public float m_EndDelay = 3f;               // The delay between the end of RoundPlaying and RoundEnding phases.
    public CameraControl m_CameraControl;       // Reference to the CameraControl script for control during different phases.
    public Text m_MessageText;                  // Reference to the overlay Text to display winning text, etc.
    public GameObject m_TankPrefab;             // Reference to the prefab the players will control.
    public RacingTankController m_Tanks;        // A collection of managers for enabling and disabling different aspects of the tanks.


    private WaitForSeconds m_StartWait;         // Used to have a delay whilst the round starts.
    private WaitForSeconds m_EndWait;           // Used to have a delay whilst the round or game ends.
    public RacingTankController m_GameWinner;
    public float zSpawnRange = 10f;
    public float xSpawnRange = 10f;
    private AudioSource bgSound;
    public Text m_TimeText;
    public Text m_CashText;

    private void Start()
    {
        // Create the delays so they only have to be made once.
        m_StartWait = new WaitForSeconds (m_StartDelay);
        m_EndWait = new WaitForSeconds (m_EndDelay);

        bgSound = GetComponent<AudioSource>();
        bgSound.volume = PlayerPrefs.GetFloat("volMusic");
        bgSound.Play();

        SpawnAllTanks();
        SetCameraTargets();

        // Once the tanks have been created and the camera is using them as targets, start the game.
        StartCoroutine (GameLoop ());
    }


    private void SpawnAllTanks()
    {
        // For all the tanks...
        // ... create them, set their player number and references needed for control.
        Vector3 position = new Vector3(Random.Range(-xSpawnRange, xSpawnRange), 0, Random.Range(-zSpawnRange, zSpawnRange));
        m_Tanks.m_Instance =
            Instantiate(m_TankPrefab, position, Quaternion.identity) as GameObject;
        m_Tanks.m_PlayerNumber = 1;
        m_Tanks.Setup();
    }


    private void SetCameraTargets()
    {
        // Create a collection of transforms the same size as the number of tanks.
        Transform[] targets = new Transform[1];

        // For each of these transforms...
        for (int i = 0; i < targets.Length; i++)
        {
            // ... set it to the appropriate tank transform.
            targets[i] = m_Tanks.m_Instance.transform;
        }

        // These are the targets the camera should follow.
        m_CameraControl.m_Targets = targets;
    }


    // This is called from start and will run each phase of the game one after another.
    private IEnumerator GameLoop ()
    {
        // Start off by running the 'RoundStarting' coroutine but don't return until it's finished.
        yield return StartCoroutine (RoundStarting ());

        // Once the 'RoundStarting' coroutine is finished, run the 'RoundPlaying' coroutine but don't return until it's finished.
        yield return StartCoroutine (RoundPlaying());

        // Once execution has returned here, run the 'RoundEnding' coroutine, again don't return until it's finished.
        yield return StartCoroutine (RoundEnding());

        SceneManager.LoadSceneAsync(0);
    }


    private IEnumerator RoundStarting ()
    {
        // As soon as the round starts reset the tanks and make sure they can't move.
        ResetAllTanks ();
        DisableTankControl ();

        // Snap the camera's zoom and position to something appropriate for the reset tanks.
        m_CameraControl.SetStartPositionAndSize ();

        // Wait for the specified length of time until yielding control back to the game loop.
        yield return m_StartWait;
    }


    private IEnumerator RoundPlaying ()
    {
        // As soon as the round begins playing let the players control the tanks.
        EnableTankControl ();

        // Clear the text from the screen.
        m_MessageText.text = string.Empty;

        // While there is not one tank left...
        while (!isGameOver())
        {
            // ... return on the next frame.
            yield return null;
        }
    }


    public IEnumerator RoundEnding ()
    {
        // Stop tanks from moving.
        DisableTankControl ();

        // Get a message based on the scores and whether or not there is a game winner and display it.
        string message = EndMessage ();

        m_MessageText.text = message;

        // Wait for the specified length of time until yielding control back to the game loop.
        yield return m_EndWait;
    }


    // This is used to check if there is one or fewer tanks remaining and thus the round should end.
    private bool isGameOver()
    {
        m_TimeText = GameObject.Find("Time").GetComponent<Text>();
        return m_TimeText.text == "00:00";
    }

    // Returns a string message to display at the end of each round.
    private string EndMessage()
    {
        // By default when a round ends there are no winners so the default end message is a draw.
        m_CashText = GameObject.Find("CashText").GetComponent<Text>();
        return "You succesfully earned " + m_CashText.text + "!";
    }


    // This function is used to turn all the tanks back on and reset their positions and properties.
    public void ResetAllTanks()
    {
        m_Tanks.Reset();
    }


    private void EnableTankControl()
    {
        m_Tanks.EnableControl();
    }


    public void DisableTankControl()
    {
        m_Tanks.DisableControl();
    }
}