using UnityEngine;
using UnityEngine.XR.ARFoundation;

/// <summary>
/// Mantiene una araña activa en una posición aleatoria del plano bloqueado.
/// Reutiliza el cálculo de posición del PackageSpawner del starter package.
/// </summary>
public class SpiderSpawner : MonoBehaviour
{
    public DrivingSurfaceManager DrivingSurfaceManager;
    public GameObject SpiderPrefab;

    private GameObject spiderInstance;
    private ARPlane spawnedOnPlane;
    private Vector3 spiderLocalPosition;

    private void Update()
    {
        if (DrivingSurfaceManager == null || SpiderPrefab == null)
            return;

        ARPlane lockedPlane = DrivingSurfaceManager.LockedPlane;

        if (lockedPlane == null)
            return;

        if (spiderInstance == null)
        {
            SpawnSpider(lockedPlane);
            return;
        }

        // Si ARCore sustituye el plano, conserva la posición
        // y vuelve a proyectar la araña sobre el nuevo plano.
        if (spawnedOnPlane != lockedPlane)
        {
            spiderLocalPosition =
                lockedPlane.transform.InverseTransformPoint(
                    spiderInstance.transform.position
                );

            spiderLocalPosition.y = lockedPlane.center.y;
            spawnedOnPlane = lockedPlane;
        }

        // Mantiene la araña sobre la superficie del plano.
        spiderInstance.transform.position =
            lockedPlane.transform.TransformPoint(spiderLocalPosition);
    }

    private void SpawnSpider(ARPlane plane)
    {
        ARPlaneMeshVisualizer visualizer =
            plane.GetComponent<ARPlaneMeshVisualizer>();

        if (visualizer == null ||
            visualizer.mesh == null ||
            visualizer.mesh.triangles.Length < 3)
            return;

        // Busca una posición aleatoria dentro del plano.
        Vector3 randomWorldPosition =
            PackageSpawner.FindRandomLocation(plane);

        // Convierte la posición mundial a coordenadas locales.
        spiderLocalPosition =
            plane.transform.InverseTransformPoint(randomWorldPosition);

        // Ajusta la altura a la superficie del plano.
        spiderLocalPosition.y = plane.center.y;

        // Convierte de nuevo a coordenadas mundiales.
        Vector3 spawnPosition =
            plane.transform.TransformPoint(spiderLocalPosition);

        spiderInstance = Instantiate(
            SpiderPrefab,
            spawnPosition,
            plane.transform.rotation
        );

        spawnedOnPlane = plane;

        if (!spiderInstance.CompareTag("Spider"))
        {
            Debug.LogWarning(
                "La raíz del prefab debe tener el tag Spider."
            );
        }
    }
}
