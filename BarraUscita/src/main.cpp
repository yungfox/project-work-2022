#include <Arduino.h>
#include <secret.h>
#include <MFRC522.h>
#include <WiFi.h>
#include <PubSubClient.h>
#include <time.h>

//#define debug

#define pinRst 22
#define pinSs 5
MFRC522 mfrc522(pinSs, pinRst);

/* #region  Definizioni Variabili Per Cconnessione Wifi e Server Mqtt */
// initialize constants
const char *SSID = WIFI_SSID;
const char *PASSWORD = WIFI_PASSWORD;
const char *MQTT_BROKER = HOST_ADDRESS;
const char *TOPIC = MQTT_TOPIC;
const int MQTT_PORT = 1883;
const int PUBLISH_INTERVAL = 5000;
/* #endregion */
// instantiate wifi, pubsub and sensor clients
WiFiClient espClient;
PubSubClient client(espClient);



// Setup
void setup()
{
  pinMode(12, OUTPUT);
  pinMode(32, OUTPUT);

  Serial.begin(9600); // Initiate a serial communication
  SPI.begin();        // Initiate  SPI bus
  mfrc522.PCD_Init(); // Initiate MFRC522
  Serial.println("Approximate your card to the reader...");
  Serial.println();

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

void InviaDati(char* franco)
{
    client.setServer(MQTT_BROKER, MQTT_PORT);
    client.connect("SbarraUscita");
    if(client.connected())
    {
      Serial.println("Inviato con Franco");
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

      //char *json = js;

      // publish to the broker
      client.publish(TOPIC, js);
      Serial.print(js);
      
      sbarra="";

    }
}



void loop()
{

  // Look for new cards
  if (!mfrc522.PICC_IsNewCardPresent())
  {
    return;
  }
  // Select one of the cards
  if (!mfrc522.PICC_ReadCardSerial())
  {
    return;
  }
  // Show UID on serial monitor
  Serial.println("UID tag :");
  String IdBiglietto = "";
  byte letter;
  for (byte i = 0; i < mfrc522.uid.size; i++)
  {
    IdBiglietto.concat(String(mfrc522.uid.uidByte[i] < 0x10 ? " 0" : " "));
    IdBiglietto.concat(String(mfrc522.uid.uidByte[i], HEX));
  }
  // c
  IdBiglietto.toUpperCase();

  // build message string by concatenating all readings separated by a comma
  // char message[100];
  char charbuffer[12];
  IdBiglietto.toCharArray(charbuffer, 12, 1);
  Serial.println(charbuffer);
  // di sicuro non debuggato da alvise
  InviaDati(charbuffer);


  
  delay(PUBLISH_INTERVAL);
}
