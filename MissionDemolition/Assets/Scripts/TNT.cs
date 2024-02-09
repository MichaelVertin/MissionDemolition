using System.Collections;
using System.Collections.Generic;
using System.Net.NetworkInformation;
using UnityEngine;

public class TNT : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnCollisionEnter(Collision collision)
    {
        Projectile proj = collision.gameObject.GetComponent<Projectile>();
        if (proj != null)
        {
            Vector3 pos = this.transform.position;
            Collider[] colliders = Physics.OverlapSphere(pos, 3.0f);
            
            foreach( Collider collider in colliders)
            {
                RigidbodySleep rs = collider.gameObject.GetComponent<RigidbodySleep>();
                if( rs != null)
                {
                    Destroy(rs.gameObject);
                }
            }
            Destroy(this.gameObject);
        }
    }
}
