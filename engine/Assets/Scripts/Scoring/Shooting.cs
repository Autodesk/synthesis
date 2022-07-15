using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Shooting : MonoBehaviour
{ //bullet
    public GameObject bullet;

    //bullet force
    public float shootForce;

    //Gun Stats
    public float timeBetweenShooting, timeBetweenShots, spread, reloadTime;
    public int magazineSize;
    public bool allowButtonHold;
    int bulletsLeft, bulletsShot;

    public Rigidbody rb;

    //recoil
    public float recoilForce;

    //bools
    bool shooting, readyToShoot, reloading;

    //Reference
    public Camera fpsCam;
    public Vector3 attackPoint;


    //bug fixing??
    public bool allowInvoke = true;



    private void Awake()
    {
        bullet = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        attackPoint = Camera.main.GetComponent<CameraController>().FocusPoint();
        bulletsLeft = magazineSize;
        readyToShoot = true;
        fpsCam = Camera.main;
        
    }
    private void Update()
    {
        ReadInput();


    }
    private void ReadInput()
    {
        shooting = Input.GetKey(KeyCode.LeftShift);

        //reloading
        if (Input.GetKeyDown(KeyCode.R) && bulletsLeft < magazineSize && !reloading) Reload();
        //auto reload
        if (readyToShoot && shooting && !reloading && bulletsLeft <= 0) Reload();

        //shooting
        if (readyToShoot && shooting && !reloading && bulletsLeft > 0)
        {
            //set bullets shot to 0
            bulletsShot = 0;

            Shoot();
        }
    }
    private void Shoot()
    {
        readyToShoot = false;

        //find the exact hit position using raycast
        Ray ray = fpsCam.ViewportPointToRay(new Vector3(0.5f, 0.8f, 0));
        RaycastHit hit;

        //check if ray hits something

        Vector3 targetPoint;

        if (Physics.Raycast(ray, out hit))
            targetPoint = hit.point;
        else
            targetPoint = ray.GetPoint(75); //just a point away from the player
        targetPoint = ray.GetPoint(75);
        //calculate direction from attackPoint to targetPOint
        Vector3 directionWithoutSpread = targetPoint - attackPoint;

        //Calculate spread
        float x = Random.Range(-spread, spread);
        float y = Random.Range(-spread, spread);

        //calculate new direction with spread
        Vector3 directionWithSpread = directionWithoutSpread; //+ new Vector3(x, y, 0);
        //DISABLED SPREAD ^^

        //instatiate bullet/projectile
        object[] instanceData = new object[1];
        instanceData[0] = transform.eulerAngles;
        //to rotate 180: transform.Rotate(Vector3.up * 180f);


        GameObject currentBullet = Instantiate(bullet, attackPoint, Quaternion.identity); //store instantiation of bullet
        //currentBullet.GetComponent<Bullet>().friendly = playerRb;


        //add forces to bullet
        currentBullet.GetComponent<Rigidbody>().AddForce(directionWithSpread.normalized * shootForce, ForceMode.Impulse);
        //currentBullet.GetComponent<Bullet>().orientation = transform.eulerAngles;
        //add recoil
        rb.AddForce(-directionWithSpread.normalized * recoilForce, ForceMode.Impulse);


        bulletsLeft--;
        bulletsShot++;

        //invoke resetShot function (if not already invoked)

        if (allowInvoke)
        {
            Invoke("ResetShot", timeBetweenShooting);
            allowInvoke = false;
        }


    }

    private void ResetShot()
    {
        //allow to shoot again
        readyToShoot = true;
        allowInvoke = true;
    }

    private void Reload()
    {
        reloading = true;
        Invoke("ReloadFinished", reloadTime);
    }
    private void ReloadFinished()
    {
        bulletsLeft = magazineSize;
        reloading = false;
    }
}
