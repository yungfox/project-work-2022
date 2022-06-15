#include <LiquidCrystal.h>

LiquidCrystal My_LCD(22,23,5,18,19,21);
#define red 26
#define green 27
#define Button 14
#define BIP 12

// ---------------------------------------------

void setup()
{
  
// ---------------------------------------------
   Serial.begin(9600);
  pinMode(red,OUTPUT);
  pinMode(green,OUTPUT);
  pinMode(Button,INPUT);
  pinMode(BIP,OUTPUT);

// ---------------------------------------------

  // Initialize The LCD. Parameters: [ Columns, Rows ]
  My_LCD.begin(16, 2);
  // Clears The LCD Display
  My_LCD.clear();
   
  My_LCD.print("Inizializzazione");
  My_LCD.setCursor(0,1);
    My_LCD.print("sbarra abbassata");

}

// ---------------------------------------------
void loop() {

// ---------------------------------------------

 if(digitalRead(Button) ==HIGH){
  digitalWrite(green,HIGH);
  digitalWrite(red,LOW);

  My_LCD.clear();
  My_LCD.print("VERDE");
  digitalWrite(BIP,HIGH);
  //delay(100);
 }
 else{
    digitalWrite(red,HIGH);
    digitalWrite(green,LOW);
    digitalWrite(BIP,LOW);

  My_LCD.clear();
  My_LCD.print("ROSSO");
 }

// ---------------------------------------------
  
 }
 
