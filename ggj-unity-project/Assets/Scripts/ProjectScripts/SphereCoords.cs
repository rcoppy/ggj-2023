using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SphereCoords : MonoBehaviour {

    // Give angles in degrees
    public float radius = 1f; // distance from origin
    public float pitch = 0f;  // aka polar/zenith/inclination
    public float yaw = 0f;    // aka azimuth

    private Vector3 lastSphereCoords;
    private Vector3 stepDeltaVector; 
    
    //^^these defaults give the vector (1, 0, 0)

	// Initialization 
	void Start () {
        lastSphereCoords = new Vector3(radius, pitch, yaw);
	}
	
	// Update is called once per frame
	public void LateUpdate () {

        pitch %= 360f;
        yaw %= 360f;

        Vector3 current = new Vector3(radius, pitch, yaw);

        stepDeltaVector = current - lastSphereCoords; 

        lastSphereCoords = current; 

	}

    // pass individual floats to set coords
    public void SetSphereCoords(float r, float p, float y)
    {
        lastSphereCoords = new Vector3(radius, pitch, yaw);

        radius = r;
        pitch = p;
        yaw = y; 
    }

    // pass a vector to set coords
    public void SetSphereCoords(Vector3 coords)
    {
        lastSphereCoords = new Vector3(radius, pitch, yaw);

        radius = coords.x;
        pitch = coords.y;
        yaw = coords.z;
    }

    // difference between new and old coord set
    public Vector3 GetDelta()
    {
        stepDeltaVector = new Vector3(radius, pitch, yaw) - lastSphereCoords;
        return stepDeltaVector; 
    }

    public Vector3 GetLastSphereCoords()
    {
        return lastSphereCoords; // (r, p, w)
    }

    public Vector3 GetCurrentSphereCoords()
    {
        return new Vector3(radius, pitch, yaw); 
    }

    // spherical coords to rectangular
    public Vector3 GetRectFromSphere()
    {
        float x, y, z;

        x = radius * Mathf.Cos(pitch * Mathf.Deg2Rad) * Mathf.Cos(yaw * Mathf.Deg2Rad);
        y = radius * Mathf.Sin(pitch * Mathf.Deg2Rad);
        z = radius * Mathf.Cos(pitch * Mathf.Deg2Rad) * Mathf.Sin(yaw * Mathf.Deg2Rad);

        return new Vector3(x,y,z);
    }
}
