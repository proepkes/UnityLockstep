using BEPUutilities;
using FixMath.NET;
using Lockstep.Framework.FastCollections;

namespace Lockstep.Framework.Pathfinding
{
	public static class GridManager
	{
        public const int DefaultCapacity = 64 * 64;

        public static int Width {get; private set;}
        public static int Height {get; private set;}
		public static uint MaxIndex {get; private set;}
        public static int ScanHeight {get; private set;}
        public static int ScanWidth {get; private set;}
        public static int GridSize {get; private set;}
        public static int ScanGridSize {get; private set;}
		public static GridNode[] Grid;
		private static ScanNode[] ScanGrid;
		public const int ScanResolution = 8;                                  
        public static long OffsetX {get; private set;}
        public static long OffsetY {get; private set;}
        private static bool _useDiagonalConnections = true;
        public static bool UseDiagonalConnections {
            get {
                return _useDiagonalConnections;
            }
            private set {
                _useDiagonalConnections = value;
            }
        }
		public static void NotifyGridChanged () {
			GridVersion++;
			Pathfinding.Pathfinder.ChangeCombineIteration ();
		}
		public static uint GridVersion {get; private set;}

        private static bool _settingsChanged = true;

        public static readonly GridSettings DefaultSettings = new GridSettings();

        private static GridSettings _settings;
        /// <summary>
        /// GridSettings for the GridManager's simulation. Make sure you set this property ONLY if you wish to change the settings.
        /// Changes will apply to the next session.
        /// </summary>
        /// <value>The settings.</value>
        public static GridSettings Settings {
            get {
                return _settings;
            }
            set {
                _settings = value;
                _settingsChanged = true;
            }
        }

        private static FastStack<GridNode> CachedGridNodes = new FastStack<GridNode> (GridManager.DefaultCapacity);
        private static FastStack<ScanNode> CachedScanNodes = new FastStack<ScanNode> (GridManager.DefaultCapacity);
        static void ApplySettings () {
            Width = Settings.Width;
            Height = Settings.Height;
            ScanHeight = Height / ScanResolution;
            ScanWidth = Width / ScanResolution;
            GridSize = Width * Height;
            OffsetX = Settings.XOffset;
            OffsetY = Settings.YOffset;

            ScanGridSize = ScanHeight * ScanWidth;
            UseDiagonalConnections = Settings.UseDiagonalConnections;
        }
		private static void Generate ()
		{

        #region Pooling; no need to create all those nodes again

            if (Grid != null) {
                int min = Grid.Length;
                CachedGridNodes.EnsureCapacity (min);
                for (int i = min - 1; i >= 0; i--) {   
				    CachedGridNodes.Add (Grid[i]);
                }
            }


            if (ScanGrid != null) {
                int min = ScanGrid.Length;
                CachedScanNodes.EnsureCapacity(min);
                for (int i = min - 1; i >= 0; i--) {    
				    CachedScanNodes.Add(ScanGrid[i]);
                }
            }
        #endregion

			long startMem = System.GC.GetTotalMemory (true);
			ScanGrid = new ScanNode[ScanGridSize];
            for (int i = ScanWidth - 1; i >= 0; i--) {
                for (int j = ScanHeight - 1; j >= 0; j--) {
                    ScanNode node = CachedScanNodes.Count > 0 ? CachedScanNodes.Pop() : new ScanNode ();
                    node.Setup(i,j);
                    ScanGrid [GetScanIndex (i, j)] = node;
				}
			}
			Grid = new GridNode[GridSize];
            for (int i = Width - 1; i >= 0; i--) {
				for (int j = Height - 1; j >= 0; j--) {
                    GridNode node = CachedGridNodes.Count > 0 ? CachedGridNodes.Pop() : new GridNode ();
                    node.Setup(i,j);
                    Grid [GetGridIndex (i,j)] = node;
				}
			}
			long usedMem = System.GC.GetTotalMemory (true) - startMem;
			//Debug.Log ("Grid generated using " + usedMem + " Bytes!");
		}
		          
