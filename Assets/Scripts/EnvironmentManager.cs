using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Diagnostics;
using System.Linq;

public class EnvironmentManager : MonoBehaviour
{
    #region Fields and properties

    VoxelGrid _voxelGrid;
    int _randomSeed = 666;

    bool _showVoids = true;

    Texture2D _inputImage;
      

    #endregion

    #region Unity Standard Methods

    void Start()
    {
        // Initialise the voxel grid
        //Vector3Int gridSize = new Vector3Int(25, 10, 25);
        //_voxelGrid = new VoxelGrid(gridSize, Vector3.zero, 1, parent: this.transform);

        //Initialise the voxel grid from image
        _inputImage = Resources.Load<Texture2D>("Data/map1");

        
        _voxelGrid = new VoxelGrid(_inputImage,Vector3.zero,1, 1, parent: this.transform);
        


        // Set the random engine's seed
        Random.InitState(_randomSeed);
    }

    void Update()
    {
        // Draw the voxels according to their Function Colors
        DrawVoxels();

        // Use the V key to switch between showing voids
        if (Input.GetKeyDown(KeyCode.V))
        {
            _showVoids = !_showVoids;
        }

        //找开始的白块当单机鼠标左键的时候
        if (Input.GetMouseButton(0))
        {
            var initialVoxel = StartVoxel();

            if (initialVoxel != null)
            {
                //print(initialVoxel.Index);
                print(initialVoxel.Qname);
                _voxelGrid.GrowPlot(initialVoxel.Index,1);
              
            }
        }


        //create random plots
        if (Input.GetKeyDown(KeyCode.Space))
        {
            //CreateRandomPlot(10, 5, 1);
            GenerateRandomPlotsAndSave(10,5,1,2,6);
        }

        //clear the gird
        if (Input.GetKeyDown(KeyCode.R))
        {
            _voxelGrid.ClearGrid();
        }
    }

    #endregion

    #region Private Methods

    //随机生成一些plot然后再测试,基于几个点的最短路径
    void GenerateRandomPlotsAndSave(int area, int maxVoxles, int minVoxels, int minRadius,int maxRadius)
    {
        Stopwatch stopwatch = new Stopwatch();
        stopwatch.Start();

        string saveFolder = "PlotsTest";

        for (int i = 0; i < area; i++)
        {
            // control influence of random seed
            int areasize = Random.Range(minVoxels, maxVoxles);
            _voxelGrid.ClearGrid();

            CreateRandomPlot(areasize,maxVoxles,minVoxels);

            //shortest path from image to grid
            //get data from voxelgrid and translate it into image
            Texture2D gridImage = _voxelGrid.ImageFromGrid();

            //resize image
            Texture2D resizedImage = ImageReadWrite.Resize256(gridImage, Color.black);//pass through grid image

            ImageReadWrite.SaveImage(resizedImage, $"{saveFolder}/Grid_{i}");
        }
        stopwatch.Stop();

        print($"Took {stopwatch.ElapsedMilliseconds} milliseconds to genetate {area} images");
    }

    // method to creat each random plot
    void CreateRandomPlot(int areasize,  int maxVoxles, int minVoxels,int height =0)
    {
        for (int i = 0; i < areasize; i++)
        {
            bool success = false;

            //if GrowPlot is not successed, do this
            while (!success)
            {
                //a random value between 0,1 - taggle
                float rand = Random.value;
                
                //only generate plot on the garden or empty land

                int x;
                int z;
                int distance;

                //类似一个概率滑块来将随机值和0.5的概率联系
                if (rand < 0.5f)
                {
                    // condition(bool) + ? + if result is true set to 0 + : else set to maxium
                    x = Random.value < 0.5f ? Random.Range(0,_voxelGrid.GridSize.x) : _voxelGrid.GridSize.x - 1;
                    z = Random.Range(0, _voxelGrid.GridSize.x);
                    distance = 1;

                }
                //>0.5, opposite
                else
                {
                    z = Random.value < 0.5f ? Random.Range(0, _voxelGrid.GridSize.z) : _voxelGrid.GridSize.z - 1;
                    x = Random.Range(0, _voxelGrid.GridSize.z);
                    distance = 2;
                }


                Vector3Int startPoint = new Vector3Int(x, 0, z);
                Vector3Int endPoint = new Vector3Int(x+distance, 0, z+distance);

                int voxelAmt = Random.Range(minVoxels, maxVoxles);

                success = _voxelGrid.GrowPlot(startPoint, voxelAmt);

                

            }

        }
    }

