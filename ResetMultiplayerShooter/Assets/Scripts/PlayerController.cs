using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class PlayerController : MonoBehaviourPunCallbacks
{
    public Transform viewPoint;
    public float mouseSensitivity = 1f;
    private float verticalRotStore;
    private Vector2 mouseInput;

    public bool invertLook;

    public float moveSpeed = 5f;
    public float runSpeed = 8f;
    public float jumpForce = 12f;
    public float gravityMod = 2.5f;
    private float activeMoveSpeed;
    private Vector3 moveDir;
    private Vector3 movement;

    public CharacterController charCont;

    public Transform groundCheckPoint;
    private bool isGrounded;
    public LayerMask groundLayers;
    private float muzzleCounters;

    //Obtenemos la camara, ya que esta ya no hace parte del objeto del jugador
    private Camera cam;

    //Prefab para el impacto del disparo
    public GameObject bulletImpact;
    //public float timeBetweenShots = 0.1f;
    private float shotCounter;
    public float muzzleDisplayTime;


    public float maxHeat = 10f;
    //public float heatPerShot = 1f;
    public float coolRate = 4f;
    public float overheatCoolRate = 5f;
    private float heatCounter;
    private bool overHeated;

    public Gun[] allGuns;
    private int selectedGun;

    public GameObject playerHitImpact;

    // Start is called before the first frame update
    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;

        cam = Camera.main;

        UIController.instance.weaponTempSlider.maxValue = maxHeat;

        SwitchGun();

        //Transform newTrans = SpawnManager.instance.GetSpawnPoint();
        //transform.position = newTrans.position;
        //transform.rotation = newTrans.rotation;
    }

    // Update is called once per frame
    void Update()
    {
        if (photonView.IsMine)
        {
            mouseInput = new Vector2(Input.GetAxisRaw("Mouse X"), Input.GetAxisRaw("Mouse Y")) * mouseSensitivity;

            transform.rotation = Quaternion.Euler(transform.rotation.eulerAngles.x,
                transform.rotation.eulerAngles.y + mouseInput.x,
                transform.rotation.eulerAngles.z);

            verticalRotStore -= mouseInput.y;
            verticalRotStore = Mathf.Clamp(verticalRotStore, -60f, 60f);

            viewPoint.rotation = Quaternion.Euler(verticalRotStore,
                transform.rotation.eulerAngles.y,
                transform.rotation.eulerAngles.z);

            if (invertLook)
            {
                viewPoint.rotation = Quaternion.Euler(verticalRotStore, viewPoint.rotation.eulerAngles.y, viewPoint.rotation.eulerAngles.z);
            }
            else
            {
                viewPoint.rotation = Quaternion.Euler(-verticalRotStore, viewPoint.rotation.eulerAngles.y, viewPoint.rotation.eulerAngles.z);
            }

            moveDir = new Vector3(Input.GetAxisRaw("Horizontal"), 0f, Input.GetAxisRaw("Vertical"));

            //Controlamos si corremos
            if (Input.GetKey(KeyCode.LeftShift))
            {
                activeMoveSpeed = runSpeed;
            }
            else
            {
                activeMoveSpeed = moveSpeed;
            }

            //Nos guardamos la velocidad de y para luego aplicarla con la gravedad
            float yVel = movement.y;

            //Nos movemos en la direccion que apunta el raton
            movement = ((transform.forward * moveDir.z) + (transform.right * moveDir.x)).normalized * activeMoveSpeed;

            //Aplicamos a velocidad en Y
            movement.y = yVel;

            //Si nuestro personaje esta tocando el suelo la velocidad en y es 0
            if (charCont.isGrounded)
            {
                movement.y = 0f;
            }

            //Esto lo tenemos por que el charCount.isGrounded no funciona el 100% de las veces
            //asi que tenemos que poner un check extra
            isGrounded = Physics.Raycast(groundCheckPoint.position, Vector3.down, 0.25f, groundLayers);

            if (Input.GetButtonDown("Jump") && isGrounded)
            {
                movement.y = jumpForce;
            }

            //Aplicamos gravedad
            movement.y += Physics2D.gravity.y * Time.deltaTime * gravityMod;

            //transform.position += movement * moveSpeed * Time.deltaTime;
            charCont.Move(movement * Time.deltaTime);


            if (allGuns[selectedGun].muzzleFlash.activeInHierarchy)
            {
                muzzleCounters -= Time.deltaTime;

                if (muzzleCounters <= 0)
                {
                    allGuns[selectedGun].muzzleFlash.SetActive(false);
                }
            }


            if (!overHeated)
            {
                //Disparamos
                if (Input.GetMouseButtonDown(0))
                {
                    Shoot();
                }

                if (Input.GetMouseButton(0) && allGuns[selectedGun].isAutomatic)
                {
                    shotCounter -= Time.deltaTime;

                    if (shotCounter <= 0)
                    {
                        Shoot();
                    }
                }

                heatCounter -= coolRate * Time.deltaTime;
            }
            else
            {
                heatCounter -= overheatCoolRate * Time.deltaTime;

                if (heatCounter <= 0)
                {
                    overHeated = false;

                    UIController.instance.overheatedMessage.gameObject.SetActive(false);
                }
            }

            if (heatCounter < 0)
            {
                heatCounter = 0f;
            }

            UIController.instance.weaponTempSlider.value = heatCounter;

            if (Input.GetAxisRaw("Mouse ScrollWheel") > 0f)
            {
                selectedGun++;

                if (selectedGun >= allGuns.Length)
                {
                    selectedGun = 0;
                }

                SwitchGun();
            }
            else if (Input.GetAxisRaw("Mouse ScrollWheel") < 0f)
            {
                selectedGun--;

                if (selectedGun < 0)
                {
                    selectedGun = allGuns.Length - 1;
                }

                SwitchGun();
            }

            for (int i = 0; i < allGuns.Length; i++)
            {
                if (Input.GetKeyDown((i + 1).ToString()))
                {
                    selectedGun = i;
                    SwitchGun();
                }
            }


            if (Input.GetKeyDown(KeyCode.Escape))
            {
                Cursor.lockState = CursorLockMode.None;
            }
            else if (Cursor.lockState == CursorLockMode.None)
            {
                if (Input.GetMouseButtonDown(0))
                {
                    Cursor.lockState = CursorLockMode.Locked;
                }
            }
        }
    }

    private void Shoot()
    {
        //Creamos un rayo desde el viewport que ve la camara
        Ray ray = cam.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f));
        ray.origin = cam.transform.position;

        if(Physics.Raycast(ray, out RaycastHit hit))
        {
            //Debug.Log("He golpeado a " + hit.collider.gameObject.name);

            if(hit.collider.gameObject.tag == "Player")
            {
                Debug.Log("Hit " + hit.collider.gameObject.GetPhotonView().Owner.NickName);

                PhotonNetwork.Instantiate(playerHitImpact.name, hit.point, Quaternion.identity);

                hit.collider.gameObject.GetPhotonView().RPC("DealDamage", RpcTarget.All, photonView.Owner.NickName);
            }
            else
            {
                //Instanciamos ligeramente por encima de la superficie para evitar los coplanares
                GameObject bulletImpactObject = Instantiate(bulletImpact, hit.point + (hit.normal * 0.002f), Quaternion.LookRotation(hit.normal, Vector3.up));

                Destroy(bulletImpactObject, 10f);
            }
        }

        shotCounter = allGuns[selectedGun].timeBetweenShots;

        heatCounter += allGuns[selectedGun].heatPerShot;

        if(heatCounter >= maxHeat)
        {
            heatCounter = maxHeat;
            overHeated = true;

            UIController.instance.overheatedMessage.gameObject.SetActive(true);
        }

        allGuns[selectedGun].muzzleFlash.SetActive(true);
        muzzleCounters = muzzleDisplayTime;
    }

    [PunRPC]
    public void DealDamage(string damager)
    {
        TakeDamage(damager);
    }

    public void TakeDamage(string damager)
    {
        Debug.Log("PUm PUm by " + damager);

        gameObject.SetActive(false);
    }

    private void LateUpdate()
    {
        if (photonView.IsMine)
        {
            cam.transform.position = viewPoint.position;
            cam.transform.rotation = viewPoint.rotation;
        }
       
    }

    private void SwitchGun()
    {
        foreach(Gun gun in allGuns)
        {
            gun.gameObject.SetActive(false);
        }

        allGuns[selectedGun].gameObject.SetActive(true);

        allGuns[selectedGun].muzzleFlash.SetActive(false);
    }
}
