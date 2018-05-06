void setup() {
pinMode(9, OUTPUT); //RED 
pinMode(6, OUTPUT); //BLUE
pinMode(10, OUTPUT);//GREEN
Serial.begin(115200);
}
int cts; //Color to set. If arduino read 101 before a color value it will set red brightness. 102 will make the same thing for green, 103 for red.
void loop() {
  int x, red, blue, colorValue;
  if (Serial.available() > 0) { //If serial port is available
    x = Serial.read();          //Read serial port
      if (x == 101){            //And if it is 101, 102 or 103 set cts. Which declares which color arduino will set when it reads a color value between 0 and 100.
        cts = 101;
      }
      if (x == 102){
        cts = 102;
      }
      if (x == 103){
        cts = 103;
      }
      int colorValue = map(x, 0, 100, 0, 255); //We gave our color value between 0-100 but arduino can give values between 0 and 255. So we mapped our color value. 
                                               //Actually if you want to lower the brightness you can lower the value 255
      if (cts == 101){ //If cts is 101 and 
        if (x < 101){  //Value coming from serial port is a color value
        analogWrite(9, colorValue); //Set red brightness.
        }}
      if (cts == 102){ //Same for green
        if (x < 101){
        analogWrite(10, colorValue);
        }}
      if (cts == 103){ //Same for blue. 
        if (x < 101){
        analogWrite(6, colorValue);
        }}
  }
}
