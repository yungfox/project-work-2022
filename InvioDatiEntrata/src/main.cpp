#include <Arduino.h>
#include <LiquidCrystal.h>
#include <WiFi.h>
#include <PubSubClient.h>
#include <secret.h>


#define Francesco 14
/* #region   */

// initialize constants
const char *SSID = WIFI_SSID;
const char *PASSWORD = WIFI_PASSWORD;
const char *MQTT_BROKER = HOST_ADDRESS;
const char *TOPIC = MQTT_TOPIC;
const int MQTT_PORT = 1883;
const int PUBLISH_INTERVAL = 5000;
// instantiate wifi, pubsub and sensor clients
WiFiClient espClient;
PubSubClient client(espClient);
/* #endregion */

void setup()
{
  Serial.begin(9600);
  pinMode(Francesco,INPUT);

  // mi connetto al wifi
  WiFi.begin(SSID, PASSWORD);
  while (WiFi.status() != WL_CONNECTED)
  {
    delay(500);
    Serial.println("Connecting to WiFi..");
  }
  Serial.println("Connected to the WiFi network!");

  
  
  // connect to the mqtt broker
  client.setServer(MQTT_BROKER, MQTT_PORT);
}
/* #region  SendData */

void InviaDati()
{
  client.setServer(MQTT_BROKER, MQTT_PORT);
  client.connect("SbarraEntrata");
  if (client.connected())
  {
    char franco[]= "F4 AA B2 89";
    Serial.println("Inviato con Francesco");
    char clientid[12];
    String sbarra = "ESP32Uscita";
    sbarra.toCharArray(clientid, 12);
    Serial.println(clientid);

    char js[60];
    js[0] = '\0';
    strcat(js, "{\"_id\":\"");
    strcat(js, franco);
    strcat(js, "\",\"Dispositivo\":\"");
    strcat(js, clientid);
    strcat(js, "\"}");

    // char *json = js;

    // publish to the broker
    client.publish(TOPIC, js);
    Serial.print(js);

    sbarra = "";
  }
}

/* #endregion */

void loop()
{

  if (digitalRead(Francesco) == HIGH)
  {
    Serial.println("INVIOOO");

    // di sicuro non debuggato da alvise
    InviaDati();
  }
  else{
    Serial.println("aaaaaaa");
  }
  delay(PUBLISH_INTERVAL);
}