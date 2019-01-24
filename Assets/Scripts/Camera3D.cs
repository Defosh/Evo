using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;

public class Camera3D : MonoBehaviour {
    public Creature target;            // The position that that camera will be following.
    public float smoothing = 5f;        // The speed with which the camera will be following.
    public Text nameTxt;                // The Text with the name of the creature.
    public Text lifetimeTxt;            // Lifetime.
    public Text fitnessTxt;             // Fitness.
    public Text generationTxt;          // Generation.
    public Text statusTxt;              // Status.
    public Text mutationTxt;            // Status.
    public Text speedTxt;               // Speed of the animation(Timescale)
    public Evo evo;                     // Main.
    private Vector3 offset;             // The initial offset from the target.

    
    // Init
    void Start () {
        evo = GameObject.Find("Evo").GetComponent<Evo>();
    }

    public void SetTimeScale(int scale) {
        evo.SetTimeScale(scale);
    }

    // Update is called once per frame
    void Update () {
        if (target != null) {
            if (offset == Vector3.zero) {
                // Calculate the initial offset.
                offset = transform.position - target.nodes[0].transform.position;
            }
            // Create a postion the camera is aiming for based on the offset from the target.
            Vector3 targetCamPos = target.GetCenter() + offset;
            if (targetCamPos.y < 20) { targetCamPos.y = 20f; }
            Vector3 newCamPos = new Vector3(targetCamPos.x, targetCamPos.y, targetCamPos.z);
            // Smoothly interpolate between the camera's current position and it's target position.
            if (newCamPos.x != float.NaN && newCamPos.y != float.NaN && newCamPos.z != float.NaN) {
                transform.position = Vector3.Lerp(transform.position, newCamPos, smoothing * Time.deltaTime);
                transform.LookAt(target.nodes[0].transform.position);
            }
        }
        speedTxt.text = (int)Time.timeScale + "x";
    }
}
