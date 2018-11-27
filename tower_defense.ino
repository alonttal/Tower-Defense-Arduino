#include <FastLED.h>
#include <LiquidCrystal_I2C.h>
#include <MFRC522.h>
#include <SPI.h>

// --------------------
// General Configuration
// --------------------
#define PRE_SETUP_DELAY 100
#define POST_SETUP_DELAY 1000
#define ON HIGH
#define OFF LOW
#define BAUD_RATE 9600
#define LEVEL_MSG(level) ("  = LEVEL: " + String(level) + " =   ")
#define SCORE_MSG(score) ("SCORE: " + String(score) + "     ")
#define LIFE_MSG(life) (byte(HEART) )
// if you want to perform an unblocking delay (for concurrent operations)
#define VIRTUAL_DELAY(last_update_pram, d_delay, X)       \
    do {                                                  \
        if (millis() - (last_update_pram) >= (d_delay)) { \
            X last_update_pram = millis();                \
        }                                                 \
    } while (0)
#define HEART 7
 byte heart_bitmap[8] = {0x0, 0xA, 0x1F, 0x1F, 0x1F, 0xE, 0x4, 0x0};
// --------------------
// LED Strip Configurations
// --------------------
#define LED_STRIP_DATA_PIN 13
// 60 leds per meter, 5 meters => 60*5=300 LEDS
#define LED_STRIP_NUM_LEDS 300
#define LED_STRIP_MODEL WS2812B
#define LED_STRIP_BRIGHTNESS 64
// for WS2812B model the colors are opposite, meaning that red -> green and
// green -> red thus, we need to set up the color ordering so that red -> red
// and green -> green
#define LED_STRIP_COLOR_ORDER GRB
#define LED_STRIP_SPEED 300
CRGB leds[LED_STRIP_NUM_LEDS];
uint8_t leds_hp[LED_STRIP_NUM_LEDS] = {0};
uint8_t leds_max_hp[LED_STRIP_NUM_LEDS] = {20};
uint16_t leds_to_add = 0;
uint8_t current_max_hp = 0;
unsigned long led_strip_last_move_time = 0;

// --------------------
// RFID Configurations
// --------------------
#define RFIDS_NUM 3
#define RFID_RST_PIN 2
#define RFID_SS_PIN_1 3
#define RFID_SS_PIN_2 4
#define RFID_SS_PIN_3 5
#define GREEN_LED_PIN_1 22
#define GREEN_LED_PIN_2 24
#define GREEN_LED_PIN_3 26
#define RED_LED_PIN_1 23
#define RED_LED_PIN_2 25
#define RED_LED_PIN_3 27
#define RFID_SAMPLE_SPEED 300
#define RANGES_NUM 8
MFRC522 rfids[RFIDS_NUM];
const uint8_t rfid_ss_pins[] = {RFID_SS_PIN_1, RFID_SS_PIN_2, RFID_SS_PIN_3};
const uint8_t rfid_green_led_pins[] = {GREEN_LED_PIN_1, GREEN_LED_PIN_2, GREEN_LED_PIN_3};
const uint8_t rfid_red_led_pins[] = {RED_LED_PIN_1, RED_LED_PIN_2, RED_LED_PIN_3};
const uint32_t known_rfid_keys[] = {0x50B6DBA4, 0x345E64A3, 0x904CE7A4, 0xDA1466A3, 0x3575A4, 0xBC49C873};
unsigned long rfid_last_sample_time[RFIDS_NUM] = {0};
unsigned long rfid_last_shoot_time[RFIDS_NUM] = {0};
uint8_t shoot_speeds[RFIDS_NUM] = {0};
uint8_t rfid_green_led_status[RFIDS_NUM] = {0};
uint8_t rfid_red_led_status[RFIDS_NUM] = {0};
// each rfid can have up to 4 different ranges (specifying all its directions)
const int16_t rfids_ranges[][RANGES_NUM] = {{0, 9, -1, -1, -1, -1, -1, -1},
                                   {0, 9, -1, -1, -1, -1, -1, -1},
                                   {0, 9, -1, -1, -1, -1, -1, -1}};

// --------------------
// LCD Screen Configurations
// --------------------
#define I2C_ADDRESS 0x27
#define LCD_WIDTH 16
#define LCD_HEIGHT 2
LiquidCrystal_I2C lcd(I2C_ADDRESS, 2, 1, 0, 4, 5, 6, 7, 3, POSITIVE);
uint8_t life = 50;
uint16_t level = 0;
uint16_t score = 0;
unsigned long int level_start_time = 0;
uint32_t next_level_delay = 0;

