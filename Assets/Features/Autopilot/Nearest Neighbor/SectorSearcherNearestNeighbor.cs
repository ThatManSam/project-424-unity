﻿using System.Collections.Generic;
using UnityEngine;

namespace Perrinn424.AutopilotSystem
{
    public class SectorSearcherNearestNeighbor : INearestNeighbourSearcher
    {
        private readonly IReadOnlyList<Vector3> path;
        private BruteForceSearcher bruteForceSearcher;
        private readonly int sectorSize;

        public int SectorSize => sectorSize;

        private readonly int maxDepth;
        private readonly float tolerance;

        public SectorSearcherNearestNeighbor(IReadOnlyList<Vector3> path, int maxDepth, float tolerance)
        {
            this.path = path;
            bruteForceSearcher = new BruteForceSearcher(path);
            sectorSize = Mathf.CeilToInt(Mathf.Sqrt(path.Count));
            this.maxDepth = maxDepth;
            this.tolerance = tolerance;
        }

        public void Search(Vector3 position)
        {
            (int nnSector, int searchedSectorSize) = SearchNearestSector(position, sectorSize, tolerance, 0);

            int securityMargin = 3;
            int from = nnSector - Mathf.CeilToInt(searchedSectorSize / 2f) - securityMargin * searchedSectorSize;
            int count = sectorSize + 2 * securityMargin * searchedSectorSize; //searchSize/2 to the left, searchSize/2 to the right, and +1 because of the middle point
            int step = 1;

            (int index, float distance) = bruteForceSearcher.SearchInBoundaries(position, from, count, step);

            Index = index;
            Distance = distance;
            Position = path[Index];
        }

        private (int, int) SearchNearestSector(Vector3 position, int searchSize, float tolerance, int depth)
        {
            (int nearestSector, float distance) = bruteForceSearcher.SearchInBoundaries(position, 0, path.Count, searchSize);

            if (distance > tolerance && depth <= maxDepth)
            {
                return SearchNearestSector(position, (searchSize / 2) + 1, tolerance, depth + 1);
            }

            return (nearestSector, searchSize);
        }

        public int Index { get; private set; }

        public float Distance { get; private set; }

        public Vector3 Position { get; private set; }
    } 
}