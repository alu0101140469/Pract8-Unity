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

public class DrivingSurfaceManager : MonoBehaviour
{
    public ARPlaneManager PlaneManager;
    public ARRaycastManager RaycastManager;
    public ARPlane LockedPlane;

    private void Awake()
    {
        if (PlaneManager == null)
            PlaneManager = GetComponent<ARPlaneManager>();

        if (RaycastManager == null)
            RaycastManager = GetComponent<ARRaycastManager>();

        if (PlaneManager == null)
            Debug.LogError("DrivingSurfaceManager necesita ARPlaneManager en el mismo XR Origin.", this);

        if (RaycastManager == null)
            Debug.LogError("DrivingSurfaceManager necesita ARRaycastManager en el mismo XR Origin.", this);
    }

    private void OnEnable()
    {
        if (PlaneManager == null)
            PlaneManager = GetComponent<ARPlaneManager>();

        if (PlaneManager != null)
        {
            PlaneManager.trackablesChanged.RemoveListener(OnPlanesChanged);
            PlaneManager.trackablesChanged.AddListener(OnPlanesChanged);
        }
    }

    public void LockPlane(ARPlane keepPlane)
    {
        if (keepPlane == null || PlaneManager == null)
            return;

        foreach (ARPlane plane in PlaneManager.trackables)
        {
            if (plane != keepPlane)
                plane.gameObject.SetActive(false);
        }

        LockedPlane = keepPlane;
    }

    private void Update()
    {
        if (LockedPlane != null && LockedPlane.subsumedBy != null)
            LockedPlane = LockedPlane.subsumedBy;
    }

    private void OnDisable()
    {
        if (PlaneManager != null)
            PlaneManager.trackablesChanged.RemoveListener(OnPlanesChanged);
    }

    private void OnPlanesChanged(ARTrackablesChangedEventArgs<ARPlane> changes)
    {
        if (LockedPlane == null)
            return;

        foreach (ARPlane plane in changes.added)
        {
            if (plane != LockedPlane)
                plane.gameObject.SetActive(false);
        }
    }
}
