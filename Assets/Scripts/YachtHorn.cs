using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class YachtHorn : MonoBehaviour
{
    public AudioSource soruce;
    public AudioClip clip;
    public float playdistance = 10f;

    public Transform gate1;
    public Transform gate2;
    public Transform gate3;
    public Transform gate4;
    public Transform gate5;
    public Transform gate6;

    public bool played1;
    public bool played2;
    public bool played3;
    public bool played4;
    public bool played5;
    public bool played6;

    public bool looped; // if true, reset after all gates triggered

    public bool initiated = false;

    private IEnumerator Start()
    {

        yield return new WaitForSeconds(2f);
        initiated = true;
    }

    private void Update()
    {

        if (!initiated) return;

      //  CheckGate(gate1, ref played1);
     //   CheckGate(gate2, ref played2);
     //   CheckGate(gate3, ref played3);
        CheckGate(gate4, ref played4);
      //  CheckGate(gate5, ref played5);
      //  CheckGate(gate6, ref played6);

        /*
        if (looped && played1 && played2 && played3 && played4 && played5 && played6)
        {
            ResetAll();
        }*/
    }

    private void CheckGate(Transform gate, ref bool playedFlag)
    {
        if (gate == null) return;

        if (Vector3.Distance(transform.position, gate.position) < playdistance)
        {
            if (!playedFlag)
            {
                if (gate == gate4) // it was gate6
                {
                    StartCoroutine(ResetHorn());
                }
                soruce.PlayOneShot(clip);
                playedFlag = true;
               
            }
        }
    }


    IEnumerator ResetHorn()
    {
      
        yield return new WaitForSeconds(50f);
        //looped = true;
        ResetAll();
    }
    private void ResetAll()
    {
       
        played1 = false;
        played2 = false;
        played3 = false;
        played4 = false;
        played5 = false;
        played6 = false;
    }
}
