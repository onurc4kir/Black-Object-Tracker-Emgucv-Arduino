// Kütüphaneler Eklendi
#include <LiquidCrystal_I2C.h>

#include <LiquidCrystal_I2C.h>

#include <Servo.h>

#include <virtuabotixRTC.h>

#include <SPI.h> // SPI Lıbrary

#include <SD.h> // SD Card Library


//servo variables names
Servo servo_sagsol;
Servo servo_altust;

File file;

//Chip Select For arduino
int pinCS = 10;

//CLK -> 2, Data -> 3, RST -> 4
virtuabotixRTC Clock(2, 3, 4);

LiquidCrystal_I2C lcd(0x27, 16, 2); // Bu kodu kullanırken ekranda yazı çıkmaz ise 0x27 yerine 0x3f yazınız !!

//Variables for servo angles
int pos1 = 80;
int pos2 = 80;

int comingData; //Store coming data from Serial Port
int angle = 1;

int saveLoop = 0;

bool isSDCardReaded = false;
void setup() // 1 kereye mahsus çalışır
{
  //Clock.setDS1302Time(57, 49, 10, 3, 13, 04, 2022);
  pinMode(3, OUTPUT);
  Serial.begin(9600); //C# ile haberleşme hızı
  servo_sagsol.attach(5); //Servo motorların Arduino'da hangi pine bağlı olduğunu tanımlama
  servo_altust.attach(6); //Servo motorların Arduino'da hangi pine bağlı olduğunu tanımlama
  servo_sagsol.write(pos1); //Başlangıçtaki servo açılarını 90 derece olarak ayarlama
  servo_altust.write(pos2); //Başlangıçtaki servo açılarını 90 derece olarak ayarlama

  //LCD başlatma
  lcd.begin();

  // SD Kart Modül Setup
  pinMode(pinCS, OUTPUT);
  if (SD.begin()) {
    lcd.print("SD Worked");
    isSDCardReaded = true;

    delay(1000); // 1 saniye isteğe bağlı bekleme 
  } else {
    lcd.print("SD is not working");
    delay(1000);

  }

}

void loop()

{
  if (isSDCardReaded) {
    if (saveLoop <= 50)
      saveLoop += 1;

    lcd.clear();
    lcd.setCursor(0, 0);

    Clock.updateTime();
    String date = (String) Clock.hours + ":" + Clock.minutes + "-" + Clock.dayofmonth + "/" + Clock.month + "/" + Clock.year;
    lcd.setCursor(1, 1);
    lcd.print(date);
    delay(30);

    char myStg[18];
    sprintf(myStg, "%d %d %d", pos1, pos2, saveLoop);
    lcd.setCursor(0, 0);
    lcd.print(myStg);

    if (Serial.available()) // If serial port is available
    {

      if (saveLoop >= 10) {

        file = SD.open("data.txt", FILE_WRITE);
        delay(30);
        if (file) {

          date.concat(" --- ");
          date.concat(myStg);

          file.println(date);
          lcd.clear();
          lcd.setCursor(0, 0);
          lcd.print("Saved");
          file.close();
          saveLoop = 0;
          delay(100);
        } else {
          saveLoop = 0;
          lcd.setCursor(0, 0);
          lcd.clear();
          lcd.print("Not Saved");
          file.close();
          delay(100);

        }

      }

      comingData = Serial.read(); //Read data from serial port

      switch (comingData) //if it is exists
      {

      case '0': //There is no object
        if (pos1 > 180) {
          pos1 = 180;
        }
        if (pos2 > 180) {
          pos2 = 180;
        }
        break;
      case '1': //pos1+ left, pos2- top

        pos1 += angle;
        pos2 -= angle;

        if (pos1 > 180) {
          pos1 = 180;
        }
        if (pos2 > 180) {
          pos2 = 180;
        }
        servo_sagsol.write(pos1);
        servo_altust.write(pos2);
        break;

      case '2': //pos1+ right pos2+ top
        pos1 -= angle;
        pos2 -= angle;

        if (pos2 > 180) {
          pos2 = 180;
        }

        servo_sagsol.write(pos1);
        servo_altust.write(pos2);
        break;

      case '3': // pos1- left, pos2- bottom

        pos1 += angle;
        pos2 += angle;

        if (pos1 < 0) {
          pos1 = 0;
        }
        servo_sagsol.write(pos1);
        servo_altust.write(pos2);
        break;

      case '4': // pos1+ right, pos2- bottom
        pos1 -= angle;
        pos2 += angle;

        if (pos1 > 180) {
          pos1 = 180;
        }
        if (pos2 < 0) {
          pos2 = 0;
        }
        servo_sagsol.write(pos1);
        servo_altust.write(pos2);
        break;

      case '5': // pos2- bottom
        pos2 -= angle;

        if (pos2 < 0) {
          pos2 = 0;
        }
        servo_sagsol.write(pos1);
        servo_altust.write(pos2);

        break;


      case '7': // Object centered

        if (pos1 < 0) {
          pos1 = 0;
        }
        if (pos2 < 0) {
          pos2 = 0;
        }
        break;

      }
    }
  }
}
