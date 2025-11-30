# 🎡 Wheel of Fortune Game

Unity tabanlı çarkıfelek oyunu. Oyuncular çarkı çevirerek farklı ödüller kazanır, bombalara dikkat eder ve zone'lar ilerleyerek oyunu tamamlamaya çalışır.

## 📋 İçindekiler

- [Geliştiriciden](#-geliştiriciden)
- [Özellikler](#-özellikler)
- [Kurulum](#-kurulum)
- [Proje Yapısı](#-proje-yapısı)
- [Oyun Mekaniği](#-oyun-mekaniği)
- [Teknik Detaylar](#-teknik-detaylar)
- [Namespace Yapısı](#-namespace-yapısı)
- [Event Sistemi](#-event-sistemi)
- [UI Sistemi](#-ui-sistemi)
- [Scriptler](#-scriptler)
- [Geliştirme Notları](#-geliştirme-notları)

---

## Geliştiriciden Mesaj
**Bu README.md dosyası Claude Code agenti ile oluşturulmuştur.** Kod yazarken tabi ki AI'dan yardım aldım fakat bu sadece takılıp Unity Discussions, StackOverFlow, Reddit veye YouTube'da araştırıp da bulamadığım konularda yardım alarak devam etti. Namespaceler ile kodları tamamıyla ayırdım. Geliştirme sürecine başlamadan önce detaylı bir şekilde yapılacak şeyleri ChatGPT'yi mentorum olarak kullanarak ayarladığım için (Interface, Dosya yapısı, UI önerileri ...) sonradan ekstra büyük bir revize sürecim olmadı. Repoda commit olmama sebebi ise başlangıçta zaman zaman internet erişimim yoktu. Bu yüzden düzenli commitlere başlayamadım ve başlayamadığım için devam ettiremedim. Normalde Git Feature Workflow kullanarak çalışıyorum. her bir feature için branch açıp geliştirilen feature develop branchinde test edilir ve stabil versiyon mainde bulunur. 

## ✨ Özellikler

### 🎮 Oyun Mekaniği
- **Çark Sistemi:** 8 dilimli dinamik çark yapısı
- **Zone Sistemi:** 100 zone'a kadar ilerleyebilme
- **Ödül Sistemi:** Çeşitli ödül tipleri (Coin, Gem, Multiplier, Bonus Item, vb.)
- **Bomba Mekaniği:** Risk-reward dengesi
- **Revive Sistemi:** Video izleyerek veya coin harcayarak devam etme

### 🎯 Zone Tipleri
- **Normal Zone:** Standart oynanış (Bronze çark)
- **Safe Zone:** Her 5 zone'da bir - bomba yok (Silver çark)
- **Super Zone:** Her 30 zone'da bir - özel ödüller (Golden çark)

### 💎 Ödül Sistemi
- **Coin:** Temel para birimi
- **Gem:** Değerli para birimi
- **Multiplier:** Ödülleri çarpan (x2, x3, vb.)
- **Bonus Items:** Özel eşyalar (Armor Points, Magic Crystals, vb.)
- **Bomb:** Game over tetikleyen özel dilim

### 🎨 UI Özellikleri
- **Dinamik Zone Bar:** Geçerli zone'u ve çevresini gösteren carousel
- **Collected Items:** Toplanan ödülleri gösteren dinamik liste
- **Exit Popup:** Kazanılan ödülleri gözden geçirme ve çıkış yapma
- **Result Popup:** Her spin sonrası ödül gösterimi
- **Game Over Popup:** Bomba sonrası revive/trash seçenekleri

---

## 🚀 Kurulum

### Gereksinimler
- Unity 2021.3 veya üzeri
- TextMeshPro package
- DOTween package (opsiyonel - animasyonlar için)

### Adımlar

1. **Projeyi Aç:**
   ```bash
   Unity Hub > Add > Proje klasörünü seç
   ```

2. **Scene'i Aç:**
   ```
   Assets//Scenes/TestCaseScene.unity
   ```

3. **Play Mode:**
   - Play butonuna bas ve oynamaya başla!

---

## 📁 Proje Yapısı

```
Assets/
├── _Project/
│   ├── Scenes/              # Ana oyun scene'i
│   ├── Scripts/
│   │   ├── Core/            # Ana yönetici sınıflar
│   │   │   ├── GameManager.cs
│   │   │   ├── UIManager.cs
│   │   │   ├── AudioManager.cs
│   │   │   └── ZoneManager.cs
│   │   ├── Data/            # ScriptableObject ve data sınıfları
│   │   │   ├── RewardData.cs
│   │   │   ├── WheelSliceData.cs
│   │   │   └── ZoneWheelConfig.cs
│   │   ├── Events/          # Event-based sistem
│   │   │   └── GameEvents.cs
│   │   ├── Interfaces/      # Interface tanımlamaları
│   │   ├── Reward/          # Ödül sistemi
│   │   │   └── RewardManager.cs
│   │   ├── Wheel/           # Çark mekaniği
│   │   │   ├── WheelController.cs
│   │   │   ├── WheelSpinner.cs
│   │   │   └── WheelSlice.cs
│   │   └── UI/              # Tüm UI bileşenleri
│   │       ├── Screens/     # Ana ekran UI'ları
│   │       ├── Popups/      # Popup'lar
│   │       └── Components/  # Tekrar kullanılabilir UI bileşenleri
│   ├── Prefabs/             # Prefab'lar
│   ├── Resources/           # Runtime yüklenen kaynaklar
│   └── Art/                 # Görseller ve animasyonlar
└── TextMesh Pro/            # TMP kaynakları
```

---

## 🎮 Oyun Mekaniği

### Temel Oynanış

1. **Spin Button:** Çarkı çevir
2. **Çark Dönüşü:** Animasyonlu dönüş
3. **Ödül Kazanma:** Gelinen dilimin ödülünü kazan
4. **Zone İlerlemesi:** Her başarılı spin'den sonra bir zone ilerle

### Zone Sistemi

```
Zone 1-4:   Normal (Bronze Wheel)
Zone 5:     Safe Zone (Silver Wheel) - Bomba yok!
Zone 6-9:   Normal (Bronze Wheel)
Zone 10:    Safe Zone (Silver Wheel)
...
Zone 30:    Super Zone (Golden Wheel) - Özel ödüller!
```

### Bomba Mekaniği

**Bomba Geldiğinde:**
1. Game Over Popup açılır
2. Oyuncu 3 seçenek görür:
   - **Trash Button:** Tüm ödüller kaybolur, Zone 1'den başla
   - **Revive (Video):** Video izle, ödülleri koru, aynı zone'dan devam et
   - **Revive (90 Coin):** Coin harca, ödülleri koru, aynı zone'dan devam et

**Önemli:** Bomba geldiğinde ödüller hemen kaybolmaz! Sadece trash button'a basılırsa kaybedilir.

### Revive Sistemi

**Video Revive:**
- Video izlenmesi gerekir
- Ödüller korunur
- Aynı zone'dan devam edilir
- Auto-spin başlar

**Coin Revive:**
- 90 coin harcanır
- Ödüller korunur
- Aynı zone'dan devam edilir
- Auto-spin başlar

### Exit Sistemi

**Exit Button (Sol Menü):**
- Spinning sırasında disabled
- Basıldığında Exit Popup açılır
- Toplanan tüm ödüller gösterilir (gruplanmış)
- İtemler soldan sağa kayarak animasyonlu gelir

**Exit Popup İçinde:**
- **Continue:** Popup'ı kapat, oyuna devam et
- **Exit:** Tüm ödüller kaybolur, Zone 1'den başla

---

## 🔧 Teknik Detaylar

### Mimari Prensipler

**SOLID Prensipleri:**
- **Single Responsibility:** Her sınıf tek bir sorumluluğa sahip
- **Interface Segregation:** IGameState, IRewardable, ISpinnable interfaces
- **Dependency Inversion:** Event-based sistem ile loose coupling

**Design Patterns:**
- **Singleton Pattern:** GameManager, UIManager, AudioManager, ZoneManager, RewardManager
- **Observer Pattern:** Event sistemi (GameEvents)
- **Strategy Pattern:** Zone tipine göre farklı wheel konfigürasyonları
- **Object Pooling:** UI item'ları için (gelecek optimizasyon)

### Event-Driven Architecture

Tüm sistem event-based çalışır. Bu sayede:
- Bileşenler birbirinden bağımsız
- Test edilebilir
- Genişletilebilir
- Debug kolay

---

## 📦 Namespace Yapısı

Proje, kodun modüler ve organize olması için namespace'lere ayrılmıştır. Her namespace belirli bir sorumluluğu temsil eder.

### Ana Namespace Yapısı

```
WheelOfFortune
├── Core                  # Ana yönetici sistemler
├── Events                # Event sistemi
├── Data                  # Veri yapıları ve ScriptableObjects
├── Interfaces            # Interface tanımlamaları
├── Reward                # Ödül sistemi
├── Wheel                 # Çark mekaniği
├── Zone                  # Zone yönetimi
├── UI                    # UI base class'ları
│   ├── Screens          # Ana ekran UI'ları
│   ├── Popups           # Popup bileşenleri
│   └── Components       # Tekrar kullanılabilir UI parçaları
├── Utils                 # Yardımcı fonksiyonlar
└── Editor               # Unity Editor toolları
```

### Namespace Detayları

#### 🎯 `WheelOfFortune.Core`
**Amaç:** Oyunun temel yönetici sistemlerini içerir (Singleton pattern ile)

**Ana Sınıflar:**
- `GameManager` - Oyun akışı ve state yönetimi
- `UIManager` - Tüm UI elementlerinin koordinasyonu
- `AudioManager` - Ses ve müzik sistemi
- `ZoneManager` - Zone ilerlemesi ve zone type kontrolü

**Önemli Metodlar:**
```csharp
// GameManager
void ChangeState(GameState newState)
void RestartGame()
void HandleRevive()

// UIManager
void ShowResultPopup(RewardData reward)
void ShowGameOverPopup()
void UpdateCoinDisplay(int amount)
void OnVolumeSliderChanged(float value)

// AudioManager
void SetMasterVolume(float volume)
float GetMasterVolume()
void PlaySFX(AudioClip clip)
void ToggleMusic()

// ZoneManager
void NextZone()
void ResetToZoneOne()
bool IsSafeZone { get; }
bool IsSuperZone { get; }
```

**Kullanım Amacı:** Oyunun çekirdeğini oluşturur. Singleton pattern ile tek instance olarak çalışır ve diğer sistemleri koordine eder.

---

#### 📡 `WheelOfFortune.Events`
**Amaç:** Event-based mimari için merkezi event hub

**Ana Sınıflar:**
- `GameEvents` - Tüm oyun event'lerini içeren static class

**Önemli Event'ler:**
```csharp
// Wheel Events
static event Action OnWheelSpinStarted
static event Action<int> OnWheelSpinCompleted

// Reward Events
static event Action<RewardData> OnRewardCollected

// Zone Events
static event Action<int> OnZoneChanged
static event Action<int> OnSafeZoneEntered
static event Action<int> OnSuperZoneEntered

// Game State Events
static event Action OnGameOver
static event Action OnBombHit
static event Action OnGameRestart
```

**Kullanım Amacı:** Loose coupling sağlar. Sistemler birbirine bağımlı olmadan iletişim kurar. Observer pattern implementasyonu.

---

#### 📊 `WheelOfFortune.Data`
**Amaç:** Tüm veri yapılarını, enum'ları ve ScriptableObject'leri içerir

**Ana Sınıflar:**
- `RewardData` - Ödül bilgilerini tutar (ScriptableObject)
- `WheelSliceData` - Çark dilimi verisi
- `ZoneWheelConfig` - Zone bazlı çark konfigürasyonu (ScriptableObject)
- `ItemData` - Item bilgileri (ScriptableObject)
- `ZoneData` - Zone metadata

**Enum'lar:**
```csharp
enum RewardType { Coin, Gem, Multiplier, BonusItem, Bomb }
enum ItemType { Armor, MagicCrystal, HealthPotion, ... }
enum ItemRarity { Common, Rare, Epic, Legendary }
enum GameState { Idle, Spinning, ShowingResult, GameOver }
```

**Kullanım Amacı:** Oyun verilerini kod mantığından ayırır. Designer'ların kod değiştirmeden değer ayarlamasını sağlar.

---

#### 🔌 `WheelOfFortune.Interfaces`
**Amaç:** Sözleşme (contract) tanımlamaları

**Ana Interface'ler:**
```csharp
interface IGameState
{
    void EnterState();
    void ExitState();
    void UpdateState();
}

interface IRewardable
{
    void CollectReward(RewardData reward);
}

interface ISpinnable
{
    void Spin();
    void StopSpin();
}

interface IZoneProgressable
{
    void AdvanceZone();
    void ResetProgress();
}
```

**Kullanım Amacı:** Polymorphism ve SOLID prensiplerini destekler. Bağımlılıkları interface'ler üzerinden yönetir.

---

#### 💎 `WheelOfFortune.Reward`
**Amaç:** Ödül toplama ve yönetim sistemi

**Ana Sınıflar:**
- `RewardManager` - Ödül tracking ve işleme (Singleton)
- `RewardItem` - Ödül UI item representation

**Önemli Metodlar:**
```csharp
// RewardManager
void CollectReward(RewardData reward)
void LoseAllRewards()
void ResetRewards()
List<RewardData> GetCollectedRewards()
Dictionary<string, int> GetGroupedRewards()
int TotalCoinsCollected { get; }
int TotalGemsCollected { get; }
```

**Kullanım Amacı:** Ödülleri merkezi olarak yönetir, multiplier hesaplamalarını yapar, toplanan ödülleri gruplar.

---

#### 🎡 `WheelOfFortune.Wheel`
**Amaç:** Çarkın mekaniği ve görsel yapısı

**Ana Sınıflar:**
- `WheelController` - Çark konfigürasyonu ve dilim oluşturma
- `WheelSpinner` - Çark dönme animasyonu ve fizik
- `WheelSlice` - Tekil çark dilimi
- `WheelTestInput` - Test amaçlı input handler

**Önemli Metodlar:**
```csharp
// WheelController
void ConfigureWheelForZone(int zoneNumber)
void RegenerateWheel()
void SetWheelType(WheelType type)

// WheelSpinner
void StartSpin()
void SpinToSlice(int targetIndex)
int GetWinningSliceIndex()
```

**Kullanım Amacı:** Çarkın tüm görsel ve mekanik işlemlerini yönetir. Zone'a göre farklı config'ler yükler.

---

#### 🗺️ `WheelOfFortune.Zone`
**Amaç:** Zone sistemi yönetimi (alternatif implementasyon)

**Ana Sınıflar:**
- `ZoneManager` - Zone progression ve state

**Not:** Bu namespace Core'daki ZoneManager'a ek bir implementasyon. Projede her ikisi de mevcut.

---

#### 🎨 `WheelOfFortune.UI`
**Amaç:** UI sisteminin base class'ları

**Ana Sınıflar:**
- `UIPanel` - Tüm panel'lar için base abstract class

**Önemli Metodlar:**
```csharp
// UIPanel
virtual void Show()
virtual void Hide()
void SetInteractable(bool interactable)
```

**Kullanım Amacı:** UI elementleri için ortak davranışlar sağlar. Show/Hide animasyonları, CanvasGroup yönetimi.

---

#### 🖼️ `WheelOfFortune.UI.Screens`
**Amaç:** Ana oyun ekranlarının UI bileşenleri

**Ana Sınıflar:**
- `ZoneNumberBarUI` - Horizontal scrolling zone carousel
- `LeftSidebarUI` - Sol menü (coins, gems, exit button, collected items)
- `RightSidebarUI` - Sağ menü
- `WheelContainerUI` - Çark container yönetimi

**Önemli Metodlar:**
```csharp
// ZoneNumberBarUI
void InitializeBar(int startZone, int zoneCount)
void UpdateCurrentZone(int zoneNumber)

// LeftSidebarUI
void UpdateCoinAmount(int amount)
void UpdateCashAmount(int amount)
void AddCollectedItem(RewardData reward)
```

**Kullanım Amacı:** Oyun sırasında sürekli görünen UI elementlerini yönetir.

---

#### 🪟 `WheelOfFortune.UI.Popups`
**Amaç:** Popup/dialog sistemleri

**Ana Sınıflar:**
- `ResultPopup` - Spin sonucu gösterimi
- `GameOverPopup` - Bomba sonrası revive/trash seçenekleri
- `ExitPopup` - Çıkış ve ödül özeti
- `SafeZonePopup` - Safe zone bilgilendirme
- `DeveloperPopup` - Developer bilgileri
- `ReviveVideoPlayer` - Video oynatma kontrolü

**Önemli Metodlar:**
```csharp
// ResultPopup
void ShowResult(RewardData reward, Action onContinue, Action onCollect)

// GameOverPopup
void ShowGameOver(int coinsLost, int gemsLost)
void OnReviveWithVideo()
void OnReviveWithCoins()

// ExitPopup
void ShowExitPopup()
void PopulateCollectedItems()
```

**Kullanım Amacı:** Kullanıcıyla etkileşimli dialog'ları yönetir. Overlay panel'lar.

---

#### 🧩 `WheelOfFortune.UI.Components`
**Amaç:** Tekrar kullanılabilir UI parçaları

**Ana Sınıflar:**
- `UIButton` - Özelleştirilmiş button component
- `CollectedItemUI` - Toplanan item gösterimi
- `ZoneNumberItem` - Zone carousel'deki tekil item
- `ResponsiveUIHelper` - Responsive UI yardımcıları

**Önemli Metodlar:**
```csharp
// UIButton
void AddListener(UnityAction callback)
void RemoveListener(UnityAction callback)
void SetInteractable(bool interactable)

// CollectedItemUI
void SetupItem(Sprite icon, int amount)
void PlaySlideInAnimation()
```

**Kullanım Amacı:** Küçük, tekrar kullanılabilir UI parçacıklarını sağlar. Composition pattern.

---

#### 🛠️ `WheelOfFortune.Utils`
**Amaç:** Genel yardımcı fonksiyonlar

**Ana Sınıflar:**
- `ListExtensions` - List için extension metodlar

**Önemli Metodlar:**
```csharp
public static T GetRandomWeighted<T>(this List<T> list, Func<T, float> weightFunc)
```

**Kullanım Amacı:** Projenin farklı yerlerinde kullanılan genel amaçlı fonksiyonlar.

---

#### 🔧 `WheelOfFortune.Editor`
**Amaç:** Unity Editor için özel toollar

**Ana Sınıflar:**
- `ZoneConfigGenerator` - Zone config'lerini otomatik oluşturma

**Kullanım Amacı:** Developer workflow'unu hızlandırır. Editor-time toollar.

---

### Namespace Kullanım Prensipleri

**1. Single Responsibility (Tek Sorumluluk):**
Her namespace belirli bir domain'i temsil eder. Örneğin:
- `Core` → Oyun yönetimi
- `Wheel` → Sadece çark mekaniği
- `UI.Popups` → Sadece popup'lar

**2. Dependency Direction (Bağımlılık Yönü):**
```
UI → Core → Events ← Data
     ↓        ↑
   Wheel   Reward
```

**3. Loose Coupling:**
- Namespace'ler birbirine `Events` üzerinden bağlanır
- Interface'ler kullanılarak concrete implementasyonlardan bağımsız çalışır

**4. Layered Architecture:**
```
┌─────────────────────────────────┐
│    UI (Screens, Popups)         │  ← Presentation Layer
├─────────────────────────────────┤
│    Core (Managers)              │  ← Business Logic Layer
├─────────────────────────────────┤
│    Wheel, Reward, Zone          │  ← Domain Layer
├─────────────────────────────────┤
│    Data, Interfaces, Events     │  ← Foundation Layer
└─────────────────────────────────┘
```

---

## 📡 Event Sistemi

### Tüm Event'ler

```csharp
// Wheel Events
GameEvents.OnWheelSpinStarted
GameEvents.OnWheelSpinCompleted(int sliceIndex)

// Reward Events
GameEvents.OnRewardCollected(RewardData reward)

// Zone Events
GameEvents.OnZoneChanged(int newZoneNumber)
GameEvents.OnSafeZoneEntered(int zoneNumber)
GameEvents.OnSuperZoneEntered(int zoneNumber)

// Game State Events
GameEvents.OnGameOver
GameEvents.OnGameRestart
GameEvents.OnBombHit

// UI Events
GameEvents.OnResultPopupClosed(bool autoSpin)
GameEvents.OnGameExit
```

### Event Kullanımı

**Subscribe:**
```csharp
private void OnEnable()
{
    GameEvents.OnWheelSpinCompleted += HandleSpinCompleted;
}

private void OnDisable()
{
    GameEvents.OnWheelSpinCompleted -= HandleSpinCompleted;
}

private void HandleSpinCompleted(int sliceIndex)
{
    Debug.Log($"Spin completed at slice {sliceIndex}");
}
```

**Trigger:**
```csharp
GameEvents.TriggerWheelSpinCompleted(sliceIndex);
```

---

## 🎨 UI Sistemi

### UI Hierarchy

```
Canvas
├── TopBar (Zone display)
├── LeftSidebar
│   ├── ExitButton
│   └── CollectedItemsContainer
├── RightSidebar
├── WheelContainer
│   └── Wheel
├── BottomBar
│   └── SpinButton
└── Popups
    ├── ResultPopup
    ├── GameOverPopup
    ├── ExitPopup
    └── SafeZonePopup
```

### UI Components

**UIPanel (Base Class):**
- Tüm panel'lar için base sınıf
- Show/Hide animasyonları
- CanvasGroup yönetimi

**UIButton:**
- Özelleştirilmiş button component
- Click events
- Interactable kontrolü

**ZoneNumberBarUI:**
- Horizontal scrolling carousel
- Dinamik zone gösterimi
- Safe/Super zone renklendirme

**CollectedItemUI:**
- Dinamik item gösterimi
- Icon + Amount display
- Slide-in animasyonu

---

## 📜 Scriptler

### Core Scripts

#### GameManager.cs
**Sorumluluk:** Oyun akışı ve state yönetimi

**Özellikler:**
- Game state kontrolü (Idle, Spinning, ShowingResult, GameOver)
- State transition validasyonu
- Event yönetimi

**Önemli Metodlar:**
```csharp
void ChangeState(GameState newState)
bool CanTransitionTo(GameState targetState)
void RestartGame()
```

#### UIManager.cs
**Sorumluluk:** Tüm UI elementlerini yönetir

**Özellikler:**
- Popup yönetimi
- Zone display güncelleme
- Event dinleme

**Önemli Metodlar:**
```csharp
void ShowResultPopup(RewardData reward)
void ShowGameOverPopup()
void ShowExitPopup()
void UpdateZoneDisplay(int zoneNumber)
```

#### AudioManager.cs
**Sorumluluk:** Ses ve müzik yönetimi

**Özellikler:**
- SFX oynatma
- Müzik kontrolü
- Volume ayarları

#### ZoneManager.cs
**Sorumluluk:** Zone sistemi yönetimi

**Özellikler:**
- Zone tracking
- Safe/Super zone kontrolü
- Zone progression

**Önemli Metodlar:**
```csharp
void NextZone()
void ResetToZone(int zoneNumber)
void ResetToZoneOne()
bool IsSafeZone
bool IsSuperZone
```

### Wheel Scripts

#### WheelController.cs
**Sorumluluk:** Çark konfigürasyonu ve yönetimi

**Özellikler:**
- Slice oluşturma
- Zone'a göre config yükleme
- Wheel regeneration

**Önemli Metodlar:**
```csharp
void ConfigureWheelForZone(int zoneNumber)
void RegenerateWheel()
void SetWheelType(WheelType type)
```

#### WheelSpinner.cs
**Sorumluluk:** Çark animasyonu ve fizik

**Özellikler:**
- Dönüş animasyonu
- Slice seçimi
- Spin event'leri

### Reward Scripts

#### RewardManager.cs
**Sorumluluk:** Ödül tracking ve yönetimi

**Özellikler:**
- Reward collection
- Reward processing (coin, gem, multiplier)
- Reward grouping

**Önemli Metodlar:**
```csharp
void CollectReward(RewardData reward)
void LoseAllRewards()
void ResetRewards()
List<RewardData> GetCollectedRewards()
```

### UI Scripts

#### ExitPopup.cs
**Sorumluluk:** Exit popup yönetimi

**Özellikler:**
- Collected items display
- Item grouping (aynı item'ları toplama)
- Horizontal scroll
- Auto-scroll to end
- Slide-in animasyonlar

#### ResultPopup.cs
**Sorumluluk:** Spin sonuç gösterimi

**Özellikler:**
- Reward display
- Continue/Collect buttons
- Auto-spin toggle

#### GameOverPopup.cs
**Sorumluluk:** Game over ve revive sistemi

**Özellikler:**
- Trash button (zone 1'e dön, ödüller kaybolur)
- Revive video (video izle, devam et)
- Revive coin (90 coin, devam et)
- Video player entegrasyonu

---

## 🛠️ Geliştirme Notları

### Event System Best Practices

1. **Subscribe/Unsubscribe:** OnEnable/OnDisable içinde
2. **Memory Leak Prevention:** Mutlaka unsubscribe
3. **Null Check:** Event trigger'larda `?.Invoke()` kullan

### UI Best Practices

1. **Prefab Kullanımı:** Tekrar eden UI elementleri için
2. **Layout Groups:** Dinamik boyutlandırma için
3. **CanvasGroup:** Show/hide animasyonları için
4. **Anchor/Pivot:** Responsive tasarım için

### Performance Optimization

1. **Object Pooling:** Sık oluşturulan objeler için (future)
2. **Event Cleanup:** Memory leak önleme
3. **Layout Rebuild:** Sadece gerektiğinde `ForceRebuildLayoutImmediate`

---


---

## 👨‍💻 İletişim

Sorular veya öneriler için:
- **Developer:** [Kaan Avdan]
- **Email:** [kaanavdan01@gmail.com]
- **GitHub:** [https://github.com/k2ann00]

---

## 🙏 Teşekkürler

- Unity Technologies
- TextMeshPro team
- DOTween (Demigiant)

---

**Son Güncelleme:** 2025-01-30
**Version:** 1.0.0
