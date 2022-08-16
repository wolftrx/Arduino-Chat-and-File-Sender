
#include <SPI.h>
#include <nRF24L01.h>
#include <RF24.h>
RF24 radio(8, 7); // CE, CSN
const byte addresses [][6] = {"00001", "00002"};    //İki adresin ayarlanması. Biri iletmek ve biri almak için
char b[32];
void setup() {
  Serial.begin(115200); // seri haberleşmeyi 115200 baudrate de başlatıyoruz.
  radio.begin();                            //Telsiz iletişiminin başlatılması
  radio.openWritingPipe(addresses[0]);      //Verileri göndereceğimiz adresin ayarlanması
  radio.openReadingPipe(1, addresses[1]);   //Verileri alacağımız adresin ayarlanması
  radio.setPALevel(RF24_PA_MIN);            //Verici ve alıcı arasındaki mesafeye göre minimum veya maksimum olarak ayarlayabilirsiniz.
}

void loop() 
{
  delay(5); //5 mikrosaniye bekliyoruz.
  radio.startListening();                    //Bu, modülü alıcı olarak ayarlar
  if (radio.available())                     //Eğer gelen veri varsa
  {
    radio.read(&b, 32); // gelen veriyi oku ve b değişkenine kaydet
    Serial.println(String(b)); //b char dizisini stringe çevir.
    delay(5); //5 mikrosaniye bekle
  }
  if(Serial.available()){ //Seri haberleşmeden eğer veri geldiyse
    String a=Serial.readString(); // gelen veriyi oku ve a değişkenine kaydet 
    a.trim(); // a stringinin başında ve sonunda boşluk veya "\n"(enter) varsa sil
    radio.stopListening();                           //Bu, modülü verici olarak ayarlar
    a.toCharArray(b,32);  //a string değişkenini char dizisine çevirir
    radio.write(&b, 32);   //b değişkeni gönderilir
    //Serial.println(a); // verinin gönderildiği seri monitöre yazdırılır.
    
  }
}
