# 🚀 Space Planet Mining Game — 5 Günlük Faz Planı

## Proje Özeti
- **Oyun:** Uzay gezegen madenciliği — Unblocking + Sorting hibrit mekanik
- **Platform:** Unity 2D, Mobile-ready
- **Araçlar:** VSCode + Claude Code + Unity
- **Seviye sayısı:** 2
- **Teslim formatı:** GitHub repo + APK/WebGL build + Designer's Note

---

## 📋 Genel Task Takip Tablosu

| ID | Faz | Task | Durum |
|----|-----|------|-------|
| 1.1 | Gün 1 | Unity proje kurulumu (2D, mobil ayarlar) | ⬜ | Done
| 1.2 | Gün 1 | Klasör yapısı oluştur (Scripts, Prefabs, Scenes, Art, Audio, Data) | ⬜ | Done
| 1.3 | Gün 1 | Temel veri yapılarını oluştur (OreBlock, CargoShip, GridCell) | ⬜ | Done (Review)
| 1.4 | Gün 1 | Grid sistemi: GridManager — NxM grid oluşturma ve hücre yönetimi | ⬜ | Done
| 1.5 | Gün 1 | Ore yerleştirme: Renk dağılımı ve grid'e ore atama | ⬜ | Done
| 1.6 | Gün 1 | Grid görselleştirme: Renkli bloklar olarak grid'i render etme | ⬜ | Done
| 1.7 | Gün 1 | "Availability" sistemi: Dış yüze açık blokları hesaplama | ⬜ | Done
| 2.1 | Gün 2 | CargoShip veri modeli (renk, mining gücü/blok sayısı) | ⬜ | Done
| 2.2 | Gün 2 | Cargo queue sistemi: Aşağıdan gelen gemi kuyruğu (3 görünür sıra) | ⬜ | Done
| 2.3 | Gün 2 | Cargo slot UI: 4-5 aktif slot, doluluk gösterimi | ⬜ | Done
| 2.4 | Gün 2 | Tap-to-select: Kuyruktan gemiye dokunarak slot'a yerleştirme | ⬜ | Done
| 2.5 | Gün 2 | Drone gönderim mekaniği: Slottaki gemi → eşleşen available ore'a drone gönder | ⬜ | Done
| 2.6 | Gün 2 | Mining animasyonu: Drone gidiş + ore kırılma + geri dönüş | ⬜ | Done
| 2.7 | Gün 2 | Grid güncelleme: Mine edilen blok kaldırılınca komşuların available olması | ⬜ | Done
| 3.1 | Gün 3 | Slot doluluk flag fonksiyonu (tüm slotlar dolu mu?) | ⬜ | Done
| 3.2 | Gün 3 | ANA FONKSİYON: Slottaki gemilerin erişilebilir ore kontrolü | ⬜ | Done
| 3.3 | Gün 3 | Game Over koşulu: Slotlar dolu + hiçbir gemi erişemiyorsa → Game Over | ⬜ | Done 
| 3.4 | Gün 3 | Level Cleared koşulu: Tüm gemiler başarıyla gönderildi → Başarı | ⬜ | Done
| 3.5 | Gün 3 | Level 1 tasarımı: Küçük grid (örn. 6x4), 2-3 renk, tutorial akışı | ⬜ | Done
| 3.6 | Gün 3 | Level veri formatı: JSON/ScriptableObject ile level tanımlama | ⬜ | Done
| 3.7 | Gün 3 | Basit tutorial overlay: Level 1'de adım adım yönlendirme | ⬜ | Done
| 4.1 | Gün 4 | Level 2 tasarımı: Büyük grid (12x7-8), 4-5 renk, daha fazla gemi | ⬜ | Done
| 4.2 | Gün 4 | Level 2 zorluk: Daha karmaşık renk dağılımı, daha az slot marjı | ⬜ | Done
| 4.3 | Gün 4 | Power-up sistemi: "Clear Path" veya "Color Change" (monetize noktası) | ⬜ | 
| 4.4 | Gün 4 | Monetization placeholder: Level 2'de bottleneck anında rewarded ad butonu | ⬜ | 
| 4.5 | Gün 4 | UI: Ana menü, level seçim ekranı, HUD (skor, kalan gemi sayısı) | ⬜ | 
| 4.6 | Gün 4 | Ses efektleri: Ore kırılma, drone gönderim, game over, level clear | ⬜ | 
| 4.7 | Gün 4 | Görsel efektler: Parçacık efekti (mining), ekran shake, renk flash | ⬜ | 
| 5.1 | Gün 5 | Sahne geçişleri ve akış: Menü → Level 1 → Level 2 → Sonuç | ⬜ | Done
| 5.2 | Gün 5 | Mobil dokunma optimizasyonu ve UI scaling | ⬜ | 
| 5.3 | Gün 5 | Build alma: WebGL ve/veya APK | ⬜ | 
| 5.4 | Gün 5 | Bug fix ve playtest turu | ⬜ | 
| 5.5 | Gün 5 | README.md + Designer's Note yazımı | ⬜ | 
| 5.6 | Gün 5 | GitHub repo düzenleme ve son push | ⬜ | 

