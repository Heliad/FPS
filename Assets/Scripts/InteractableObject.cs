using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InteractableObject : MonoBehaviour
{
    [SerializeField] InventoryObjectType objectType;

    Shader outline;
    List<Material> material;
    float outlineWidth = 0;
    Color color = Color.white;

    public InventoryObjectType type { get { return objectType; } } 

    void Start ()
    {
        outline = Shader.Find("Outline");
        material = new List<Material>();

        foreach (var r in GetComponentsInChildren<Renderer>())
        {
            material.AddRange(r.materials.OfType<Material>().Where(m =>
            {
                if (m.shader.name == "Outline")
                    return true;
                else
                    return false;
            }));
        }
    }

    void Update()
    {
        foreach (Material item in material)
        {
            item.SetFloat("_Outline", outlineWidth);
            item.SetColor("_OutlineColor", color);
        }
    }

    public void SetShaderFloat(float f)
    {
        outlineWidth = f;
    }

    public void SetShaderColor(Color c)
    {
        color = c;
    }
}
