using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;

public class Creature : MonoBehaviour {
    public GameObject node;
    public GameObject muscle;
    public List<GameObject> nodes = new List<GameObject>();
    public List<Muscle> muscles = new List<Muscle>();
    public CreatureData data;
    private Camera3D camera3D;
    private Vector3 startPos;
    private Vector3 center;
    private bool start = true;
    private Evo evo;
    private float fitness;
    private float lifetime;
    private bool remove = false;

    public int GetID() { return data.id; }

    public float GetFitness() { return fitness; }
    public void SetLifetime(float _lifetime) { lifetime = _lifetime; }
    public float GetLifetime() { return lifetime; }

    public Vector3 GetCenter() { return center; }

    public void Birth(CreatureData _data, Evo _evo) {
        data = _data;
        data.status = "ALIVE";
        lifetime = 0;
        evo = _evo;
        fitness = 0;
        CreateNodes(data.nodesPositions.Count);
        CreateMuscles(data.connections.Count);
    }

    public void SetCam(Camera3D cam) {
        camera3D = cam;
    }

    private void CreateNodes(int numNodes) {
        for (int i = 0; i < numNodes; i++) {
            GameObject nodeObj = (Instantiate(node) as GameObject);
            nodeObj.name = "N" + i;
            nodes.Add(GameObject.Find(nodeObj.name));
            nodes[i].transform.parent = transform;
            nodes[i].transform.localPosition = data.nodesPositions[i];
        }
    }
    
    private void CreateMuscles(int numMuscles) {
        for (int i = 0; i < numMuscles; i++) {
            string[] connectedNodes = data.connections[i].Split('_');
            int[] n = new int[2];
            int.TryParse(connectedNodes[0], out n[0]);
            int.TryParse(connectedNodes[1], out n[1]);
            Vector3 posNodeA = data.nodesPositions[n[0]];
            Vector3 posNodeB = data.nodesPositions[n[1]];
            Rigidbody rbA = nodes[n[0]].GetComponent<Rigidbody>();
            Rigidbody rbB = nodes[n[1]].GetComponent<Rigidbody>();
            Vector3 delta = posNodeA - posNodeB;
            Quaternion rotation = Quaternion.LookRotation(delta) * Quaternion.Euler(90, 0, 0);
            string name = "M" + n[0] + "_" + n[1];
            GameObject muscleObj = (Instantiate(muscle) as GameObject);
            muscleObj.name = name;
            muscles.Add(GameObject.Find(name).GetComponent<Muscle>());
            int idx = muscles.Count - 1;
            muscles[idx].ResizeMuscle(posNodeA, posNodeB);
            Vector3 newPos = transform.position + posNodeB + delta * 0.5f;
            muscles[idx].transform.position = newPos;
            muscles[idx].transform.rotation = rotation;
            muscles[idx].transform.parent = transform;
            FixedJoint fixA = muscles[idx].cylinderA.AddComponent<FixedJoint>();
            fixA.breakForce = Evo.breakForce;
            fixA.breakTorque = Evo.breakTorque;
            fixA.connectedBody = rbA;
            FixedJoint fixB = muscles[idx].cylinderB.AddComponent<FixedJoint>();
            fixB.breakForce = Evo.breakForce;
            fixB.breakTorque = Evo.breakTorque;
            fixB.connectedBody = rbB;
        }
    }

    public void Dead() {
        fitness = float.NaN;
        if (camera3D != null) {
            camera3D.statusTxt.text = data.status = "DEAD";
            camera3D.fitnessTxt.text = "Fitness: " + fitness;
        }
        foreach (Muscle m in muscles) { m.RemoveJoints(); }
        foreach (GameObject n in nodes) { n.GetComponent<Node>().SetDrag(); }
        evo.Die(data.id);
    }

    public void RemoveCreature () {
        remove = true;
        foreach (Muscle m in muscles) { Destroy(m); }
        foreach (GameObject n in nodes) { Destroy(n); }
    }

    private Vector3 Center() {
        float mostLeft = 0;
        float mostRight = 0;
        float mostDown = 0;
        float mostTop = 0;
        float mostFront = 0;
        float mostBack = 0;
        for (int i = 0; i < nodes.Count; i++){
            Vector3 pos = nodes[i].transform.position;
            if (pos.x < mostLeft) { mostLeft = pos.x; }
            if (pos.x >= mostRight) { mostRight = pos.x; }
            if (pos.y < mostDown) { mostDown = pos.y; }
            if (pos.y >= mostTop) { mostTop = pos.y; }
            if (pos.z < mostFront) { mostFront = pos.z; }
            if (pos.z >= mostBack) { mostBack = pos.z; }
        }
        float centerX = mostLeft + (mostRight - mostLeft) * .5f; 
        float centerY = mostDown + (mostTop- mostDown) * .5f;
        float centerZ = mostFront + (mostBack - mostFront) * .5f;
        return new Vector3(centerX, centerY, centerZ);
    }

    // Use this for initialization
    void Start() {
        name = data.id + "_" + data.category;
        center = Center();
    }

    private void Update() {
        if (camera3D != null) {
            camera3D.nameTxt.text = name;
            camera3D.lifetimeTxt.text = "Time: " + lifetime.ToString("#.00");
            camera3D.fitnessTxt.text = "Fitness: " + fitness.ToString("#.00");
            camera3D.statusTxt.text = data.status;
            camera3D.mutationTxt.text = "MutationRate: " + data.mutationRate.ToString("#.00");
            camera3D.generationTxt.text = "Generation: " + data.generation;
        }
    }

    private void FixedUpdate() {
        if (!remove) {
            lifetime += Time.fixedDeltaTime;
            if (data.status == "ALIVE" && lifetime > 5f) {
                if (start) {
                    startPos = center = Center();
                    start = false;
                }
                center = Center();
                for (int i = 0; i < muscles.Count; i++) {
                    float periode = data.musclesData[i][0];
                    float extractedTime = data.musclesData[i][1];
                    float startExtraction = data.musclesData[i][2];
                    float deltaTime = lifetime % periode;
                    if ((deltaTime > startExtraction && deltaTime < (startExtraction + extractedTime)
                       && muscles[i].extracted == false)
                       || ((deltaTime < startExtraction || deltaTime > (startExtraction + extractedTime))
                       && muscles[i].extracted == true)){ muscles[i].SetPulse(Evo.force); }
                }
                fitness = (center.x - startPos.x) / 10;
            }
        }
    }
}
