using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CreatureData {
    public int id;
    public List<Vector3> nodesPositions = new List<Vector3>();
    public List<string> connections = new List<string>();
    public List<string> possibleConnections = new List<string>();
    public List<float[]> musclesData = new List<float[]>();
    public int generation;
    public float fitness;
    public string category;
    public string status;
    public float mutationRate;
    
    public CreatureData (int _id) {
        id = _id;
        do {
            nodesPositions = CreateNodesData();
            possibleConnections = PossibleConnections(nodesPositions.Count);
            connections = CreateMuscles(nodesPositions, ref possibleConnections);
            musclesData = CreateMusclesData(connections.Count);
        } while (FindLonelyNodes(nodesPositions.Count, connections));
        AdjustNodes(ref nodesPositions);
        category = "N" + nodesPositions.Count + "M" + connections.Count;
        fitness = 0;
        status = "BORN";
        mutationRate = MutationRate();
    }

    public void Copy (CreatureData origin) {
        nodesPositions = new List<Vector3>(origin.nodesPositions);
        connections = new List<string>(origin.connections);
        possibleConnections = new List<string>(origin.possibleConnections);
        musclesData = new List<float[]>(origin.musclesData);
        mutationRate = origin.mutationRate;
    }

    public void Mutate () {
        List<Vector3> newNodesPositions;
        List<string> newConnections;
        List<string> newPossibleConnections;
        List<float[]> newMusclesData;
        do {
            newNodesPositions = new List<Vector3>(nodesPositions);
            newConnections = new List<string>(connections);
            newPossibleConnections = new List<string>(possibleConnections);
            newMusclesData = new List<float[]>(musclesData);

            //Mutate Node add Node
            if (WillMutate(mutationRate) && Evo.maxNodes > newNodesPositions.Count) {
                MutateAddNode(ref newNodesPositions, ref newPossibleConnections, ref newConnections,
                              ref newMusclesData);
            }
            //Mutate Node remove Node
            if (WillMutate(mutationRate) && Evo.minNodes < newNodesPositions.Count) {
                MutateRemoveNode(ref newNodesPositions, ref newConnections, 
                                 ref newPossibleConnections);
            }
 
            // Mutate the nodesPositions
            if (WillMutate(mutationRate)) {
                int idx = Random.Range(0, newNodesPositions.Count - 1);
                newNodesPositions[idx] = MutateNodePosition(newNodesPositions[idx]);
            }

            //Mutate Muscles add Muscle
            if (WillMutate(mutationRate)) {
                MutateAddMuscle(ref newConnections, ref newPossibleConnections, newNodesPositions,
                                ref newMusclesData);
            }
            //Mutate Muscles remove Muscle
            if (WillMutate(mutationRate) && (newConnections.Count > newNodesPositions.Count)) {
                Debug.Log("Mutate Remove Muscle");
                newConnections.RemoveAt(Random.Range(0, newConnections.Count - 1));
            }
        } while (FindLonelyNodes(newNodesPositions.Count, newConnections));
        
        //AdjustNodes
        AdjustNodes(ref newNodesPositions);

        //Mutate Muscles data
        for (int i = 0; i < newMusclesData.Count; i++) {
            if (WillMutate(mutationRate)) { newMusclesData[i] = CreateMuscleData(); }
        }

        // Mutate the mutationRate
        if (WillMutate(mutationRate)) { MutationRate(); }

        // Update the possible Connections after mutation of this creature
        possibleConnections = UpdatePossibleConnections(newConnections, newNodesPositions);

        //Set new calculated data to this Creature
        nodesPositions = newNodesPositions;
        connections = newConnections;
        musclesData = newMusclesData;
        category = "N" + nodesPositions.Count + "M" + connections.Count;
    }

    private bool WillMutate (float _mutationRate) {
        bool isMutate = false;
        if (_mutationRate > Random.Range(0f, 1f)) { isMutate = true; }
        return isMutate;
    }

    private void MutateAddNode(ref List<Vector3> _nodesPositions, 
                               ref List<string> _possibleConnections,
                               ref List<string> _connections,
                               ref List<float[]> _musclesData) {
        Debug.Log("Mutate Add Node");
        _nodesPositions.Add(FindNodePos(_nodesPositions));
        for (int i = 0; i < nodesPositions.Count - 2; i++) {
            _possibleConnections.Add(i + "_" + (_nodesPositions.Count - 1));
        }
        _connections.Add(_possibleConnections[_possibleConnections.Count - 1]);
        _musclesData.Add(CreateMuscleData());
        _possibleConnections.RemoveAt(_possibleConnections.Count - 1);
    }

    private void MutateRemoveNode(ref List<Vector3> _nodesPositions, ref List<string> _connections,
                                  ref List<string> _possibleConnections) {
        Debug.Log("Mutate Remove Node");
        int idx = Random.Range(1, _nodesPositions.Count - 1);
        _nodesPositions.RemoveAt(idx);
        List<string> tempConnections = new List<string>();
        for (int i = 0; i < _connections.Count; i++)
        {
            int[] c = GetConnectionIdx(_connections[i]);
            if (c[0] != idx && c[1] != idx)
            {
                if (c[0] > idx) { c[0]--; }
                if (c[1] > idx) { c[1]--; }
                tempConnections.Add(c[0] + "_" + c[1]);
            }
        }
        _connections = tempConnections;
        _possibleConnections = UpdatePossibleConnections(_connections, _nodesPositions);
    }

    private void MutateAddMuscle(ref List<string> _connections, 
        ref List<string> _possibleConnections, List<Vector3> _nodesPositions, 
        ref List<float[]> _musclesData) {
        string newMuscle = CreateMuscle(ref _possibleConnections, _nodesPositions);
        if (newMuscle != "None") {
            Debug.Log("Mutate Add Muscle: " + newMuscle);
            _connections.Add(newMuscle);
            _musclesData.Add(CreateMuscleData());
        }
    }

    private float MutationRate (){
        return Random.Range(Evo.minMutationRate, Evo.maxMutationRate);
    }

    private Vector3 MutateNodePosition (Vector3 pos) {
        if (WillMutate(mutationRate)) { pos.x = GetNewPos(pos.x); }
        if (WillMutate(mutationRate)) { pos.y = GetNewPos(pos.y); }
        if (WillMutate(mutationRate)) { pos.z = GetNewPos(pos.z); }
        return pos;
    }

    private float GetNewPos(float p) {
        float maxR = 10f * mutationRate;
        float newMin = Mathf.Max(p - maxR, -20f);
        float newMax = Mathf.Min(p + maxR, 20f);
        return Random.Range(newMin, newMax);
    }

    private List<Vector3> CreateNodesData () {
        int numNodes = Random.Range(Evo.minNodes, Evo.maxNodes);
        List<Vector3> list = new List<Vector3>();
        for (int i = 0; i < numNodes; i++) {
            Vector3 pos = FindNodePos(nodesPositions);
            list.Add(pos);
        }
        return list;
    }

    private void AdjustNodes (ref List<Vector3> _nodesPositions) {
        int lowestNode = 0;
        int mostLeftNode = 0;
        int mostRightNode = 0;
        for (int i = 0; i < _nodesPositions.Count; i++) {
            Vector3 pos = _nodesPositions[i];
            if (pos.y < _nodesPositions[lowestNode].y) { lowestNode = i; }
            if (pos.x < _nodesPositions[mostLeftNode].x) { mostLeftNode = i; }
            if (pos.x >= _nodesPositions[mostRightNode].x) { mostRightNode = i; }
        }
        float offsetX = ((20 - _nodesPositions[mostRightNode].x) 
                        - (_nodesPositions[mostLeftNode].x + 20)) / 2;
        float offsetY = -20 - (_nodesPositions[lowestNode].y);
        for (int i = 0; i < _nodesPositions.Count; i++) {
            Vector3 p = _nodesPositions[i];
            _nodesPositions[i] = new Vector3(p.x + offsetX, p.y + offsetY, p.z);
        }
    }

    private Vector3 FindNodePos(List<Vector3> nodesPositions) {
        Vector3 pos = new Vector3(Random.Range(-20, 20), Random.Range(-20, 20), Random.Range(-20, 20));
        bool noPos = true;
        if (nodesPositions.Count > 0) {
            while (noPos) {
                foreach (Vector3 p in nodesPositions) {
                    float distance = Vector3.Distance(pos, p);
                    if (distance >= 10) {
                        noPos = false;
                        break;
                    }
                    noPos = true;
                }
                if (noPos) {
                    pos = new Vector3(Random.Range(-20, 20), Random.Range(-20, 20), Random.Range(-20, 20));
                }
            }
        }
        return pos;
    }

    private List<string> PossibleConnections(int numNodes) {
        // returns all possible connections between existing nodes
        List<string> list = new List<string>();
        for (int i = 0; i < numNodes; i++) {
            for (int j = i + 1; j < numNodes; j++) {
                string combination = i + "_" + j;
                list.Add(combination);
            }
        }
        return list;
    }

    private List<string> UpdatePossibleConnections(List<string> _connections, 
                                                   List<Vector3> _nodesPositions) {
        List<string> list = PossibleConnections(_nodesPositions.Count);
        for (int i = 0; i < _connections.Count; i++) {
            for(int j = 0; j < list.Count; j++) {
                if (_connections[i] == list[j]) {
                    list.RemoveAt(j);
                    break;
                }
            }
        }
        return list;
    }

    private List<string> CreateMuscles(List<Vector3> _nodesPositions, 
                                       ref List<string> _possibleConnections) {
        int numMuscles = NumMuscles(_nodesPositions.Count);
        List<string> list = new List<string>();
        for (int i = 0; i < numMuscles; i++) {
            string newMuscle = CreateMuscle(ref possibleConnections, _nodesPositions);
            if (newMuscle != "None") { list.Add(newMuscle); }
        }
        return list;
    }

    private string CreateMuscle(ref List<string> _possibleConnections, List<Vector3> nPos) {
        int[] combination = ConnectNodes(ref _possibleConnections);
        Vector3 posNodeA = nPos[combination[0]];
        Vector3 posNodeB = nPos[combination[1]];
        float distance = Vector3.Distance(posNodeA, posNodeB);
        string ret = "None";
        if (distance >= 10f) { ret = combination[0] + "_" + combination[1]; }
        return ret;
    }

    private List<float[]> CreateMusclesData (int numMuscles) {
        List<float[]> list = new List<float[]>();
        for (int i = 0; i < numMuscles; i++) { list.Add(CreateMuscleData()); }
        return list;
    }

    private float[] CreateMuscleData() {
        float[] muscleData = new float[3];
        float periode = Random.Range(Evo.minPeriode, Evo.maxPeriode);
        float extractedTime = Random.Range(Evo.minExtractedTime, periode);
        float startExtraction = Random.Range(0, periode - extractedTime);
        muscleData[0] = periode;
        muscleData[1] = extractedTime;
        muscleData[2] = startExtraction;
        return muscleData;
    }

    private int NumMuscles(int numNodes) {
        // calculates the minimum number of muscles to connect all the nodes
        int maxMuscles = numNodes * (numNodes - 1) / 2;
        int numM = Random.Range(numNodes, maxMuscles);
        return numM;
    }

    private int[] ConnectNodes (ref List<string> _possibleConnections) {
        // finds a free connection for a new muscle
        int[] ret = new int[2];
        if (_possibleConnections.Count > 0) {
            int idx = Random.Range(0, _possibleConnections.Count - 1);
            string[] nodes = _possibleConnections[idx].Split('_');
            int.TryParse(nodes[0], out ret[0]);
            int.TryParse(nodes[1], out ret[1]);
            _possibleConnections.RemoveAt(idx);
        }
        else { Debug.Log("No connections left."); }
        return ret;
    }

    private bool FindLonelyNodes(int numNodes, List<string> _connections) {
        bool found = true;
        for (int i = 0; i < numNodes; i++) {
            found = true;
            foreach (string con in _connections) {
                int[] c = GetConnectionIdx(con);
                if (i == c[0] || i == c[1]) {
                    found = false;
                }
            }
            if (found){
                break;
            }
        }
        return found;
    }
    
    private int[] GetConnectionIdx(string connection) {
        int[] c = new int[2];
        string[] connectedNodes = connection.Split('_');
        int.TryParse(connectedNodes[0], out c[0]);
        int.TryParse(connectedNodes[1], out c[1]);
        return c;
    }
}
