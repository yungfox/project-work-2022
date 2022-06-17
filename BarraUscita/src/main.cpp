#include <Arduino.h>
#include <secret.h>
#include <MFRC522.h>
#include <WiFi.h>
#include <PubSubClient.h>
#include <time.h>
#include <ArduinoJson.h>

//#define debug

#define pinRst 22
#define pinSs 5

/* #region  Definizioni Variabili Per Cconnessione Wifi e Server Mqtt */
// initialize constants
const char *SSID = WIFI_SSID;
const char *PASSWORD = WIFI_PASSWORD;
const char *MQTT_BROKER = HOST_ADDRESS;
const char *TOPIC = MQTT_TOPIC;
const int MQTT_PORT = 1883;
const int PUBLISH_INTERVAL = 5000;
/* #endregion */

/* #region Inizializzo gli oggetti  */

MFRC522 mfrc522(pinSs, pinRst);
WiFiClient espClient;
PubSubClient client(espClient);
/* #endregion */

/* #region  funzione di callback per l'iscrizione */
void callback(char *topic, byte *payload, unsigned int length)
{
  // char input[MAX_INPUT_LENGTH];
  Serial.println("Message arrived in topic: ");
  Serial.println(topic);
  Serial.println("Message:");
  char js[length];
  js[length]='\0';
  for (int i = 0; i < length; i++)
  {
    js[i]=(char)payload[i];
  }
  //Serial.print(js);
  StaticJsonDocument<32> doc;

  DeserializationError error = deserializeJson(doc, js, length);

  if (error) {
    Serial.print("deserializeJson() failed: ");
    Serial.println(error.c_str());
    return;
  }

  const char* id = doc["_id"]; // "F4 AA B2 89"
  const char* Dispositivo = doc["Dispositivo"]; // "ESP32Uscita"

 
  Serial.println(Dispositivo);
  Serial.println("-----------------------");
}
/* #endregion */

/* #region  Invia Dati con ricezione di stringa */
void InviaDati(char *datiEntrata)
{
  //client.connect("SbarraUscita");
  if (client.connected())
  {
    char clientid[12];
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

/* #region  Lettura RFID  */
void LetturaRFID()
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
  String IdBiglietto = "";
  byte letter;
  for (byte i = 0; i < mfrc522.uid.size; i++)
  {
    IdBiglietto.concat(String(mfrc522.uid.uidByte[i] < 0x10 ? " 0" : " "));
    IdBiglietto.concat(String(mfrc522.uid.uidByte[i], HEX));
  }
  IdBiglietto.toUpperCase();
  char charbuffer[12];
  IdBiglietto.toCharArray(charbuffer, 12, 1);
  //  di sicuro non debuggato da alvise
  InviaDati(charbuffer);
 
}

/* #endregion */

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
  WiFi.setHostname(HOSTNAME);
  while (WiFi.status() != WL_CONNECTED)
  {
    delay(500);
    Serial.println("Connecting to WiFi..");
  }
  Serial.println("Connected to the WiFi network!");

  // connect to the mqtt broker
  client.setServer(MQTT_BROKER, MQTT_PORT);

  client.setCallback(callback);
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
  
  //Serial.println("hi hi hi aaaaaaaaaaaaaahhhhhhhhhhh");
  client.subscribe(TOPIC);
}

void loop()
{
  LetturaRFID();
  client.loop();
}
