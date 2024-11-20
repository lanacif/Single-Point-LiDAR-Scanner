using UnityEngine;
using UnityEngine.UI; // Importante para acessar a classe Button

public class ButtonController : MonoBehaviour
{
    public Button myButton; // Referência ao seu botão, definida via Inspector

    void Start()
    {
        // Opcional: Adiciona o método ButtonClicked como listener do evento de clique do botão
        myButton.onClick.AddListener(ButtonClicked);
    }

    // Método que é chamado quando o botão é clicado
    public void ButtonClicked()
    {
        // Desativa o botão
        myButton.interactable = false;
    }

    // Se precisar reativar o botão em algum ponto, você pode chamar este método
    public void EnableButton()
    {
        myButton.interactable = true;
    }
}