---

## Gün 1 — Temel Altyapı & Grid Sistemi

**Hedef:** Projeyi ayağa kaldır, gezegen grid'ini çalışır halde gör.

### Task 1.1 — Unity Proje Kurulumu
- Yeni Unity 2D proje oluştur
- Platform ayarını Android/iOS olarak ayarla
- Ekran çözünürlüğünü mobil orana getir (9:16 veya 9:19.5)
- **Claude Code notu:** `unity-project` klasörü oluştur, `.gitignore` ekle

### Task 1.2 — Klasör Yapısı
```
Assets/
├── Scripts/
│   ├── Core/           # GridManager, GameManager, LevelManager
│   ├── Data/           # ScriptableObject tanımları
│   ├── Ship/           # CargoShip, CargoQueue, CargoSlot
│   ├── Mining/         # DroneController, OreBlock
│   ├── UI/             # MenuUI, HUD, GameOverPanel
│   └── Utils/          # Helpers, Extensions
├── Prefabs/
├── Scenes/             # MainMenu, Level1, Level2
├── Art/
│   ├── Sprites/
│   └── Particles/
├── Audio/
│   ├── SFX/
│   └── Music/
├── Data/               # Level JSON/ScriptableObject dosyaları
└── Plugins/
```

### Task 1.3 — Temel Veri Yapıları
```
OreBlock:
  - color (enum: Red, Blue, Green, Yellow, Purple)
  - gridPosition (Vector2Int)
  - isAvailable (bool)
  - isMined (bool)

CargoShip:
  - color (OreColor)
  - miningPower (int) → kaç blok mine edebilir
  - dronesRemaining (int)

GridCell:
  - position (Vector2Int)
  - oreBlock (OreBlock veya null)
  - neighbors (List<GridCell>)
```

### Task 1.4 — GridManager
- NxM boyutunda grid oluşturma
- Her hücrenin komşularını hesaplama (4-yönlü: yukarı, aşağı, sol, sağ)
- Grid'i sahneye yerleştirme (kamera merkezli)

### Task 1.5 — Ore Yerleştirme Sistemi
- Level verisinden renk dağılımını oku
- Grid hücrelerine renkleri ata
- **KRİTİK KURAL:** Toplam ore sayısı = toplam gemi mining gücü toplamı (eşitlik kontrolü)

### Task 1.6 — Grid Görselleştirme
- Her ore bloğu için sprite/kare oluştur
- Renge göre renklendirme
- Available olan blokları parlak, olmayanları koyu göster

### Task 1.7 — Availability Sistemi
- Grid'in dış kenarlarındaki bloklar başlangıçta available
- Bir blok mine edilince, komşularından grid dışına "path" olan bloklar available olur
- Flood-fill veya BFS ile availability güncelleme

**Gün 1 sonu kontrol:** Grid ekranda görünüyor, renkler doğru, dış kenar blokları "available" olarak işaretli.

---

## Gün 2 — Gemi Sistemi & Mining Mekaniği

**Hedef:** Gemileri yerleştir, drone'ları gönder, ore mine et.

### Task 2.1 — CargoShip Veri Modeli
- ScriptableObject veya class olarak tanımla
- Renk + mining gücü (drone sayısı)
- Prefab: Renkli gemi görseli + üzerinde sayı

### Task 2.2 — Cargo Queue
- Ekranın altında gemi kuyruğu
- İlk 3 sıra görünür, geri kalanı "ekran dışında"
- Queue'dan çekilen gemi yerini bir sonrakine bırakır (kayma animasyonu)

### Task 2.3 — Cargo Slot UI
- Ekranın alt-orta bölgesinde 4-5 yuvarlak/kare slot
- Slot durumları: Boş, Dolu (gemi var), Aktif (drone gönderiyor)
- Doluluk göstergesi (renk veya ikon)

