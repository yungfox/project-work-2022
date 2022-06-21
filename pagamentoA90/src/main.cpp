/* #region  Dichiarazioni Librerie */
#include <Arduino.h>
#include <secret.h>
#include <MFRC522.h>
#include <WiFi.h>
#include <PubSubClient.h>
#include <time.h>
#include <ArduinoJson.h>
#include <LiquidCrystal.h>
#include <Wire.h>

/* #endregion */
/* #region  Definizione Pin */
//#define debug
#define pinRst 22
#define pinSs 5
#define ledpinblu 33
#define ledpinverde 12
#define ledpinrosso 32

/* #endregion */
/* #region  Definizioni Variabili Per Cconnessione Wifi e Server Mqtt */
// initialize constants
const char *SSID = WIFI_SSID;
const char *PASSWORD = WIFI_PASSWORD;
const char *MQTT_BROKER = HOST_ADDRESS;
const char *TOPIC_SEND = MQTT_TOPIC_SEND;
const char *TOPIC_RECIEVE = MQTT_TOPIC_RECIVE;
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
  //Serial.println("Message arrived in topic: ");
  //Serial.println(topic);
  Serial.println("Message:");
  char js[length];
  js[length] = '\0';
  for (int i = 0; i < length; i++)
  {
    js[i] = (char)payload[i];
  }
   Serial.print(js);
  StaticJsonDocument<16> doc;

  DeserializationError error = deserializeJson(doc, js, length);

  if (error)
  {
    Serial.print("deserializeJson() failed: ");
    Serial.println(error.c_str());
    return;
  }

  int stato = doc["stato"];                  // "F4 AA B2 89"
  //const char *Dispositivo = doc["Dispositivo"]; // "ESP32Uscita"
  if(stato==1)
  {
    digitalWrite(ledpinverde,HIGH);
    digitalWrite(ledpinrosso,LOW);
  }
  else
  {
     digitalWrite(ledpinrosso,HIGH);
     digitalWrite(ledpinverde,LOW);
  }
  digitalWrite(ledpinblu,LOW);
  Serial.println(stato);
  Serial.println("-----------------------");
}
/* #endregion */
/* #region  Invia Dati con ricezione di stringa */
void InviaDati(char *datiEntrata)
{
  // client.connect("SbarraUscita");
  if (client.connected())
  {
    digitalWrite(ledpinrosso,LOW);
    digitalWrite(ledpinverde,LOW);
    digitalWrite(ledpinblu,HIGH);
    char clientid[15];
    String sbarra = "ESP32Pagamento";
    sbarra.toCharArray(clientid, 15);
    // Serial.println(clientid);

    char js[70];
    js[0] = '\0';
    strcat(js, "{\"_id\":\"");
    strcat(js, datiEntrata);
    strcat(js, "\",\"Dispositivo\":\"");
    strcat(js, clientid);
    strcat(js, "\"}");

    // publish to the broker
    client.publish(TOPIC_SEND, js);
    Serial.print(js);
  }
  delay(PUBLISH_INTERVAL);
  client.subscribe(TOPIC_RECIEVE);
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
    IdBiglietto.concat(String(mfrc522.uid.uidByte[i], DEC));
  }
  IdBiglietto.toUpperCase();
  char charbuffer[30];
  IdBiglietto.toCharArray(charbuffer, 30, 1);
  //  di sicuro non debuggato da alvise
 // Serial.println(charbuffer);
  InviaDati(charbuffer);
}

/* #endregion */
/* #region  Setup */
// Setup
void setup()
{
  pinMode(ledpinverde, OUTPUT);
  pinMode(ledpinrosso, OUTPUT);
  pinMode(ledpinblu, OUTPUT);

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
      // LetturaRFID();
    }
    else
    {
      Serial.print("failed with state ");
      Serial.print(client.state());
      delay(1000);
    }
  }

  // Serial.println("hi hi hi aaaaaaaaaaaaaahhhhhhhhhhh");
  client.subscribe(TOPIC_RECIEVE);
}
/* #endregion */
/* #region  Loop */
void loop()
{
  LetturaRFID();
  client.loop();
}

/* #endregion */
