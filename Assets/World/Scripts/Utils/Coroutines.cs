using System.Collections;
using System.Collections.Generic;
using Unity.Barracuda;
using UnityEngine;

public sealed class Coroutines
{
    static Coroutines() { }
    private Coroutines() { }

    public static Coroutines Instance { get; } = new Coroutines();

    public IEnumerator BasicWaiter(float secondsToWait)
    {
        yield return new WaitForSeconds(secondsToWait);

        //float counter = 0;
        //while (counter < secondsToWait)
        //{
        //    //Increment Timer until counter >= waitTime
        //    counter += Time.deltaTime;
        //    //Debug.Log("We have waited for: " + counter + " seconds");
        //    //Wait for a frame so that Unity doesn't freeze
        //    //Check if we want to quit this function
        //    //if (quit)
        //    //{
        //    //    //Quit function
        //    //    yield break;
        //    //}
        //    yield return null;
        //}

        //yield return null;
    }
}
