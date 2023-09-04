using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Text;
using System.Threading.Tasks;
using System.Linq;
using UnityEngine.SceneManagement;

public class Adjecency : MonoBehaviour
{
    public GameObject plat;
    public float area = 8.0f;
    [SerializeField] float timer = 2f;
   
    private Vector3 mousePos;
    private Vector3 objectPos;

    public static Dictionary<string, Vector2> state;
    Dictionary<string, Vector2> oldState;
    public Dictionary<string, Dictionary<string, float>> graph;
    List<string> nodes;
    int count;
    int pathCount;
    Path firstPath;
    List<Path> allPaths;
    string currentPlat;
    string finalPlat;

    int platMask; 


    // Start is called before the first frame update
    void Start(){
        count = 0;
        state = new Dictionary<string, Vector2>();
        oldState = new Dictionary<string, Vector2>();
        graph = new Dictionary<string, Dictionary<string, float>>();

        pathCount = 0;
        allPaths = new List<Path>();
        platMask = LayerMask.GetMask("Platform");

        foreach (GameObject p in GameObject.FindGameObjectsWithTag("Platform")){
            p.name = count.ToString();
            oldState.Add(p.name, p.transform.position);
            state.Add(p.name, p.transform.position);
            graph.Add(p.name, new Dictionary<string, float>());
            count++;
        }  
        getNeighbours(GameObject.FindGameObjectsWithTag("Platform"));
        nodes = new List<string>(state.Keys); 
    }

    // Update is called once per frame
    void Update(){
        RaycastHit2D hit = Physics2D.Raycast(GameObject.Find("Circle").transform.position, Vector2.down);
        if(hit){
            currentPlat = hit.transform.gameObject.name;
        }

        timer -= Time.deltaTime;
        if(timer <= 0){
            detectMovement();      
            timer = 2.0f;
        }

        if (Input.GetMouseButtonDown(0)){
            allPaths.Clear();
            mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            RaycastHit2D newHit = Physics2D.Raycast(mousePos, Vector2.down);
            if (newHit){
                finalPlat = newHit.transform.gameObject.name;

                firstPath = new Path(0, new List<string>(), 0f);
                pathExists(currentPlat, finalPlat, ref firstPath);
                allPaths.OrderByDescending(x => x.totalWeight).ToList();
                var content = (string.Join(" -> ", allPaths[0].platforms));
                Debug.Log(content + " with cost " + allPaths[0].totalWeight);
                GameObject.Find("Circle").GetComponent<AI>().GetPath(allPaths[0]);
            }
        }

        if (Input.GetMouseButtonDown(1)){
            newPlat();
        }
    }

    void getNeighbours(GameObject[] plats){
        foreach (GameObject p in plats){
            if (!graph.ContainsKey(p.name)) {
                graph.Add(p.name, new Dictionary<string, float>());
            }
            graph[p.name].Clear();

            foreach (Collider2D ir in Physics2D.OverlapAreaAll(p.transform.GetChild(0).position, p.transform.GetChild(1).position, platMask)){
                Transform target = ir.transform;
                Vector2 dirToTarget = (target.position - p.transform.position).normalized;
                float distFromTarget = Vector2.Distance(p.transform.position, target.position);

                RaycastHit2D hit = Physics2D.Raycast(p.transform.position, dirToTarget, distFromTarget);
                
                if (hit){
                    if (!graph[p.name].ContainsKey(hit.transform.gameObject.name)){
                        graph[p.name].Add(hit.transform.gameObject.name, getCost(p.transform.position, hit.transform.position));
                    }
                }
            }
        }              
    }

    void pathExists(string start, string end, ref Path path){
        if (start == end) {
            var ls = new List<string>();
            foreach (var x in path.platforms) ls.Add(x);
            Path newPath = new Path(pathCount, ls, path.totalWeight);
            newPath.platforms.Add(start);
            newPath.ID = allPaths.Count;
            allPaths.Add(newPath);
            return;
        }

        if (!path.platforms.Contains(start))path.platforms.Add(start);

        if(graph[start].ContainsKey(end)){
            var ls = new List<string>();
                foreach (var x in path.platforms) ls.Add(x);
                ls.Add(end);
                Path newPath = new Path(pathCount, ls, path.totalWeight);
                newPath.totalWeight += graph[start][end];
                newPath.ID = allPaths.Count;
                allPaths.Add(newPath);
                return;
        }

        if(graph[start].Count > 0){
            foreach(string vert in graph[start].Keys){
                if(!path.platforms.Contains(vert)){
                    var ls = new List<string>();
                    foreach (var x in path.platforms) ls.Add(x);
                    Path newPath = new Path(pathCount, ls,path.totalWeight);
                    newPath.totalWeight += graph[start][vert];
                    pathExists(vert, end,ref newPath);
                }
            }
        }
    }

    void updateGraph(Transform p){
        Collider2D[] radius;
        radius = Physics2D.OverlapCircleAll(p.position, area);
        List<GameObject> find = new List<GameObject>();
        foreach (Collider2D near in radius){
            if(near.tag == "Platform"){
                find.Add(near.gameObject);
            }
            getNeighbours(find.ToArray());
        }
    }

    void detectMovement(){
        List<Transform> moveList = new List<Transform>();

        foreach (string name in nodes){
            oldState[name] = state[name];
            state[name] = (Vector2)GameObject.Find(name).transform.position;
            if(!(oldState[name] == state[name])){
                moveList.Add(GameObject.Find(name).transform);
                Debug.Log("platform " + name + " has moved");

                foreach (Transform p in moveList){
                    updateGraph(p);
                }

                recalculatePath();
            }
            
        }
    }

    void recalculatePath(){
        RaycastHit2D hit = Physics2D.Raycast(GameObject.Find("Circle").transform.position, Vector2.down);
        if(hit){
            currentPlat = hit.transform.gameObject.name;
        }

        if(currentPlat == finalPlat){
            return;
        }

        if(finalPlat!=null){
            allPaths.Clear();
            pathExists(currentPlat, finalPlat, ref firstPath);
            allPaths.OrderByDescending(x => x.totalWeight).ToList();
            var content = (string.Join(" -> ", allPaths[0].platforms));
            Debug.Log(content + " with cost " + allPaths[0].totalWeight);
            GameObject.Find("Circle").GetComponent<AI>().GetPath(allPaths[0]);
        }
    }

    float getCost(Vector2 first, Vector2 second){
        return((Math.Abs(first.x-second.x)) + (second.y-first.y));
    }

    void newPlat(){
        mousePos = Input.mousePosition;
        mousePos.z = 2.0f;
        objectPos = Camera.main.ScreenToWorldPoint(mousePos);
        GameObject unique = Instantiate(plat, objectPos, Quaternion.identity);
        unique.name = count.ToString();

        unique.transform.position = objectPos;
        state.Add(unique.name, unique.transform.position);
        graph.Add(unique.name, new Dictionary<string, float>());
        updateGraph(unique.transform);
        count++;
    }


}
