﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement2 : MonoBehaviour
{

    private Rigidbody2D rigidbody2D;       //Store a reference to the Rigidbody2D component required to use 2D Physics.
    public KeyCode moveLeft = KeyCode.LeftArrow;
    public KeyCode moveRight = KeyCode.RightArrow;
    public KeyCode moveDown = KeyCode.DownArrow;
    public KeyCode moveUp = KeyCode.UpArrow;

    private float targetTime = 2.0f;  // max keypress time
    private float currentTime = 0.0f; // already pressed time
    private float Speed = 0;

    public float hSpeedMax = 6;
    public float hRampupTime = 0.2f;

    public float vSpeedDefault = 6;
    public float vDownSpeedMax = 12;
    public float vUpSpeedMax = 4;
    public float vDownRampupTime = 2f;
    public float vUpRampupTime = 1f;

    public bool Enabled = true;
    public bool EnablePitchChange = true;

    private Transform _trail;

    private ParallelMusicManager _pmm;
    private playerCollison _pC;

    public float CurrentVerticalSpeed
    {
        get
        {
            if (rigidbody2D != null)
                return rigidbody2D.velocity.y;
            else
                return vSpeedDefault;
        }
    }


    // Use this for initialization
    void Start()
    {
        //Get and store a reference to the Rigidbody2D component so that we can access it.
        rigidbody2D = GetComponent<Rigidbody2D>();
        rigidbody2D.velocity = Vector2.down * vSpeedDefault;
        _pmm = Hub.Get<ParallelMusicManager>();
        _pC = Hub.Get<playerCollison>();
        _trail = GameObject.FindWithTag("trail")?.transform;
    }

    // Update is called once per frame
    void Update()
    {
        if (!Enabled)
        {
            rigidbody2D.velocity = Vector2.zero;
            return;
        }

        var velocity = rigidbody2D.velocity;

        float horizontalSpeed = GetHorizontalSpeed(velocity);
        float verticalSpeed = GetVerticalSpeed(velocity);

        rigidbody2D.velocity = new Vector2(horizontalSpeed, verticalSpeed);

        //Rotate Trail to movement
        float rot_z = Mathf.Atan2(verticalSpeed, horizontalSpeed) * Mathf.Rad2Deg;
        _trail.rotation = Quaternion.Euler(0f, 0f, rot_z + 90);

        //transform.rotation = Quaternion.LookRotation(rigidbody2D.velocity);

        if (EnablePitchChange && !_pC.CollisionDisabled)
        {
            float pitchChangePercent = (-verticalSpeed - vSpeedDefault) / vDownSpeedMax;
            _pmm.ChangePitch(pitchChangePercent);
        }
        else if (_pC.CollisionDisabled)
        {
            _pmm.ChangePitch(0);
        }
    }

    private float GetHorizontalSpeed(Vector2 currentVelocity)
    {
        float horizontalSpeed = 0;
        if (Input.GetKey(moveLeft))
        {
            if (hRampupTime > 0)
            {
                float speedChange = hSpeedMax / hRampupTime * Time.deltaTime;
                float speed = Mathf.Min(0, currentVelocity.x); //Instant direction change
                speed -= speedChange;

                speed = Mathf.Max(speed, -hSpeedMax);
                horizontalSpeed = speed;
            }
            else
            {
                horizontalSpeed = -hSpeedMax;
            }
        }
        else if (Input.GetKey(moveRight))
        {
            if (hRampupTime > 0)
            {
                float speedChange = hSpeedMax / hRampupTime * Time.deltaTime;
                float speed = Mathf.Max(0, currentVelocity.x); //Instant direction change
                speed += speedChange;
                speed = Mathf.Min(speed, hSpeedMax);
                horizontalSpeed = speed;
            }
            else
            {
                horizontalSpeed = hSpeedMax;
            }
        }

        return horizontalSpeed;
    }

    private float GetVerticalSpeed(Vector2 currentVelocity)
    {
        float verticalSpeed = currentVelocity.y;

        bool down = Input.GetKey(moveDown);
        bool up = Input.GetKey(moveUp);

        if (down)
        {
            verticalSpeed = Mathf.Lerp(currentVelocity.y, -vDownSpeedMax, Time.deltaTime / vDownRampupTime);
        }
        else if (up)
        {
            verticalSpeed = Mathf.Lerp(currentVelocity.y, -vUpSpeedMax, Time.deltaTime / vUpRampupTime);
        }
        else
        {
            float rampup = currentVelocity.y < vSpeedDefault ? vUpRampupTime : vDownRampupTime;
            verticalSpeed = Mathf.Lerp(verticalSpeed, -vSpeedDefault, Time.deltaTime / rampup);
        }

        return verticalSpeed;
    }
}
