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
using System.Linq;

using Unity.Collections;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;

public class ReticleBehaviour : MonoBehaviour
{
    public GameObject Child;
    public DrivingSurfaceManager DrivingSurfaceManager;
    public ARPlane CurrentPlane;

    private readonly List<ARRaycastHit> hits = new List<ARRaycastHit>();

    private void Start()
    {
        if (Child == null && transform.childCount > 0)
            Child = transform.GetChild(0).gameObject;

        if (Child != null)
            Child.SetActive(false);
    }

    private void Update()
    {
        CurrentPlane = null;

        if (DrivingSurfaceManager == null ||
            DrivingSurfaceManager.RaycastManager == null ||
            DrivingSurfaceManager.PlaneManager == null ||
            Camera.main == null)
        {
            if (Child != null)
                Child.SetActive(false);
            return;
        }

        Vector3 screenCenter = Camera.main.ViewportToScreenPoint(
            new Vector3(0.5f, 0.5f, 0f)
        );

        hits.Clear();
        DrivingSurfaceManager.RaycastManager.Raycast(
            screenCenter,
            hits,
            TrackableType.PlaneWithinBounds
        );

        ARRaycastHit? selectedHit = null;
        ARPlane lockedPlane = DrivingSurfaceManager.LockedPlane;

        if (lockedPlane == null)
        {
            if (hits.Count > 0)
                selectedHit = hits[0];
        }
        else
        {
            // Una vez colocado el coche, la retícula solo acepta el plano bloqueado.
            foreach (ARRaycastHit candidate in hits)
            {
                if (candidate.trackableId == lockedPlane.trackableId)
                {
                    selectedHit = candidate;
                    break;
                }
            }
        }

        if (selectedHit.HasValue)
        {
            CurrentPlane = DrivingSurfaceManager.PlaneManager.GetPlane(
                selectedHit.Value.trackableId
            );

            if (CurrentPlane != null)
                transform.position = selectedHit.Value.pose.position;
        }

        if (Child != null)
            Child.SetActive(CurrentPlane != null);
    }
}
