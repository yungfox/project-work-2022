#include <SPI.h>
#include <MFRC522.h>
#include <WiFi.h>
#include <PubSubClient.h>



//#define debug


#define pinRst  22
#define pinSs   5
MFRC522 mfrc522(pinSs, pinRst);

//initialize constants
const char *SSID = "Tu_wi-fi_americano";
const char *PASSWORD = "password1234";
const char *MQTT_BROKER = "192.168.137.77";
const char *TOPIC = "InviaMqtt";
const int MQTT_PORT = 1883;
const int PUBLISH_INTERVAL = 5000;
//instantiate wifi, pubsub and sensor clients
WiFiClient espClient;
PubSubClient client(espClient);
void setup() 
{
  pinMode(12, OUTPUT);
  pinMode(32, OUTPUT);
  Serial.begin(9600);   // Initiate a serial communication
  SPI.begin();      // Initiate  SPI bus
  mfrc522.PCD_Init();   // Initiate MFRC522
  Serial.println("Approximate your card to the reader...");
  Serial.println();
  //connect to wifi
    WiFi.begin(SSID, PASSWORD);
    while (WiFi.status() != WL_CONNECTED) {
        delay(500);
        Serial.println("Connecting to WiFi..");
    }
    Serial.println("Connected to the WiFi network!");

    //connect to the mqtt broker
    client.setServer(MQTT_BROKER, MQTT_PORT);
    client.connect("esp32");

  

}
void loop() 
{
  // Look for new cards
  if ( ! mfrc522.PICC_IsNewCardPresent()) 
  {
    return;
  }
  // Select one of the cards
  if ( ! mfrc522.PICC_ReadCardSerial()) 
  {
    return;
  }
  //Show UID on serial monitor
  Serial.print("UID tag :");
  String IdBiglietto= "";
  byte letter;
  for (byte i = 0; i < mfrc522.uid.size; i++) 
  {
     //Serial.print(mfrc522.uid.uidByte[i] < 0x10 ? " 0" : " ");
     //Serial.print(mfrc522.uid.uidByte[i], HEX);
     IdBiglietto.concat(String(mfrc522.uid.uidByte[i] < 0x10 ? " 0" : " "));
     IdBiglietto.concat(String(mfrc522.uid.uidByte[i], HEX));
  }
  //c
  IdBiglietto.toUpperCase();

  Serial.println(IdBiglietto);
  delay(5000);

 
  
 
} 

/*
while (client.connected()) {
        //get readings
        float humidity = dht.readHumidity();
        float temperature = dht.readTemperature();
        float heat_index = dht.computeHeatIndex(temperature, humidity, false);

        //convert readings to string
        char temperature_str[10], humidity_str[10], heat_index_str[10];
        sprintf(temperature_str, "%f", temperature);
        sprintf(humidity_str, "%f", humidity);
        sprintf(heat_index_str, "%f", heat_index);

        //build message string by concatenating all readings separated by a comma
        char message[100];
        strcat(message, temperature_str);
        strcat(message, ",");
        strcat(message, humidity_str);
        strcat(message, ",");
        strcat(message, heat_index_str);
        
        //publish to the broker
        client.publish(TOPIC, message);
        
        //reset message buffer
        message[0] = '\0';

        //wait for the specified interval
        delay(PUBLISH_INTERVAL);
    }
*/
