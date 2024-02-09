using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Net.Sockets;
using UnityEngine;
using UnityEngine.UIElements;

[RequireComponent(typeof(Rigidbody))]
public class RigidbodySleep : MonoBehaviour
{
    private static int stableCount = 0;
    private static int unstableCount = 0;
    private bool _isStable = true;
    private static bool rebuildAll;
    public List<RigidbodySleep> contacts = new List<RigidbodySleep>();

    public int framesWithoutProgress = 0;
    public int framesUntilGiveUp = 1000;
    public int giveUpFrames = 0;
    public int stableFrames = 0;

    private int sleepCountDown = 10;
    private int origSleepCountDown = 10;
    private Rigidbody rigid;
    private bool rebuildMode = false;
    public bool rebuildDifficulty = true;
    private float rebuildPosSpeed = 1.0f;
    private float rebuildRotSpeed = 2.0f;
    private float upwardMult = 8.0f;

    private Vector3 origPos, origRot, origScale;

    public bool isStable
    {
        get{return _isStable;}
        set
        {
            if( !rebuildDifficulty )
            {
                return;
            }
            if(value && !_isStable)
            {
                _isStable = true;
                stableCount++;
                unstableCount--;
                foreach(RigidbodySleep rigidBody in contacts )
                {
                    rigidBody.isStable = true;
                }
            }
            if(_isStable && !value)
            {
                _isStable = false;
                stableCount--;
                unstableCount++;
                stableFrames = 0;
                // when set to unstable, set contacts to unstable
                foreach(RigidbodySleep rigidBody in contacts)
                {
                    rigidBody.isStable = false;
                }
            }

        }
    }

    // Start is called before the first frame update
    void Awake()
    {
        rigid = GetComponent<Rigidbody>();
        origPos = transform.position;
        origRot = transform.eulerAngles;
        origScale = transform.localScale;
        _isStable = true;
        stableCount++;
        framesUntilGiveUp = (int)Random.Range(5f, 30f);
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        /*
        if(sleepCountDown > 0)
        {
            rigid.Sleep();
            sleepCountDown--;
        }
        */



        // too far from original position
        if (rebuildDifficulty && (
            (origPos - this.transform.position).magnitude > .2f  || 
            angleDifference(origRot,this.transform.eulerAngles).magnitude > 2f ) )
        {
            /*
            Debug.Log("Too far from original position");
            Debug.Log(origPos);
            Debug.Log(this.transform.position);
            Debug.Log(origRot);
            Debug.Log(this.transform.eulerAngles);
            */

            // failed to proccess for another frame
            framesWithoutProgress++;
            
            // if too many failed, 
            if (framesWithoutProgress > framesUntilGiveUp)
            {
                // give up for a few frames
                Debug.Log("Giving up");
                giveUpFrames = 100;
                framesUntilGiveUp = (int)Random.Range(400f, 1000f);
                Vector3 rot = this.rigid.angularVelocity;
                rot.z += 30 * Random.Range(-1f,1f);
                this.rigid.angularVelocity = rot;
                framesWithoutProgress = 0;
            }
        }
        else
        {
            framesWithoutProgress = 0;
        }


        if (this.rigid.velocity.magnitude < 1f)
        {
            stableFrames++;
            if (stableFrames > 100)
            {
                isStable = true;
            }
        }
        else
        {
            stableFrames = 0;
        }

        if (rebuildDifficulty && isStable && giveUpFrames <= 0)
        {
            Vector3 velocity = this.rigid.velocity;
            // find vector from current to original position
            Vector3 distanceVector = origPos - this.transform.position;
            velocity = distanceVector;
            velocity.Normalize();
            velocity *= Mathf.Min(rebuildPosSpeed, distanceVector.magnitude);
            velocity *= distanceVector.magnitude;
            if (velocity.y > 0)
            {
                velocity.y *= upwardMult;
            }
            this.rigid.velocity = velocity;

            Vector3 angleVel = this.rigid.angularVelocity;
            Vector3 angularDistanceVector = origRot - this.transform.eulerAngles;
            if (angularDistanceVector.z > 180)
            {
                angularDistanceVector.z -= 360;
            }
            if( angularDistanceVector.z < -180 )
            {
                angularDistanceVector.z += 360;
            }
            angleVel = angularDistanceVector;
            angleVel.Normalize();
            angleVel *= Mathf.Min(rebuildRotSpeed, angularDistanceVector.magnitude);
            this.rigid.angularVelocity = angleVel;
        }
        giveUpFrames--;
    }

