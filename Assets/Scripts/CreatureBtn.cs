using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CreatureBtn : MonoBehaviour {
    public Evo evo;
    public GameObject panel;
    public GameObject node;
    public LineRenderer muscle;
    public Text statusTxt;
    private List<GameObject> nodes = new List<GameObject>();
    private List<LineRenderer> muscles = new List<LineRenderer>();
    private CreatureData data;
    private Button btn;

	// Use this for initialization
	void Start () {
        evo = GameObject.Find("Evo").GetComponent<Evo>();
        btn = GetComponent<Button>();
    }

    public void MakeThumb(CreatureData _data) {
        data = _data;
        MakeNodes();
        MakeMuscles();
        Alive();
    }

    public int GetCreatureID() {
        if (data != null) {
            return data.id;
        } else { return -1; }
    }

    private void MakeNodes() {
        foreach (GameObject n in nodes) { 
            Destroy(n);
        }
        nodes = new List<GameObject>();
        for (int i = 0; i < data.nodesPositions.Count; i++) {
            GameObject newNode = (Instantiate(node) as GameObject);
            newNode.transform.SetParent(panel.transform, false);
            newNode.transform.localPosition = data.nodesPositions[i];
            nodes.Add(newNode);
        }
    }

    private void MakeMuscles() {   
        foreach (LineRenderer lr in muscles) {
            Destroy(lr);
        }
        muscles = new List<LineRenderer>();
        for (int i = 0; i < data.connections.Count; i++) {
            string[] connectedNodes = data.connections[i].Split('_');
            int[] n = new int[2];
            int.TryParse(connectedNodes[0], out n[0]);
            int.TryParse(connectedNodes[1], out n[1]);
            Vector3 posNodeA = data.nodesPositions[n[0]];
            Vector3 posNodeB = data.nodesPositions[n[1]];
            LineRenderer newMuscle = (Instantiate(muscle) as LineRenderer);
            newMuscle.transform.SetParent(panel.transform, false);
            newMuscle.SetPosition(0, posNodeA);
            newMuscle.SetPosition(1, posNodeB);
            muscles.Add(newMuscle);
        }
    }

    public void Click() {
        if (data != null) {
            evo.Stop();
            evo.AnimateCreature(data.id);
        }
    }

    public void SetActive(bool state){
        btn.interactable = state;
    }

    public void Alive() {
        SetColor(255, 255, 255, 0.5f);
        SetActive(true);
        statusTxt.text = "";
    }

    public void Selected() {
        SetColor(248, 173, 13, .5f);
    }

    public void Dead() {
        SetColor(0, 0, 0, 0.5f);
        SetActive(false);
        statusTxt.text = "DEAD";
    }

    public void SetColor(int r, int g, int b, float a) {
        panel.GetComponent<Image>().color = Evo.ConvertColor(r, g, b, a);
    }

	// Update is called once per frame
	void Update () {
		
	}
}