### Task 2.4 — Gemi Yerleştirme (Touch Input)
- Kuyruktan bir gemiye dokun → seçili olur (highlight)
- Boş bir slot'a dokun → gemi slot'a yerleşir
- Slot doluysa → yerleştirme reddedilir (hafif shake animasyonu)
- **Alternatif:** Drag & drop da düşünülebilir ama tap-tap daha basit başlangıç için

### Task 2.5 — Drone Gönderim Mekaniği
- Slot'a yerleşen gemi otomatik drone göndermeye başlar
- Drone hedefi: Geminin rengiyle eşleşen en yakın "available" ore bloğu
- Eğer eşleşen available ore yoksa → drone gönderilmez, gemi bekler
- Her drone bir ore bloğu mine eder
- Gemi tüm drone'larını gönderince slot'tan kalkar (slot boşalır)

### Task 2.6 — Mining Animasyonu
- Drone: Slot'tan ore'a doğru uçuş (basit lerp veya DOTween)
- Ore kırılma: Scale down + parçacık efekti (placeholder)
- Drone dönüş: Ore'dan gemiye geri uçuş
- **Dopamine boost:** Her mine'da hafif bir "pop" hissi

### Task 2.7 — Grid Dinamik Güncelleme
- Mine edilen blok grid'den kaldırılır
- Komşu blokların availability'si yeniden hesaplanır (Task 1.7'deki BFS)
- Yeni available olan bloklar görsel olarak güncellenir (koyu → parlak geçişi)

**Gün 2 sonu kontrol:** Gemiyi slot'a koyabiliyorsun, drone gidip ore mine ediyor, grid güncelleniyor.

---

## Gün 3 — Oyun Mantığı & Level 1

**Hedef:** Oyun kazanılabilir/kaybedilebilir hale gelsin, Level 1 oynanabilir olsun.

### Task 3.1 — Slot Doluluk Flag Fonksiyonu
```
bool AreAllSlotsFull()
  → Tüm slotları kontrol et
  → Hepsi doluysa true döndür
  → Her slot değişiminde çağrılır
```

### Task 3.2 — ANA FONKSİYON: Erişilebilirlik Kontrolü
```
bool CanAnyShipMine()
  → Her dolu slot'taki geminin rengini al
  → O renkte available ore var mı kontrol et
  → En az bir gemi gönderebiliyorsa true döndür

Bu fonksiyon şu anlarda çağrılır:
  - Bir gemi slot'a yerleştiğinde
  - Bir drone mining tamamladığında (grid güncellenince)
  - Queue'dan yeni gemi çekildiğinde
```

### Task 3.3 — Game Over Kontrolü
```
Eğer:
  AreAllSlotsFull() == true
  VE CanAnyShipMine() == false
  VE Queue'da hâlâ gemi var
→ GAME OVER

Not: AreAllSlotsFull() true olsa bile, slottaki gemilerden biri
mine edebiliyorsa oyun devam eder. Mine ettikçe slot boşalır,
yeni gemi gelir. Bu yüzden ANA FONKSİYON false positive'leri
engeller.
```

### Task 3.4 — Level Cleared Kontrolü
```
Eğer:
  Queue boş
  VE Tüm slotlar boş (tüm gemiler görevini tamamladı)
  VE Grid'de mine edilmemiş ore kalmadı (opsiyonel ek kontrol)
→ LEVEL CLEARED
```

### Task 3.5 — Level 1 Tasarımı
- **Grid:** 6x4 (24 blok) — küçük ve öğretici
- **Renkler:** 2-3 renk (Kırmızı, Mavi, belki Yeşil)
- **Gemiler:** 6-8 gemi, her biri 3-4 mining gücü
- **Slotlar:** 4 slot
- **Amaç:** Mekanik öğretme, kaybetmesi zor ama mümkün
- **Renk dağılımı:** Dış katmanlar karışık, iç katmanlar belirli — oyuncuyu doğru sıraya teşvik

### Task 3.6 — Level Veri Formatı
```json
{
  "levelId": 1,
  "gridWidth": 6,
  "gridHeight": 4,
  "slotCount": 4,
  "visibleQueueRows": 3,
  "oreGrid": [
    ["R","B","R","G","B","R"],
    ["B","R","G","R","G","B"],
    ["R","G","B","B","R","G"],
    ["G","B","R","G","B","R"]
  ],
  "shipQueue": [
    {"color":"R","power":3},
    {"color":"B","power":4},
    {"color":"G","power":2},
    ...
  ]
}
```
- LevelData ScriptableObject veya JSON olarak sakla
- LevelManager: Level verisini yükle → GridManager ve QueueManager'a aktar

