#include <FastLED.h>
#include <LiquidCrystal_I2C.h>
#include <MFRC522.h>
#include <SPI.h>
#include <SoftwareSerial.h>
#include "rfid1.h"

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
#define LIFE_MSG(life) (byte(HEART))
// if you want to perform an unblocking delay (for concurrent operations)
#define VIRTUAL_DELAY(last_update_pram, d_delay, X)       \
    do {                                                  \
        if (millis() - (last_update_pram) >= (d_delay)) { \
            X last_update_pram = millis();                \
        }                                                 \
    } while (0)
#define HEART 7
#define START_MESSAGE 's'
#define END_MESSAGE 'e'
byte heart_bitmap[8] = {0x0, 0xA, 0x1F, 0x1F, 0x1F, 0xE, 0x4, 0x0};
bool game_started = false;
// --------------------
// LED Strip Configurations
// --------------------
#define LED_STRIP_DATA_PIN 13
// 60 leds per meter, 5 meters => 60*5=300 LEDS
#define LED_STRIP_NUM_LEDS 95
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
#define RFIDS_NUM 9
#define RFID_RST_PIN 2
#define RFID_MISO_PIN_1 3
#define RFID_MISO_PIN_2 4
#define RFID_MISO_PIN_3 5
#define RFID_MISO_PIN_4 6
#define RFID_MISO_PIN_5 7
#define RFID_MISO_PIN_6 8
#define RFID_MISO_PIN_7 9
#define RFID_MISO_PIN_8 10
#define RFID_MISO_PIN_9 11
#define RFID_SS_PIN 50
#define RFID_MOSI_PIN 51
#define RFID_SCK_PIN 52
#define RFID_IRQ_PIN 55
#define GREEN_LED_PIN_1 23
#define GREEN_LED_PIN_2 25
#define GREEN_LED_PIN_3 27
#define GREEN_LED_PIN_4 29
#define GREEN_LED_PIN_5 31
#define GREEN_LED_PIN_6 33
#define GREEN_LED_PIN_7 35
#define GREEN_LED_PIN_8 37
#define GREEN_LED_PIN_9 39
#define RED_LED_PIN_1 22
#define RED_LED_PIN_2 24
#define RED_LED_PIN_3 26
#define RED_LED_PIN_4 28
#define RED_LED_PIN_5 30
#define RED_LED_PIN_6 32
#define RED_LED_PIN_7 34
#define RED_LED_PIN_8 36
#define RED_LED_PIN_9 38
#define RFID_SAMPLE_SPEED 300
#define RANGES_NUM 8
RFID1 rfid;
uchar rfid_miso_pins[] = {RFID_MISO_PIN_1, RFID_MISO_PIN_2, RFID_MISO_PIN_3,
                          RFID_MISO_PIN_4, RFID_MISO_PIN_5, RFID_MISO_PIN_6,
                          RFID_MISO_PIN_7, RFID_MISO_PIN_8, RFID_MISO_PIN_9};
const uint8_t rfid_green_led_pins[] = {GREEN_LED_PIN_1, GREEN_LED_PIN_2, GREEN_LED_PIN_3,
                                       GREEN_LED_PIN_4, GREEN_LED_PIN_5, GREEN_LED_PIN_6,
                                       GREEN_LED_PIN_7, GREEN_LED_PIN_8, GREEN_LED_PIN_9};
const uint8_t rfid_red_led_pins[] = {RED_LED_PIN_1, RED_LED_PIN_2, RED_LED_PIN_3,
                                     RED_LED_PIN_4, RED_LED_PIN_5, RED_LED_PIN_6,
                                     RED_LED_PIN_7, RED_LED_PIN_8, RED_LED_PIN_9};
