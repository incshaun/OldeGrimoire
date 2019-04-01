using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectPlacer : MonoBehaviour
{
    public GameObject objectTemplate;

    public int numberOfObjects = 30;

    public float radiusMin = 5.0f;
    public float radiusMax = 15.0f;

    public float timeInterval = 0.3f;

    private float currentTime = 0.0f;

    // Update is called once per frame
    void Update()
    {
        currentTime += Time.deltaTime;

        if ((currentTime > timeInterval) && (numberOfObjects > 0))
        {
            currentTime = 0.0f;
            numberOfObjects--;
            GameObject g = Instantiate(objectTemplate);
            g.transform.position = Random.onUnitSphere * Random.Range (radiusMin, radiusMax);
            g.GetComponent<MeshRenderer>().material.color = Random.ColorHSV(0, 1, 0.5f, 1, 0.5f, 1);
        }
    }
}
