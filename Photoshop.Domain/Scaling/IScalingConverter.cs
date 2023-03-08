namespace Photoshop.Domain.Scaling;

public interface IScalingConverter
{
    ImageData Convert(ImageData source, ScalingType scalingType, int newWidth, int newHeight);

    void SetB(float value);

    void SetC(float value);
}