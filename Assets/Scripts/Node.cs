using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Node : MonoBehaviour {
    private Rigidbody rb;

    void Awake() {
        rb = this.GetComponent<Rigidbody>();
    }
        
    // Use this for initialization
    void Start () {
        rb.isKinematic = false;
        rb.useGravity = true;
	}

    public void SetDrag()
    {
        rb.drag = .2f;
    }

    // Update is called once per frame
    void Update () {
		
	}
}
