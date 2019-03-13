using UnityEngine;
using System;

/*
 * Definitions of the messages that are communicated across the network.
 */

/*
 * The different types of message (control information). 
 * This includes:
 *   - Add: register a new object on the server.
 *   - IdSet: return confirmation of a new object, with its identifier.
 *   - Update: share the location of an existing object
 */
public enum MessageStatus { Add, IdSet, Update };

/*
 * The different types of object that may exist:
 *   - Participant: User operating one of the clients.
 *   - Waypoint: Marker dropped by one of the users.
 *   - Defunct: Object related to a client that is not longer connected.
 */
public enum LocationType { Participant, Waypoint, Defunct };

/*
 * The message exchanged. For simplicity, only one type of 
 * message is communicated, with both control and data mixed
 * into a single class. The communication protocol is simple
 * enough that there is not too much wasted data.
 */
[Serializable]
public class LocationData
{
    // Control information.
    public MessageStatus status;
    // Type of object whose location is provided.
    public LocationType locType;
    // A unique identifier distinguishing this object from any other.
    public int identifier;
    // Position of the object.
    public float latitude;
    public float longitude;
}

