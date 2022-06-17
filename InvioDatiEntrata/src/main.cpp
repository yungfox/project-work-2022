#include <Arduino.h>
#include <LiquidCrystal.h>
#include <WiFi.h>
#include <PubSubClient.h>
#include <secret.h>

#define Button 14
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

/* #region  SendData */

void InviaDati()
{
   // EVENTO DI INVIO ALL'ABBASSAMENTO DELLA SBARRA
  Serial.println("SONO QUA CASSOOO");
  String id = "F4 AA B2 89";
  char datiEntrata[12];
  datiEntrata[0]='\0';
  id.toCharArray(datiEntrata, 12);
  // client.connect("SbarraUscita");
  if (client.connected())
  {
    char clientid[12];
    clientid[0]='\0';
    String sbarra = "ESP32Uscita";
    sbarra.toCharArray(clientid, 12);
    // Serial.println(clientid);

    char js[60];
    js[0] = '\0';
    strcat(js, "{\"_id\":\"");
    strcat(js, datiEntrata);
    strcat(js, "\",\"Dispositivo\":\"");
    strcat(js, clientid);
    strcat(js, "\"}");

    // publish to the broker
    client.publish(TOPIC, js);
    Serial.print(js);
  }
  delay(PUBLISH_INTERVAL);
  client.subscribe(TOPIC);
}
/* #endregion */


void setup()
{
  Serial.begin(9600);
  pinMode(Button, INPUT);

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


// connessione con MAC ADDRESS all MQTT Server
  while (!client.connected())
  {
    String client_id = "esp32-client-";
    client_id += String(WiFi.macAddress());
    Serial.printf("The client %s connects to the public mqtt broker\n", client_id.c_str());
    if (client.connect(client_id.c_str()))
    {
      Serial.println("Public emqx mqtt broker connected");
      //LetturaRFID();
    }
    else
    {
      Serial.print("failed with state ");
      Serial.print(client.state());
      delay(1000);
    }
  }
  
}

void loop()
{

  if (digitalRead(Button) == HIGH)
  {
    Serial.println("INVIOOO");

    // di sicuro non debuggato da alvise
    InviaDati();
    delay(PUBLISH_INTERVAL);

  }
  
}