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
			public bool IsBonfire { get; set; }
			public bool IsTorch { get; set; }

			public GraphNode(Vector2Int coordinates) {
				WallsRemaining = new List<WallDirection>();
				TimesVisited = 0;
				Coordinates = coordinates;
			}

			public int GetLevelChunkIndex() {
				int returnValue = 0;
				if (WallsRemaining.Count == 0) {
					returnValue = 11;
				}
				if (WallsRemaining.Count == 1) {
					switch (WallsRemaining[0]) {
					case WallDirection.NORTH:
						returnValue = 1;
						break;
					case WallDirection.EAST:
						returnValue = 2;
						break;
					case WallDirection.SOUTH:
						returnValue = 3;
						break;
					case WallDirection.WEST:
						returnValue = 4;
						break;
					default:
						throw new KeyNotFoundException("Illegal wall direction: " + WallsRemaining[0]);
					}
				}
				if (WallsRemaining.Count == 2) {
					if (WallsRemaining.Contains(WallDirection.NORTH) && WallsRemaining.Contains(WallDirection.EAST)) {
						returnValue = 5;
					}
					if (WallsRemaining.Contains(WallDirection.NORTH) && WallsRemaining.Contains(WallDirection.SOUTH)) {
						returnValue = 6;
					}
					if (WallsRemaining.Contains(WallDirection.NORTH) && WallsRemaining.Contains(WallDirection.WEST)) {
						returnValue = 7;
					}
					if (WallsRemaining.Contains(WallDirection.EAST) && WallsRemaining.Contains(WallDirection.SOUTH)) {
						returnValue = 8;
					}
					if (WallsRemaining.Contains(WallDirection.EAST) && WallsRemaining.Contains(WallDirection.WEST)) {
						returnValue = 9;
					}
					if (WallsRemaining.Contains(WallDirection.SOUTH) && WallsRemaining.Contains(WallDirection.WEST)) {
						returnValue = 10;
					}
				}
				if (returnValue == 0) {
					throw new KeyNotFoundException("I can't decide which wall index I am: " + WallsRemaining.Count);
				}
				if (IsBonfire) {
					returnValue *= -1;
				} else if (IsTorch) {
					returnValue += 100;
				}
				return returnValue;
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
		private int bonfireFrequency;
		private float torchProbability;
		private Random random;
		private GraphNode[,] levelGraph;

		public LevelGenerator(int width, int height, int bonfireFrequency, float torchProbability) {
			this.width = width;
			this.height = height;
			this.bonfireFrequency = bonfireFrequency;
			this.torchProbability = torchProbability;
			random = new Random();
			InitializeLevelGraph();
			BuildLevelGraph();
			RestoreOuterWallsToLevelGraph();
			AddBonfires();
			AddTorches();
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

		private void AddBonfires() {
			for (int x = 0; x < width; x++) {
				for (int y = 0; y < height; y++) {
					if (!IsCorner(x, y) && ((x + ((bonfireFrequency / 2) * y)) % bonfireFrequency == 0)) {
						levelGraph[x, y].IsBonfire = true;
					}
				}
			}
		}

		private void AddTorches() {
			for (int x = 0; x < width; x++) {
				for (int y = 0; y < height; y++) {
					levelGraph[x, y].IsTorch = !IsCorner(x, y) && !levelGraph[x, y].IsBonfire && Random.Range(0f, 1f) < torchProbability;
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

		private bool IsCorner(int x, int y) {
			return (x == 0 || x == width - 1) && (y == 0 || y == height - 1);
		}
	}
}
