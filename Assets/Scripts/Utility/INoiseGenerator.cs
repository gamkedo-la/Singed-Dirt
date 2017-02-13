public interface INoiseGenerator {
    float GetNoise(float x, float y);
    float GetNoise(float x, float y, float z);
    void SetSeed(int seed);
}
