#include <SPI.h>
#include <nRF24L01.h>
#include <RF24.h>
RF24 radio(8, 7); // CE, CSN
const byte addresses [][6] = {"00001", "00002"};  //alıcı ve verici adresleri

char b[32];
void setup() {
  Serial.begin(115200);
  radio.begin();                           //Telsiz iletişiminin başlatılması
  radio.openWritingPipe(addresses[1]);     //Verileri göndereceğimiz adresin ayarlanması
  radio.openReadingPipe(1, addresses[0]);  //Verileri alacağımız adresin ayarlanması
  radio.setPALevel(RF24_PA_MIN); //Verici ve alıcı arasındaki mesafeye göre minimum veya maksimum olarak ayarlayabilirsiniz.
}

void loop() 
{  
  delay(5);
  if(Serial.available()){
    //Eğer seri porttan veri gelmişse
    String a=Serial.readString();

    //Gelen Stringi oku ve a değişkeninde tut
    a.trim();

    // a stringinin başında ve sonunda bulunan boşlukları ve "\n"(enter) varsa sil
    a.toCharArray(b,32);
    // a stringini b char dizisine çevir. 
    radio.stopListening();         //Bu, modülü verici olarak ayarlar
    radio.write(&b, 32);  //veri gönderilir.
    //Serial.println(a);

    //Verinin gönderildiği seri monitöre yazdırılır.
    delay(5);

    //5 mikrosaniye beklenir.
  }

  
  radio.startListening();                            //Bu, modülü alıcı olarak ayarlar
  if(radio.available()){

    //nrf24l01 modülüne veri geldiyse
    radio.read(&b, 32); //veriyi oku
    Serial.println(String(b));

    // veriyi stringe çevir ve seri monitöre yazdır
  }

}
