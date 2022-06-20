#include <Arduino.h>
#include <WiFi.h>
#include <PubSubClient.h>
#include <secret.h>

bool statoinviato1=false;
bool statoOccupato1=false;
bool statoinviato2=false;
bool statoOccupato2=false;


int pinTrigger1 = 32;
int pinEcho1    = 33;

int Verde1     = 17;
int Rosso1     = 16;

int pinTrigger2 = 22;
int pinEcho2    = 23;

int Verde2     = 12;
int Rosso2     = 14;

int sogliaControllo = 15;

bool occupato1 = false;
bool occupato2 = false;

char *statoOccupato = "1";
char *statoLibero = "0";

int conversione = 58.31;

String idPiazzola1 = "001";
String idPiazzola2 = "002";

// initialize constants
const char *SSID = WIFI_SSID;
const char *PASSWORD = WIFI_PASSWORD;
const char *MQTT_BROKER = HOST_ADDRESS;
char *TOPIC1 = MQTT_TOPIC_SEND1;
char *TOPIC2 = MQTT_TOPIC_SEND2;
const int MQTT_PORT = 1883;
const int PUBLISH_INTERVAL = 5000;
// instantiate wifi, pubsub and sensor clients
WiFiClient espClient;
PubSubClient client(espClient);
// ++++++++++++++++++++++++++++++++++++++++++++++++++++++++


void reconnect()
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



void invioDati1(){
  
   // EVENTO DI INVIO ALL'ABBASSAMENTO DELLA SBARRA
  Serial.println("INVIO DATI 1 --- ");
  String id = idPiazzola1;
  char datiPiazzola1[4];
  datiPiazzola1[0]='\0';
  id.toCharArray(datiPiazzola1,4);
  char *stato;
  if (client.connected())
  {
    char clientid[12];
    clientid[0]='\0';
    if(occupato1 == 1){
      stato = statoOccupato;
    }else{
      stato = statoLibero; 
    }
    char js[60];
    js[0] = '\0';
    strcat(js, "{\"_id\":\"");
    strcat(js, datiPiazzola1);
    strcat(js, "\",\"stato\":\"");
    strcat(js, stato);
    strcat(js, "\"}");

    // publish to the broker
    
    client.publish(TOPIC1, js);
    Serial.print(js);
  }
  
  //delay(PUBLISH_INTERVAL);
  //client.subscribe(TOPIC);
}
// ++++++++++++++++++++++++++++++++++++++++++++++++++++++++
void invioDati2(){
  
   // EVENTO DI INVIO ALL'ABBASSAMENTO DELLA SBARRA
  Serial.println("INVIO DATI 2 --- ");
  String id = idPiazzola2;
  char datiPiazzola2[4];
  datiPiazzola2[0]='\0';
  id.toCharArray(datiPiazzola2, 4);
  char *stato;
  if (client.connected())
  {
    char clientid[12];
    clientid[0]='\0';
    if(occupato2 == 1){
      stato = statoOccupato;
    }else{
      stato = statoLibero; 
    }

    char js[30];
    js[0] = '\0';
    strcat(js, "{\"_id\":\"");
    strcat(js, datiPiazzola2);
    strcat(js, "\",\"stato\":\"");
    strcat(js, stato);
    strcat(js, "\"}");

    // publish to the broker
    client.publish(TOPIC2, js);
    Serial.print(js);
  }
 
  //delay(PUBLISH_INTERVAL);
  
}



//+++++++++++++++++++++++++++++++++++++
void setup()
{
  pinMode(pinTrigger1, OUTPUT);
  pinMode(pinEcho1, INPUT);
  pinMode(Verde1, OUTPUT);
  pinMode(Rosso1, OUTPUT);


  pinMode(pinTrigger2, OUTPUT);
  pinMode(pinEcho2, INPUT);
  pinMode(Verde2, OUTPUT);
  pinMode(Rosso2, OUTPUT);


  Serial.begin(9600);
  
// ++++++++++++++++++++++++++++++++++++++++++++++++++++++++

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
    reconnect();
  }

}
void loop()
{
  if(!client.connected())
  {
    reconnect();
  }
  digitalWrite(pinTrigger1, LOW);
  digitalWrite(pinTrigger1, HIGH);
  delayMicroseconds(10);
  digitalWrite(pinTrigger1, LOW);
    
  // Calcolo del tempo attraverso il pin di echo
  long durata1 = pulseIn(pinEcho1, HIGH);
  long distanza1 = durata1/conversione;

  digitalWrite(pinTrigger2, LOW);
  digitalWrite(pinTrigger2, HIGH);
  delayMicroseconds(10);
  digitalWrite(pinTrigger2, LOW);
  
    // Calcolo del tempo attraverso il pin di echo
  long durata2 = pulseIn(pinEcho2, HIGH);
  long distanza2 = durata2/conversione;

if (distanza1 == 0){
    Serial.println("Sono a zero");
}
  else{
if( distanza1 > sogliaControllo ){
    occupato1 = false;
    if(!statoOccupato1){
        invioDati1(); 
        statoOccupato1=true;
        statoinviato1=false;
    }
    digitalWrite(Verde1,HIGH);
    digitalWrite(Rosso1,LOW);
    //Serial.println("occupato 1 FALSE");
    

  }
  else{ 
    occupato1 = true;
    if(!statoinviato1){
        invioDati1(); 
        statoinviato1=true;
        statoOccupato1=false;
    }
    //Serial.println("occupato 1 TRUE");

  }

}

 // ++++++++++++++++++++++++++++++++++++++++++++++++++++++++

if (distanza2 == 0){
}else{
  if( distanza2 > sogliaControllo ){
    occupato2 = false;
    if(!statoOccupato2){
        invioDati2(); 
        statoOccupato2=true;
        statoinviato2=false;
    }
    /*digitalWrite(Verde2,HIGH);
    digitalWrite(Rosso2,LOW);*/
    //Serial.println("occupato 2 FALSE");
  }
  else{ 
    occupato2 = true;
    if(!statoinviato2){
        invioDati2(); 
        statoinviato2=true;
        statoOccupato2=false;
    }
    //Serial.println("occupato 2 TRUE");

  }

  }

  delay(100);
  
}