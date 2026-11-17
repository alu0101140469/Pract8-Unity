/*
 * Copyright 2021 Google LLC
 *
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 *
 *      http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.ARFoundation;

public class PackageSpawner : MonoBehaviour
{
    public DrivingSurfaceManager DrivingSurfaceManager;
    public PackageBehaviour Package;
    public GameObject PackagePrefab;

    private ARPlane spawnedOnPlane;
    private Vector3 packageLocalPosition;
    private bool missingPrefabReported;

    /// <summary>
    /// Devuelve un punto aleatorio uniforme dentro del triángulo ABC.
    /// </summary>
    public static Vector3 RandomInTriangle(Vector3 a, Vector3 b, Vector3 c)
    {
        float r1 = Mathf.Sqrt(Random.value);
        float r2 = Random.value;
        return (1f - r1) * a + r1 * (1f - r2) * b + r1 * r2 * c;
    }

    /// <summary>
    /// Elige un triángulo real de la malla del plano y transforma su punto a coordenadas mundo.
    /// </summary>
    public static Vector3 FindRandomLocation(ARPlane plane)
    {
        if (plane == null)
            return Vector3.zero;

        Vector3 fallback = plane.transform.TransformPoint(plane.center);
        ARPlaneMeshVisualizer visualizer = plane.GetComponent<ARPlaneMeshVisualizer>();
        if (visualizer == null || visualizer.mesh == null)
            return fallback;

        Mesh mesh = visualizer.mesh;
        int[] triangles = mesh.triangles;
        Vector3[] vertices = mesh.vertices;

        if (triangles == null || triangles.Length < 3 || vertices == null || vertices.Length < 3)
            return fallback;

        int triangleCount = triangles.Length / 3;
        int firstIndex = Random.Range(0, triangleCount) * 3;

        int ia = triangles[firstIndex];
        int ib = triangles[firstIndex + 1];
        int ic = triangles[firstIndex + 2];

        if (ia < 0 || ib < 0 || ic < 0 ||
            ia >= vertices.Length || ib >= vertices.Length || ic >= vertices.Length)
        {
            return fallback;
        }

        Vector3 localPoint = RandomInTriangle(vertices[ia], vertices[ib], vertices[ic]);
        return plane.transform.TransformPoint(localPoint);
    }

    public void SpawnPackage(ARPlane plane)
    {
        if (plane == null)
            return;

        if (PackagePrefab == null)
        {
            if (!missingPrefabReported)
            {
                Debug.LogError("PackageSpawner: asigna Package Prefab en el Inspector.", this);
                missingPrefabReported = true;
            }
            return;
        }

        Vector3 spawnPosition = FindRandomLocation(plane);
        GameObject packageObject = Instantiate(PackagePrefab, spawnPosition, Quaternion.identity);
        PackageBehaviour behaviour = packageObject.GetComponent<PackageBehaviour>();

        if (behaviour == null)
        {
            Debug.LogError("Package Prefab debe tener PackageBehaviour en su raíz.", packageObject);
            Destroy(packageObject);
            return;
        }

        Package = behaviour;
        spawnedOnPlane = plane;
        packageLocalPosition = plane.transform.InverseTransformPoint(spawnPosition);
        packageLocalPosition.y = plane.center.y;
    }

    private void Update()
    {
        if (DrivingSurfaceManager == null)
            return;

        ARPlane lockedPlane = DrivingSurfaceManager.LockedPlane;
        if (lockedPlane == null)
            return;

        if (Package == null)
        {
            SpawnPackage(lockedPlane);
            return;
        }

        // Si ARCore sustituye el plano bloqueado por uno subsumido, conserva la posición.
        if (spawnedOnPlane != lockedPlane)
        {
            packageLocalPosition = lockedPlane.transform.InverseTransformPoint(Package.transform.position);
            packageLocalPosition.y = lockedPlane.center.y;
            spawnedOnPlane = lockedPlane;
        }

        // Proyecta el paquete sobre el plano y permite que siga sus pequeños reajustes.
        Package.transform.position = lockedPlane.transform.TransformPoint(packageLocalPosition);
    }
}
