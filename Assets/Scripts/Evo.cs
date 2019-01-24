using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Evo : MonoBehaviour {
    public static int minNodes = 4;                 //Minimum of nodes of an creature.
    public static int maxNodes = 8;                 //Maximum of nodes of an crature.
    public static float breakForce = 6000f;         //Force needed to break fixjoint between node and muscle.
    public static float breakTorque = 6000f;        //Torque needed to break fixjoint between node and muscle.
    public static float force = 2000;               //Force with that an muscle extractes.
    public static float minPeriode = 1f;            //Minimum time in sec a muscle extracts.
    public static float maxPeriode = 3f;            //Maximum time in sec a muscle extracts.
    public static float minExtractedTime = 1f;      //Minimum time a muscle is extracted.
    public static float minMutationRate = .01f;     //Minimum Mutation rate.
    public static float maxMutationRate = .5f;      //Maximum Mutation rate.
    public Camera camera3D;                         //3DCam to show the anmated creature.
    public GameObject creatureObj;                  //Prefab of the creature.
    public GameObject panel;                        //Panel to add the creature thumbs. (btns)
    public Button creatureBtn;                      //A Button instance to add to the panel.
    public Button createBtn;                        //Interface to create an generation.
    public Button goBtn;                            //Starts the animations/calculations for the current generation.
    public Button continuousBtn;                    //Full automated mode.
    public Text genTxt;                             //Text to show the user the current generation.
    private List<CreatureBtn> creatureBtns = new List<CreatureBtn>(); //List of the creature buttons.
    private int generation;                         //Current generation.
    private int population;                         //Count of an population.
    private Creature creature;                      //Prefab of the creature.
    private List<CreatureData> creatureDB = new List<CreatureData>();   //List of data for all created creatures.
    private int currentIdx= -1;                     //currentIdxxof creature for animation.
    private int startIdx = -1;                      //First creature for animation of an generation.
    private CreatureData currentData = null;        //Current creature data for animation.
    private bool continuous = false;                //Continuous mode.
    private bool stop = false;                      //Stop continuous mode.
    private float timescale = 1;                    //Timescale.                        
    private float maxLifeTime = 30;                 //Amount of time for one creature to get its fitness.
    private bool remove;                            //Creature will removed.
    private Camera3D cam3D;                         //Script of the 3DCamera.

    void Awake() {
        Debug.Log("Evo Awake called.");
    }

    // Use this for initialization
    void Start () {
        int btnHeight = (int)creatureBtn.GetComponent<RectTransform>().rect.height;
        int width = (int)panel.GetComponent<RectTransform>().rect.width;
        int height = (int)panel.GetComponent<RectTransform>().rect.height;
        int columns = height / btnHeight;
        population = (width / btnHeight) * columns;
        int rows = population / columns;
        for (int i = 0; i < population; i++) {
            Button newCreatureBtn = Instantiate(creatureBtn) as Button;
            creatureBtns.Add(newCreatureBtn.GetComponent<CreatureBtn>());
            newCreatureBtn.transform.SetParent(panel.transform, false);
            int x = ((i - (rows * (i / rows))) * btnHeight) - width;
            int y = -i / rows * btnHeight;
            Vector3 pos = new Vector3(x, y, -btnHeight);
            newCreatureBtn.transform.localPosition = pos;
        }
        cam3D = camera3D.GetComponent<Camera3D>();
        camera3D.enabled = false;
        Physics.gravity = new Vector3(0, -30, 0);
    }

    public void CreateClick() {
        //Test();
        MakeGeneration();
    }

    public void MakeGeneration () {
        Debug.Log("MakeGeneration");
        if (creature != null) { RemoveCreature(creature.data.id); }
        generation = creatureDB.Count / population + 1;
        genTxt.text = "Gen: " + generation;
        int idx = GetStartIndex() - population;
        for (int i = 0; i < population; i++) {
            CreatureData data;
            if ((generation > 1)) {
                if (i < (population / 2)) {
                    data = CloneCreature(creatureDB[idx], false);
                } else {
                    data = CloneCreature(creatureDB[idx - population / 2], true);
                }
            } else {
                data = new CreatureData(creatureDB.Count) { generation = this.generation };
            }
            creatureDB.Add(data);
            creatureBtns[i].MakeThumb(data);
            idx++;
        }
        createBtn.interactable = false;
        goBtn.interactable = true;
        continuousBtn.interactable = true;
    }

    public void Test() {
        generation++;
        genTxt.text = "Gen: " + generation;
        if (creature != null) { RemoveCreature(creature.data.id); }
        if ((generation > 1)) {
            creatureDB[0] = CloneCreature(creatureDB[0], true);
        } else {
            creatureDB.Add(new CreatureData(creatureDB.Count) { generation = this.generation });
        }
        creatureBtns[0].MakeThumb(creatureDB[0]);
        goBtn.interactable = true;
        continuousBtn.interactable = true;
    }

    public void Stop() { stop = true; }

    public void Continuous() {
        continuous = !continuous;
        if (continuous) {
            Go();
        } else {
            stop = true;
        }
    }

    public void Go() {
        if (creatureDB.Count > 0) {
            if (creature != null) { RemoveCreature(creature.data.id); }
            if (currentIdx < 0) { currentIdx = startIdx = creatureDB.Count - population; }
            if (currentIdx < 0) { currentIdx = startIdx = 0; }
            currentData = null;
            createBtn.interactable = false;
            goBtn.interactable = false;
            stop = false;
        }
    }

    private CreatureData CloneCreature(CreatureData origin, bool mutate) {
        CreatureData clone = new CreatureData(creatureDB.Count) { generation = generation };
        clone.Copy(origin);
        if (mutate) {
            clone.Mutate();
        }
        return clone;
    }

    public void SetTimeScale(float _timeScale) { Time.timeScale = timescale = _timeScale; }

    public void AnimateCreature (int id) {
        Debug.Log("AnimateCreature");
        Time.timeScale = 0;
        int idx = GetCreatureDBIndexByID(id);
        if (idx < creatureDB.Count) {
            if (creature != null) { RemoveCreature(creature.data.id); }
            GameObject newCreatureObj = Instantiate(creatureObj) as GameObject;
            newCreatureObj.name = "Creature";
            newCreatureObj.transform.position = new Vector3(0f, 21f, 0f);
            creature = GameObject.Find("Creature").GetComponent<Creature>();
            creature.Birth(creatureDB[idx], this);
            SetCamera(creature, true);
            creatureBtns[GetCreatureBtn(id)].Selected();
        }
        else { Debug.Log("No CreatureData with idx: " + idx + "for id: " + id + " in DB found!"); }
        Time.timeScale = timescale;
    }

    public void Die(int id) {
        Debug.Log("Die");
        creatureBtns[GetCreatureBtn(id)].Dead();
        creature.SetLifetime(maxLifeTime - 5);
    }

    private int GetStartIndex() { return (generation - 1) * population; }

    private void SortCreatures() {
        creatureDB.Sort(delegate (CreatureData c1, CreatureData c2) {
                int comparedGeneration = c1.generation.CompareTo(c2.generation);
                if (comparedGeneration == 0) {
                    return c2.fitness.CompareTo(c1.fitness);
                }
                return comparedGeneration;
        });
        int idx = GetStartIndex();
        for (int i = 0; i < population; i++) {
            creatureBtns[i].MakeThumb(creatureDB[idx]);
            if (creatureDB[idx].status == "DEAD") { creatureBtns[i].Dead(); }
            idx++;
        }
    }

    private void KillBadCreatures() {
        int idx = GetStartIndex();
        for (int i = idx + population / 2; i < (idx + population); i++) {
            creatureDB[i].status = "DEAD";
            creatureDB[i].fitness = float.NaN;
            Die(creatureDB[i].id);
        }
    }

    private int GetCreatureDBIndexByID(int id) {
        int ret = -1;
        for (int i = 0; i < creatureDB.Count; i++) {
            if(creatureDB[i].id == id) {
                ret = i;
                break;
            }
        }
        return ret;
    }

    private int GetCreatureBtn(int id) {
        int ret = 0;
        for (int i = 0; i < creatureBtns.Count; i++) {
            int bcID = creatureBtns[i].GetCreatureID();
            if (bcID == id) {
                ret = i;
                break;
            }
        }
        return ret;
    }

    private void RemoveCreature(int id) {
        remove = true;
        Debug.Log("RemoveCreature");
        Time.timeScale = 0;
        SetCamera(null, false);
        int idx = GetCreatureDBIndexByID(id);
        if (creature != null) {
            creature.RemoveCreature();
            if (idx != -1 && creature != null) {
                if (creatureDB[idx].status != "DEAD") {
                    creatureBtns[GetCreatureBtn(creatureDB[idx].id)].Alive();
                }
                DestroyImmediate(creature.gameObject);
            }
        }
        while (creature != null);
        remove = false;
        Time.timeScale = timescale;
    }

    private void SetCamera(Creature creature = null, bool enable = false) {
        camera3D.enabled = enable;
        if (creature != null) {
            cam3D.target = creature;
            creature.SetCam(cam3D);
        }
    }

    void FixedUpdate() {
        if (Input.GetKey("escape")) { Quit(); }
        if (currentIdx!= -1 && remove == false) {
            if (currentData == null && stop == false) {
                currentData = creatureDB[currentIdx];
                if (currentData.status == "DEAD") {
                    currentIdx++;
                    currentData = null;
                } else { AnimateCreature(currentData.id); }
            }
            if (stop && currentData != null) {
                currentData = null;
                RemoveCreature(creatureDB[currentIdx].id);
                goBtn.interactable = true; 
            } else if (currentData != null && creature.GetLifetime() >= maxLifeTime) {
                creatureDB[currentIdx].fitness = creature.GetFitness();
                currentData = null;
                if ((currentIdx < (startIdx + population - 1))
                    && ((currentIdx) < (creatureDB.Count))) {
                    currentIdx++;
                } else {
                    RemoveCreature(creatureDB[currentIdx].id);
                    currentIdx = -1;
                    SortCreatures();
                    KillBadCreatures();
                    createBtn.interactable = true;
                    goBtn.interactable = false;
                    continuousBtn.interactable = false;
                    if (continuous) {
                        MakeGeneration();
                        Go();
                    }
                }
            }
        }
    }

    public void Quit() {
        Application.Quit();
    }

    public static Color ConvertColor(int r, int g, int b, float a){
        return new Color(r / 255.0f, g / 255.0f, b / 255.0f, a);
    }
}
