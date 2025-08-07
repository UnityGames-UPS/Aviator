using Unity;
using UnityEngine;

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class CurveFiller : MonoBehaviour
{
  [SerializeField] private Material fillMaterial;
  public int points = 50;
  public float curveWidth = 10f;
  public float heightMultiplier = 2f;

  void Start()
  {
    GenerateFilledCurve();
  }

  void GenerateFilledCurve()
  {
    Mesh mesh = new Mesh();
    Vector3[] vertices = new Vector3[points * 2];
    int[] triangles = new int[(points - 1) * 6];

    for (int i = 0; i < points; i++)
    {
      float t = i / (float)(points - 1);
      float x = t * curveWidth;
      float y = Mathf.Pow(t, 2) * heightMultiplier;

      vertices[i] = new Vector3(x, y, 0); // curve top
      vertices[i + points] = new Vector3(x, 0, 0); // bottom flat line
    }

    int triIndex = 0;
    for (int i = 0; i < points - 1; i++)
    {
      int top = i;
      int bottom = i + points;

      triangles[triIndex++] = top;
      triangles[triIndex++] = top + 1;
      triangles[triIndex++] = bottom;

      triangles[triIndex++] = top + 1;
      triangles[triIndex++] = bottom + 1;
      triangles[triIndex++] = bottom;
    }

    mesh.vertices = vertices;
    mesh.triangles = triangles;
    mesh.RecalculateNormals();

    MeshFilter mf = GetComponent<MeshFilter>();
    MeshRenderer mr = GetComponent<MeshRenderer>();
    mf.mesh = mesh;
    mr.material = fillMaterial;
  }


  void Update()
  {
    Debug.Log("Calling");
    GenerateFilledCurve();
  }
}
