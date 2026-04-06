using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class TaskRunner : Singleton<TaskRunner>
{
    private Dictionary<string,List<Coroutine>> _coroutines = new Dictionary<string,List<Coroutine>>();
    public void Delay(float seconds, Action action,string name = "") 
    {
        Coroutine coroutine = StartCoroutine(Execute(seconds, action));
        if (name != "")
        {
            if (!_coroutines.ContainsKey(name))
            {
                _coroutines[name] = new List<Coroutine>();
            }
            _coroutines[name].Add(coroutine);
        }
    }
public Coroutine Move(Transform target, Vector3 endPos, Quaternion endRot, float duration, string name = "")
    {
        Coroutine c = StartCoroutine(TweenRoutine(target, endPos, endRot, duration));
        if (!string.IsNullOrEmpty(name))
        {
            if (!_coroutines.ContainsKey(name)) _coroutines[name] = new List<Coroutine>();
            _coroutines[name].Add(c);
        }
        return c;
    }

    private IEnumerator TweenRoutine(Transform target, Vector3 endPos, Quaternion endRot, float duration)
    {
        Vector3 startPos = target.position;
        Quaternion startRot = target.rotation;
        float elapsed = 0;

        while (elapsed < duration)
        {
            if (target == null) yield break; // Safety check
            
            elapsed += Time.deltaTime;
            float t = elapsed / duration;

            // Optional: Add Easing (SmoothStep)
            //float smoothedT = t * t * (3f - 2f * t); 

            target.position = Vector3.Lerp(startPos, endPos, t);
            target.rotation = Quaternion.Lerp(startRot, endRot, t);
            
            yield return null;
        }

        // Ensure final snap
        if (target != null)
        {
            target.position = endPos;
            target.rotation = endRot;
        }
    }
    public void Cancel(string name)
    {
        if (_coroutines.ContainsKey(name))
        {
            foreach (var coroutine in _coroutines[name])
            {
               if (coroutine != null) StopCoroutine(coroutine);
            }
            _coroutines.Remove(name);
        }       
    }

    private IEnumerator Execute(float seconds, Action action)
    {
        yield return new WaitForSeconds(seconds);
        action?.Invoke();
    }
}