        public static void Setup () {
            //Nothing here to see
        }

		public static void Initialize ()
		{
			Pathfinder.Reset ();
			GridVersion = 1;                       
			_settingsChanged = true;
            if (_settingsChanged) {
                if (_settings == null)
                    _settings = DefaultSettings;
                ApplySettings ();

				//TODO: This might cause desyncs... will test further
                Generate ();

                for (int i = GridSize - 1; i >= 0; i--) {
    				Grid [i].Initialize ();
    			}
            }
            else {
                //If we're using the same settings, no need to generate a new grid or neighbors
                for (int i = GridSize - 1; i >= 0; i--) {
                    Grid[i].FastInitialize();
                }
            }
			MaxIndex = GetGridIndex (Width - 1, Height - 1);
		}      

		public static GridNode GetNode (int xGrid, int yGrid)
		{
            if (xGrid < 0 || xGrid >= Width || yGrid < 0 || yGrid >= Height) 
				return null;
			return Grid [GetGridIndex (xGrid, yGrid)];
		}
		
		static int indexX;
		static int indexY;

		public static GridNode GetNode (Fix64 xPos, Fix64 yPos)
		{
			GetCoordinates (xPos, yPos, out indexX, out indexY);
			if (!ValidateCoordinates (indexX, indexY)) {
				//Debug.LogError ("No node at position: " + xPos.ToFloat() + ", " + yPos.ToFloat());
				return null;
			}
			return (GetNode (indexX, indexY));
		}

		public static bool ValidateCoordinates (int xGrid, int yGrid)
		{
			return xGrid >= 0 && xGrid < Width && yGrid >= 0 && yGrid < Height;
		}

		public static bool ValidateIndex (int index)
		{
			return index >= 0 && index < GridSize;
		}


        public static Vector2 GetOffsettedPos (Vector2 worldPos)
        {
            return new Vector2(
                worldPos.X - OffsetX,
                worldPos.Y - OffsetY
                );
        }
		public static void GetCoordinates (Fix64 xPos, Fix64 yPos, out int xGrid, out int yGrid)
		{
            xGrid = (int)((xPos + F64.C0p5 - 1 - OffsetX).RawValue >> 32);
			yGrid = (int)((yPos + F64.C0p5 - 1 - OffsetY).RawValue >> 32);
		}

		public static bool GetScanCoordinates (long xPos, long yPos, out int xGrid, out int yGrid)
		{
			//xGrid = (int)((((xPos + F64.C0p5 - 1 - OffsetX) >> FixedMath.SHIFT_AMOUNT) + ScanResolution / 2) / ScanResolution);
			//yGrid = (int)((((yPos + F64.C0p5 - 1 - OffsetY) >> FixedMath.SHIFT_AMOUNT) + ScanResolution / 2) / ScanResolution);

			GridNode gridNode = GetNode (xPos, yPos);
			if (gridNode.IsNull())
			{
				xGrid = 0;
				yGrid = 0;
				return false;
			}

			ScanNode scanNode =  gridNode.LinkedScanNode;
			xGrid = scanNode.X;
			yGrid = scanNode.Y;

			return true;
		}

		public static ScanNode GetScanNode (int xGrid, int yGrid)
		{
			//if (xGrid < 0 || xGrid >= NodeCount || yGrid < 0 || yGrid >= NodeCount) return null;
			if (!ValidateScanCoordinates (xGrid, yGrid))
				return null;
			return ScanGrid [GetScanIndex (xGrid, yGrid)];
		}

		public static uint GetGridIndex (int xGrid, int yGrid)
		{
            return (uint)(xGrid * Height + yGrid);
		}

		public static bool ValidateScanCoordinates (int scanX, int scanY)
		{
			return scanX >= 0 && scanX < ScanWidth && scanY >= 0 && scanY < ScanHeight;
		}

		public static int GetScanIndex (int xGrid, int yGrid)
		{
			return xGrid * ScanHeight + yGrid;
		}      
	}
}