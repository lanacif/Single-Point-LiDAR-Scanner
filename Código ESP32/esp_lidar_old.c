#include "freertos/FreeRTOS.h"
#include "freertos/task.h"
#include "esp_system.h"
#include "driver/uart.h"
#include "string.h"
#include "driver/gpio.h"
#include "nvs_flash.h"
#include "esp_wifi.h"
#include "esp_netif.h"
#include "mqtt_client.h"
#include "protocol_examples_common.h"

//Minimum buffer size for RX
static const int RX_BUF_SIZE = 129;
//Size of response from TF LUNA in bytes
static const int RX_TFL_BUF_SIZE = 9;

#define TXD_PIN (GPIO_NUM_4)
#define RXD_PIN (GPIO_NUM_5)

void init_uart(void) {
    const uart_config_t uart_config = {
        .baud_rate = 115200,
        .data_bits = UART_DATA_8_BITS,
        .parity = UART_PARITY_DISABLE,
        .stop_bits = UART_STOP_BITS_1,
        .flow_ctrl = UART_HW_FLOWCTRL_DISABLE,
        .source_clk = UART_SCLK_APB,
    };
    // We won't use a buffer for sending data.
    uart_driver_install(UART_NUM_1, RX_BUF_SIZE * 2, 0, 0, NULL, 0);
    uart_param_config(UART_NUM_1, &uart_config);
    uart_set_pin(UART_NUM_1, TXD_PIN, RXD_PIN, UART_PIN_NO_CHANGE, UART_PIN_NO_CHANGE);
}

esp_mqtt_client_handle_t init_mqtt(void)
{
    esp_mqtt_client_config_t mqtt_cfg = {
        .broker.address.uri = "mqtt://192.168.18.3",
        .credentials.client_id = "ESP32",
    };

    esp_mqtt_client_handle_t client = esp_mqtt_client_init(&mqtt_cfg);
    esp_mqtt_client_start(client);

    return client;
}

void app_main(void)
{
    init_uart();
    nvs_flash_init();
    esp_netif_init();
    esp_event_loop_create_default();
    //This helper function configures Wi-Fi or Ethernet, as selected in menuconfig.
    example_connect();
    esp_mqtt_client_handle_t mqtt_client = init_mqtt();

    //Trigger command (single measure)
    const uint8_t trigger_command[] = {0x5A, 0x04, 0x04, 0x00};
    //Response var
    uint8_t* response_tfl = (uint8_t*) malloc(RX_TFL_BUF_SIZE);
    memset(response_tfl, 1, RX_TFL_BUF_SIZE);
    
    //Frequency = 0 (trigger mode)
    const uint8_t enable_trigger[] = {0x5A, 0x06, 0x03, 0x00, 0x00, 0x00};

    //Force trigger mode
    while((int)((response_tfl[4] << 8) | response_tfl[3]) != 0){
        uart_write_bytes(UART_NUM_1, (const char*) enable_trigger, sizeof(enable_trigger));
        //Response 
        uart_read_bytes(UART_NUM_1, response_tfl, RX_TFL_BUF_SIZE, 1000 / portTICK_PERIOD_MS);
    }

    //Subscribe to the "LiDAR" topic
    esp_mqtt_client_subscribe(mqtt_client, "LiDAR", 0);

    //Wait Unity


    while (1) {
        uart_write_bytes(UART_NUM_1, (const char*) trigger_command, sizeof(trigger_command));
        
        const int rxBytes = uart_read_bytes(UART_NUM_1, response_tfl, RX_TFL_BUF_SIZE, 1000 / portTICK_PERIOD_MS);
        
        if (rxBytes > 0) {
            //printf("Distância: %d cm \n", (int)((response_tfl[3] << 8) | response_tfl[2]));
            printf("%d\n", (int)((response_tfl[3] << 8) | response_tfl[2]));
        }
        
        char* str_sensor_data = (char*)malloc(6);
        sprintf(str_sensor_data, "%d", (int)((response_tfl[3] << 8) | response_tfl[2]));

        esp_mqtt_client_publish(mqtt_client, "LiDAR", str_sensor_data, 0, 0, 0);
        vTaskDelay(50 / portTICK_PERIOD_MS);
    }

}