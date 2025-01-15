using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShowPosition : MonoBehaviour {

        public GPSTracking locationService;
	
        public GameObject markerPrefab;
        
        public float globeRadius = 0.5f;
        
        public float rotationSpeed = 30.0f;
        
        private GameObject marker;
        
	void Update () {
          // Rotate the globe.
          transform.rotation *= Quaternion.AngleAxis (rotationSpeed * Time.deltaTime, Vector3.up);
		
          // Plot position.
          float latitude;
          float longitude;
          float altitude;
          if (locationService.retrieveLocation (out latitude, out longitude, out altitude))
          {
            Vector3 position = globeRadius * 
              new Vector3 (Mathf.Cos (latitude * Mathf.Deg2Rad) * Mathf.Cos (longitude * Mathf.Deg2Rad),
                           Mathf.Sin (latitude * Mathf.Deg2Rad),
                           Mathf.Cos (latitude * Mathf.Deg2Rad) * Mathf.Sin (longitude * Mathf.Deg2Rad));
              
            if (marker == null)
            {
              marker = Instantiate (markerPrefab);
              marker.transform.SetParent (transform);
            }
            marker.transform.localPosition = position;
            marker.transform.up = transform.TransformDirection (position);
          }
	}
}
