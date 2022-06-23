// definisco librerie Arduino implicate nel programma
#include <Arduino.h>
// le seguenti due, consentono al ESP32 di utilizzare il modulo del WiFi per comunicare con un broker, in questo caso
#include <WiFi.h>
#include <PubSubClient.h>
// libreria creata per dichiarare e richiamare facilmente le credenziali di WiFi e broker
#include <secret.h>

// definisco in delle variabili, lo stato del parcheggio e lo stato a loro abbinato per facilitare i controlli 
bool statoinviato1=false;
bool statoOccupato1=false;
bool statoinviato2=false;
bool statoOccupato2=false;

// definisco i PIN dei sensori di controllo e dei led che mostrino lo stato in tempo reale, delle piazzole di parcheggio
int pinTrigger1 = 32;
int pinEcho1    = 33;

int Verde1     = 17;
int Rosso1     = 16;

int pinTrigger2 = 22;
int pinEcho2    = 23;

int Verde2     = 12;
int Rosso2     = 14;

// definisco una soglia di controllo entro la quale il sensore dichiara il posto come "Occupato" o "Libero"
int sogliaControllo = 15;

// variabile di stato che determinerà ciò che verrà inviato all'interno del JSON ( che indica lo stato del parcheggio, inviato ai server)
bool occupato1 = false;
bool occupato2 = false;

// definisco ciò che verrà saacritto all'interno del JSON per dichiarare lo stato del parcheggio. 
// Definisco come char per facilitare l'inserimento nel JSON, per evitare errori di conversione
char *statoOccupato = "true";
char *statoLibero = "false";

// variabile numerica che serve per la conversione del valore temporale misurato dai sensori, che venendo diviso per questa variabile,
// verrà convertito in misura (cm)
int conversione = 58.31;

// id della piazzola su cui verrà posizionato il sensore, che verrà inviato nel JSON e utilizzato come identificativo per
// la ricerca e le funzioni svolte poi all'interno del DB
String idPiazzola1 = "001";
String idPiazzola2 = "002";

// inizializzo costanti dichiarate all'interno della libreria secret
const char *SSID = WIFI_SSID;
const char *PASSWORD = WIFI_PASSWORD;
const char *MQTT_BROKER = HOST_ADDRESS;
char *TOPIC1 = MQTT_TOPIC_SEND1;
char *TOPIC2 = MQTT_TOPIC_SEND2;
const int MQTT_PORT = 1883;
const int PUBLISH_INTERVAL = 5000;

// istaniamento del wifi, pubsub e sensor clients
WiFiClient espClient;
PubSubClient client(espClient);

// ++++++++++++++++++++++++++++++++++++++++++++++++++++++++
// dichiaro una funzione di reconnect che, in caso crollo della connessione durante l'attività dei sensori, ristabilisce non appena è
// possibile, una connessione col broker
void reconnect()
{
  // creazione stringa che definisce l'indirizzo del client, alla vista del broker, come esp32-client-(MACaddress),
  // in modo da constatare che non ci siano client con lo stesso nome tra quelli visibili al broker
    String client_id = "esp32-client-";
    client_id += String(WiFi.macAddress());
    // stampo in console, un messaggio dove dichiaro che che l'operazione di connessione sta per iniziare, dichiarando il nome unico
    // con cui il client verrà visto dal broker
    Serial.printf("The client %s connects to the public mqtt broker\n", client_id.c_str());
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
      Serial.print("failed with state ");
      Serial.print(client.state());
      delay(1000);
    }
}

