using UnityEngine;
using System.Collections;

public class TerrainDeformer : MonoBehaviour, ITerrainDeformer
{
    [Header("Performance options")]
    [SerializeField] bool m_delayLodUpdate;
    [SerializeField] bool m_instantScar = true;

    [Header("Deformation options")]
    [SerializeField] float m_radiusWorldUnits = 5f;
    [SerializeField] float m_heightWorldUnits = 3f;
    [SerializeField] AnimationCurve m_profile;
    [SerializeField] float m_deformationDuration = 0f;

    [Header("Bumpiness options")]
    [SerializeField] float m_bumpScale = 5f;
    [SerializeField] float m_bumpHeightWorldUnits = 0.3f;
    [SerializeField] AnimationCurve m_bumpBlend;

    [Header("Scar options")]
    [SerializeField] int m_scarTextureIndex = 0;
    [SerializeField] AnimationCurve m_scarBlend;

    private bool m_deformed = false;
    private Rigidbody m_rigidbody;
    private Terrain m_terrain;
    private TerrainData m_terrainData;
    private int m_xBase;
    private int m_yBase;
    private int m_xSize;
    private int m_ySize;

	public float RadiusWorldUnits { get { return m_radiusWorldUnits; } }

    void Awake()
    {
		StoreRB ();
    }

	public void StoreRB(){
		m_rigidbody = GetComponent<Rigidbody>();
		if (m_rigidbody == null) {
			Debug.Log ("no rigibody on " + gameObject.name);
		} else {
			Debug.Log ("rigid body was found for " + gameObject.name);
		}
	}

    public void DeformTerrain(Terrain terrain, Vector3 position)
    {
        if (m_deformed)
            return;
		m_rigidbody.isKinematic = true; // This is to ensure no weird camera stuff happens after hitting terrain (Singed Dirt only) - Jeremy Kenyon
        m_deformed = true;

        m_terrain = terrain;
        m_terrainData = terrain.terrainData;

        int heightMapWidth = m_terrainData.heightmapWidth;
        int heightMapHeight = m_terrainData.heightmapHeight;

        //print(string.Format("height: {0}, width: {1}", heightMapWidth, heightMapHeight));

        int radius = Mathf.CeilToInt(m_radiusWorldUnits / m_terrainData.heightmapScale.x);
        float height = m_heightWorldUnits / m_terrainData.heightmapScale.y;
        float bumpHeight = m_bumpHeightWorldUnits / m_terrainData.heightmapScale.y;

        // get the normalized position of this game object relative to the terrain
        Vector3 coord = (position - terrain.gameObject.transform.position);
        coord.x = coord.x / m_terrainData.size.x;
        coord.y = coord.y / m_terrainData.size.y;
        coord.z = coord.z / m_terrainData.size.z;

        // get the position of the terrain heightmap where the collision happened
        float xPos = coord.x * heightMapWidth;
        float yPos = coord.z * heightMapHeight;

        //print("xPos: " + xPos + ", yPos: " + yPos);

        int posXInTerrain = (int) xPos;
        int posYInTerrain = (int) yPos;

		// ensure not trying to deform terrain outside of the bounds of the mesh
        int xMin = Mathf.Max(0, posXInTerrain - radius);
        int xMax = Mathf.Min(heightMapWidth , posXInTerrain + radius);
        int yMin = Mathf.Max(0, posYInTerrain - radius);
        int yMax = Mathf.Min(heightMapHeight, posYInTerrain + radius);

        //print(string.Format("xMin: {0}, xMax: {1}, yMin: {2}, yMax: {3}", xMin, xMax, yMin, yMax));

        int xMinToCentre = posXInTerrain - xMin;
        int yMinToCentre = posYInTerrain - yMin;

        m_xSize = xMax - xMin;
        m_ySize = yMax - yMin;

        m_xBase = xMin;
        m_yBase = yMin;

        float[,] sampleHeights = new float[m_xSize, m_ySize];
        float[,] sampleScarBlend = new float[m_xSize - 1, m_ySize - 1];

        float offsetX = Random.Range(0f, 10000f);
        float offsetY = Random.Range(0f, 10000f);

		// this just figures out how much to affect the terrain
        for (int i = 0; i < m_xSize; i++)
        {
            float sampleXPos = i - xMinToCentre + posXInTerrain;

            for (int j = 0; j < m_ySize; j++)
            {
				// how far away is each coord from impact point
                float sampleYPos = j - yMinToCentre + posYInTerrain;
                float xDiff = (sampleXPos - xPos) * m_terrainData.heightmapScale.x;
                float yDiff = (sampleYPos - yPos) * m_terrainData.heightmapScale.z;
                float dist = Mathf.Sqrt(xDiff * xDiff + yDiff * yDiff) / m_radiusWorldUnits;

                float bump = Mathf.PerlinNoise(offsetX + i / m_bumpScale, offsetY + j / m_bumpScale) * 2f - 1f;
                //print(string.Format("{0}, {1}, {2}", i * m_bumpinessScale, j * m_bumpinessScale, bump));
                bump *= bumpHeight * m_bumpBlend.Evaluate(dist);

                sampleHeights[i, j] = height * m_profile.Evaluate(dist) + bump;

                if (i < m_xSize - 1 && j < m_ySize - 1)
                    sampleScarBlend[i, j] = m_scarBlend.Evaluate(dist);
            }
        }

        if (m_deformationDuration < 0.01f)
        {
            SetHeights(sampleHeights, 1f);
            SetScar(sampleScarBlend, 1f);
            Debug.Log("I'm the first destroy");
            for (int i = transform.childCount - 1; i >= 0; i--)
            {
                transform.GetChild(i).transform.rotation = Quaternion.identity;
                Debug.Log(transform.GetChild(i).name);
                transform.GetChild(i).parent = null;
            }
            Destroy(gameObject);
            //m_rigidbody.isKinematic = true;
        }
        else
        {
            StartCoroutine(DeformIncrementally(sampleHeights, sampleScarBlend));
        }
    }


