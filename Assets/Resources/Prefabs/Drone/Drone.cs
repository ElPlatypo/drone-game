using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class Drone : MonoBehaviour
{

    public float movespeed = 5;
    public BoxCollider groundtrigger;
    public bool isactive = true;
    public int health = 100;
    private bool isgrounded;
    public CharacterController controller;


    public int scrapinventory = 0;

    void Start()
    {
        GameDirector.current.activedrones.Add(gameObject);
        for (int i = 0; i < GameDirector.current.cameras.Length; i++)
        {
            
            if (GameDirector.current.cameras[i] == null)
            {
                GameDirector.current.cameras[i] = gameObject.GetComponentInChildren<Camera>();
                break;
            }
        }
        controller.enabled = false;
    }

    // Update is called once per frame
    void Update()
    {
        if (controller.enabled)
        {
            Vector3 move = transform.right * Input.GetAxis("Horizontal") + transform.forward * Input.GetAxis("Vertical");
            if (!isgrounded)
            {
                move.y += Physics.gravity.y * Time.deltaTime * 50; 
            }
            controller.Move(move * Time.deltaTime * movespeed);

            if (Input.GetKey(KeyCode.E))
            {
                controller.transform.Rotate(0, Time.deltaTime * 100, 0);
            }
            if (Input.GetKey(KeyCode.Q))
            {
                controller.transform.Rotate(0, -Time.deltaTime * 100, 0);
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.name == "floor")
        {
            isgrounded = true;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.name == "floor")
        {
            isgrounded = false;
        }
    }

    public void ChangeHealth(int amount)
    {
        health += amount;
        if (health <= 0)
        {
            health = 0;
            isactive = false;
        }
        else
        {
            isactive = true;
        }
    }

}
