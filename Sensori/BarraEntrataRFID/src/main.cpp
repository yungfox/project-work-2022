/* #region  Dichiarazioni Librerie */
#include <Arduino.h>
#include <secret.h>
#include <MFRC522.h>
#include <WiFi.h>
#include <PubSubClient.h>
#include <ArduinoJson.h>
#include <Wire.h>
#include <LiquidCrystal.h>

#define debug
/* #endregion */

/* #region  Definizione Pin */
//#define debug

// pin rst per la lettura dell Rfid
#define pinRst 22
#define pinSs 5

// pin led verde che segnala che il pagamento è stato effettuato
#define ledpinverde 17
// pin led rosso che segnala che il pagamento è stato  respinto
#define ledpinrosso 16
bool bloccainvio = false;

int posti = 50;
//bool primavolta=false;

const int rs = 13, en = 12, d4 = 14, d5 = 27, d6 = 26, d7 = 25;
LiquidCrystal lcd(rs, en, d4, d5, d6, d7);

/* #endregion */
/* #region  Definizioni Variabili Per Cconnessione Wifi e Server Mqtt */
// Ricevo le credenziali di accesso per wifi e informazioni sensibili dalla libreria secret.h definita all'interno del progetto
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

/* #region  */
void reconnect()
{
  // creazione stringa che definisce l'indirizzo del client, alla vista del broker, come esp32-client-(MACaddress),
  // in modo da constatare che non ci siano client con lo stesso nome tra quelli visibili al broker
  String client_id = "esp32-client-";
  client_id += String(WiFi.macAddress());
// stampo in console, un messaggio dove dichiaro che che l'operazione di connessione sta per iniziare, dichiarando il nome unico
// con cui il client verrà visto dal broker
#ifdef debug
  Serial.printf("The client %s connects to the public mqtt broker\n", client_id.c_str());
#endif
  // seleziono tramite la stringa creata in precedenza, il nostro client e lo riconnetto al vroker
  if (client.connect(client_id.c_str()))
  {
    // nel caso la funzione abbia successo , stampo un messaggio in console che riporta il successo della connessione al broker
    Serial.println("Public emqx mqtt broker connected");
  }
  else
  {
// in caso contrario stampo il messaggio di insuccesso di riconnessone insieme allo stato di errore e pongo un delay di 1000ms
// dopo i quale, l'operazione di riconnessione verrà ripetuta
#ifdef debug
    Serial.print("failed with state ");
    Serial.print(client.state());
#endif
    delay(2000);
  }
  client.subscribe(TOPIC_RECIEVE);
  bloccainvio = false;
}
/* #endregion */

/* #region  funzione di callback per l'iscrizione */
void callback(char *topic, byte *payload, unsigned int length)
{
#ifdef debug
  Serial.println("Message:");
#endif
  char js[length];
  js[length] = '\0';
  for (int i = 0; i < length; i++)
  {
    js[i] = (char)payload[i];
  }
#ifdef debug
  Serial.print(js);
#endif
  StaticJsonDocument<32> doc;

  DeserializationError error = deserializeJson(doc, js, length);

  if (error)
  {
#ifdef debug
    Serial.print("deserializeJson() failed: ");
    Serial.println(error.c_str());
#endif
    return;
  }

  int stato = doc["stato"]; // "F4 AA B2 89"
  int postiRicezione = 0;
  postiRicezione = doc["spot"]; 
  if(postiRicezione !=0){
    posti = postiRicezione;
  }
  // const char *Dispositivo = doc["Dispositivo"]; // "ESP32Uscita"
  if (stato == 1)
  {
    digitalWrite(ledpinverde, HIGH);
    digitalWrite(ledpinrosso, LOW);
    delay(5000);
    digitalWrite(ledpinverde, LOW);
  }
  else
  {
    digitalWrite(ledpinrosso, HIGH);
    digitalWrite(ledpinverde, LOW);
    lcd.clear();
    lcd.setCursor(0, 0);
    lcd.print("Errore");
    lcd.setCursor(0, 1);
    lcd.print("Ticket Usato");
    delay(2000);
    digitalWrite(ledpinrosso, LOW);
    lcd.clear();
    
  }

  bloccainvio = false;

//  digitalWrite(ledpinblu, LOW);
#ifdef debug
  Serial.println(stato);
  Serial.println("-----------------------");
#endif
}
/* #endregion */
/* #region  Invia Dati con ricezione di stringa */
void InviaDati(char *datiEntrata)
{
  // client.connect("SbarraUscita");
  if (client.connected())
  {
    digitalWrite(ledpinrosso, LOW);
    digitalWrite(ledpinverde, LOW);
    // digitalWrite(ledpinblu, HIGH);
    char clientid[15];
    String sbarra = "ESP32Entrata";
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
#ifdef debug
    Serial.print(js);
#endif
  }
  else
  {
    // digitalWrite(ledpinblu, LOW);
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
  Serial.print("Sono qui");
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
  if(!bloccainvio){
    InviaDati(charbuffer);
    bloccainvio = true;
  }
  
   
  
}

/* #endregion */
/* #region  Setup */
// Setup
void setup()
{

  lcd.begin(16, 2);

  pinMode(ledpinverde, OUTPUT);
  pinMode(ledpinrosso, OUTPUT);

  Serial.begin(9600); // Initiate a serial communication
  SPI.begin();        // Initiate  SPI bus
  mfrc522.PCD_Init(); // Initiate MFRC522
#ifdef debug
  Serial.println("Approximate your card to the reader...");
  Serial.println();
#endif
  // mi connetto al wifi
  WiFi.begin(SSID, PASSWORD);
  WiFi.setHostname(HOSTNAME);
  while (WiFi.status() != WL_CONNECTED)
  {
    delay(500);
#ifdef debug
    Serial.println("Connecting to WiFi..");
#endif
  }
#ifdef debug
  Serial.println("Connected to the WiFi network!");
#endif
  // connect to the mqtt broker
  client.setServer(MQTT_BROKER, MQTT_PORT);

  client.setCallback(callback);
  while (!client.connected())
  {
    reconnect();
    //<Serial.println("Retrying connection!!!");
  }
  digitalWrite(ledpinrosso, HIGH);
  
 
}
/* #endregion */
/* #region  Loop */
void loop()
{
  lcd.setCursor(0, 0);
  lcd.print("POSTI DISP.");
  lcd.setCursor(0, 1);
  lcd.print(posti);
  // richiamo la funzione di lettura
  LetturaRFID();
  // verifico che il client sia conensso e iscrivo
  client.loop();
}

/* #endregion */
