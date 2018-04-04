void setup() {
  // put your setup code here, to run once:
pinMode(9, OUTPUT);
pinMode(6, OUTPUT);
Serial.begin(57600);
}

void loop() {
  // put your main code here, to run repeatedly:
  int x, red, blue;
  if (Serial.available() > 0) {
    x = Serial.read();
    if (x > 100){
      red = map(x, 101, 201, 0, 255);
      analogWrite(6, red);
    } else {
    if (x < 101){
      blue = map(x, 0, 100, 0, 255);
      analogWrite(9, blue);
    }}
    delay(2);
  }
}
