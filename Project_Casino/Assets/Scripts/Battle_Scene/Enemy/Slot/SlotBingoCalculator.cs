using UnityEngine;

public class SlotBingoCalculator : MonoBehaviour
{
    public int[,] grid = new int[3, 3];

    public int CalculateDamage(int[,] currentGrid)
    {
        int bingoCount = 0;

        for (int y = 0; y < 3; y++)
        {
            if (currentGrid[0, y] == currentGrid[1, y] && currentGrid[1, y] == currentGrid[2, y])
                bingoCount++;
        }

        for (int x = 0; x < 3; x++)
        {
            if (currentGrid[x, 0] == currentGrid[x, 1] && currentGrid[x, 1] == currentGrid[x, 2])
                bingoCount++;
        }

        if (currentGrid[0, 0] == currentGrid[1, 1] && currentGrid[1, 1] == currentGrid[2, 2])
            bingoCount++;

        if (currentGrid[0, 2] == currentGrid[1, 1] && currentGrid[1, 1] == currentGrid[2, 0])
            bingoCount++;

        int totalDamage = bingoCount * 10;

        Debug.Log($"총 {bingoCount}줄 빙고 플레이어에게 {totalDamage} 데미지");
        return totalDamage;
    }
}
