using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Com.Tempest.Nightmare {

	public class LevelGenerator {

        public const int TOTAL_LAYERS = 3;
        public const int CHUNK_INDEX_LAYER = 0;
        public const int CHUNK_OBJECT_TYPE_LAYER = 1;
        public const int CHUNK_RANDOMIZED_TYPE_LAYER = 2;

		public static string ResourcePathForIndex(int index, int type) {
			string path = "LevelChunks/";
            path += "Base" + type + "/";
			return path + "LevelChunk" + index;
		}

		public enum WallDirection {
			NORTH,
			SOUTH,
			EAST,
			WEST
		}

		public class GraphNode {

            public const int BONFIRE = 1;
            public const int CHEST = 2;
            public const int TORCH = 3;
            public const int MIRROR = 4;
            public const int PORTAL = 5;
	
			public List<WallDirection> WallsRemaining { get; set; }
			public int TimesVisited { get; set; }
			public Vector2Int Coordinates { get; set; }
            public int RandomizedType { get; set; }
			public bool IsBonfire { get; set; }
            public bool IsChest { get; set; }
			public bool IsTorch { get; set; }
            public bool IsMirror { get; set; }
            public bool IsPortal { get; set; }

			public GraphNode(Vector2Int coordinates) {
				WallsRemaining = new List<WallDirection>();
				TimesVisited = 0;
				Coordinates = coordinates;
			}

			public int GetLevelChunkIndex() {
				if (WallsRemaining.Count == 0) {
					return 11;
				}
				if (WallsRemaining.Count == 1) {
					switch (WallsRemaining[0]) {
					case WallDirection.NORTH:
                        return 1;
					case WallDirection.EAST:
                        return 2;
					case WallDirection.SOUTH:
                        return 3;
					case WallDirection.WEST:
                        return 4;
					default:
						throw new KeyNotFoundException("Illegal wall direction: " + WallsRemaining[0]);
					}
				}
				if (WallsRemaining.Count == 2) {
					if (WallsRemaining.Contains(WallDirection.NORTH) && WallsRemaining.Contains(WallDirection.EAST)) {
                        return 5;
					}
					if (WallsRemaining.Contains(WallDirection.NORTH) && WallsRemaining.Contains(WallDirection.SOUTH)) {
                        return 6;
					}
					if (WallsRemaining.Contains(WallDirection.NORTH) && WallsRemaining.Contains(WallDirection.WEST)) {
                        return 7;
					}
					if (WallsRemaining.Contains(WallDirection.EAST) && WallsRemaining.Contains(WallDirection.SOUTH)) {
                        return 8;
					}
					if (WallsRemaining.Contains(WallDirection.EAST) && WallsRemaining.Contains(WallDirection.WEST)) {
                        return 9;
					}
					if (WallsRemaining.Contains(WallDirection.SOUTH) && WallsRemaining.Contains(WallDirection.WEST)) {
                        return 10;
					}
				}
			    throw new KeyNotFoundException("I can't decide which wall index I am: " + WallsRemaining.Count);
			}

            public int GetChunkType() {
                if (IsBonfire) return BONFIRE;
                if (IsChest) return CHEST;
                if (IsTorch) return TORCH;
                if (IsMirror) return MIRROR;
                if (IsPortal) return PORTAL;
                throw new System.Exception("Chunk was not assigned a type.");
            }
		}

		public class GraphWall {
	
			public GraphNode SourceNode { get; set; }
			public WallDirection Direction { get; set; }

			public GraphWall(GraphNode sourceNode, WallDirection direction) {
				SourceNode = sourceNode;
				Direction = direction;
			}

			public WallDirection OppositeDirection() {
				switch (Direction) {
				case WallDirection.NORTH:
					return WallDirection.SOUTH;
				case WallDirection.SOUTH:
					return WallDirection.NORTH;
				case WallDirection.EAST:
					return WallDirection.WEST;
				case WallDirection.WEST:
					return WallDirection.EAST;
				default:
					throw new KeyNotFoundException("Illegal wall direction: " + Direction);
				}
			}
		}

		private int width;
		private int height;
		private int bonfireFrequency;
		private int bonfireOffset;
        private int numChests;
        private int numMirrors;
		private GraphNode[,] levelGraph;

		public LevelGenerator(int width, int height, int bonfireFrequency, int bonfireOffset, int numChests, int numMirrors) {
			this.width = width;
			this.height = height;
			this.bonfireFrequency = bonfireFrequency;
			this.bonfireOffset = bonfireOffset;
            this.numChests = numChests;
            this.numMirrors = numMirrors;
			InitializeLevelGraph();
			BuildLevelGraph();
			RestoreOuterWallsToLevelGraph();
            AddPortals();
			AddBonfires();
            AddObjects();
            RandomizeTileTypes();
		}

		private void InitializeLevelGraph() {
			levelGraph = new GraphNode[width, height];
			for (int x = 0; x < width; x++) {
				for (int y = 0; y < height; y++) {
					Vector2Int coordinates = new Vector2Int(x, y);
					levelGraph[x, y] = new GraphNode(coordinates);
					if (x != 0)
						levelGraph[x, y].WallsRemaining.Add(WallDirection.WEST);
					if (x != width - 1)
						levelGraph[x, y].WallsRemaining.Add(WallDirection.EAST);
					if (y != 0)
						levelGraph[x, y].WallsRemaining.Add(WallDirection.SOUTH);
					if (y != height - 1)
						levelGraph[x, y].WallsRemaining.Add(WallDirection.NORTH);
				}
			}
		}

		private void BuildLevelGraph() {
			List<GraphWall> wallList = new List<GraphWall>();
			GraphNode startingNode = levelGraph[0, 0];
			foreach (WallDirection wall in startingNode.WallsRemaining) {
				wallList.Add(new GraphWall(startingNode, wall));
			}
	
			while (wallList.Count > 0) {
				GraphWall currentWall = wallList[Random.Range(0, wallList.Count)];
				GraphNode sourceNode = currentWall.SourceNode;
				GraphNode destinationNode = GetDestinationNode(currentWall);
				if (sourceNode.TimesVisited < 2 || destinationNode.TimesVisited < 2) {
					sourceNode.WallsRemaining.Remove(currentWall.Direction);
					destinationNode.WallsRemaining.Remove(currentWall.OppositeDirection());
					if (destinationNode.TimesVisited == 0) {
						foreach (WallDirection direction in destinationNode.WallsRemaining) {
							wallList.Add(new GraphWall(destinationNode, direction));
						}
					}
					sourceNode.TimesVisited++;
					destinationNode.TimesVisited++;
				}
				wallList.Remove(currentWall);
			}
		}

		private void RestoreOuterWallsToLevelGraph() {
			for (int x = 0; x < width; x++) {
				for (int y = 0; y < height; y++) {
					if (x == 0)
						levelGraph[x, y].WallsRemaining.Add(WallDirection.WEST);
					if (x == width - 1)
						levelGraph[x, y].WallsRemaining.Add(WallDirection.EAST);
					if (y == 0)
						levelGraph[x, y].WallsRemaining.Add(WallDirection.SOUTH);
					if (y == height - 1)
						levelGraph[x, y].WallsRemaining.Add(WallDirection.NORTH);
				}
			}
		}

        private void AddPortals() {
            if (width < 4 || height < 4) return;
            levelGraph[1, 1].IsPortal = true;
            levelGraph[width - 2, 1].IsPortal = true;
            levelGraph[1, height - 2].IsPortal = true;
            levelGraph[width - 2, height - 2].IsPortal = true;
        }

		private void AddBonfires() {
			for (int x = 0; x < width; x++) {
				for (int y = 0; y < height; y++) {
					if ((x + ((bonfireFrequency / 2) * y)) % bonfireFrequency == bonfireOffset) {
                        GraphNode node = levelGraph[x, y];
                        node.IsBonfire = !node.IsPortal;
					}
				}
			}
		}

        private void AddObjects() {
            float remainingTiles = 0;
            for (int x = 0; x < width; x++) {
                for (int y = 0; y < height; y++) {
                    GraphNode node = levelGraph[x, y];
                    if (!node.IsPortal && !node.IsBonfire) remainingTiles++;
                }
            }
            int mirrorInterval = (int)(remainingTiles / numMirrors);
            int chestInterval = (int)((remainingTiles - 1) / numChests);

            int currentMirrorStep = 1;
            int currentChestStep = 1;
            bool hasPlacedMirror = false;
            bool hasPlacedChest = false;
            int totalMirrors = 0;
            int totalChests = 0;

            for (int x = 0; x < width; x++) {
                for (int y = 0; y < height; y++) {
                    GraphNode node = levelGraph[x, y];
                    if (!node.IsPortal && !node.IsBonfire) {
                        if (currentMirrorStep > mirrorInterval && hasPlacedMirror) {
                            hasPlacedMirror = false;
                            currentMirrorStep = 1;
                        }
                        if (currentChestStep > chestInterval && hasPlacedChest) {
                            hasPlacedChest = false;
                            currentChestStep = 1;
                        }
                        float mirrorProbability = (float)currentMirrorStep / (float)mirrorInterval;
                        float chestProbability = (float)currentChestStep / (float)chestInterval;
                        if (Random.Range(0f, 1f) < mirrorProbability && !hasPlacedMirror && totalMirrors < numMirrors) {
                            node.IsMirror = true;
                            hasPlacedMirror = true;
                            totalMirrors++;
                        } else if (Random.Range(0f, 1f) < chestProbability && !hasPlacedChest && totalChests < numChests) {
                            node.IsChest = true;
                            hasPlacedChest = true;
                            totalChests++;
                        } else {
                            node.IsTorch = true;
                        }
                        currentMirrorStep++;
                        currentChestStep++;
                    }
                }
            }
        }

        private void RandomizeTileTypes() {
            for (int x = 0; x < width; x++) {
                for (int y = 0; y < height; y++) {
                    levelGraph[x, y].RandomizedType = Random.Range(1, 3);
                }
            }
        }

		public int[,,] SerializeLevelGraph() {
			int[,,] output = new int[TOTAL_LAYERS, width, height];
			for (int x = 0; x < width; x++) {
				for (int y = 0; y < height; y++) {
					output[CHUNK_INDEX_LAYER, x, y] = levelGraph[x, y].GetLevelChunkIndex();
                    output[CHUNK_OBJECT_TYPE_LAYER, x, y] = levelGraph[x, y].GetChunkType();
                    output[CHUNK_RANDOMIZED_TYPE_LAYER, x, y] = levelGraph[x, y].RandomizedType;
				}
			}
			return output;
		}

		public GraphNode GetDestinationNode(GraphWall wall) {
			GraphNode sourceNode = wall.SourceNode;
			Vector2Int coordinates = sourceNode.Coordinates;
			switch (wall.Direction) {
			case WallDirection.NORTH:
				return levelGraph[coordinates.x, coordinates.y + 1];
			case WallDirection.SOUTH:
				return levelGraph[coordinates.x, coordinates.y - 1];
			case WallDirection.EAST:
				return levelGraph[coordinates.x + 1, coordinates.y];
			case WallDirection.WEST:
				return levelGraph[coordinates.x - 1, coordinates.y];
			default:
				throw new KeyNotFoundException("Destination node called with illegal direction enum: " + wall.Direction);
			}
		}
	}
}
