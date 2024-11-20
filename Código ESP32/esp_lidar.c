//#include "esp_heap_trace.h"
#include <stdio.h>
#include <freertos/FreeRTOS.h>
#include <esp_system.h>
#include <esp_err.h>
#include <driver/i2c.h>
#include "string.h"
#include "driver/gpio.h"
#include "nvs_flash.h"
#include "esp_wifi.h"
#include "esp_netif.h"
#include "mqtt_client.h"
#include "driver/ledc.h"
#include "protocol_examples_common.h"
#include <math.h>

// Stepper mode (1 FULL-MODE 0 HALF-MODE)
#define STEPPER_MODE 1

// Stepper initial position
#define STEPPER_INITIAL_POSITION 73

// Stepper pins
#define MOTOR_PIN_1 GPIO_NUM_19
#define MOTOR_PIN_2 GPIO_NUM_18
#define MOTOR_PIN_3 GPIO_NUM_5
#define MOTOR_PIN_4 GPIO_NUM_17

// Switch pin
#define LIMIT_SWITCH_GPIO    25

// Servo speed
#define SERVO_DUTY_SPEED 4750 //4800

// SDA, SCL PINS
#define SDA_PIN GPIO_NUM_21
#define SCL_PIN GPIO_NUM_22

// TF-Luna slave address
#define TF_LUNA_I2C_ADDR 0x10

// TF-Luna used registers
#define TF_LUNA_DIST_LOW_REG 0x00
#define TF_LUNA_TRIGGER_REG 0x24

// I2C address of AS5600 module
#define AS5600_I2C_ADDR 0x36
// AS5600 used register
#define AS5600_ANGLE_HIGH_REG 0x0E

// Timer do PWM, High speed, gpio PWM, Canal
#define LEDC_HS_TIMER          LEDC_TIMER_0
#define LEDC_HS_MODE           LEDC_HIGH_SPEED_MODE
#define LEDC_HS_CH0_GPIO       (12)
#define LEDC_HS_CH0_CHANNEL    LEDC_CHANNEL_0

#if STEPPER_MODE
    // Full mode
    const int steps[][4] = {
        {1, 1, 0, 0},
        {0, 1, 1, 0},
        {0, 0, 1, 1},
        {1, 0, 0, 1},
        {1, 1, 0, 0},
        {0, 1, 1, 0},
        {0, 0, 1, 1},
        {1, 0, 0, 1},
    };
    //const float step_angle = 0.18;
    const float step_angle  = 5.625/64*2;
#else
    // Half-mode
    const int steps[][4] = {
        {1, 0, 0, 0},
        {1, 1, 0, 0},
        {0, 1, 0, 0},
        {0, 1, 1, 0},
        {0, 0, 1, 0},
        {0, 0, 1, 1},
        {0, 0, 0, 1},
        {1, 0, 0, 1},
    };
    //const float step_angle  = 0.09;
    const float step_angle  = 5.625/64;
#endif


int current_step = 0;

bool operating = false;

static void mqtt_event_handler(void *handler_args, esp_event_base_t base, int32_t event_id, void *event_data)
{
    esp_mqtt_event_handle_t event = event_data;
    char *clean_string = malloc(6);
    switch ((esp_mqtt_event_id_t)event_id) {
        case MQTT_EVENT_DATA:
            if (strcmp(event->topic, "Control") == 0) {
                snprintf(clean_string, 6, "%.*s", event->data_len, event->data);
                if (strcmp(clean_string, "start") == 0){
                    operating = true;
                }
                if (strcmp(clean_string, "stop") == 0){
                    operating = false;
                }
            }
            break;

            default:
                break;
    }
    free(clean_string);
}

esp_mqtt_client_handle_t init_mqtt(void)
{
    esp_mqtt_client_config_t mqtt_cfg = {
        .broker.address.uri = "mqtt://192.168.18.3",
        .credentials.client_id = "ESP32",
    };

    esp_mqtt_client_handle_t client = esp_mqtt_client_init(&mqtt_cfg);
    esp_mqtt_client_register_event(client, ESP_EVENT_ANY_ID, mqtt_event_handler, NULL);
    esp_mqtt_client_start(client);

    return client;
}

// Function to initialize the I2C master   
static void i2c_master_init(void)
{
    i2c_config_t conf = {
        .mode = I2C_MODE_MASTER,
        .sda_io_num = SDA_PIN,
        .sda_pullup_en = GPIO_PULLUP_ENABLE,
        .scl_io_num = SCL_PIN,
        .scl_pullup_en = GPIO_PULLUP_ENABLE,
        .master.clk_speed = 400000, 
    };
    i2c_param_config(I2C_NUM_0, &conf);
    i2c_driver_install(I2C_NUM_0, conf.mode, 0, 0, 0);
}

