using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Experimental.GlobalIllumination;

public class ShadowTurn : MonoBehaviour
{
    [SerializeField] Vector3 max_shadow_scale;
    [SerializeField] Vector3 min_shadow_scale;
    [SerializeField] float cycle_time = 60f;
    private float elapsed_time = 0f;
    float cycles = 0f;
    float half_cycles = 0f;
    [SerializeField] Light scene_light;
    bool day_time = true;
    public bool paused = false;
    int sign = 1;
    float cos_direction = 1;
    List<float> half_cycle_times = new List<float>();
    public float light_modifier = 1;




    // Start is called before the first frame update
    void Start()
    {
       //elapsed_time = cycle_time/2;
    }

    // Update is called once per frame
    void Update()
    {
        if (paused) { return; }


        float t = Mathf.Clamp01((elapsed_time )/cycle_time);
       
        if(t == 1) 
        {
            elapsed_time = 0f;
        }
       
        
        elapsed_time += Time.deltaTime * sign;

        float angle = (-t * 360);
        
        transform.rotation = Quaternion.Euler(0, 0, (angle));

        
        float sin = -Mathf.Sin((angle) * Mathf.Deg2Rad);
        if(cos_direction != Mathf.Sign(sin))
        {
            half_cycles++;
            if(half_cycles % 2 == 0)
            {
                cycles++;
            }
        }
        cos_direction = Mathf.Sign(sin);

     

        float normalized = Mathf.Clamp((sin + 1), 0.5f, 2f);

        float scale = Mathf.Clamp01(normalized);

        transform.localScale = new Vector3(1, scale, 1);

        float light_sin = -Mathf.Sin((angle / light_modifier) * Mathf.Deg2Rad);
        float light_normalized = Mathf.Clamp((light_sin + 1), 0.5f, 2f);

        scene_light.intensity = ((light_normalized) - 1) * 1.5f;


    }

    public void setCycleTime(float time) 
    {
        elapsed_time *= time / cycle_time; 
        Debug.Log(time/cycle_time);
        cycle_time = time;
    }

    public float getTotalCycleTime()
    {
        float total = 0;
        foreach(float time in half_cycle_times)
        {
            total += time;
        }
        return total;
    }

    public float getHalfCycles() 
    {
        return half_cycles;
    }


}
