using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ReplicantLoading : ReplicantDriver
{

    public ReplicantDriver RDriver;
    public ReplicantUIWithBackend RBack;
    public GameObject Chatobj;

    private void OnEnable()
    {
        StartCoroutine(WaitLoad());
    }

    IEnumerator WaitLoad()
    {
        yield return new WaitUntil(() => RDriver.IsAILoaded == true);

        Chatobj.SetActive(true);
        gameObject.SetActive(false);

    }



}
