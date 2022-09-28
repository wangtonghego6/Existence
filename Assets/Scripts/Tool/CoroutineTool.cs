using System;
using System.Collections;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public static class CoroutineTool
{
    public static IEnumerator WaitForSeconds(float time)
    {
        float currTime = 0;
        while (currTime<time)
        {
            currTime += Time.deltaTime;
            yield return null;
        }
    }
}
