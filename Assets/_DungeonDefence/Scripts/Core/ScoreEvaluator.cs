using UnityEngine;

public static class ScoreEvaluator
{
    private const int BASE_POINTS_PER_WAVE = 200;
    private const int POINTS_PER_KILL = 10;
    private const int POINTS_PER_GOLD_TOTAL = 1;

    public static int CalculateScore(int waves, int kills, int gold)
    {
        int score = 0;

        
        for (int i = 1; i <= waves; i++)
        {
            score += BASE_POINTS_PER_WAVE + (i * 20); 
        }

        
        if (waves >= 10) score += 1000;
        if (waves >= 20) score += 3000;
        if (waves >= 30) score += 10000; 

        score += kills * POINTS_PER_KILL;
        score += gold * POINTS_PER_GOLD_TOTAL;

        return score;
    }

    public static int CalculatePerkPoints(int score)
    {
        return Mathf.FloorToInt(score / 100f);
    }
}



