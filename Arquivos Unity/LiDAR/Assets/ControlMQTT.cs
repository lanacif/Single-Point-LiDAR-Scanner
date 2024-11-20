using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using uPLibrary.Networking.M2Mqtt;
using uPLibrary.Networking.M2Mqtt.Messages;
using System.Text;
using System.Threading;
using System.Globalization;


public class ControlMQTT : MonoBehaviour
{
    public GameObject SpherePrefab;
    static MqttClient client;
    private SynchronizationContext mainThreadContext;
    
    // Start is called before the first frame update
    void Start()
    {
        mainThreadContext = SynchronizationContext.Current;
        // Cria uma instância do cliente MQTT
        client = new MqttClient("192.168.18.3");
        // Associa o evento de recebimento de mensagem
        client.MqttMsgPublishReceived += Client_MqttMsgPublishReceived;
        // Gera um ID único para a conexão MQTT
        client.Connect("Unity");
        // Inscreve no tópico LiDAR
        client.Subscribe(new string[] { "LiDAR" }, new byte[] { MqttMsgBase.QOS_LEVEL_AT_MOST_ONCE });
        // Publica string de início
        client.Publish("Control", Encoding.UTF8.GetBytes("start"), MqttMsgBase.QOS_LEVEL_EXACTLY_ONCE, false);
    }

    void Client_MqttMsgPublishReceived(object sender, MqttMsgPublishEventArgs e)
    {
        // Uma nova mensagem foi recebida no tópico inscrito
        string message = Encoding.UTF8.GetString(e.Message);
        string[] parts = message.Split(','); 

        float ro = float.Parse(parts[0]);
        float phi = float.Parse(parts[1], CultureInfo.GetCultureInfo("en-US"));
        float theta = float.Parse(parts[2], CultureInfo.GetCultureInfo("en-US"));

        //ro = 800;

        float x = ro*Mathf.Cos(Mathf.Deg2Rad*theta);
        float y = ro*Mathf.Sin(Mathf.Deg2Rad * theta)*Mathf.Sin(Mathf.Deg2Rad*phi);
        float z = ro*Mathf.Sin(Mathf.Deg2Rad*theta)*Mathf.Cos(Mathf.Deg2Rad*phi);

        //Debug.Log("ro: " + ro + " fi: " + phi + " teta: " + theta);
        //Debug.Log("Valores: " + x + " - " + y + " - " + z);

        //print("Nova mensagem: " + message);   
        //print(phi);
        
        // Volta para a thread principal do Unity e cria o objeto na posição desejada
        mainThreadContext.Post(_ =>
        {
            // Rotaciona o sensor virtual
            transform.rotation = Quaternion.Euler(-theta, -90-phi, 90f);
            // Spawna esfera
            //Instantiate(SpherePrefab, new Vector3(Random.Range(6, 30), Random.Range(6, 30), Random.Range(6, 30)), Quaternion.identity);
            Instantiate(SpherePrefab, new Vector3(-z, -x, -y), Quaternion.identity);
        }, null);
    }

    void OnApplicationQuit()
    {
        // Cria o AutoResetEvent para sincronização
        AutoResetEvent confirmationEvent = new AutoResetEvent(false);

        // Registra o evento MqttMsgPublished para receber a confirmação
        client.MqttMsgPublished += (sender, e) =>
        {
            if (e.IsPublished)
            {
                // Confirmação publicada com sucesso
                confirmationEvent.Set();
            }
        };

        // Publica string de stop
        client.Publish("Control", Encoding.UTF8.GetBytes("stop"), MqttMsgBase.QOS_LEVEL_EXACTLY_ONCE, false);

        confirmationEvent.WaitOne(5000);
 
        client.Disconnect();
    }
}