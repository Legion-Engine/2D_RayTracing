﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class UnityEventRayTracedLight : UnityEvent<RayTracedLight>
{

}

public class RayTracer : MonoBehaviour
{
    private List<RayCollider> m_colliders;
    private List<RayHit> m_rayHits;
    private List<Ray> m_rays;
    private List<RayTracedLight> m_lights;
    private UnityEventRayTracedLight m_onLightAdd;
    private UnityEventRayTracedLight m_onLightRemove;

    public void Awake()
    {
    }

    public void register(Ray ray)
    {
        if(m_rays == null) m_rays = new List<Ray>();
        m_rays.Add(ray);
    }

    public void unRegister(Ray ray)
    {
        m_rays.Remove(ray);
    }

    public void register(RayTracedLight light)
    {
        if(m_lights == null)
        {
            m_lights = new List<RayTracedLight>();
        }
        m_lights.Add(light);
        m_onLightAdd?.Invoke(light);
    }

    public void unRegister(RayTracedLight light)
    {
        m_lights.Remove(light);
        m_onLightRemove?.Invoke(light);
    }

    public void register(RayCollider collider)
    {
        if (m_colliders == null)
        {
            m_colliders = new List<RayCollider>();
            m_onLightAdd = new UnityEventRayTracedLight();
            m_onLightRemove = new UnityEventRayTracedLight();
        }
        m_colliders.Add(collider);
        if (m_lights != null)
        {
            for (int i = 0; i < m_lights.Count; ++i)
            {
                collider.onLightAdd(m_lights[i]);
            }
        }
    }

    public void unRegister(RayCollider collider)
    {
        if (m_colliders == null) return;
        m_colliders.Remove(collider);
    }

    public void callBackOnLightAdd(UnityAction<RayTracedLight> action)
    {
        m_onLightAdd?.AddListener(action);
    }

    public void callBackOnLightRemove(UnityAction<RayTracedLight> action)
    {
        m_onLightAdd?.RemoveListener(action);
    }

    public void Update()
    {
        if (m_rays == null) return;
        m_rayHits?.Clear();
        if (m_colliders != null)
        {
            for (int i = 0; i < m_colliders.Count; ++i)
            {
                m_colliders[i].clearHits();
            }
        }

        for (int r = 0; r < m_rays.Count; ++r)
        {
            bool hasBounce = true;
            List<Ray> rays = m_rays[r].getBounces();
            rays.Insert(0, m_rays[r]);
            for (int i = 0; i < rays.Count; ++i)
            {
                RayHit hit = collide(rays[i]);
                if (!hit.nullHit)
                {
                    if(m_rayHits == null) m_rayHits = new List<RayHit>();
                    m_rayHits.Add(hit);
                    hit.collider.registerHit(hit);
                }
                else
                {
                }

                if(!rays[i].hasBounce())
                {
                    rays[i].resetReflect();
                    break;
                }
            }
        }

        if (m_colliders != null)
        {
            for (int i = 0; i < m_colliders.Count; ++i)
            {
                m_colliders[i].applyHits();
            }
        }
    }

    public RayHit collide(Ray ray)
    {
        if (m_colliders == null) return new RayHit(ray);
        RayHit hit = new RayHit(ray);
        float dist = 0;
        for (int i = 0; i < m_colliders.Count; ++i)
        {
            if(m_colliders[i].collide(ray, out RayHit newHit))
            {
                if(hit.nullHit || (newHit.point-ray.position).magnitude < dist)
                {
                    dist = (newHit.point - ray.position).magnitude;
                    hit = newHit;
                }
            }
        }
        Ray reflect = ray.reflect(hit);
        return hit;
    }
}
