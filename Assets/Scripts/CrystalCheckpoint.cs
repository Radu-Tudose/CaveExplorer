using UnityEngine;

public class CrystalCheckpoint : MonoBehaviour
{
    public Material inactiveMaterial;
    public Material activeMaterial;

    private static CrystalCheckpoint currentCheckpoint;
    private bool playerNearby = false;

    void Update()
    {
        if (playerNearby && Input.GetKeyDown(KeyCode.E))
        {
            Activate();
        }
    }

    public void Activate()
    {
        // Deactivate the previous checkpoint
        if (currentCheckpoint != null && currentCheckpoint != this)
        {
            currentCheckpoint.SetMaterial(currentCheckpoint.inactiveMaterial);
        }

        currentCheckpoint = this;
        SetMaterial(activeMaterial);
    }

    void SetMaterial(Material material)
    {
        Renderer[] renderers = GetComponentsInChildren<Renderer>();

        foreach (Renderer renderer in renderers)
        {
            renderer.material = material;
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerNearby = true;
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerNearby = false;
        }
    }

    public static Vector3 GetRespawnPosition()
    {
        if (currentCheckpoint != null)
        {
            return currentCheckpoint.transform.position;
        }

        return Vector3.zero;
    }

    public static bool HasCheckpoint()
    {
        return currentCheckpoint != null;
    }

    public static void ResetCheckpoint()
    {
        currentCheckpoint = null;
    }
}

