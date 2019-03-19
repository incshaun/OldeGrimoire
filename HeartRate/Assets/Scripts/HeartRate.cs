using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HeartRate : MonoBehaviour {
  
  [Tooltip ("An object to act as parent for the spectrum chart")]
  public GameObject spectograph;
  [Tooltip ("A template object for a bar in the spectrum chart")]
  public GameObject barTemplate;
  
  [Tooltip ("A text object where data is output")]
  public Text message;

  // The number of sensor readings to use for analysis. Larger
  // numbers allow smaller frequency intervals but slow down
  // processing and response times.
  private int maxReadings = 256;
  
  // A list of the last maxReadings sensor values.
  private List <double> accelerometerReadings = new List <double> ();
  
  // The width of the spectrum chart.
  private float spectroScale = 4.0f;
  // An offset, to center, the spectrum chart.
  private float spectroOffset = -2.0f;
  // Number of adjacent bins in the spectrum that are collapsed in
  // a single bar. Speeds up display of the chart.
  private int spectroStep = 4;
  
  // An smoothed value for the peak measured at each frame.
  private float smoothedPeak = 0.0f;
  // The proportion of the smoothed value retained on each frame.
  private float smoothingFactor = 0.98f;

  // When searching for peak amplitudes, restrict to
  // frequencies in this range. These ones are tuned
  // to likely heart rate values (in beats per second).
  private float minFreq = 0.50f;
  private float maxFreq = 3.20f;
  
  // Use a fixed update to get a regular sample of the sensor.
  void FixedUpdate () {
    
    // Sample the sensor. 
    Vector3 acc = Input.acceleration;
    accelerometerReadings.Add (acc.magnitude);
    
    // Add new readings to the list, and discard excess at the start.
    while (accelerometerReadings.Count > maxReadings)
    {
      accelerometerReadings.RemoveAt (0);
    }
  
    if (accelerometerReadings.Count == maxReadings)
    {
      // Prepare for the fast fourier transform. Convert all
      // real sensor readings to complex numbers.
      double [] readings = accelerometerReadings.ToArray ();
      double [] readingsComplex = new double [maxReadings];
      for (int i = 0; i < maxReadings; i++)
      {
        readingsComplex[i] = 0.0f;
      }      
      int m = (int) (Mathf.Log (maxReadings) / Mathf.Log (2.0f));
      // Transform sensor data into frequency values.
      FFTLibrary.Complex.FFT (1, m, readings, readingsComplex);
      
      // The sensor sample rate.
      float baseFreq = 1.0f / Time.deltaTime;
      // The frequency range occupied by each bin.
      float binFreq = baseFreq / maxReadings;
      
      // Clear the spectrum chart.
      if (spectograph != null)
      {
        foreach (Transform child in spectograph.transform)
        {
          GameObject.Destroy(child.gameObject);
        }
      }
      
      // Find the peak amplitude.
      float maxLevel = 0.0f;
      bool maxFound = false;
      float peakFreq = 0.0f;
      
      // Element 0: DC (constant) level.
      for (int i = 1; i < maxReadings / 2; i += spectroStep)
      {
        float total = 0.0f;
        for (int j = 0; j < spectroStep; j++)
        {
          int index = i + j;
          // Get magnitude of complex number (squared).
          float v = 1000.0f * (float) (readings[index] * readings[index] + readingsComplex[index] * readingsComplex[index]);
          float freq = index * binFreq;
          if ((freq >= minFreq) && (freq <= maxFreq))
          {
            if ((!maxFound) || (maxLevel < v))
            {
              maxLevel = v;
              maxFound = true;
              peakFreq = freq;
            }
          }
          
          total += v;
        }
  
        // Create one bar every spectroStep bins.
        GameObject g = GameObject.Instantiate(barTemplate);
        g.transform.position = new Vector3(spectroScale * (float) i / (float) (maxReadings / 2) + spectroOffset, 0, 0);
        g.transform.localScale = new Vector3(spectroScale / (float) (maxReadings / (2 * spectroStep)), 0.1f + total, 1.0f / (float) (maxReadings / (2 * spectroStep)));
        g.transform.SetParent(spectograph.transform);
      }
      
      // Smooth peak frequency so not too jumpy.
      smoothedPeak = smoothingFactor * smoothedPeak + (1.0f - smoothingFactor) * peakFreq;
      
      if (message != null)
      {
        message.text = "Base freq: " + baseFreq + "Hz" + "\n" +
                       "Bin freq: " + binFreq + "Hz" + "\n" + 
                       "Peak freq: " + smoothedPeak + "Hz" + " = " + (smoothedPeak * 60) + "bpm";
      }
    }
    else
    {
      if (message != null)
      {
        message.text = "Starting: " + (100.0f * accelerometerReadings.Count / maxReadings) + "%";
      }
    }
  }
}
