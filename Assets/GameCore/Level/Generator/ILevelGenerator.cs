public interface ILevelGenerator
{
    LevelData Generate(DifficultyType difficulty, int seed = -1);
}
