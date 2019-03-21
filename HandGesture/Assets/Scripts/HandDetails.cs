using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;
using Leap.Unity;
using Leap;

[Serializable]
public class HandDetails {
  
  public string id;
  public Hand hand;
  
  public HandDetails (HandModelBase handBase)
  {
    id = getID ();
    hand = handBase.GetLeapHand ();
  }
  
  public byte [] serialize ()
  {
    BinaryFormatter bf = new BinaryFormatter ();
    MemoryStream ms = new MemoryStream ();
    bf.Serialize (ms, this);
    return ms.ToArray ();
  }

  public static HandDetails deserialize (byte [] b)
  {
    BinaryFormatter formatter = new BinaryFormatter();
    HandDetails cd = (HandDetails) formatter.Deserialize (new MemoryStream (b));
    return cd;
  }
  
  public static string getID ()
  {
    return SystemInfo.deviceUniqueIdentifier;
  }
}
