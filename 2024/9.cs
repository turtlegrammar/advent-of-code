using static Advent.Extensions;
using System.Linq;
using System.Collections.Generic;
using System.Security.Cryptography.X509Certificates;

using Advent; namespace Advent2024;

public static class Day9
{
    public static (long, long) Run(string file)
    {
        var haveMoved = new HashSet<int>();
        var input = File.ReadAllText(file).ToCharArray().Select(c => c - '0').ToArray();

        var diskMap = Unpack(input);

        var defrag1 = Defragment(diskMap);
        var defrag2 = Defragment2(diskMap);

        return (
            Checksum(defrag1), 
            Checksum(defrag2)
        );

        long Checksum(List<int> diskMap) =>
            diskMap.Select((x, i) => x == -1 ? 0 : i * (long)x).Sum();

        List<int> Defragment(List<int> diskMapInput)
        {
            var diskMap = diskMapInput.Select(x => x).ToList();
            int left = 0;
            int right = diskMap.Count - 1;

            while (left < right)
            {
                while (diskMap[left] != -1 && left < right)
                    left++;
                while (diskMap[right] == -1 && left < right)
                    right--;
                if (diskMap[left] == -1 && diskMap[right] != -1 && left < right)
                {
                    diskMap[left] = diskMap[right];
                    diskMap[right] = -1;
                }
            }

            return diskMap;
        }

        List<int> Defragment2(List<int> diskMapInput)
        {
            var diskMap = diskMapInput.Select(x => x).ToList();

            int right = diskMap.Count - 1;
            int endOfRight = right;
            while (right > 0)
            {
                int left = 0;
                int endOfLeft = 0;

                // Right Block
                while (diskMap[right] == -1 && right > 0)
                    right--;
                endOfRight = right;
                while (diskMap[endOfRight] == diskMap[right] && endOfRight > 0)
                    endOfRight--;

                if (right == 0 || endOfRight == 0)
                    break;

                while ((endOfLeft - left) < (right - endOfRight) && endOfLeft <= endOfRight + 1)
                {
                    left = endOfLeft;
                    // 1. Find free space on left
                    while (left < diskMap.Count && diskMap[left] != -1)
                        left++;
                    endOfLeft = left;
                    if (endOfLeft == diskMap.Count)
                        break;
                    while (endOfLeft < diskMap.Count && diskMap[endOfLeft] == -1)
                        endOfLeft++;
                }

                if ((endOfLeft - left) >= (right - endOfRight) && endOfLeft <= endOfRight + 1)
                {
                    var init = diskMap[right];
                    for (int j = 0; j < (right - endOfRight); j++)
                    {
                        diskMap[left+j] = diskMap[right - j];
                        diskMap[right - j] = -1;
                    }
                }

                right = endOfRight;
            }

            return diskMap;
        }

        List<int> Unpack(int[] input)
        {
            var result = new List<int>();
            for (int i = 0; i < input.Length; i += 2)
            {
                var fileId = i / 2;
                var fileBlocks = input[i];
                var freeSpace = i + 1 < input.Length ? input[i + 1] : 0;
                for (int j = 0; j < fileBlocks; j++)
                    result.Add(fileId);
                for (int j = 0; j < freeSpace; j++)
                    result.Add(-1);
            }
            return result;
        }
    }
}