### Task 3.7 — Tutorial Overlay (Level 1)
- Basit panel/oklar ile adım adım:
  1. "Aşağıdaki gemilere dokun" → Queue'yu highlight et
  2. "Bir slot'a yerleştir" → Slotları highlight et
  3. "Drone'lar otomatik gidecek" → Grid'deki hedefi göster
  4. "Tüm gemileri gönder!" → Dismiss
- Sadece Level 1'de göster, Level 2'de gösterme

**Gün 3 sonu kontrol:** Level 1 baştan sona oynanabilir. Kazanma ve kaybetme çalışıyor.

---

## Gün 4 — Level 2, Polish & Monetization

**Hedef:** Level 2 hazır, power-up var, oyun "juicy" hissettiriyor.

### Task 4.1 — Level 2 Grid Tasarımı
- **Grid:** 12x7 veya 12x8 (~84-96 blok)
- **Renkler:** 4-5 renk
- **Gemiler:** 15-20+ gemi
- **Slotlar:** 4-5 slot
- İç katmanlarda "tuzak" renk dizilimleri (oyuncuyu düşünmeye zorla)

### Task 4.2 — Level 2 Zorluk Ayarı
- Renk dağılımı kasıtlı olarak sıralama gerektiren şekilde
- Bazı renklerin ore'ları grid'in derininde → önce dış katmanları temizleme gereksinimi
- Queue sıralaması oyuncuyu "yanlış gemiyi erken koymaya" teşvik edecek şekilde
- **Dengeleme:** Mümkün ama zorlayıcı olmalı — test ederek ayarla

### Task 4.3 — Power-Up: "Asteroid Drill" (Clear Path)
- Herhangi bir renk bloğunu 1 tane patlatır (dış yüze açık olması gerekmez)
- Sınırlı kullanım: Level başına 1-2 adet ücretsiz
- Monetize edilebilir: Ekstra drill satın alma veya rewarded ad ile kazanma
- UI: Ekranın köşesinde drill ikonu + kalan sayı

### Task 4.4 — Monetization Placeholder
- Level 2'de "bottleneck" anında (slotlar dolu + erişim yok durumuna yaklaşırken):
  - Popup: "Sıkıştın mı? Bir reklam izleyerek ekstra slot aç!" butonu
  - Sadece UI placeholder — gerçek ad SDK entegrasyonu yok
  - README'de bu noktayı açıkla
- Opsiyonel: "Undo" butonu (son hamleni geri al) — rewarded ad ile

