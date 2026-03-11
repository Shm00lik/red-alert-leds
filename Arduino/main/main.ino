#include <FastLED.h>

#define NUM_LEDS 86
#define DATA_PIN 13
#define BRIGHTNESS 255

CRGB leds[NUM_LEDS];


void setup() {
  Serial.begin(9600);

  FastLED.addLeds<NEOPIXEL, DATA_PIN>(leds, NUM_LEDS);
  FastLED.setBrightness(BRIGHTNESS);

  fill_solid(leds, NUM_LEDS, CRGB::Black);

  FastLED.show();
}


void loop() {
  delay(1000);

  if (Serial.available() == 0) {
    Serial.println("No data!");
    return;
  }

  int inputColor = Serial.read();
  Serial.println(inputColor);

  CRGB color = getColor(inputColor);

  fill_solid(leds, NUM_LEDS, color);
  FastLED.show();
}

CRGB getColor(int inputColor) {
  switch (inputColor) {
    case 49:
      return CRGB::OrangeRed;
    case 50:
      return CRGB::Red;
    case 51:
      return CRGB::Green;
    default:
      return CRGB::Black;
  }
}