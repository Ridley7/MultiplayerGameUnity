using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
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

    //Obtenemos la camara, ya que esta ya no hace parte del objeto del jugador
    private Camera cam;

    // Start is called before the first frame update
    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;

        cam = Camera.main;
    }

    // Update is called once per frame
    void Update()
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
    }

    private void LateUpdate()
    {
        cam.transform.position = viewPoint.position;
        cam.transform.rotation = viewPoint.rotation;
    }
}
