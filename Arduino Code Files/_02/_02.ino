#define SLAVE 2
// SLAVE ID / Address

#define processDelay 3
// processing delay in milli seconds

#define TxIndicator 8
// TX Indicator LED Pin Number - OUTPUT

#define CarrierSensePin 4
// Carrier Sense Pin Number - INPUT


#define SendTimePin A5
#define ParamPin A4
#define SensorPin A3

unsigned int sendValue=0;
// time to wait before SENDing next packet

unsigned int sensorValue=0;
// some changing value to send while Transmitting

unsigned long currentMillis=0, previousMillis = 0;      
unsigned int counter=0;

void setup() {
  pinMode(TxIndicator, OUTPUT);  // to show when TRANSMITTING
  pinMode(CarrierSensePin, INPUT);  // to check for Carrier
  digitalWrite(CarrierSensePin, HIGH); // pulled-up input
  Serial.begin(2400);  //2400 bits per second
}

void loop() {
  delay(processDelay); 
  // simulate processing delay on node
  
  currentMillis = millis();
  
  // get analog values from potentiometers
  sendValue = analogRead(SendTimePin);
  sensorValue = analogRead(SensorPin);

  if (counter>999)
    counter=0;
    // reset counter

  if(digitalRead(CarrierSensePin)==LOW){ 
    // CARRIER SENSE - checkign state of 555 output pin
    // pin 3 of 555 is connected to Digital Pin 4 of uC
    if(currentMillis - previousMillis > sendValue) {
      previousMillis = currentMillis;      
      digitalWrite(TxIndicator, HIGH);  // indicate Transmission
      Serial.println("");
      Serial.print(SLAVE);
      Serial.print(SLAVE);
      Serial.print(SLAVE);
      Serial.print(":");
      Serial.print("0");
      if(counter<10)
        Serial.print("0");
      if(counter<100)
        Serial.print("0");
      Serial.print(counter++);
      Serial.print(":");
      if(sendValue<10)
        Serial.print("0");
      if(sendValue<100)
        Serial.print("0");
      if(sendValue<1000)
        Serial.print("0");
      Serial.print(sendValue);
      Serial.print(":");
      if(sensorValue<10)
        Serial.print("0");
      if(sensorValue<100)
        Serial.print("0");
      if(sensorValue<1000)
        Serial.print("0");
      Serial.print(sensorValue);
      Serial.println(":");
      Serial.flush();
      digitalWrite(TxIndicator, LOW);  // indicate NO Transmission
    }    
  }
}