// Function to read the distance from the TF-Luna sensor
static uint16_t tf_luna_read_distance()
{
    uint8_t dist_low = 0;
    uint8_t dist_high = 0;

    i2c_cmd_handle_t cmd = i2c_cmd_link_create();
    i2c_master_start(cmd);
    i2c_master_write_byte(cmd, (TF_LUNA_I2C_ADDR << 1) | I2C_MASTER_WRITE, true);
    i2c_master_write_byte(cmd, TF_LUNA_DIST_LOW_REG, true);
    i2c_master_start(cmd);
    i2c_master_write_byte(cmd, (TF_LUNA_I2C_ADDR << 1) | I2C_MASTER_READ, true);
    i2c_master_read_byte(cmd, &dist_low, I2C_MASTER_ACK);
    i2c_master_read_byte(cmd, &dist_high, I2C_MASTER_NACK);
    i2c_master_stop(cmd);
    i2c_master_cmd_begin(I2C_NUM_0, cmd, 1000 / portTICK_PERIOD_MS);
    i2c_cmd_link_delete(cmd);

    return ((uint16_t)dist_high << 8) | dist_low;
}

// Function to read the position from the AS5600 module
static uint16_t as5600_read_position()
{
    uint8_t angle_low = 0;
    uint8_t angle_high = 0;

    i2c_cmd_handle_t cmd = i2c_cmd_link_create();
    i2c_master_start(cmd);
    i2c_master_write_byte(cmd, (AS5600_I2C_ADDR << 1) | I2C_MASTER_WRITE, true);
    i2c_master_write_byte(cmd, AS5600_ANGLE_HIGH_REG, true);
    i2c_master_start(cmd);
    i2c_master_write_byte(cmd, (AS5600_I2C_ADDR << 1) | I2C_MASTER_READ, true);
    i2c_master_read_byte(cmd, &angle_high, I2C_MASTER_ACK);
    i2c_master_read_byte(cmd, &angle_low, I2C_MASTER_NACK);
    i2c_master_stop(cmd);
    i2c_master_cmd_begin(I2C_NUM_0, cmd, 1000 / portTICK_PERIOD_MS);
    i2c_cmd_link_delete(cmd);

    return ((uint16_t)angle_high << 8) | angle_low;
}

static void pwm_init()
{
    //Configurações do Timer que será usado para o PWM
    ledc_timer_config_t ledc_timer = {
        .duty_resolution = LEDC_TIMER_16_BIT,    //Resolução do duty
        .freq_hz = 50,                           //Frequência do PWM
        .speed_mode = LEDC_HS_MODE,              //Modo do timer
        .timer_num = LEDC_HS_TIMER,              //índice do timer
        .clk_cfg = LEDC_AUTO_CLK,                //Seleção automática da fonte de clock
    };
    // Set config do timer
    ledc_timer_config(&ledc_timer);

    //Configurações do canal que será usado para o PWM
    ledc_channel_config_t ledc_channel = {
        .channel    = LEDC_HS_CH0_CHANNEL,
        .duty       = SERVO_DUTY_SPEED,
        .gpio_num   = LEDC_HS_CH0_GPIO,
        .speed_mode = LEDC_HS_MODE,
        .hpoint     = 0,
        .timer_sel  = LEDC_HS_TIMER,
        .flags.output_invert = 0
    };

    //Set do canal do PWM
    ledc_channel_config(&ledc_channel);
}

void motor_step(int step)
{
    gpio_set_level(MOTOR_PIN_1, steps[step][0]);
    gpio_set_level(MOTOR_PIN_2, steps[step][1]);
    gpio_set_level(MOTOR_PIN_3, steps[step][2]);
    gpio_set_level(MOTOR_PIN_4, steps[step][3]);
}

// Function to move the LiDAR to the initial position
float move_to_initial_position()
{
    int i = 0;
    float stepper_motor_angle_deg = 0;
    
    // Positions at 0 degrees
    while(gpio_get_level(LIMIT_SWITCH_GPIO)){
        motor_step(current_step);
        // 10ms between each step.
        vTaskDelay(pdMS_TO_TICKS(10));
        if(current_step == 0)
            current_step = 7;
        else
            current_step--;
    }
    
    // Positions at defined degrees.
    while(i < round(STEPPER_INITIAL_POSITION/step_angle)){
        motor_step(current_step);
        i++;
        // 10ms between each step.
        vTaskDelay(pdMS_TO_TICKS(10));
        if(current_step == 7)
            current_step = 0;
        else
            current_step++;

        stepper_motor_angle_deg += step_angle;
    }

    return stepper_motor_angle_deg;
}