// prima operazione di invio dei dati. Questa operazione verrà utilizzata solo dal primo sensore tra quelli collegati al nostro ESP32
void invioDati1(){
  // tramite un messaggio in console, dichiaro che sta per essere inviato il JSON al broker
  // comando utilizzato principalmente per debug   
  Serial.println("INVIO DATI 1 --- ");
  // converto in formato char l'id del sensore, in modo poi da inserirlo nel file che verrà inviato al broker
  String id = idPiazzola1;
  char datiPiazzola1[4];
  // svuoto la variabile char, prima di popolarla con i dati prodotti nelle stringhe soprastanti
  datiPiazzola1[0]='\0';
  id.toCharArray(datiPiazzola1,4);
  char *stato;
  // in caso il client fosse connesso, posso procedere con l'invio dei dati
  if (client.connected())
  {
    char clientid[12];
    clientid[0]='\0';
    // in base allora stato letto dal sensore, stampo uno stato sul file che verrà inviato
    if(occupato1 == 1){
      stato = statoOccupato;
    }else{
      stato = statoLibero; 
    }
    // creo il JSON seguendo il formato per noi standard e lo popolo con i dati da noi interessati
    char js[30];
    js[0] = '\0';
    strcat(js, "{\"_id\":\"");
    strcat(js, datiPiazzola1);
    strcat(js, "\",\"Status\":");
    strcat(js, stato);
    strcat(js, "}");

    // eseguo l'operazione di publish del file verso il broker    
    client.publish(TOPIC1, js);
    Serial.print(js);
  }
  
}
// ++++++++++++++++++++++++++++++++++++++++++++++++++++++++
// seconda operazione di invio dei dati. Questa operazione verrà utilizzata solo dal primo sensore tra quelli collegati al nostro ESP32
void invioDati2(){
  // tramite un messaggio in console, dichiaro che sta per essere inviato il JSON al broker
  // comando utilizzato principalmente per debug 
  Serial.println("INVIO DATI 2 --- ");
  // converto in formato char l'id del sensore, in modo poi da inserirlo nel file che verrà inviato al broker
  String id = idPiazzola2;
  char datiPiazzola2[4];
  // svuoto la variabile char, prima di popolarla con i dati prodotti nelle stringhe soprastanti
  datiPiazzola2[0]='\0';
  id.toCharArray(datiPiazzola2, 4);
  char *stato;
  // in caso il client fussse connesso, posso procedere con l'invio dei dati
  if (client.connected())
  {
    char clientid[12];
    clientid[0]='\0';
    // in base allora stato letto dal sensore, stampo uno stato sul file che verrà inviato
    if(occupato2 == 1){
      stato = statoOccupato;
    }else{
      stato = statoLibero; 
    }
    // creo il JSON seguendo il formato per noi standard e lo popolo con i dati da noi interessati
    char js[30];
    js[0] = '\0';
    strcat(js, "{\"_id\":\"");
    strcat(js, datiPiazzola2);
    strcat(js, "\",\"Status\":");
    strcat(js, stato);
    strcat(js, "}");

    // eseguo l'operazione di publish del file verso il broker    
    client.publish(TOPIC2, js);
    Serial.print(js);
  }
 
}
//+++++++++++++++++++++++++++++++++++++
// inizializzo la funzione di setup, che serve per impostare i PIN e la loro modalità all'interno del nostro ESP

void setup()
{
  // dichiaro se i PIN del primo sensore e le luci a lui correlate
  pinMode(pinTrigger1, OUTPUT);
  pinMode(pinEcho1, INPUT);
  pinMode(Verde1, OUTPUT);
  pinMode(Rosso1, OUTPUT);

  // dichiaro se i PIN del primo sensore e le luci a lui correlate
  pinMode(pinTrigger2, OUTPUT);
  pinMode(pinEcho2, INPUT);
  pinMode(Verde2, OUTPUT);
  pinMode(Rosso2, OUTPUT);
  
  // inizializzo la porta seriale che ci servirà per eseguire un primo debug visivo del programma
  Serial.begin(9600);
  
// ++++++++++++++++++++++++++++++++++++++++++++++++++++++++

  // Dichiaro la stringa principale per la CONNESSIONE ALLA RETE WIFI
  // dichiaro all'interno del modulo del wifi, la password e il nome della rete, dichiarati nella libreria secret e inizializzati
  // nelle prime righe del programma
  WiFi.begin(SSID, PASSWORD);
  // controllo lo stato del WiFi e se risulta sconnesso tenta di riconnettersi
  while (WiFi.status() != WL_CONNECTED)
  {
    // nel caso il dispositivo risulti sconnesso dal WiFi, tenterà di riconnettersi e ogni mezzo secondo stamperà a console il messaggioi
    // di riconnessione
    delay(500);
    Serial.println("Connecting to WiFi..");
  }
  // nel caso invece il dispositivo risulti già connesso, avremo un messaggio in console che dichiarerà che siamo connessi alla rete con successo!
  Serial.println("Connected to the WiFi network!");

  // connetto le porte MQTT al broker MQTT
  client.setServer(MQTT_BROKER, MQTT_PORT);


// connessione con MAC ADDRESS all MQTT Server
// nel caso non il dispositivo non fosse connesso, entro nella funzione di reconnect()
 while (!client.connected())
  {
    // funzione di riconnessione
    reconnect();
  }

}

