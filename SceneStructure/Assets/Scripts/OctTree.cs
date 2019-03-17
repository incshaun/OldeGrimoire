using UnityEngine;
using System.Collections.Generic;
using GoogleARCore;

class OctTree
{
  // The threshold level of leaf division. Ensure that the tree
  // depth does not grow completely out of control.
  protected float OctTreeLeafSize = 0.01f;
  
  // The maximum number of elements stored per
  // Oct Tree node. 
  protected int maxPointPerVoxel = 5;
  
  // The object used to represent a single voxel.
  protected GameObject nodeShape;
  
  // The data stored in a node in an Oct Tree.
  protected class OctTreeElement
  {
    // The coordinates of the feature point.
    public PointCloudPoint point;
    // The colour of the feature point.
    public Color colour;
    // A handy reference to the Oct Tree node
    // containing this element.
    public OctTreeNode containedIn;
    
    public OctTreeElement (PointCloudPoint p, Color c)
    {
      point = p;
      colour = c;
      containedIn = null;
    }
  }
  
  // An Oct Tree consists of a link to root node.
  // Each node contains up to 8 links to child nodes
  // representing subsets of the space belonging
  // to the parent node.
  protected class OctTreeNode
  {
    // The coordinates of the space represented by
    // the node.
    public Vector3 minCorner;
    public Vector3 maxCorner;
    
    // A reference to this node's parent node.
    public OctTreeNode parent;
    // References to the 8 child nodes.
    public OctTreeNode [] children = new OctTreeNode [8];
    
    // The data stored in this node.
    public List <OctTreeElement> containedEntities;
    
    // number of non-null children.
    public int leafcount; 
    
    // The shape of this node.
    public GameObject voxel;
    
    // Convert a set of left/right, above/below, front/back flags
    // into the number of the child element (from 0 to 7).
    public static int encode (bool x, bool y, bool z)
    {
      int r = 0;
      if (x) { r += 4; }
      if (y) { r += 2; }
      if (z) { r += 1; }
      return r;
    }
        
    public OctTreeNode(Vector3 minc, Vector3 maxc, OctTreeNode parnt) {
      minCorner = minc;
      maxCorner = maxc;
      parent = parnt;
      
      for (int i = 0; i < 8; i++)
      {
        children[i] = null;
      }
      
      leafcount = 0;
      containedEntities = new List <OctTreeElement> ();
      voxel = null;
    }
  }
  
  // The Oct Tree - reference to the root node.
  protected OctTreeNode octtreeroot;
  
  public OctTree (GameObject n) 
  {
    nodeShape = n;
    octtreeroot = null;
  }
  
  // Add a new feature point to the Oct Tree. 
  public void addPoint (PointCloudPoint point, Color c)
  {
    placeNodeInTree (new OctTreeElement (point, c));
  }

  // Take an element out of the Oct Tree.
  private void removeFromContainment (OctTreeElement o)
  {
    // remove o from current containment.
    if (o.containedIn != null)
    {
      if (!o.containedIn.containedEntities.Remove (o))
      {
        Debug.Log ("Oct Tree Element not found in container");
      }
    }
  }
  
  // Move an element to a destination Oct Tree node, 
  // taking care of housekeeping
  private void changeContainment (OctTreeElement o, OctTreeNode dest)
  {
    removeFromContainment (o);
    
    // add to dest.
    o.containedIn = dest;
    
    if (dest != null)
    {
      while (o.containedIn.containedEntities.Count > maxPointPerVoxel)
      {
        o.containedIn.containedEntities.RemoveAt (0);
      }
      
      // update list
      o.containedIn.containedEntities.Add (o);
    }
  }
  