// TODO: the damage and speed need to be set according to the tower that is
// located on the TowerBase.
// TODO: also maybe we should also add a extra_range, so towers may have
// different ranges.
// struct Gun {
//     int led_pin;
//     bool led_active = false;

//     int shoot_speed = 200;
//     const int SHOT_LIGHT_DELAY = 50;
//     int damage = 1;
//     int last_shoot_time = 0;

//     void __turn_on_shooting_led() {
//         digitalWrite(led_pin, HIGH);
//         led_active = true;
//     }

//     void __perform_shot(int i) {
//         path.update_mob_hp(i, max(0, path.led_hps[i] - damage));
//         path.update_mob_color(i);
//         FastLED.show();
//     }

//     void perform_shot(int index_to_shoot) {
//         VIRTUAL_DELAY(last_shoot_time, shoot_speed, __turn_on_shooting_led();
//                       __perform_shot(index_to_shoot););
//         if (millis() - last_shoot_time >= SHOT_LIGHT_DELAY && led_active) {
//             disable_gun();
//         }
//     }

//     void disable_gun() {
//         digitalWrite(led_pin, LOW);
//         led_active = false;
//     }
// };
// --------------------
//        Inits
// --------------------

void init_led_strip() {
    Serial.println("Initializing LED strip...");
    FastLED.addLeds<LED_STRIP_MODEL, LED_STRIP_DATA_PIN, LED_STRIP_COLOR_ORDER>(leds, LED_STRIP_NUM_LEDS);
    FastLED.setBrightness(LED_STRIP_BRIGHTNESS);
    FastLED.clear();
    FastLED.show();
}

void init_rfids() {
    Serial.println("Initializing RFIDs strip...");
    SPI.begin();
    for (int i = 0; i < RFIDS_NUM; i++) {
        rfids[i].PCD_Init(rfid_ss_pins[i], RFID_RST_PIN);
        rfids[i].PCD_DumpVersionToSerial();  // for debug
        pinMode(rfid_green_led_pins[i], OUTPUT);
        pinMode(rfid_red_led_pins[i], OUTPUT);
    }
}

void init_lcd_display() {
    Serial.println("Initializing LCD display...");
    lcd.begin(LCD_WIDTH, LCD_HEIGHT);
    lcd.createChar(HEART, heart_bitmap);
    lcd.clear();
    lcd_print_level();
    lcd_print_score_and_life();
}

/////////////////////////////////////////////////////////////

void toggle_led(uint8_t led_pins[], uint8_t led_statuses[], uint8_t led_index, uint8_t new_status) {
    uint8_t i = led_index;
    uint8_t* status = &led_statuses[i];
    if (*status != new_status) {
        digitalWrite(led_pins[i], new_status);
        *status = new_status;
    }
}
void toggle_green_led(uint8_t led_index, uint8_t new_status) {
    toggle_led(rfid_green_led_pins, rfid_green_led_status, led_index, new_status);
}
void toggle_red_led(uint8_t led_index, uint8_t new_status) {
    toggle_led(rfid_red_led_pins, rfid_red_led_status, led_index, new_status);
}

void lcd_print_level() {
    lcd.setCursor(0, 0);
    lcd.print(LEVEL_MSG(level));
}

void lcd_print_score_and_life() {
    lcd.setCursor(0, 1);
    lcd.print(SCORE_MSG(score));
    lcd.setCursor(13, 1);
    lcd.write(byte(HEART));
    lcd.print(life);
}

struct CRGB get_hp_color(uint8_t led_index) {
    double hp_ratio = (double)leds_hp[led_index] / leds_max_hp[led_index];
    if (hp_ratio > 0.9)
        return CRGB::Green;
    else if (hp_ratio > 0.8)
        return CRGB::GreenYellow;
    else if (hp_ratio > 0.6)
        return CRGB::Yellow;
    else if (hp_ratio > 0.4)
        return CRGB::Orange;
    else if (hp_ratio > 0.25)
        return CRGB::DarkOrange;
    else if (hp_ratio > 0.1)
        return CRGB::OrangeRed;
    else if (leds_hp[led_index] > 0)
        return CRGB::Red;
    else
        return CRGB::Black;
}

