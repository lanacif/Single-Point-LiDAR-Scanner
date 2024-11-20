using UnityEngine;
using UnityEngine.UI;

public class Swiper : MonoBehaviour
{
    public Camera cam;

    public Slider sliderX;
    public Slider sliderY;

    public float sensibilidadeRotacao = 0.1f;
    public float sensibilidadePinca = 0.1f;

    public float sensibilidadeSlider = 0.00001f; //invertida

    private float larguraMinControleCamera = 0.20f; // 20% da largura da tela da esquerda
    private float larguraMaxControleCamera = 0.80f; // 80% da largura da tela da direita

    private Vector2 toqueAnterior1;
    private Vector2 toqueAnterior2;

    void Update()
    {
        if (Input.touchCount == 2)
        {
            Touch toque1 = Input.GetTouch(0);
            Touch toque2 = Input.GetTouch(1);

            toqueAnterior1 = toque1.position - toque1.deltaPosition;
            toqueAnterior2 = toque2.position - toque2.deltaPosition;

            float distanciaAnterior = (toqueAnterior1 - toqueAnterior2).magnitude;
            float distanciaAtual = (toque1.position - toque2.position).magnitude;

            float diferenca = distanciaAtual - distanciaAnterior;

            cam.transform.position += cam.transform.forward * diferenca * sensibilidadePinca;
        }

        if (Input.touchCount == 1)
        {
            Touch toque = Input.GetTouch(0);

            if (toque.phase == TouchPhase.Moved && toque.position.x > Screen.width * larguraMinControleCamera && toque.position.x < Screen.width * larguraMaxControleCamera)
            //if (toque.phase == TouchPhase.Moved)
            {
                // Rotação em torno do eixo Y (horizontal).
                cam.transform.Rotate(0f, -toque.deltaPosition.x * sensibilidadeRotacao, 0f, Space.World);
                
                // Rotação em torno do eixo X (vertical), utilizando o método 'RotateAround' para evitar a inversão dos eixos após 90 graus de rotação.
                Vector3 pontoDeRotacao = cam.transform.position - cam.transform.forward * 10f; // Ponto um pouco à frente da câmera para uma rotação suave.
                cam.transform.RotateAround(pontoDeRotacao, -cam.transform.right, -toque.deltaPosition.y * sensibilidadeRotacao);
            }
            /*
            // Área esquerda para controle do sliderX
            if (toque.phase == TouchPhase.Moved && toque.position.x <= Screen.width * larguraMinControleCamera)
            {
                // Atualiza o valor do sliderX proporcionalmente ao movimento vertical
                sliderX.value += toque.deltaPosition.y * sensibilidadeSlider;
            }

            // Área direita para controle do sliderY
            if (toque.phase == TouchPhase.Moved && toque.position.x >= Screen.width * larguraMaxControleCamera)
            {
                // Atualiza o valor do sliderY proporcionalmente ao movimento vertical
                sliderY.value += toque.deltaPosition.y * sensibilidadeSlider;
            }
            */
        }
    }
}
