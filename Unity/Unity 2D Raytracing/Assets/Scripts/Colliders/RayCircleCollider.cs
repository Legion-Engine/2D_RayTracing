﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class RayCircleCollider : RayCollider
{
    [SerializeField] private float m_radius;
    [SerializeField] private readonly Vector2 m_offset;
    private Sprite m_sprite;
    private List<Ray> m_rays;
    [SerializeField] private List<RayHit> m_hits;
    [SerializeField] private Texture2D m_lightTexture;

    protected override void init()
    {
        m_tracer.register(this);
        m_sprite = this.GetComponent<SpriteRenderer>().sprite;
        if(m_lightTexture == null) m_lightTexture = new Texture2D(m_sprite.texture.width*2, m_sprite.texture.height*2);
        for(int y = 0; y < m_lightTexture.height*2; ++y)
            for(int x = 0; x < m_lightTexture.width *2; ++x)
            {
                m_lightTexture.SetPixel(x, y, Color.black);
            }
        m_lightTexture.Apply();
    }

    public void OnDestroy()
    {
        m_tracer.unRegister(this);
    }

    public override bool collide(Ray ray, out RayHit hit)
    {
        if (ray == null || ray.origin == this)
        {
            hit = new RayHit(null);
            return false;
        }
        Vector2 pos = center;
        Vector2 dist = pos - ray.position; // Distance between ray start and circle center

        float centerToRay = Vector2.Dot(dist, ray.normal.normalized);

        if (Mathf.Abs(centerToRay) > m_radius)
        {
            hit = new RayHit(ray);
            return false;
        }

        Vector2 closestRayPoint = ray.position + ray.direction * Vector2.Dot(dist, ray.direction.normalized);
        float closestRayPointToPoi = Mathf.Sqrt(Mathf.Pow(m_radius, 2) - Mathf.Pow(centerToRay, 2));

        Vector2 poi = closestRayPoint - (closestRayPointToPoi * ray.direction.normalized);
        Vector2 rayToPoi = poi - ray.position;
        if (rayToPoi.normalized != ray.direction)
        {
            hit = new RayHit(ray);
            return false;
        }
        Vector2 normal = (poi - center).normalized;
        Vector2 pixel = textureSpaceCoord(poi);
        hit = new RayHit(ray, poi, new Vector2Int((int)pixel.x, (int)pixel.y), normal, this, m_sprite.texture.GetPixel((int)pixel.x, (int)pixel.y));
        return true;
    }

    public override void registerHit(RayHit hit)
    {
        m_hits.Add(hit);
        m_lightTexture.SetPixel(hit.pixel.x, hit.pixel.y, hit.ray.color);
    }

    public override void applyHits()
    {
        m_lightTexture.Apply();
        //Sprite sprite = Sprite.Create(m_lightTexture, m_sprite.rect, m_sprite.pivot);
        //this.gameObject.GetComponent<SpriteRenderer>().sprite = sprite;
    }

    public override void clearHits()
    {
        m_hits.Clear();
    }

    private Vector2 textureSpaceUV(Vector2 worldPos)
    {
        Texture2D tex = m_sprite.texture;
        Vector2 texSpaceCoord = textureSpaceCoord(worldPos);

        Vector2 uvs = texSpaceCoord;
        uvs.x /= tex.width;
        uvs.y /= tex.height;

        return uvs;
    }

    private Vector2 textureSpaceCoord(Vector2 worldPos)
    {
        float ppu = m_sprite.pixelsPerUnit;

        Vector2 localPos = transform.InverseTransformPoint(worldPos) * ppu;

        Vector2 texSpacePivot = new Vector2(m_sprite.rect.x, m_sprite.rect.y) + m_sprite.pivot;
        Vector2 texSpaceCoord = texSpacePivot + localPos;

        return texSpaceCoord;
    }

    public Vector2 center
    {
        get
        {
            return new Vector2(this.transform.position.x, this.transform.position.y) + m_offset;
        }
    }

    public float radius
    {
        get
        {
            return m_radius;
        }
    }
}

#if (UNITY_EDITOR)
[CustomEditor(typeof(RayCircleCollider))]
public class RayCircleColliderEditor : Editor
{
    private RayCircleCollider m_collider;

    public void OnSceneGUI()
    {
        m_collider = this.target as RayCircleCollider;
        Handles.color = Color.red;
        Handles.DrawWireDisc(m_collider.center, Vector3.forward, m_collider.radius);
    }
}
#endif