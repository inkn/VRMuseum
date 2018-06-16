//------------------------------------------------------------------------------
// Copyright 2016 Baofeng Mojing Inc. All rights reserved.
//------------------------------------------------------------------------------

using UnityEngine;
public class PlaceCursor : MonoBehaviour {

    private float distance = 1.5f;
    Transform vrHead;
    void Start()
    {
        vrHead = transform.parent;
    }

    private float distance_default = 1.5f;

    void Update()
    {
        if (this == null)
        {
            return;
        }
        if (gameObject.activeInHierarchy)
        {
            Camera cam = Mojing.SDK.getMainCamera();
            if (cam != null)
            {
                float dist = distance;
                dist = dist / Mathf.Abs(Mathf.Cos(Mathf.RoundToInt(vrHead.rotation.eulerAngles.y) * Mathf.Deg2Rad));
                distance_default = dist;
                transform.position = cam.transform.position + cam.transform.forward * dist;
            }
        }
    }
}