    //返回voxel
    Voxel StartVoxel() 
    {

        Voxel selected = null;
        

   

        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        
        if(Physics.Raycast(ray,out RaycastHit hit))
        {
            //Get this voxel propiety
            Transform objectHit = hit.transform;

            if (objectHit.CompareTag("Voxel"))
            {
                //get its name(index)
                string voxelName = objectHit.name;
                var index = voxelName.Split('_').Select(v => int.Parse(v)).ToArray();
                
                

                //reture its index
                selected = _voxelGrid.Voxels[index[0], index[1], index[2]];

                //give white color
                Drawing.DrawCube(selected.Index, _voxelGrid.VoxelSize, Color.white);
                //selected.VoxelCollider.GetComponent<Renderer>().material.color = Color.white;

            }
        }

        return selected;
    }

    /// <summary>
    /// Draws the voxels according to it's state and Function Corlor
    /// </summary>
    void DrawVoxels()
    {
        foreach (var voxel in _voxelGrid.Voxels)
        {
            if (voxel.IsActive)
            {
                Vector3 pos = (Vector3)voxel.Index * _voxelGrid.VoxelSize + transform.position;
                if (voxel.FColor    ==   FunctionColor.Black)   Drawing.DrawCube(pos, _voxelGrid.VoxelSize, Color.black);
                else if (voxel.FColor == FunctionColor.Red)     Drawing.DrawCube(pos, _voxelGrid.VoxelSize, Color.red);
                else if (voxel.FColor == FunctionColor.Yellow)  Drawing.DrawCube(pos, _voxelGrid.VoxelSize, Color.yellow);
                else if (voxel.FColor == FunctionColor.Green)   Drawing.DrawCube(pos, _voxelGrid.VoxelSize, Color.green);
                else if (voxel.FColor == FunctionColor.Cyan)    Drawing.DrawCube(pos, _voxelGrid.VoxelSize, Color.cyan);
                else if (voxel.FColor == FunctionColor.Magenta) Drawing.DrawCube(pos, _voxelGrid.VoxelSize, Color.magenta);
                else if (voxel.FColor == FunctionColor.Blue)    Drawing.DrawCube(pos, _voxelGrid.VoxelSize, Color.blue);
                else if (voxel.FColor == FunctionColor.White)    Drawing.DrawCube(pos, _voxelGrid.VoxelSize, Color.white);
                else if (voxel.FColor == FunctionColor.Gray)    Drawing.DrawCube(pos, _voxelGrid.VoxelSize, Color.gray);
                else if (_showVoids && voxel.Index.y == 0)
                    Drawing.DrawTransparentCube(pos, _voxelGrid.VoxelSize);
            }
        }
    }

    void GetQuality()
    {
        foreach (var voxel in _voxelGrid.Voxels)
        {
            if (voxel.IsActive)
            {
                //get the quality name
                print(voxel.Qname);

                Vector3 position = (Vector3)voxel.Index * _voxelGrid.VoxelSize + transform.position;

                if (voxel.FColor == FunctionColor.Red) voxel.Qname = ColorQuality.House;
                else if (voxel.FColor == FunctionColor.Yellow) voxel.Qname = ColorQuality.Street;
                else if (voxel.FColor == FunctionColor.Blue) voxel.Qname = ColorQuality.Backyard;
                else if (voxel.FColor == FunctionColor.Magenta) voxel.Qname = ColorQuality.Frontyard;
                else if (voxel.FColor == FunctionColor.Green) voxel.Qname = ColorQuality.Tree;
                else if (voxel.FColor == FunctionColor.Cyan) voxel.Qname = ColorQuality.LandTexture;
                else if (voxel.FColor == FunctionColor.White) voxel.Qname = ColorQuality.Plot;
                else if (_showVoids && voxel.Index.y == 0) voxel.Qname = ColorQuality.EmptyLand;


            }
        }
    }

    #endregion

    #region Public Method

    //UI Bottom
    public void VoxeliseImage()
    {
        _voxelGrid.SetStatesFromImage(_inputImage);
    }

    #endregion
}
