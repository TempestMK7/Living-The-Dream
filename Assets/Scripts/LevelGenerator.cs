using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Com.Tempest.Nightmare {

	public class LevelGenerator {

		public enum WallDirection {
			NORTH,
			SOUTH,
			EAST,
			WEST
		}

		public class GraphNode {
	
			public List<WallDirection> WallsRemaining { get; set; }

			public int TimesVisited { get; set; }

			public Vector2Int Coordinates { get; set; }

			public GraphNode(Vector2Int coordinates) {
				WallsRemaining = new List<WallDirection>();
				TimesVisited = 0;
				Coordinates = coordinates;
			}

			public int GetLevelChunkIndex() {
				if (WallsRemaining.Count == 0) {
					return 0;
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
		}

		public class GraphWall {
	
			public GraphNode SourceNode { get; }

			public WallDirection Direction { get; }

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
		private GraphNode[,] levelGraph;

		public LevelGenerator(int width, int height) {
			this.width = width;
			this.height = height;
			InitializeLevelGraph();
			BuildLevelGraph();
			RestoreOuterWallsToLevelGraph();
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

		public int[,] SerializeLevelGraph() {
			int[,] output = new int[width, height];
			for (int x = 0; x < width; x++) {
				for (int y = 0; y < height; y++) {
					output[x, y] = levelGraph[x, y].GetLevelChunkIndex();
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