    private void SetHeights(float[,] sampleHeights, float frac)
    {
        var heights = m_terrainData.GetHeights(m_xBase, m_yBase, m_xSize, m_ySize);
        m_terrainData.SetHeights(m_xBase, m_yBase, AddHeights(heights, sampleHeights, frac));
    }


    private void SetScar(float[,] sampleScarBlend, float frac)
    {
        var maps = m_terrainData.GetAlphamaps(m_xBase, m_yBase, m_xSize - 1, m_ySize - 1);

        if (m_scarTextureIndex <= maps.GetLength(2) - 1)
            m_terrainData.SetAlphamaps(m_xBase, m_yBase, AddScar(maps, sampleScarBlend, frac));
    }


    private float[,] AddHeights(float[,] heights, float[,] sampleHeights, float frac)
    {
        //print(string.Format("heights: [{0},{1}], sampleHeights: [{2},{3}]", 
        //    heights.GetLength(0), heights.GetLength(1), sampleHeights.GetLength(0), sampleHeights.GetLength(1)));
        //print(string.Format("xSize: {0}, ySize: {1}", m_xSize, m_ySize));

        var sumHeights = new float[m_ySize, m_xSize];

        for (int i = 0; i < m_xSize; i++)
        {
            for (int j = 0; j < m_ySize; j++)
            {
                sumHeights[j, i] = heights[j, i] + frac * sampleHeights[i, j];
            }
        }

        return sumHeights;
    }


    private float[,,] AddScar(float[,,] maps, float[,] sampleScarBlend, float frac)
    {
        int textures = maps.GetLength(2);

        for (int i = 0; i < m_xSize - 1; i++)
        {
            for (int j = 0; j < m_ySize - 1; j++)
            {
                float blend = frac * sampleScarBlend[i, j];
    
                for (int k = 0; k < textures; k++)
                {
                    float existing = maps[j, i, k];
                    float existingScar = maps[j, i, m_scarTextureIndex];

                    float newValue = k == m_scarTextureIndex
                        ? existing + blend
                        : existingScar < 1f
                            ? existing * (1f - (blend / (1f - existingScar)))
                            : 0f;

                    maps[j, i, k] = newValue;
                }
            }
        }

        return maps;
    }


    private IEnumerator DeformIncrementally(float[,] sampleHeights, float[,] sampleScarBlend)
    {
		MeshRenderer tempMR = gameObject.GetComponent<MeshRenderer> ();
		if (tempMR != null) {
			tempMR.enabled = false;
		}

		Collider tempCO = gameObject.GetComponent<Collider> ();
		if (tempCO != null) {
			tempCO.enabled = false;
		}

        float startTime = Time.time;
        float duration = 0;
        float totalFrac = 0;

        if (m_instantScar)
            SetScar(sampleScarBlend, 1f);

        while (duration <= m_deformationDuration)
        {
            duration = Time.time - startTime;
            float frac = Mathf.Min(1f - totalFrac, Time.deltaTime / m_deformationDuration);
            totalFrac += frac;

            var heights = m_terrainData.GetHeights(m_xBase, m_yBase, m_xSize, m_ySize);

            if (m_delayLodUpdate)
                m_terrainData.SetHeightsDelayLOD(m_xBase, m_yBase, AddHeights(heights, sampleHeights, frac));
            else
                m_terrainData.SetHeights(m_xBase, m_yBase, AddHeights(heights, sampleHeights, frac));

            if (!m_instantScar)
                SetScar(sampleScarBlend, frac);

            yield return null;
        }

        if (m_delayLodUpdate)
            m_terrain.ApplyDelayedHeightmapModification();

        Destroy(gameObject);
    }
}
