using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Experimental.GlobalIllumination;

public class ShadowTurn : MonoBehaviour
{
    [SerializeField] Vector3 max_shadow_scale;
    [SerializeField] Vector3 min_shadow_scale;
    [SerializeField] float cycle_time = 60f;
    private float elapsed_time = 0f;
    float cycles = 0f;
    public float half_cycles = 0f;
    [SerializeField] Light scene_light;
    bool day_time = true;
    public bool paused = false;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (paused) {  return; }

        elapsed_time += Time.deltaTime;

  

        if (elapsed_time >= cycle_time * (cycles + 1))
        {
            cycles++;
            day_time = true;
        }

        if (elapsed_time >= cycle_time * (cycles + 0.5f))
        {
            half_cycles++;
            day_time = false;
            Debug.Log(half_cycles);
        }

        // Time progress in current cycle
        float t = Mathf.Clamp01((elapsed_time - (cycle_time * cycles)) / cycle_time);

        // Angle goes from 0 to 360 over the cycle
        float angle = -t * 360f;
        transform.rotation = Quaternion.Euler(0, 0, angle);

        // Alternate cosine wave direction every full cycle
        float radians = angle * Mathf.Deg2Rad;
        float wave = Mathf.Cos(radians + (cycles % 2 == 0 ? 0f : Mathf.PI));
        float normalized = Mathf.Clamp((wave + 1f) / 2f, 0.5f, 2f);

        // Scale and light based on normalized value
        transform.localScale = new Vector3(1, normalized, 1);
        scene_light.intensity = Mathf.Lerp(0f, 1.5f, normalized);


        //float angle = (-t * 360) - 180;
        
        //transform.rotation = Quaternion.Euler(0, 0, (angle* 2) - 2);

        //float cos = -Mathf.Cos((angle - 180) * Mathf.Deg2Rad);

        //float normalized = Mathf.Clamp((cos + 1), 0.5f, 2f);

        //float scale = Mathf.Clamp01(normalized);

        //transform.localScale = new Vector3(1, scale, 1);

        //scene_light.intensity = (1 - normalized) * 1.5f;

    }

    public void setCycleTime(float time) 
    {
        elapsed_time *= cycle_time / time; 
        cycle_time = time;
    }
}
