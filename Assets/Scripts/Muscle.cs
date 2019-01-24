using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Muscle : MonoBehaviour {
    public GameObject pistonA;
    public GameObject pistonB;
    public GameObject cylinderA;
    public GameObject cylinderB;
    public GameObject cubeFront;
    public GameObject cubeBack;
    public GameObject cubeLeft;
    public GameObject cubeRight;
    public FixedJoint jointA;
    public FixedJoint jointB;
    public bool extracted;
    private Rigidbody rb;
    private Rigidbody rbA;
    private Rigidbody rbB;
    private Rigidbody rbCylA;
    private Rigidbody rbCylB;
    private SpringJoint sJointA;
    private SpringJoint sJointB;
    private Creature creature;
    private bool dead = false;

    // Use this for initialization
    void Awake () {
        rb = this.GetComponent<Rigidbody>();
        rbA = pistonA.GetComponent<Rigidbody>();
        rbB = pistonB.GetComponent<Rigidbody>();
        rbCylA = cylinderA.GetComponent<Rigidbody>();
        rbCylB = cylinderB.GetComponent<Rigidbody>();
        sJointA = pistonA.GetComponent<SpringJoint>();
        sJointB = pistonB.GetComponent<SpringJoint>();
        extracted = false;
	}

    void Start() {
        extracted = false;
        rb.isKinematic = false;
        rb.useGravity = true;
        if (transform.parent != null) {
            creature = (Creature)transform.parent.GetComponent(typeof(Creature));
        } else {
            DestroyImmediate(this);
        }
    }

    public void ResizeMuscle(Vector3 posA, Vector3 posB) {
        float distance = Vector3.Distance(posA, posB) - 12;
        pistonA.transform.localPosition = new Vector3(0.0f, distance * .5f, 0.0f);
        pistonB.transform.localPosition = new Vector3(0.0f, -distance * .5f, 0.0f);
        distance = Vector3.Distance(pistonA.transform.localPosition, pistonB.transform.localPosition) - 1;
        Vector3 scaleFront = cubeFront.transform.localScale;
        Vector3 scaleBack = cubeBack.transform.localScale;
        Vector3 scaleLeft = cubeLeft.transform.localScale;
        Vector3 scaleRight = cubeRight.transform.localScale;
        scaleFront.y = scaleFront.y + distance;
        scaleBack.y = scaleBack.y + distance;
        scaleLeft.y = scaleLeft.y + distance;
        scaleRight.y = scaleRight.y + distance;
        cubeFront.transform.localScale = scaleFront;
        cubeBack.transform.localScale = scaleBack;
        cubeLeft.transform.localScale = scaleLeft;
        cubeRight.transform.localScale = scaleRight;
        jointA.connectedBody = rbA;
        jointB.connectedBody = rbB;
    }

    public void SetPulse(float force) {
        if (!dead)
        {
            float dir = 1.0f;
            if (extracted == false)
            {
                dir = 1.0f;
                if (sJointA != null) { sJointA.maxDistance = .1f; }
                if (sJointB != null) { sJointB.maxDistance = .1f; }
            }
            else
            {
                dir = -1.0f;
                if (sJointA != null) { sJointA.maxDistance = 1.2f; }
                if (sJointB != null) { sJointB.maxDistance = 1.2f; }
            }
            rbCylA.AddForce(dir * transform.up * force, ForceMode.Force);
            rbCylB.AddForce(-1 * dir * transform.up * force, ForceMode.Force);
            extracted = !extracted;
        }
    }

    public void RemoveJoints() {
        dead = true;
        Destroy(jointA);
        Destroy(jointB);
        Destroy(sJointA);
        Destroy(sJointB);
        Destroy(cylinderA.GetComponent<FixedJoint>());
        Destroy(cylinderB.GetComponent<FixedJoint>());
    }

    public void Broken () {
        if (creature != null) { creature.Dead(); }
    }
}
