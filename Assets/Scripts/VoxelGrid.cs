using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class VoxelGrid
{
    #region Public fields

    public Vector3Int GridSize;
    public Voxel[,,] Voxels;
    public Corner[,,] Corners;
    public Face[][,,] Faces = new Face[3][,,];
    public Edge[][,,] Edges = new Edge[3][,,];
    public Vector3 Origin;
    public Vector3 Corner;
    public float VoxelSize { get; private set; }

    #endregion

    #region Constructors

    /// <summary>
    /// Constructor for a basic <see cref="VoxelGrid"/>
    /// Adds a game object containing a collider to each of first layer voxels
    /// </summary>
    /// <param name="size">Size of the grid</param>
    /// <param name="origin">Origin of the grid</param>
    /// <param name="voxelSize">The size of each <see cref="Voxel"/></param>
    public VoxelGrid(Vector3Int size, Vector3 origin, float voxelSize, Transform parent = null)
    {
        GridSize = size;
        Origin = origin;
        VoxelSize = voxelSize;

        Voxels = new Voxel[GridSize.x, GridSize.y, GridSize.z];

        for (int x = 0; x < GridSize.x; x++)
        {
            for (int y = 0; y < GridSize.y; y++)
            {
                for (int z = 0; z < GridSize.z; z++)
                {
                    if (y == 0)
                    {
                        Voxels[x, y, z] = new Voxel(
                            new Vector3Int(x, y, z),
                            this,
                            createCollider: true,
                            parent: parent);
                    }
                    else
                    {
                        Voxels[x, y, z] = new Voxel(
                            new Vector3Int(x, y, z),
                            this);
                    }
                    
                }
            }
        }

        MakeFaces();
        MakeCorners();
        MakeEdges();
    }

    /// <summary>
    /// Create voxel grid from the size of input image
    /// </summary>
    /// <param name="input"></input image>
    /// <param name="origin"></origin>
    /// <param name="voxelSize"></size>
    /// <param name="height"></param>
    /// <param name="parent"></param>
    /// 
    public VoxelGrid(Texture2D input, Vector3 origin, int height,float voxelSize, Transform parent = null)
    {
       
        // create new grid with image size
        GridSize = new Vector3Int(input.width, height, input.height);

        Origin = origin;
        VoxelSize = voxelSize;

        Voxels = new Voxel[GridSize.x, GridSize.y, GridSize.z];

        for (int x = 0; x < GridSize.x; x++)
        {
            for (int y = 0; y < GridSize.y; y++)
            {
                for (int z = 0; z < GridSize.z; z++)
                {
                    if (y == 0)
                    {
                        Voxels[x, y, z] = new GraphVoxel(
                            new Vector3Int(x, y, z),
                            this,
                            1f,
                            createCollider: true,
                            parent: parent);
                    }
                    else
                    {
                        Voxels[x, y, z] = new GraphVoxel(
                            new Vector3Int(x, y, z),
                            this,
                            1f);
                    }
                }
            }
        }


        MakeFaces();
        MakeCorners();
        MakeEdges();
    }

    #endregion

    #region Grid elements constructors

    /// <summary>
    /// Creates the Faces of each <see cref="Voxel"/>
    /// </summary>
    private void MakeFaces()
    {
        // make faces
        Faces[0] = new Face[GridSize.x + 1, GridSize.y, GridSize.z];

        for (int x = 0; x < GridSize.x + 1; x++)
            for (int y = 0; y < GridSize.y; y++)
                for (int z = 0; z < GridSize.z; z++)
                {
                    Faces[0][x, y, z] = new Face(x, y, z, Axis.X, this);
                }

        Faces[1] = new Face[GridSize.x, GridSize.y + 1, GridSize.z];

        for (int x = 0; x < GridSize.x; x++)
            for (int y = 0; y < GridSize.y + 1; y++)
                for (int z = 0; z < GridSize.z; z++)
                {
                    Faces[1][x, y, z] = new Face(x, y, z, Axis.Y, this);
                }

        Faces[2] = new Face[GridSize.x, GridSize.y, GridSize.z + 1];

        for (int x = 0; x < GridSize.x; x++)
            for (int y = 0; y < GridSize.y; y++)
                for (int z = 0; z < GridSize.z + 1; z++)
                {
                    Faces[2][x, y, z] = new Face(x, y, z, Axis.Z, this);
                }
    }

    /// <summary>
    /// Creates the Corners of each Voxel
    /// </summary>
    private void MakeCorners()
    {
        Corner = new Vector3(Origin.x - VoxelSize / 2, Origin.y - VoxelSize / 2, Origin.z - VoxelSize / 2);

        Corners = new Corner[GridSize.x + 1, GridSize.y + 1, GridSize.z + 1];

        for (int x = 0; x < GridSize.x + 1; x++)
            for (int y = 0; y < GridSize.y + 1; y++)
                for (int z = 0; z < GridSize.z + 1; z++)
                {
                    Corners[x, y, z] = new Corner(new Vector3Int(x, y, z), this);
                }
    }

    /// <summary>
    /// Creates the Edges of each Voxel
    /// </summary>
    private void MakeEdges()
    {
        Edges[2] = new Edge[GridSize.x + 1, GridSize.y + 1, GridSize.z];

        for (int x = 0; x < GridSize.x + 1; x++)
            for (int y = 0; y < GridSize.y + 1; y++)
                for (int z = 0; z < GridSize.z; z++)
                {
                    Edges[2][x, y, z] = new Edge(x, y, z, Axis.Z, this);
                }

        Edges[0] = new Edge[GridSize.x, GridSize.y + 1, GridSize.z + 1];

        for (int x = 0; x < GridSize.x; x++)
            for (int y = 0; y < GridSize.y + 1; y++)
                for (int z = 0; z < GridSize.z + 1; z++)
                {
                    Edges[0][x, y, z] = new Edge(x, y, z, Axis.X, this);
                }

        Edges[1] = new Edge[GridSize.x + 1, GridSize.y, GridSize.z + 1];

        for (int x = 0; x < GridSize.x + 1; x++)
            for (int y = 0; y < GridSize.y; y++)
                for (int z = 0; z < GridSize.z + 1; z++)
                {
                    Edges[1][x, y, z] = new Edge(x, y, z, Axis.Y, this);
                }
    }

    #endregion

    #region Grid operations


    /// <summary>
    /// Get the Faces of the <see cref="VoxelGrid"/>
    /// </summary>
    /// <returns>All the faces</returns>
    public IEnumerable<Face> GetFaces()
    {
        for (int n = 0; n < 3; n++)
        {
            int xSize = Faces[n].GetLength(0);
            int ySize = Faces[n].GetLength(1);
            int zSize = Faces[n].GetLength(2);

            for (int x = 0; x < xSize; x++)
                for (int y = 0; y < ySize; y++)
                    for (int z = 0; z < zSize; z++)
                    {
                        yield return Faces[n][x, y, z];
                    }
        }
    }

    /// <summary>
    /// Get the Voxels of the <see cref="VoxelGrid"/>
    /// </summary>
    /// <returns>All the Voxels</returns>
    public IEnumerable<Voxel> GetVoxels()
    {
        for (int x = 0; x < GridSize.x; x++)
            for (int y = 0; y < GridSize.y; y++)
                for (int z = 0; z < GridSize.z; z++)
                {
                    yield return Voxels[x, y, z];
                }
    }

    /// <summary>
    /// Get the Corners of the <see cref="VoxelGrid"/>
    /// </summary>
    /// <returns>All the Corners</returns>
    public IEnumerable<Corner> GetCorners()
    {
        for (int x = 0; x < GridSize.x + 1; x++)
            for (int y = 0; y < GridSize.y + 1; y++)
                for (int z = 0; z < GridSize.z + 1; z++)
                {
                    yield return Corners[x, y, z];
                }
    }

    /// <summary>
    /// Get the Edges of the <see cref="VoxelGrid"/>
    /// </summary>
    /// <returns>All the edges</returns>
    public IEnumerable<Edge> GetEdges()
    {
        for (int n = 0; n < 3; n++)
        {
            int xSize = Edges[n].GetLength(0);
            int ySize = Edges[n].GetLength(1);
            int zSize = Edges[n].GetLength(2);

            for (int x = 0; x < xSize; x++)
                for (int y = 0; y < ySize; y++)
                    for (int z = 0; z < zSize; z++)
                    {
                        yield return Edges[n][x, y, z];
                    }
        }
    }

    #endregion

    #region Public Methods


    /// <summary>
    /// Tries to create a black blob from the
    /// specified origin and with the specified size
    /// </summary>
    /// <param name="origin">The index of the origin</param>
    /// <param name="radius">The radius of the blob in voxels</param>
    /// <param name="picky">If the blob should skip voxels randomly as it expands</param>
    /// <param name="flat">If the blob should be located on the first layer or use all</param>
    /// <returns></returns>
    
    public bool GrowPlot(Vector3Int origin, int radius, int height = 0)
    {

        //List<Vector3> growingVoxel
        //A list to store the growing voxel
        List<Voxel> growingVoxel = new List<Voxel>();

        //Give them white coluor as plot
        FunctionColor plotcolor = FunctionColor.White;

        //check if the origin is valid and add it to voxel list
        if (Util.ValidateIndex(GridSize, origin) )
        {
            growingVoxel.Add(Voxels[origin.x, height, origin.z]);
        }
        else return false;

        //Iterate through the neighboring layer within the radius
        for (int i = 0; i < radius; i++)
        {
            
            List<Voxel> availableVoxels = new List<Voxel>();

            foreach (var voxel in growingVoxel)
            {
                //Get neighbors in 2D or 3D

                Voxel[] neighbors;


                if (height == 0)
                {
                    neighbors = voxel.GetFaceNeighboursXZ().ToArray();
                                       
                }
                else
                {
                    neighbors = voxel.GetFaceNeighbours().ToArray();
                }

                //Iterate each neighbors + and check if is available
                foreach (var neighbour in neighbors)
                {
                    //check if is the available plot voxel
                 
                    //+ if color is blue(backyard area that allows to grow)
                    if (neighbour.FColor == FunctionColor.Blue && neighbour.IsActive && Util.ValidateIndex(GridSize, neighbour.Index) && !growingVoxel.Contains(neighbour) && !availableVoxels.Contains(neighbour) )
                    {
                        availableVoxels.Add(neighbour);
                    }
                }

            }

            if (availableVoxels.Count == 0) break;

            //add these available voxels to growing voxels list
            foreach (var availableVoxel in availableVoxels)
            {
                if (availableVoxel.FColor == FunctionColor.Blue)
                {
                    growingVoxel.Add(availableVoxel);
                }
               
            }
        }

        // set the plot color and quality
        foreach (var voxel in growingVoxel)
        {
            if (voxel.FColor == FunctionColor.Blue)
            {
                voxel.FColor = plotcolor;
                voxel.Qname = ColorQuality.Plot;
            }
            
        }

        return true;
    }

    // A method to store the possible voxel after xxx evaluation result  + animate one by one


    // A method to check voxel with xxx evaluation result  + animate one by one
    public bool AvailablePlotVoxel()
    {

        return true;
    }

   
    /// <summary>
    /// Reads an image pixel data and set the color pixels and corresponding label/quality to the grid
    /// </summary>
    /// <param name="image">The reference image</param>
    /// <param name="layer">The target layer</param>
    public void SetStatesFromImage(Texture2D inputImage, int layer = 0)
    {
        // Iterate through the XZ plane
        for (int x = 0; x < GridSize.x; x++)
        {
            for (int z = 0; z < GridSize.z; z++)
            {
                // Get the pixel color from the image
                Color pixel = inputImage.GetPixel(x, z);

                //read voxel as its child:graphvoxel

                GraphVoxel voxel = (GraphVoxel)Voxels[x, 0, z];
                //read RGB channel
                
                //0,0,1
                if (pixel.b > pixel.r && pixel.b > pixel.g)
                {
                    Voxels[x, layer, z].FColor = FunctionColor.Blue;
                    Voxels[x, layer, z].Qname = ColorQuality.Backyard;

                }
                //0,1,1
                else if (pixel.b > pixel.r && pixel.g > pixel.r)
                {
                    Voxels[x, layer, z].FColor = FunctionColor.Cyan;
                    Voxels[x, layer, z].Qname = ColorQuality.LandTexture;
                }
                
                else if (pixel.r == 0.5f && pixel.g == 0.5f)
                {
                    Voxels[x, layer, z].FColor = FunctionColor.Gray;   
                }
              
                //0,1,0
                else if (pixel.g > pixel.r && pixel.g > pixel.b)
                {
                    Voxels[x, layer, z].FColor = FunctionColor.Green;
                    //Voxels[x, layer, z]._voxelGO.GetComponent<MeshRenderer>().material = Resources.Load<Material>("Materials/Tree");
                    Voxels[x, layer, z].Qname = ColorQuality.Tree;
                }
                //1,0,1
                else if (pixel.r > pixel.g && pixel.b > pixel.g && pixel.r <= pixel.b)
                {
                    Voxels[x, layer, z].FColor = FunctionColor.Magenta;
                    //Voxels[x, layer, z]._voxelGO.GetComponent<MeshRenderer>().material = Resources.Load<Material>("Materials/FrontYard");
                    Voxels[x, layer, z].Qname = ColorQuality.Frontyard;
                }    
                //1,1,0 pixel.g / pixel.r>= 1f
                else if (pixel.r > pixel.b && pixel.g > pixel.b)
                {
                    Voxels[x, layer, z].FColor = FunctionColor.Yellow;
                    //Voxels[x, layer, z]._voxelGO.GetComponent<MeshRenderer>().material = Resources.Load<Material>("Materials/Street");
                    Voxels[x, layer, z].Qname = ColorQuality.Street;
                }
                //1,0,0
                else if (pixel.r > pixel.g && pixel.r > pixel.b)
                {
                    Voxels[x, layer, z].FColor = FunctionColor.Red;
                    //Voxels[x, layer, z]._voxelGO.GetComponent<MeshRenderer>().material = Resources.Load<Material>("Materials/House");
                    Voxels[x, layer, z].Qname = ColorQuality.House;
                }

                //1,1,1
                else if (pixel.r == 1f && pixel.g == 1f && pixel.b == 1f)
                {
                    Voxels[x, layer, z].FColor = FunctionColor.White;
                    //Voxels[x, layer, z]._voxelGO.GetComponent<MeshRenderer>().material = Resources.Load<Material>("Materials/Plot");
                    Voxels[x, layer, z].Qname = ColorQuality.Plot;
                }
                
              
                // Check if pixel is red
                //if (pixel.r >pixel.g)
                //{
                //    // Set respective color to voxel
                //    Voxels[x, layer, z].FColor = FunctionColor.Red;
                //}
            }
        }

    }


    public void ClearGrid()
    {
        foreach (var voxel in Voxels)
        {
            voxel.FColor = FunctionColor.Empty;
        }
    }

    //Translate image input into grid with color
    //public Texture2D FromImageToGrid()
    //{
    //    TextureFormat textureFormat;
    //    //RGB png 8 depth
    //    textureFormat = TextureFormat.RGB24;

    //    //new image
    //    Texture2D gridImage = new Texture2D(GridSize.x, GridSize.z, textureFormat, true);

    //    //iterare through all voxles, get their color and write to grid image
    //    for (int i = 0; i < GridSize.x; i++)
    //    {
    //        for (int j = 0; j < GridSize.z; j++)
    //        {
    //            //get all voxel from x,z
    //            var voxel = Voxels[i, 0, j];

    //            // normalize and define the input color to voxel color 
    //            Color ci;
    //            //if (ci == Color.black) voxel.FColor = FunctionColor.Black;
    //            if (voxel.IsActive && Util.ValidateIndex(GridSize, voxel.Index) && voxel.FColor == FunctionColor.Red) voxel.Qname = ColorQuality.House;  


    //        }
    //    }
    //}

    // Generate image from Grid, voxel to pixel, read the plot on the top layer(ovverlapping) FROM VOXEL DATA TO IMAGE
    public Texture2D ImageFromGrid(int layer = 0, bool overlapping = false)
    {
        TextureFormat textureFormat;
        //RGB png 8 depth
        textureFormat = TextureFormat.RGB24;

        //new image
        Texture2D gridImage = new Texture2D(GridSize.x, GridSize.z, textureFormat, true);

        //iterare through all voxles, get their color and write to grid image
        for (int i = 0; i < GridSize.x; i++)
        {
            for (int j = 0; j < GridSize.z; j++)
            {
                //get all voxel from x,z
                var voxel = Voxels[i, 0, j];

                // assign color based on function color
                Color co;
                if (voxel.FColor == FunctionColor.White) co = Color.white;
                else if (voxel.FColor == FunctionColor.Red) co = Color.red;
                else if (voxel.FColor == FunctionColor.Black) co = Color.black;
                else if (voxel.FColor == FunctionColor.Blue) co = Color.blue;
                else if (voxel.FColor == FunctionColor.Yellow) co = Color.yellow;
                else if (voxel.FColor == FunctionColor.Green) co = Color.green;
                else if (voxel.FColor == FunctionColor.Cyan) co = Color.cyan;
                else if (voxel.FColor == FunctionColor.Magenta) co = Color.magenta;
                else if (voxel.FColor == FunctionColor.Gray) co = Color.gray;
                else co = new Color(1.0f, 0.64f, 0f); //orange  rgb value/max255

                gridImage.SetPixel(i, j, co);

            }
        }

        //from memory value to actual pixel
        gridImage.Apply();
        return gridImage;
    }

    #endregion
}


/// <summary>
/// Color coded values
/// </summary>
public enum FunctionColor
{
    Empty = -1,
    Black = 0,
    Red = 1,
    Yellow = 2,
    Green = 3,
    Cyan = 4,
    Magenta = 5,
    Blue = 6,
    White = 7,
    Gray = 8

}

public enum ColorQuality
{
    Plot = 0,
    House = 1,
    Street = 2,
    Backyard = 3,
    Frontyard = 4,
    SmallBuilding = 5,
    Tree = 6,
    EmptyLand = 7,
    LandTexture = 8

}