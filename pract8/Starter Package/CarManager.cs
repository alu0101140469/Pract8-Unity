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

using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.InputSystem;

/**
 * Spawns a <see cref="CarBehaviour"/> when a plane is tapped.
 */
public class CarManager : MonoBehaviour
{
    public GameObject CarPrefab;
    public ReticleBehaviour Reticle;
    public DrivingSurfaceManager DrivingSurfaceManager;
    public ScoreManager ScoreManager;
    public CarBehaviour Car;

    private void Update()
    {
        if (Car != null || !WasTapped() || Reticle == null || Reticle.CurrentPlane == null)
            return;

        if (CarPrefab == null)
        {
            Debug.LogError("CarManager: asigna Car Prefab en el Inspector.", this);
            return;
        }

        if (DrivingSurfaceManager == null)
        {
            Debug.LogError("CarManager: asigna Driving Surface Manager en el Inspector.", this);
            return;
        }

        GameObject carObject = Instantiate(CarPrefab);
        Car = carObject.GetComponent<CarBehaviour>();

        if (Car == null)
        {
            Debug.LogError("Car Prefab debe tener el componente CarBehaviour.", carObject);
            Destroy(carObject);
            return;
        }

        Car.Reticle = Reticle;
        Car.ScoreManager = ScoreManager;
        Car.transform.position = Reticle.transform.position;

        DrivingSurfaceManager.LockPlane(Reticle.CurrentPlane);
    }

    private bool WasTapped()
    {
        // Entrada táctil para Android/iOS.
        if (Touchscreen.current != null &&
            Touchscreen.current.primaryTouch.press.wasPressedThisFrame)
        {
            return true;
        }

        // Clic de ratón para probar en el Editor.
        if (Mouse.current != null &&
            Mouse.current.leftButton.wasPressedThisFrame)
        {
            return true;
        }

        return false;
    }
}
