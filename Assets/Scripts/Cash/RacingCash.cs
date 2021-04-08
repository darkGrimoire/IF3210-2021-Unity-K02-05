using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class RacingCash : MonoBehaviour
{
    public float lifeTime = 20f;
    public float rotation = 0.5f;
    public int value = 50;
    public AudioSource m_CashAudio;

    public void Start()
    {
        Invoke(nameof(DestroySelf), lifeTime);
    }

    void Update()
    {
        transform.Rotate(new Vector3(0, rotation, 0));
    }

    void DestroySelf()
    {
        if(gameObject != null) Destroy(gameObject);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.tag != "Player") return;
        other.GetComponent<RacingTankCash>().Add(value);
        DestroySelf();
    }

}
