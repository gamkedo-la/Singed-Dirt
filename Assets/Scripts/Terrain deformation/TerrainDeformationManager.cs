using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Terrain))]
public class TerrainDeformationManager : MonoBehaviour
{
    [SerializeField] bool m_delayLodUpdate;
    [SerializeField] bool m_allowErosion = true;
    [SerializeField] float m_erosionRate = 1f;
    [SerializeField] int m_errosionChunkWidthDivider = 8;
    [SerializeField] int m_errosionChunkHeightDivider = 8;
    [SerializeField] float m_settledDifference = 0.01f;

    private Terrain m_terrain;
    private TerrainData m_terrainData;
    private float[,] m_originalHeights;
    private float[,,] m_originalAlphaMaps;
    private int m_width;
    private int m_height;
    private int m_alphaMapWidth;
    private int m_alphaMapHeight;

    private bool m_errosionOn;
    private float m_maxDifference = 0;


    void Awake()
    {
        m_terrain = GetComponent<Terrain>();
        m_terrainData = m_terrain.terrainData;

        m_width = m_terrainData.heightmapWidth;
        m_height = m_terrainData.heightmapHeight;

        m_alphaMapWidth = m_terrainData.alphamapWidth;
        m_alphaMapHeight = m_terrainData.alphamapHeight;
    }


    void Start()
    {
        m_originalHeights = m_terrainData.GetHeights(0, 0, m_width, m_height);
        m_originalAlphaMaps = m_terrainData.GetAlphamaps(0, 0, m_alphaMapWidth, m_alphaMapHeight);

        StartCoroutine(Erode());
    }


    private IEnumerator Erode()
    {
        int chunkWidth = (m_width - 1) / m_errosionChunkWidthDivider;
        int chunkHeight = (m_height - 1) / m_errosionChunkHeightDivider;

        int jBlock = 0;
        int iBlock = 0;

        while (true)
        {
            if (m_errosionOn && m_allowErosion)
            {
                if (iBlock == 0 && jBlock == 0)
                    m_maxDifference = 0;

                int width = jBlock == m_errosionChunkWidthDivider - 1 ? chunkWidth + 1 : chunkWidth;
                int height = iBlock == m_errosionChunkHeightDivider - 1 ? chunkHeight + 1 : chunkHeight;

                var newHeights = new float[height, width];

                int jStart = jBlock * chunkWidth;
                int iStart = iBlock * chunkHeight;

                var currentHeights = m_terrainData.GetHeights(jStart, iStart, width, height);

                for (int i = 0; i < height; i++)
                {
                    for (int j = 0; j < width; j++)
                    {
                        float currentHeight = currentHeights[i, j];
                        float originalHeight = m_originalHeights[i + iStart, j + jStart];

                        float difference = Mathf.Abs(currentHeight - originalHeight);

                        if (difference > m_maxDifference)
                            m_maxDifference = difference;

                        newHeights[i, j] = Mathf.Lerp(
                            currentHeight,
                            originalHeight,
                            Time.deltaTime * m_erosionRate);
                    }
                }

                if (m_delayLodUpdate)
                    m_terrainData.SetHeightsDelayLOD(jStart, iStart, newHeights);  
                else
                    m_terrainData.SetHeights(jStart, iStart, newHeights);

                iBlock++;
                iBlock = iBlock % m_errosionChunkHeightDivider;

                if (iBlock == 0)
                {
                    jBlock++;
                    jBlock = jBlock % m_errosionChunkWidthDivider;
                }

                if (iBlock == 0 && jBlock == 0
                    && m_maxDifference <= m_settledDifference)
                {
                    m_errosionOn = false;

                    if (m_delayLodUpdate)
                        m_terrain.ApplyDelayedHeightmapModification();

                    //print("Erosion off, max difference = " + m_maxDifference);
                }
            }

            yield return null;
        }
    }


    void OnCollisionEnter(Collision col)
    {
        var terrainDeformer = col.gameObject.GetComponent<ITerrainDeformer>();

        if (terrainDeformer != null)
        {
            if (m_allowErosion)
                StartCoroutine(TurnOnErosion());

            terrainDeformer.DeformTerrain(m_terrain, col.contacts[0].point);
        }
    }


    void OnTriggerEnter(Collider other)
    {
        var terrainDeformer = other.gameObject.GetComponent<ITerrainDeformer>();

        if (terrainDeformer != null)
        {
            if (m_allowErosion)
                StartCoroutine(TurnOnErosion());

            terrainDeformer.DeformTerrain(m_terrain, other.transform.position);
        }
    }

	public void ApplyDeform(TerrainDeformer tdScript, Vector3 where){
		Debug.Log ("ApplyDeform got called");
		if (m_allowErosion)
			StartCoroutine(TurnOnErosion());
		tdScript.DeformTerrain (m_terrain, where);
	}


    private IEnumerator TurnOnErosion()
    {
        print("Turning on erosion");
        yield return new WaitForSeconds(1f);

        m_errosionOn = true;
        print("Erosion on");
    }


    private void OnDestroy()
    {
        m_terrainData.SetHeights(0, 0, m_originalHeights);
        m_terrainData.SetAlphamaps(0, 0, m_originalAlphaMaps);
    }
}