unsigned long rfid_last_sample_time[RFIDS_NUM] = {0};
unsigned long rfid_last_shoot_time[RFIDS_NUM] = {0};
uint16_t shoot_speeds[RFIDS_NUM] = {0};
uint16_t shoot_damage[RFIDS_NUM] = {0};
uint8_t rfid_green_led_status[RFIDS_NUM] = {0};
uint8_t rfid_red_led_status[RFIDS_NUM] = {0};
// each rfid can have up to 4 different ranges (specifying all its directions)
const int16_t rfids_ranges[][RANGES_NUM] = {{0, 9, -1, -1, -1, -1, -1, -1},
                                            {0, 9, -1, -1, -1, -1, -1, -1},
                                            {0, 9, -1, -1, -1, -1, -1, -1},
                                            {0, 9, -1, -1, -1, -1, -1, -1},
                                            {0, 9, -1, -1, -1, -1, -1, -1},
                                            {0, 9, -1, -1, -1, -1, -1, -1},
                                            {0, 9, -1, -1, -1, -1, -1, -1},
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

// --------------------
// Bluetooth Configurations
// --------------------
#define RX_PIN 12  // need to connect to the TX output in hc06
#define TX_PIN 13  // need to connect to the RX input in hc06
SoftwareSerial BT(RX_PIN, TX_PIN);

// --------------------
// Towers Configurations
// --------------------
#define TOWERS_NUM 6
const uint32_t tower_ids[] = {0x7012E4A4, 0xBC49C873, 0x345E64A3, 0xE1E663A3, 0xC4D863A3, 0xDA1466A3};
// 0x9D7650D3
uint8_t tower_levels[TOWERS_NUM] = {1};
uint8_t tower_extra_ranges[] = {0, 0, 0, 0, 0, 0};
uint16_t tower_speeds[] = {500, 500, 500, 600, 300, 700};  // higher is slower
uint8_t tower_bonus_speeds[] = {0, 1, 3, 1, 2, 1};
uint16_t tower_damages[] = {1, 1, 1, 1, 1, 1};
uint8_t tower_bonus_damage[] = {3, 2, 1, 2, 1, 3};
// --------------------
//        Inits
// --------------------

void init_led_strip() {
    Serial.print("Initializing LED strip...");
    FastLED.addLeds<LED_STRIP_MODEL, LED_STRIP_DATA_PIN, LED_STRIP_COLOR_ORDER>(leds, LED_STRIP_NUM_LEDS);
    FastLED.setBrightness(LED_STRIP_BRIGHTNESS);
    FastLED.clear();
    FastLED.show();
    //flash_lead_strips();
    Serial.println("Done");
}

void init_rfids() {
    Serial.print("Initializing RFIDs...");
    for (uint8_t i = 0; i < RFIDS_NUM; i++) {
        pinMode(rfid_green_led_pins[i], OUTPUT);
        pinMode(rfid_red_led_pins[i], OUTPUT);
    }
    flash_rfid_leds();
    Serial.println("Done");
}

void init_lcd_display() {
    Serial.print("Initializing LCD display...");
    lcd.begin(LCD_WIDTH, LCD_HEIGHT);
    lcd.createChar(HEART, heart_bitmap);
    lcd.clear();
    lcd_print_level();
    lcd_print_score_and_life();
    Serial.println("Done");
}

void init_bluetooth() {
    Serial.print("Initializing BT...");
    BT.begin(9600);
    Serial.println("Done");
}

/////////////////////////////////////////////////////////////
void flash_lead_strips() {
  for (uint16_t j = 0; j < LED_STRIP_NUM_LEDS; j++) {
      leds[j] = CRGB::Blue;
      FastLED.show();
      delay(5);
    }
    delay(3000);
    FastLED.clear();
    FastLED.show();
}

void flash_rfid_leds() {
    for (int i = 0; i < RFIDS_NUM; i++) {
        toggle_green_led(i, ON);
        toggle_red_led(i, ON);
        delay(200);
    }
    delay(3000);
    for (int i = 0; i < RFIDS_NUM; i++) {
        toggle_green_led(i, OFF);
        toggle_red_led(i, OFF);
        delay(200);
    }
}

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
            if (life == 0) {
                end_game();
                return;
            }
        } for (uint16_t j = LED_STRIP_NUM_LEDS - 1; j > 0; j--) {
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

// void read_rfid(uint8_t rfid_index) {
//     uint8_t i = rfid_index;
//     rfids[i].PICC_ReadCardSerial();
//     Serial.println("RFID number " + String(i) + " is reading.");
//     Serial.print(F("PICC type: "));
//     MFRC522::PICC_Type piccType = rfids[i].PICC_GetType(rfids[i].uid.sak);
//     Serial.println(rfids[i].PICC_GetTypeName(piccType));

//     unsigned long int uid = 0;
//     Serial.print(F("Scanned PICC's UID: "));
//     for (int j = 0; j < rfids[i].uid.size; j++) {
//         uid = (uid << 8) + rfids[i].uid.uidByte[j];
//     }
//     Serial.println(uid, HEX);
// }

// void track_rfid_card(uint8_t rfid_index) {
//     const uint8_t i = rfid_index;
//     VIRTUAL_DELAY(
//         rfid_last_sample_time[i], RFID_SAMPLE_SPEED,
// //        rfids[i].PICC_Select();
//         if (rfids[i].PICC_ReadCardSerial() || rfids[i].PICC_IsNewCardPresent()) {
//             read_rfid(i);
//             shoot_speeds[i] = 100; // for now hard coded (should be set according to the rfid card placed)
//             toggle_green_led(i, ON);
//         } else {
//             shoot_speeds[i] = 0;
//             toggle_green_led(i, OFF);
//             toggle_red_led(i, OFF);
//         });
// }

void enable_tower(uint8_t rfid_index, int8_t tower_num) {
  shoot_speeds[rfid_index] = tower_speeds[tower_num] - tower_bonus_speeds[tower_num] * tower_levels[tower_num];
  shoot_damage[rfid_index] = tower_damages[tower_num] + tower_bonus_damage[tower_num] * tower_levels[tower_num];
  toggle_green_led(rfid_index, ON);
}
void disable_tower(uint8_t rfid_index) {
    shoot_speeds[rfid_index] = 0;
    shoot_damage[rfid_index] = 0;
    toggle_green_led(rfid_index, OFF);
    toggle_red_led(rfid_index, OFF);
}

int track_rfid_card(uint8_t rfid_index) {
    const uint8_t i = rfid_index;
    rfid.begin((uchar)RFID_IRQ_PIN, (uchar)RFID_SCK_PIN, (uchar)RFID_MOSI_PIN, (uchar)rfid_miso_pins[i], (uchar)RFID_SS_PIN, (uchar)RFID_RST_PIN);
    rfid.init();
    rfid.antennaOn();
    uchar status;
    uchar str[MAX_LEN];
    // Search card, return card types
    status = rfid.request(PICC_REQIDL, str);
    if (status != MI_OK) {
      disable_tower(rfid_index);
      return;
    }
    rfid.showCardType(str);
    status = rfid.anticoll(str);
    if (status == MI_OK) {
        uchar serialNumber[5];
        memcpy(serialNumber, str, 5);
        rfid.showCardID(serialNumber);
        int8_t tower_num = get_tower_num(serialNumber);
        Serial.println(); Serial.println("Read Tower: " + String(tower_num));
        if (tower_num != -1) {
          enable_tower(rfid_index, tower_num);
        } else {
            Serial.println("Could not read tower ID");
        } 
    } else {
      disable_tower(rfid_index);
    }
    rfid.antennaOff();
    rfid.halt();
}

int8_t get_tower_num(char id [4]) {
    uint32_t card_id = 0;
    for(int i = 0; i < 4; i++){
        card_id <<= 8;
        card_id += 0xFF & id[i];
    }
    for (int i = 0; i < TOWERS_NUM; i++) {
        if (tower_ids[i] == card_id) return i;
    }
    return -1;
}

void track_rfid_cards_loop() {
    for (int i = 0; i < RFIDS_NUM; i++) {
      VIRTUAL_DELAY(rfid_last_sample_time[i], RFID_SAMPLE_SPEED, track_rfid_card(i););
    }
}

void update_score() {
    score += level;
    lcd_print_score_and_life();
    send_score();
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
                if (leds_hp[shoot_index] > 0) {
                    toggle_red_led(i, ON);
                    leds_hp[shoot_index] -= shoot_damage[i];  // this needs to be decreased according to tower's damage
                    Serial.println("Shooting index: " + String(shoot_index) + " HP droped: " + String(leds_hp[shoot_index] + 1) + " -> " + String(leds_hp[shoot_index]));
                    leds[shoot_index] = get_hp_color(shoot_index);
                    FastLED.show();
                    if (leds_hp[shoot_index] == 0) {
                        update_score();
                    }
                    found_target = true;
                    break;
                }
            }
        } if (!found_target) toggle_red_led(i, OFF););
}