void app_main()
{
    bool end = false;
    bool imminent_turn = false;
    float servo_angle_deg;
    uint16_t TF_Luna_distance;

    // Configure the limit switch pin as an input
    gpio_config_t config;
    config.pin_bit_mask = (1ULL << LIMIT_SWITCH_GPIO);
    config.mode = GPIO_MODE_INPUT;
    config.pull_up_en = GPIO_PULLUP_ENABLE;
    config.pull_down_en = GPIO_PULLDOWN_DISABLE;
    gpio_config(&config);

    // Configure stepper pins
    esp_rom_gpio_pad_select_gpio(MOTOR_PIN_1);
    esp_rom_gpio_pad_select_gpio(MOTOR_PIN_2);
    esp_rom_gpio_pad_select_gpio(MOTOR_PIN_3);
    esp_rom_gpio_pad_select_gpio(MOTOR_PIN_4);
    gpio_set_direction(MOTOR_PIN_1, GPIO_MODE_OUTPUT);
    gpio_set_direction(MOTOR_PIN_2, GPIO_MODE_OUTPUT);
    gpio_set_direction(MOTOR_PIN_3, GPIO_MODE_OUTPUT);
    gpio_set_direction(MOTOR_PIN_4, GPIO_MODE_OUTPUT);
    
    nvs_flash_init();
    esp_netif_init();
    esp_event_loop_create_default();
    // This helper function configures Wi-Fi or Ethernet, as selected in menuconfig.
    example_connect();
    esp_mqtt_client_handle_t mqtt_client = init_mqtt();
    i2c_master_init();

    // Subscribe to the "Control" topic
    // esp_mqtt_client_subscribe(mqtt_client, "Control", 2);

    // Subscribe to the "LiDAR" topic
    //esp_mqtt_client_subscribe(mqtt_client, "LiDAR", 0);

    float stepper_motor_angle_deg = move_to_initial_position();

    pwm_init();

    vTaskDelay(pdMS_TO_TICKS(500));


    while(!end){
        servo_angle_deg = (as5600_read_position() / 4096.0) * 360.0;
        // Se a pos. ang. atual for maior que 350 graus
        if(servo_angle_deg > 358)
            imminent_turn = true;
        else if(imminent_turn == true && servo_angle_deg < 3){
            // Para o servo da base
            //ledc_set_duty(LEDC_HS_MODE, LEDC_HS_CH0_CHANNEL, 0);
            //ledc_update_duty(LEDC_HS_MODE, LEDC_HS_CH0_CHANNEL);
            
            // Passo 
            motor_step(current_step);
            
            //Lembrar de 10ms entre cada passo
            if(current_step == 7)
                current_step = 0;
            else
                current_step++;
            
            stepper_motor_angle_deg += step_angle;

            //vTaskDelay(pdMS_TO_TICKS(200));
            // Ativa servo da base
            //ledc_set_duty(LEDC_HS_MODE, LEDC_HS_CH0_CHANNEL, SERVO_DUTY_SPEED);
            //ledc_update_duty(LEDC_HS_MODE, LEDC_HS_CH0_CHANNEL);
            imminent_turn = false;
        }

        TF_Luna_distance = tf_luna_read_distance();

        // Yaw
        servo_angle_deg = (as5600_read_position() / 4096.0) * 360.0;

        char* concatenated_data = (char*)malloc(20);
        sprintf(concatenated_data, "%u,%.3f,%.3f", TF_Luna_distance, servo_angle_deg, stepper_motor_angle_deg);

        esp_mqtt_client_publish(mqtt_client, "LiDAR", concatenated_data, 0, 0, 0);
        free(concatenated_data);
        vTaskDelay(pdMS_TO_TICKS(10));

        if(stepper_motor_angle_deg >= 180){
            end = true;
            // Move the stepper to the initial position
            stepper_motor_angle_deg = move_to_initial_position();
            // Turn off the servo motor
            ledc_set_duty(LEDC_HS_MODE, LEDC_HS_CH0_CHANNEL, 0);
            ledc_update_duty(LEDC_HS_MODE, LEDC_HS_CH0_CHANNEL);
        }
    }
}