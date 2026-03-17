using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Slingshot : MonoBehaviour
{
    [Header("Inscribed")]
    public GameObject   projectilePrefab;
    public float        velocityMult = 10f;
    public GameObject projLinePrefab;
    public AudioSource rubbersnap;   // assign in Inspector
    public LineRenderer rubberBand;
    public Transform leftAnchor;   // assign in Inspector
    public Transform rightAnchor;  // optional second band

    [Header("Dynamic")]
    public GameObject launchPoint;
    public Vector3    launchPos;
    public GameObject projectile;
    public bool       aimingMode;
    

    void Awake()
    {
        Transform launchPointTrans = transform.Find("LaunchPoint");
        launchPoint = launchPointTrans.gameObject;
        launchPoint.SetActive(false);
        launchPos = launchPointTrans.position;

        // Initialize the rubber band
        rubberBand.positionCount = 2;
        rubberBand.enabled = false;
    }

    void OnMouseEnter()
    {
        //print("Slingshot: OnMouseEnter()");
        launchPoint.SetActive(true);
    }

    void OnMouseExit()
    {
        //print("Slingshot: OnMouseExit()");
        launchPoint.SetActive(false);
    }

    void OnMouseDown()
    {
        aimingMode = true; // user pressed mouse button while over slingshot

        projectile = Instantiate(projectilePrefab) as GameObject; // Instantiate a projectile
        projectile.transform.position = launchPos; // start it at the launchPoint
        projectile.GetComponent<Rigidbody>().isKinematic = true; // set it to isKinematic for now

        // Turn on rubber band when aiming starts
        rubberBand.enabled = true;
    }

    void Update()
    {
        // if slingshot is not in aimingMode, don't run this code
        if (!aimingMode) return;

        // get the current mouse position in 2D screen coordinates
        Vector3 mousePos2D = Input.mousePosition;
        mousePos2D.z = -Camera.main.transform.position.z;
        Vector3 mousePos3D = Camera.main.ScreenToWorldPoint(mousePos2D);

        //find the delta from the launchPos to the mousePos3D
        Vector3 mouseDelta = mousePos3D - launchPos;

        // limit mouseDelta to the radius of the slingshot spherecollider
        float maxMagnitude = this.GetComponent<SphereCollider>().radius;
        if (mouseDelta.magnitude > maxMagnitude)
        {
            mouseDelta.Normalize();
            mouseDelta *= maxMagnitude;
        }

        // move the projectile to this new position
        Vector3 projPos = launchPos + mouseDelta;
        projectile.transform.position = projPos;

        // Update rubber band positions every frame
        rubberBand.SetPosition(0, leftAnchor.position);
        rubberBand.SetPosition(1, projectile.transform.position);

        if (Input.GetMouseButtonUp(0)) 
        {
            // the mouse has been released
            aimingMode = false;

            Rigidbody projRB = projectile.GetComponent<Rigidbody>();
            projRB.isKinematic = false;
            projRB.collisionDetectionMode = CollisionDetectionMode.Continuous;
            projRB.velocity = -mouseDelta * velocityMult;

            FollowCam.SWITCH_VIEW(FollowCam.eView.slingshot);
            FollowCam.POI = projectile;

            Instantiate<GameObject>(projLinePrefab, projectile.transform);

            // Hide rubber band after firing
            // rubberBand.enabled = false;

            rubbersnap.Play();
            projectile = null;
            MissionDemolition.SHOT_FIRED();
        }
    }
}