    /*
    private void OnCollisionEnter(Collision collision)
    {
        Projectile proj = collision.gameObject.GetComponent<Projectile>();
        if( proj != null )
        {
            Rigidbody rigid = proj.GetComponent<Rigidbody>();
            if( rigid.velocity.magnitude > 5.0f )
            {
                Destroy(this.gameObject);
            }
        }
    }
    */

    private void OnCollisionEnter(Collision collision)
    {
        RigidbodySleep rigidBody = collision.gameObject.GetComponent<RigidbodySleep>();
        if (rigidBody != null)
        {
            contacts.Add(rigidBody);
            Rigidbody rigid = rigidBody.GetComponent<Rigidbody>();
            /* if (rigid.velocity.magnitude > 0.1f)
            if(rigid.velocity.magnitude > 0.1f)
            {
                isStable = false;
            }
            */
        }
        
        // set to unstable if collide with projectile
        Projectile proj = collision.gameObject.GetComponent<Projectile>();
        if (proj != null)
        {
            Rigidbody rigid = proj.GetComponent<Rigidbody>();
            if (rigid.velocity.magnitude > 2f && 
                ( rigid.velocity - this.rigid.velocity ).magnitude > 2f )
            {
                isStable = false;
                SpreadStability();
            }
        }

        SpreadStability();

        proj = collision.gameObject.GetComponent<Projectile>();
        if (proj != null)
        {
            Rigidbody rigid = proj.GetComponent<Rigidbody>();
            if (!rebuildDifficulty && rigid.velocity.magnitude > 5.0f)
            {
                Debug.Log(rebuildMode);
                Destroy(this.gameObject);
            }
        }
    }

    private void OnCollisionExit(Collision collision)
    {
        RigidbodySleep rigidBody = collision.gameObject.GetComponent<RigidbodySleep>();
        if (rigidBody != null)
        {
            contacts.Remove(rigidBody);
            Rigidbody rigid = rigidBody.GetComponent<Rigidbody>();
            /* if (rigid.velocity.magnitude > 0.1f)
            if (rigid.velocity.magnitude > 0.1f)
            {
                isStable = false;
            }
            */
        }

        SpreadStability();
    }

    private void SpreadStability()
    {
        if( !rebuildDifficulty )
        {
            return;
        }
        float spreadRequirement;
        if( isStable )
        {
            spreadRequirement = 1f;
        }
        else
        {
            spreadRequirement = 3f;
        }
        foreach(RigidbodySleep rigidBody in contacts )
        {
            float spreadDifficulty;
            if( this.isStable )
            {
                spreadDifficulty = 0f;
            }
            else
            {
                spreadDifficulty = .5f;
            }

            // skip if both are moving fast
            if(this.rigid.velocity.magnitude > 2f && rigidBody.rigid.velocity.magnitude > 2f )
            {
                continue;
            }

            float velocityDifference = (this.rigid.velocity - rigidBody.rigid.velocity).magnitude;

            if ( velocityDifference > spreadRequirement  &&
                this.rigid.velocity.magnitude > rigidBody.rigid.velocity.magnitude)
            {
                rigidBody.isStable = this.isStable;
            }
            // this' veloicty > other's velocity
            //if(this.rigid.velocity.magnitude > rigidBody.rigid.velocity.magnitude + spreadDifficulty )
            //{
            //    rigidBody.isStable = this.isStable;
            //}
        }
    }

    public Vector3 angleDifference(Vector3 one, Vector3 other)
    {
        Vector3 diff = one - other;
        if( diff.x >= 180 )
        {
            diff.x -= 360;
        }
        if( diff.y >= 180 )
        {
            diff.y -= 360;
        }
        if( diff.z >= 180 )
        {
            diff.z -= 360;
        }
        if (diff.x <= -180)
        {
            diff.x += 360;
        }
        if (diff.y <= -180)
        {
            diff.y += 360;
        }
        if (diff.z <= -180)
        {
            diff.z += 360;
        }

        return diff;
    }
}