  // Place an element in the tree. This tree has the ability to grow
  // to encompass all points added, even if it needs to add new root
  // nodes.
  protected void placeNodeInTree (OctTreeElement o)
  {
    if (o.containedIn == null)
    {
      // not in the tree. Place it in the root, then shuffle.
      if (octtreeroot == null)
      {
        float size = OctTreeLeafSize;
        // no tree yet either.
        octtreeroot = new OctTreeNode (new Vector3 (o.point.Position.x - size / 2.0f, o.point.Position.y - size / 2.0f, o.point.Position.z - size / 2.0f), new Vector3 (o.point.Position.x + size / 2.0f, o.point.Position.y + size / 2.0f, o.point.Position.z + size / 2.0f), null);
      }
      
      changeContainment (o, octtreeroot);	
    }
    
    // Check if node fits.
    // move upwards if does not fit.
    while ((o.point.Position.x <= o.containedIn.minCorner.x) ||
      (o.point.Position.y <= o.containedIn.minCorner.y) ||
      (o.point.Position.z <= o.containedIn.minCorner.z) ||
      (o.point.Position.x > o.containedIn.maxCorner.x) ||
      (o.point.Position.y > o.containedIn.maxCorner.y) ||
      (o.point.Position.z > o.containedIn.maxCorner.z))
    {
      // does not fit - move up.
      if (o.containedIn.parent == null)
      {
        // need to grow the tree upwards.
        // make sure we expand in a direction that works.
        bool crossx = o.point.Position.x <= o.containedIn.minCorner.x;
        bool crossy = o.point.Position.y <= o.containedIn.minCorner.y;
        bool crossz = o.point.Position.z <= o.containedIn.minCorner.z;
        
        float size = (o.containedIn.maxCorner.x - o.containedIn.minCorner.x);
        Vector3 pmin = new Vector3 (crossx ? o.containedIn.minCorner.x - size :
        o.containedIn.minCorner.x,
        crossy ? o.containedIn.minCorner.y - size : o.containedIn.minCorner.y,
        crossz ? o.containedIn.minCorner.z - size : o.containedIn.minCorner.z);
        Vector3 pmax = new Vector3 (crossx ? o.containedIn.maxCorner.x :
        o.containedIn.maxCorner.x + size,
        crossy ? o.containedIn.maxCorner.y : o.containedIn.maxCorner.y + size,
        crossz ? o.containedIn.maxCorner.z : o.containedIn.maxCorner.z + size); 
        OctTreeNode newoct = new OctTreeNode (pmin, pmax, null);
        newoct.children[OctTreeNode.encode (crossx, crossy, crossz)] = octtreeroot;
        newoct.leafcount++;
        octtreeroot.parent = newoct;
        octtreeroot = newoct;
      }
      
      changeContainment (o, o.containedIn.parent);
      
      // prune empty leaves.
      for (int i = 0; i < 8; i++)
      {
        if (o.containedIn.children[i] != null && o.containedIn.children[i].leafcount == 0 && o.containedIn.children[i].containedEntities.Count == 0)
        {
          o.containedIn.children[i] = null;
          o.containedIn.leafcount--;
        }
      }
    }
    
    // now move downwards if possible.
    while (o.containedIn.maxCorner.x - o.containedIn.minCorner.x > OctTreeLeafSize)
    {
      if ((o.containedIn.leafcount == 0) && (o.containedIn.containedEntities.Count <= 1))
        break;
      
      Vector3 mid = 0.5f * (o.containedIn.maxCorner + o.containedIn.minCorner);
      
      bool crossx = o.point.Position.x <= mid.x;
      bool crossy = o.point.Position.y <= mid.y;
      bool crossz = o.point.Position.z <= mid.z;
      int cindex = OctTreeNode.encode (crossx, crossy, crossz);
      Vector3 cmin = new Vector3 (crossx ? o.containedIn.minCorner.x : mid.x,
                                  crossy ? o.containedIn.minCorner.y : mid.y,
                                  crossz ? o.containedIn.minCorner.z : mid.z);
      Vector3 cmax = new Vector3 (crossx ? mid.x : o.containedIn.maxCorner.x,
                                  crossy ? mid.y : o.containedIn.maxCorner.y,
                                  crossz ? mid.z : o.containedIn.maxCorner.z); 
      if (o.containedIn.children[cindex] == null)
      {
        o.containedIn.children[cindex] = new OctTreeNode (cmin, cmax, o.containedIn);
        o.containedIn.leafcount++;
      }
      changeContainment (o, o.containedIn.children[cindex]);
    }
    
    // Shrink tree from top if possible
    while (octtreeroot != null && octtreeroot.leafcount <= 1 && octtreeroot.containedEntities.Count == 0)
    {
      for (int i = 0; i < 8; i++)
      {
        // no entities at this level, and at most 1 child occupied.
        if (octtreeroot.children[i] != null)
        {
          octtreeroot = octtreeroot.children[i];
          break;
        }
      }
    }
  }

  // Update the scene representation of the Oct Tree.
  public void renderOctTree (GameObject parent)
  {
    // Start at the root, and recurse.
    renderOctTreeNode (octtreeroot, parent);
  }
  
  // Ensure that a node in the tree has a representation
  // in the scene. Only leaf nodes are shown. 
  protected void renderOctTreeNode(OctTreeNode root, GameObject parent) 
  {
    if (root != null)
    {
      if (root.leafcount == 0)
      {
        if (root.voxel == null)
        {
          root.voxel = UnityEngine.Object.Instantiate (nodeShape);
        }
        
        Color c = new Color (0, 0, 0);
        foreach (OctTreeElement e in root.containedEntities)
        {
          c += e.colour;
        }
        c = (1.0f / root.containedEntities.Count) * c;
        root.voxel.transform.position = 0.5f * (root.minCorner + root.maxCorner);
        root.voxel.transform.localScale = root.maxCorner - root.minCorner;
        root.voxel.transform.SetParent (parent.transform);
        root.voxel.GetComponent <MeshRenderer> ().material.color = c;
      }
      else
      {
        if (root.voxel != null)
        {
          GameObject.Destroy (root.voxel);
          root.voxel = null;
        }
        for (int i = 0; i < 8; i++)
        {
          renderOctTreeNode (root.children[i], parent);
        }
      }
    }
  }
}