### Task 4.5 — UI Ekranları
- **Ana Menü:** Oyun logosu + "Play" butonu + Level seçim
- **Level Seçim:** Level 1 (açık), Level 2 (Level 1'den sonra açılır veya her zaman açık)
- **HUD:** Kalan gemi sayısı, aktif slot durumu, power-up butonları
- **Game Over Panel:** "Tekrar Dene" + "Ana Menü" butonları
- **Level Clear Panel:** Yıldız/skor + "Sonraki Level" butonu
- **Uzay teması:** Koyu arka plan, yıldızlar, hafif parlama efektleri

### Task 4.6 — Ses Efektleri
Aşağıdaki ücretsiz kaynaklardan bulunabilir:
- freesound.org, mixkit.co, kenney.nl/assets
- **Gereken sesler:**
  - Gemi seçme (kısa "bip")
  - Slot'a yerleştirme ("dock" sesi)
  - Drone gönderim ("whoosh")
  - Ore mine etme ("crunch/crack")
  - Game Over (düşük tonlu alarm)
  - Level Clear (fanfar/zafer sesi)
  - Power-up kullanımı (güçlü efekt sesi)

### Task 4.7 — Görsel Polish (Juice)
- **Ore kırılma:** Parçacık sistemi (rengi ore renginde)
- **Gemi yerleştirme:** Hafif bounce/scale animasyonu
- **Slot dolunca:** Slotlar kırmızıya döner (uyarı)
- **Available ore:** Hafif pulse/glow animasyonu
- **Game Over:** Ekran kırmızı flash
- **Level Clear:** Confetti/yıldız parçacıkları
- **Arka plan:** Yavaş kayan yıldızlar (parallax)

**Gün 4 sonu kontrol:** İki level oynanabilir, ses ve görsel efektler yerinde, power-up çalışıyor.

---

## Gün 5 — Entegrasyon, Build & Teslim

**Hedef:** Her şey bağlı, build alınmış, GitHub'a yüklenmiş.

### Task 5.1 — Sahne Akışı
- MainMenu → Level1 → (Başarı) → Level2 → (Başarı) → Tebrik ekranı
- Her sahneden geri dönüş (Ana Menü butonu)
- Sahne geçiş efekti (basit fade in/out)

### Task 5.2 — Mobil Optimizasyon
- Touch input'ları test et (multi-touch sorunları)
- UI elemanları parmak boyutuna uygun (min 44x44 px touch target)
- Farklı ekran oranlarında UI scaling (Canvas Scaler ayarları)
- Performans: Parçacık limitleri, batching kontrolleri

### Task 5.3 — Build Alma
- **WebGL:** Hızlı test için (tarayıcıda çalışır, link paylaşılabilir)
- **APK:** Android build (Player Settings: paket adı, versiyon, minimum API)
- Build'ları test et, crash/bug kontrol

### Task 5.4 — Playtest & Bug Fix
- Her iki level'ı en az 3 kez oyna
- Edge case'leri test et:
  - Tüm slotları doldur → Game Over tetikleniyor mu?
  - Hızlı tap spam → crash var mı?
  - Level 1'de tutorial düzgün mü?
  - Power-up doğru çalışıyor mu?
- Bulunan bugları düzelt

### Task 5.5 — README & Designer's Note
```markdown
# Space Mining Puzzle

## Nasıl Oynanır
[Kısa açıklama]

## Designer's Note
[1 paragraf: Neden bu mekaniği seçtin, unblocking+sorting nasıl birleşiyor]

## Monetization & Level Design
[Level 2'deki bottleneck noktaları ve monetization fırsatları]

## Teknik Notlar
[Kullanılan araçlar, mimari kararlar]

## Build
- WebGL: [link]
- APK: [link veya releases]
```

### Task 5.6 — GitHub Repo
- Repo oluştur, `.gitignore` kontrol
- Son commit'leri düzenle
- README'yi finalize et
- Release olarak APK/WebGL build'ı ekle

**Gün 5 sonu kontrol:** Repo hazır, build çalışıyor, README yazılmış, teslime hazır.

---

## ⚠️ Claude Code ile Çalışma Notları

### Dosya Oluşturma Stratejisi
Claude Code tüm script dosyalarını oluşturacak. Her gün başında o günün task'larına göre dosyaları sırayla oluştur. Unity'de sadece şunları elle yapman gerekecek:
- Sahne oluşturma ve GameObject hiyerarşisi
- Prefab'lara script atama (drag & drop)
- Sprite/asset import etme
- Build ayarları

### Claude Code'a Verilecek Promptlar İçin İpuçları
- Her task'ı tek tek ver, birden fazla script'i aynı anda isteme
- "Bu script MonoBehaviour olacak ve şu public field'lara sahip olacak" gibi Unity-spesifik talimatlar ekle
- Her script'ten sonra "bu script hangi GameObject'e eklenmeli" diye sor
- Hata aldığında Console log'unu direkt yapıştır

### Riskler & B Planları
| Risk | B Planı |
|------|---------|
| Availability BFS çok karmaşık gelirse | Sadece dış kenar available olsun, iç bloklar için basit komşu kontrolü yap |
| Drone animasyonu zaman alırsa | Animasyon yerine instant mine (görsel placeholder) |
| Level 2 dengeleme tutmazsa | Level 1'in büyütülmüş versiyonu olarak git |
| Ses bulma zaman alırsa | Unity'nin varsayılan sesleri veya sessiz bırak, son gün ekle |
| Tutorial çok vakit alırsa | Sadece 1 satır metin göster: "Gemilere dokunarak slot'a yerleştir!" |

---

## 🎯 Günlük "Done" Kriterleri

| Gün | Minimum "Done" Kriteri |
|-----|----------------------|
| 1 | Grid ekranda, renkli bloklar görünüyor, available olanlar belirgin |
| 2 | Gemi seçip slot'a koyabiliyorsun, drone mine ediyor, grid güncelleniyor |
| 3 | Level 1 oynanabilir, kazanma/kaybetme çalışıyor |
| 4 | Level 2 oynanabilir, ses+efektler var, power-up çalışıyor |
| 5 | Build alınmış, GitHub'da, README hazır |