// funzione di loop all'interno del ESP32
// le funzioni all'interno di questo blocco, verranno eseguite all'infinito dal nostro ESP32
void loop()
{
  // controllo ancora una volta che il dispositivo sia connesso e, in caso contrario cerco la riconnessione
  if(!client.connected())
  {
    // funzione di riconnessione
    reconnect();
  }

  // attivazione del primo sensore ad ultrasuoni
  // do un primo impulso di 10 micorsecondi che sarà l'impulso utilizzato dal sensore che determinerà poi
  // la distanza con i primo oggetto intercettato, in tempo passato dal primo impulso, a quello di risposta
  digitalWrite(pinTrigger1, LOW);
  digitalWrite(pinTrigger1, HIGH);
  delayMicroseconds(10);
  digitalWrite(pinTrigger1, LOW);
    
  // Calcolo del tempo attraverso il pin di echo, quindi il tempo che ci permette di calcolare la distanza del primo oggetto 
  // che il nostro sensore ha intercettato
  long durata1 = pulseIn(pinEcho1, HIGH);
  long distanza1 = durata1/conversione;

  // attivazione del primo sensore ad ultrasuoni
  // do un primo impulso di 10 micorsecondi che sarà l'impulso utilizzato dal sensore che determinerà poi
  // la distanza con i primo oggetto intercettato, in tempo passato dal primo impulso, a quello di risposta
  digitalWrite(pinTrigger2, LOW);
  digitalWrite(pinTrigger2, HIGH);
  delayMicroseconds(10);
  digitalWrite(pinTrigger2, LOW);
  
  // Calcolo del tempo attraverso il pin di echo, quindi il tempo che ci permette di calcolare la distanza del primo oggetto 
  // che il nostro sensore ha intercettato
  long durata2 = pulseIn(pinEcho2, HIGH);
  long distanza2 = durata2/conversione;
// IMPORTANTE
// OGNI VOLTA CHE MANDO L'IMPULSO DI UN SENSORE, CALCOLO LA DISTANZA SUBITO SUCCESSIVAMENTE POICHÈ
// LA FUNZIONE PULSEIN ASPETTA E BLOCCA TUTTE LE FUNZIONI PER ASPETTARE IL SEGNALE DI RISPOSTA E NEL CASO FACESSIMO
// PRIMA I DUE IMPULSI E POI IL CACOLO, IL SECONDO IMPULSO VERREBBE BLOCCATTO DALLA PRIMA FUNZIONED I PULSEIN

// puliamo i sensori da eventuali problemi di lettura che potrebbero segnare la distanza letta come "0"
if (distanza1 == 0){
  // nel caso il sensore legga distanza "0",saltiamo la misura e portiamo un riscontro in console un modo da mostrare che la misura
  // è stata letta come 0 e non come misura reale
    Serial.println("Sono a zero");
}
  else{
// nel caso non fosse letta come "0", controlliamo che la misura letta sia maggiore della soglia di controllo per definire se il posto
// risulta occupato o libero
if( distanza1 > sogliaControllo ){
  // nel caso fosse maggiore della soglia di controllo, allora il posto risulta libero dato che la distanza che indicherebbe una presenza 
  // all' interno del parcheggio non risulta soddisfatta e quindi lo stato di "occupato" risulta false ( posto libero )
    occupato1 = false;
    if(!statoOccupato1){
      // invio lo stato del posto che risulterà libero
        invioDati1(); 
        statoOccupato1=true;
        statoinviato1=false;
        delay(1000);
    }
    // accendo il LED verde, che indica la disponibilità del parcheggio 
    digitalWrite(Verde1,HIGH);
    digitalWrite(Rosso1,LOW);
    

  }
  else{ 
    occupato1 = true;
    if(!statoinviato1){
        invioDati1(); 
        statoinviato1=true;
        statoOccupato1=false;
        delay(1000);
    }
    digitalWrite(Rosso1,HIGH);
    digitalWrite(Verde1,LOW);

  }

}

 // ++++++++++++++++++++++++++++++++++++++++++++++++++++++++

// puliamo i sensori da eventuali problemi di lettura che potrebbero segnare la distanza letta come "0"
if (distanza2 == 0){
  // nel caso il sensore legga distanza "0",saltiamo la misura e portiamo un riscontro in console un modo da mostrare che la misura
  // è stata letta come 0 e non come misura reale
    Serial.println("Sono a zero");
}else{
  if( distanza2 > sogliaControllo ){
  // nel caso fosse maggiore della soglia di controllo, allora il posto risulta libero dato che la distanza che indicherebbe una presenza 
  // all' interno del parcheggio non risulta soddisfatta e quindi lo stato di "occupato" risulta false ( posto libero )
    occupato2 = false;
    if(!statoOccupato2){
        // invio lo stato del posto che risulterà libero
        invioDati2(); 
        statoOccupato2=true;
        statoinviato2=false;
        delay(1000);
    }
    digitalWrite(Rosso2,HIGH);
    digitalWrite(Verde2,LOW);
  }
  else{ 
    occupato2 = true;
    if(!statoinviato2){
        invioDati2(); 
        statoinviato2=true;
        statoOccupato2=false;
        delay(1000);
    }
    // accendo il LED verde, che indica la disponibilità del parcheggio 
    digitalWrite(Verde2,HIGH);
    digitalWrite(Rosso2,LOW);
    
  }
  }
  // aspetto 100 ms prima di ripetere il ciclo di stampa e di attivazione dei led
  delay(100);
  
}