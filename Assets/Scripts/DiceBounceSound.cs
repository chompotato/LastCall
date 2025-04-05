using UnityEngine;

public class DiceBounceSound : MonoBehaviour
{
    private AudioClip bounceSFX1;
    private AudioClip bounceSFX2;
    private float minVolume = 0.1f;  // Lowered minimum volume for subtle bounces
    private float maxVolume = 1.0f;
    private AudioSource audioSource;
    private Rigidbody rb;
    private float lastYVelocity;
    private float minImpactThreshold = 0.01f; // Lowered threshold for more sensitivity

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.spatialBlend = 1.0f;
        audioSource.playOnAwake = false;
    }

    void FixedUpdate()
    {
        lastYVelocity = rb.linearVelocity.y; // Unity 6: Using linearVelocity
    }

    private void OnCollisionEnter(Collision collision)
    {
        if ((bounceSFX1 == null && bounceSFX2 == null) || audioSource == null || collision.contacts.Length == 0)
            return;

        float impactForce = Mathf.Abs(lastYVelocity);
        if (impactForce < minImpactThreshold) return; // Now even gentle touches trigger sound

        float volume = Mathf.Clamp(impactForce / 5f, minVolume, maxVolume); // Adjusted volume scaling

        // Randomly select one of the two bounce sounds
        AudioClip selectedClip = (Random.value < 0.5f) ? bounceSFX1 : bounceSFX2;

        if (selectedClip != null)
            audioSource.PlayOneShot(selectedClip, volume);
    }

    public void SetBounceSounds(AudioClip clip1, AudioClip clip2, float minVol, float maxVol)
    {
        bounceSFX1 = clip1;
        bounceSFX2 = clip2;
        minVolume = minVol;
        maxVolume = maxVol;
    }
}