void try_to_shoot_loop() {
    for (int i = 0; i < RFIDS_NUM; i++) {
        try_to_shoot(i);
    }
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

void wait_for_bt_connection() {
    while (true) {
        if (BT.available()) {
            if (BT.read() == START_MESSAGE) {
                game_started = true;
                break;
            }
        }
    }
}

void check_bluetooth_messages() {
    while (BT.available()) {
        char message = BT.read();
        if (message == END_MESSAGE) {
          end_game();
        } else if ((uint8_t)(message - '0') < TOWERS_NUM) {
          // expecting to receive the tower number that leveled up
          uint8_t tower_num = message - 48;
          tower_levels[tower_num]++;
          Serial.println("Upgraded tower number " + String(tower_num) + " to level " + String(tower_levels[tower_num]));
        }
    }
}

void send_score() {
    BT.println(score);
}

void end_game() {
    game_started = false;
    BT.println(END_MESSAGE);
}

void setup() {
    delay(PRE_SETUP_DELAY);
    Serial.begin(BAUD_RATE);
    init_led_strip();
    //init_rfids();
    //init_lcd_display();
    //init_bluetooth();
    Serial.println("Finished initialization.");
    delay(POST_SETUP_DELAY);
    Serial.println("Starting game!");
}

void loop() {
    if (!game_started) {
        //wait_for_bt_connection();
        game_started = true;
    } else {
        next_level_loop();
        track_rfid_cards_loop();
        move_leds();
        try_to_shoot_loop();
        //check_bluetooth_messages();
    }
}
