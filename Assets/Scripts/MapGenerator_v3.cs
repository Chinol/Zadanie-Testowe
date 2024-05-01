using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using Unity.Collections;
using UnityEditor;
using UnityEngine;


public class MapGenerator_v3 : MonoBehaviour
{
    public GameObject grass_prefab;
    public GameObject desert_prefab;
    public GameObject snow_prefab;
    public GameObject shallow_water_prefab;
    public GameObject deep_water_prefab;
    public GameObject jungle_prefab;

    public GameObject tree_prefab;
    public GameObject rock_prefab;
    public GameObject bush_prefab;
    public GameObject snowman_prefab;

    [SerializeField] private int map_width = 50;
    [SerializeField] private int map_height = 50;
    
    [SerializeField] private float zoom = 20;

    private List<List<float>> altitudeMap = new List<List<float>>();
    private List<List<float>> temperatureMap = new List<List<float>>();
    private List<List<float>> moistureMap = new List<List<float>>();
    
    private int x_offset;
    private int y_offset;

    // Start is called before the first frame update
    void Start()
    {
        altitudeMap = GenerateNoiseMap();
        temperatureMap = GenerateNoiseMap();
        moistureMap = GenerateNoiseMap();
        GenerateWorldMap();
    }   


    List<List<float>> GenerateNoiseMap()
    {
        x_offset = UnityEngine.Random.Range(0,100000);
        y_offset = UnityEngine.Random.Range(0,100000); 

        List<List<float>> noiseMap = new List<List<float>>();

        for (int x = 0; x < map_width; x++)
        {
            noiseMap.Add(new List<float>());
            for (int y = 0; y < map_height; y++)
            {
                noiseMap[x].Add((float)GeneratePerlin(x, y, x_offset, y_offset));
            }
        }
        return noiseMap;
    }

    float GeneratePerlin(int x, int y, int x_offset, int y_offset)
    {
        float noise = Mathf.PerlinNoise((x + x_offset) / zoom, (y + y_offset) / zoom);

        if (noise > 1f)
        {
            noise = 1;
        }
        else if (noise < 0f)
        {
            noise = 0;
        }

        return noise;
    }

    void GenerateWorldMap()
    {
        GameObject tile;
        for (int x = 0; x <map_width; x++ )
        {
            for (int y = 0; y < map_height; y++)
            {
                float alt = altitudeMap[x][y];
                float temp = temperatureMap[x][y];
                float moist = moistureMap[x][y];

                if (alt < 0.2)
                {
                    tile = Instantiate(deep_water_prefab,new Vector3(x,y,0), Quaternion.identity);
                    tile.name = string.Format("DEEP WATER_tile_x: {0}_y: {1}_alt: {2}", x, y, alt);  
                }
                else if (Between(alt, 0.2f, 0.25f))
                {
                    tile = Instantiate(shallow_water_prefab,new Vector3(x,y,0), Quaternion.identity);
                    tile.name = string.Format("SHALLOW WATER_tile_x: {0}_y: {1}_alt: {2}", x, y, alt); 
                }
                else if (Between(alt, 0.25f, 1f))
                {
                    if (Between(moist, 0.0f, 1f) && Between(temp, 0.0f, 0.2f))
                    {
                        tile = Instantiate(snow_prefab,new Vector3(x,y,0), Quaternion.identity);
                        tile.name = string.Format("SNOW_tile_x: {0}_y: {1}_alt: {2}_moist: {3}_temp: {4}", x, y, alt, moist, temp);
                        spawnSnowman(x,y); 
                    }
                    else if (Between(moist, 0.0f, 1f) && Between(temp, 0.2f, 0.5f))
                    {
                        tile = Instantiate(grass_prefab,new Vector3(x,y,0), Quaternion.identity);
                        tile.name = string.Format("GRASS_tile_x: {0}_y: {1}_alt: {2}_moist: {3}_temp: {4}", x, y, alt, moist, temp);
                        spawnvegetation(x,y); 
                    }
                    else if (Between(moist, 0.6f, 1f) && Between(temp, 0.6f, 1f))
                    {
                        tile = Instantiate(jungle_prefab,new Vector3(x,y,0), Quaternion.identity);
                        tile.name = string.Format("JUNGLE_tile_x: {0}_y: {1}_alt: {2}_moist: {3}_temp: {4}", x, y, alt, moist, temp); 
                    }
                    else if (Between(moist, 0.0f, 0.3f) && Between(temp, 0.6f, 1f))
                    {
                        tile = Instantiate(desert_prefab,new Vector3(x,y,0), Quaternion.identity);
                        tile.name = string.Format("DESERT_tile_x: {0}_y: {1}_alt: {2}_moist: {3}_temp: {4}", x, y, alt, moist, temp);
                        spawnRock(x,y) ;
                    }
                    else
                    {
                        tile = Instantiate(grass_prefab,new Vector3(x,y,0), Quaternion.identity);
                        tile.name = string.Format("GRASS_tile_x: {0}_y: {1}_alt: {2}_moist: {3}_temp: {4}", x, y, alt, moist,temp);    
                    }
                }
            }
        }
    }



    Boolean Between(float val, float start, float end)
    {
        bool isBetween = false;
        if (start <= val && val < end)
        {
            isBetween = true;
        }
        
        return isBetween;
    }

    void spawnvegetation(int x, int y)
    {
        float vegetation = UnityEngine.Random.Range(0,100);
        if (vegetation <= 15)
        {
            float bushOrTree = UnityEngine.Random.Range(0,100); 
            if (bushOrTree > 60)
            {
                GameObject tree = Instantiate(tree_prefab,new Vector3(x, y,0),  Quaternion.identity);
                tree.GetComponent<SpriteRenderer>().sortingOrder = map_height+1-y;    
            }
            else
            {
                GameObject bush = Instantiate(bush_prefab,new Vector3(x, y,0),  Quaternion.identity);
                bush.GetComponent<SpriteRenderer>().sortingOrder = map_height+1-y;     
            }
        }
    }

    void spawnRock(int x, int y)
    {
        float rockSpawnRate = UnityEngine.Random.Range(0,100);
        if (rockSpawnRate <= 5)
        {
            GameObject rock = Instantiate(rock_prefab,new Vector3(x, y,0),  Quaternion.identity);
            rock.GetComponent<SpriteRenderer>().sortingOrder = map_height+1-y;    
        }
    }

    void spawnSnowman(int x, int y)
    {
        float snowmanSpawnRate = UnityEngine.Random.Range(0,100);
        if (snowmanSpawnRate <= 2)
        {
            GameObject snowman = Instantiate(snowman_prefab,new Vector3(x, y,0),  Quaternion.identity);
            snowman.GetComponent<SpriteRenderer>().sortingOrder = map_height+1-y;    
        }
    }
}