void move_leds() {
    VIRTUAL_DELAY(
        led_strip_last_move_time, LED_STRIP_SPEED,
        if (leds_max_hp[LED_STRIP_NUM_LEDS - 1] > 0) {
          life--;
          lcd_print_score_and_life();
        }
        for (uint16_t j = LED_STRIP_NUM_LEDS - 1; j > 0; j--) {
            leds_max_hp[j] = leds_max_hp[j - 1];
            leds_hp[j] = leds_hp[j - 1];
            leds[j] = leds[j - 1];
        } if (leds_to_add > 0) {
            leds_to_add--;
            leds_max_hp[0] = current_max_hp;
            leds_hp[0] = current_max_hp;
            leds[0] = get_hp_color(0);
        } else {
            leds_max_hp[0] = 0;
            leds_hp[0] = 0;
            leds[0] = CRGB::Black;
        } FastLED.show(););
}

void read_rfid(uint8_t rfid_index) {
    uint8_t i = rfid_index;
    rfids[i].PICC_ReadCardSerial();
    Serial.println("RFID number " + String(i) + " is reading.");
    Serial.print(F("PICC type: "));
    MFRC522::PICC_Type piccType = rfids[i].PICC_GetType(rfids[i].uid.sak);
    Serial.println(rfids[i].PICC_GetTypeName(piccType));

    unsigned long int uid = 0;
    Serial.print(F("Scanned PICC's UID: "));
    for (int j = 0; j < rfids[i].uid.size; j++) {
        uid = (uid << 8) + rfids[i].uid.uidByte[j];
    }
    Serial.println(uid, HEX);
}

void track_rfid_card(uint8_t rfid_index) {
    const uint8_t i = rfid_index;
    VIRTUAL_DELAY(
        rfid_last_sample_time[i], RFID_SAMPLE_SPEED,
//        rfids[i].PICC_Select();
        if (rfids[i].PICC_ReadCardSerial() || rfids[i].PICC_IsNewCardPresent()) {
            read_rfid(i);
            shoot_speeds[i] = 100; // for now hard coded (should be set according to the rfid card placed)
            toggle_green_led(i, ON);
        } else {
            shoot_speeds[i] = 0;
            toggle_green_led(i, OFF);
            toggle_red_led(i, OFF);
        });
}


void try_to_shoot(uint8_t rfid_index) {
    uint8_t i = rfid_index;
    if (shoot_speeds[i] == 0) return;
    VIRTUAL_DELAY(
        rfid_last_shoot_time[i], shoot_speeds[i],
        bool found_target = false;
        for (uint8_t j = 0; j < RANGES_NUM && !found_target; j += 2) {
            uint16_t base_range = rfids_ranges[i][j];
            uint16_t top_range = rfids_ranges[i][j + 1];
            if (base_range == -1) continue;
            for (int16_t k = 0; k < (top_range - base_range + 1); k++) {
                uint16_t shoot_index = top_range - k;
                if(leds_hp[shoot_index] > 0) {
                    toggle_red_led(i, ON);
                    leds_hp[shoot_index] -= 1; // this needs to be decreased according to tower's damage
                    Serial.println("Shooting index: " + String(shoot_index) + " HP droped: " + String(leds_hp[shoot_index] + 1) + " -> " + String(leds_hp[shoot_index]));
                    leds[shoot_index] = get_hp_color(shoot_index);
                    FastLED.show();
                    if (leds_hp[shoot_index] == 0) { 
                        score += level;
                        lcd_print_score_and_life();
                    }
                    found_target = true;
                    break;
                }
            }
        }
        if (!found_target) toggle_red_led(i, OFF);
    );
}

void next_level() {
    level++;
    Serial.println("Starting level " + String(level));
    lcd_print_level();
    current_max_hp = level + 4;
    leds_to_add = level + 4;
}

void next_level_loop() {
    VIRTUAL_DELAY(level_start_time, next_level_delay,
                  next_level();
                  next_level_delay = (level + 100) * LED_STRIP_SPEED;  // for now some boring delay
    );
}

void setup() {
    delay(PRE_SETUP_DELAY);
    Serial.begin(BAUD_RATE);
    init_led_strip();
    init_rfids();
    init_lcd_display();
    Serial.println("Finished initialization.");
    delay(POST_SETUP_DELAY);
    Serial.println("Starting game!");
}

void loop() {
    next_level_loop();
    for (int i = 0; i < RFIDS_NUM; i++) {
        track_rfid_card(i);
    }
    move_leds();
    for (int i = 0; i < RFIDS_NUM; i++) {
        try_to_shoot(i);
    }
}
