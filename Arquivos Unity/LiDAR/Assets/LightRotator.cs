using UnityEngine;
using UnityEngine.UI;

public class LightRotator : MonoBehaviour
{
    public Slider sliderX;
    public Slider sliderY;
    public Light  lightToControl;

    private float rotationX;
    private float rotationY;

    void Start()
    {
        // Assumindo que seus sliders começam com um valor que representa a rotação inicial da câmera.
        rotationX = lightToControl.transform.eulerAngles.x;
        rotationY = lightToControl.transform.eulerAngles.y;

        // Adiciona listeners para os eventos de mudança de valor dos sliders
        sliderX.onValueChanged.AddListener(HandleSliderXChanged);
        sliderY.onValueChanged.AddListener(HandleSliderYChanged);
    }

    private void HandleSliderXChanged(float value)
    {
        rotationX = value;
        UpdatelightToControleraRotation();
    }

    private void HandleSliderYChanged(float value)
    {
        rotationY = value;
        UpdatelightToControleraRotation();
    }

    private void UpdatelightToControleraRotation()
    {
        // Configura a nova rotação da câmera baseando-se nos valores dos sliders
        lightToControl.transform.rotation = Quaternion.Euler(rotationX, rotationY, 0f);
    }

    void OnDestroy()
    {
        // Lembre-se de remover os listeners quando o script for destruído
        sliderX.onValueChanged.RemoveListener(HandleSliderXChanged);
        sliderY.onValueChanged.RemoveListener(HandleSliderYChanged);
    }
}
