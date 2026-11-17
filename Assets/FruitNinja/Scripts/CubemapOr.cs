using UnityEngine;

/// <summary>
/// Donne au shader de la pomme d'or (FruitNinja/OrReflet) le reflet précalculé de CETTE scène :
/// une cubemap par décor (low poly / high poly), pour que l'or reflète le bon environnement.
/// </summary>
[ExecuteAlways]
public class CubemapOr : MonoBehaviour
{
    public Cubemap reflet;

    static readonly int Id = Shader.PropertyToID("_RefletOrGlobal");

    private void OnEnable()
    {
        if (reflet != null) Shader.SetGlobalTexture(Id, reflet);
    }
}
