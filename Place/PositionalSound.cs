using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class PositionalSound : MonoBehaviour
{
    public AudioSource Source;
    public float VolumeMultiplier;

    public void Update()
    {
        float distance = Vector3.Distance(GameController.Camera.transform.position, transform.position);
        float volume = Mathf.Clamp(1 - (distance / 6), 0f, 0.6f);
        volume *= Mathf.Clamp01(1 / GameController.Camera.orthographicSize);
        Source.volume = volume * VolumeMultiplier;
    }
}