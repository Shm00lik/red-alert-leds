#include <FastLED.h>
#define NUM_LEDS 86
#define DATA_PIN 13
#define BRIGHTNESS 255
#define NEW_DATA_HEADER 0xAA
#define ACK_DATA 1
#define BAUDRATE 115200
#define DELAY_BETWEEN_UPDATES 1

CRGB leds[NUM_LEDS];

void setup() {
  Serial.begin(BAUDRATE);
  FastLED.addLeds<NEOPIXEL, DATA_PIN>(leds, NUM_LEDS);
  FastLED.setBrightness(BRIGHTNESS);
  fill_solid(leds, NUM_LEDS, CRGB::Black);
  pinMode(LED_BUILTIN, OUTPUT);
  FastLED.show();
}

void loop() {
  delay(DELAY_BETWEEN_UPDATES);
  if (Serial.read() == NEW_DATA_HEADER) {
    Serial.write(ACK_DATA);
    digitalWrite(LED_BUILTIN, HIGH);
    Serial.readBytes((char*)leds, NUM_LEDS * 3);
    FastLED.show();
    Serial.write(ACK_DATA);
  } else {
    digitalWrite(LED_BUILTIN, LOW);
  }
}