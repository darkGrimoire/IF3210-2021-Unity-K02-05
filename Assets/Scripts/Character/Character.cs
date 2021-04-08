using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class Character : NetworkBehaviour, IPooledObject
{
    public float lifeTime = 20f;
    public float rotation = 0.5f;
    public LayerMask m_TankMask;
    public float m_ExplosionRadius = 3f;

    public void OnObjectSpawned()
    {
        Invoke(nameof(DestroySelf), lifeTime);
    }

    // destroy for everyone on the server
    [Server]
    void DestroySelf()
    {
        //NetworkServer.Destroy(gameObject);
        NetworkServer.UnSpawn(gameObject);
        gameObject.SetActive(false);
    }

    /*[ServerCallback]
    private void OnTriggerEnter (Collider other)
    {
        // Collect all the colliders in a sphere from the shell's current position to a radius of the explosion radius.
        Collider[] colliders = Physics.OverlapSphere (transform.position, m_ExplosionRadius, m_TankMask);

        // Go through all the colliders...
        for (int i = 0; i < colliders.Length; i++)
        {
            // ... and find their rigidbody.
            Rigidbody targetRigidbody = colliders[i].GetComponent<Rigidbody> ();

            // If they don't have a rigidbody, go on to the next collider.
            if (!targetRigidbody)
                continue;

            // Find the TankHealth script associated with the rigidbody.
            TankHealth targetHealth = targetRigidbody.GetComponent<TankHealth> ();

            // If there is no TankHealth script attached to the gameobject, go on to the next collider.
            if (!targetHealth)
                continue;

            // Calculate the amount of damage the target should take based on it's distance from the shell.
            float damage = 75f;

            // Deal this damage to the tank.
            targetHealth.RpcTakeDamage (damage);
        }
    }*/
}
