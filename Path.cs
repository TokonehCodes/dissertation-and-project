using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Path {
    public int ID;
    public List<string> platforms;
    public float totalWeight;
    
    public Path(int ID, List<string> platforms, float totalWeight){
        this.ID = ID;
        this.platforms = platforms;
        this.totalWeight = totalWeight;
    }
}
