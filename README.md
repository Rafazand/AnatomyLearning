# AnatomyLearning — Aplikasi Edukasi Anatomi Tubuh Manusia (MR)

Aplikasi Mixed Reality (MR) berbasis Meta Quest untuk pembelajaran interaktif anatomi tubuh manusia. Dibangun dengan Unity 6 dan Meta XR SDK, aplikasi ini menyediakan dua mode belajar: **Explore** (eksplorasi bebas organ) dan **Quiz** (uji pemahaman).

---

## Fitur Utama

- **Mode Explore** — Sentuh dan pegang organ 3D untuk melihat nama, deskripsi, dan informasi edukatif secara langsung di ruang nyata
- **Mode Quiz** — Jawab pertanyaan dengan menaruh organ ke kotak jawaban; skor akhir ditampilkan di akhir sesi
- **Hand Controller Support** — Interaksi via kontroler Meta Quest dengan haptic feedback
- **Spatial Audio** — Suara 3D untuk setiap interaksi (grab, snap, benar/salah)
- **Data JSON** — Konten organ dan soal kuis dimuat dari file JSON, mudah diperbarui tanpa mengubah kode
- **Narasi** — Dukungan narasi audio

---

## Platform & Persyaratan

| Komponen | Versi |
|---|---|
| Unity Editor | 6000.0.62f1 |
| Meta XR SDK | 83.0.3 |
| Target Platform | Meta Quest 2 / 3 / 3S |
| Min Android API | Level 29 (Quest requirement) |
| Bahasa | C# |

---

## Struktur Project

```
AnatomyLearning/
├── Assets/
│   ├── Scripts/           # 17 script C# inti
│   ├── Scenes/            # SampleScene.unity (scene utama)
│   ├── Resources/         # Data JSON (organ & kuis)
│   ├── Sfx/               # Audio clips interaksi
│   ├── InteractionSDK/    # Meta Interaction SDK
│   └── Oculus/            # OVR-specific assets
├── Packages/              # Dependency packages
├── ProjectSettings/       # Konfigurasi project Unity
└── AnatomyLearning.apk    # APK siap install
```

---

## Arsitektur Script

### Manajemen Utama
| Script | Fungsi |
|---|---|
| `MenuManager.cs` | Navigasi antar mode (Menu, Explore, Quiz) |
| `NarrationSettings.cs` | Singleton toggle narasi audio global |

### Mode Explore
| Script | Fungsi |
|---|---|
| `BodyFrontInfoController.cs` | Menampilkan panel info organ dari JSON |
| `OrganLabelOnTouch.cs` | Menampilkan label saat organ disentuh |
| `LabelToggleCounter.cs` | Manajemen visibilitas label dengan reference counting |
| `OrganTouchFeedback.cs` | Visual feedback (label, outline) saat organ disentuh |

### Mode Quiz
| Script | Fungsi |
|---|---|
| `QuizManager.cs` | Memuat soal dari JSON, acak 10 soal, hitung skor |
| `QuizQuestionData.cs` | Data class untuk deserialisasi JSON soal kuis |
| `QuizSimple.cs` | Alternatif UI kuis sederhana pilihan ganda A-D |

### Sistem Interaksi Organ
| Script | Fungsi |
|---|---|
| `OrganId.cs` | ID unik untuk setiap organ |
| `MagnetSnapDual.cs` | Grab/release organ dengan animasi snap ke SnapZone |
| `SnapZone.cs` | Kotak jawaban: feedback hijau (benar) / merah (salah) + haptic |

### Visual & Audio
| Script | Fungsi |
|---|---|
| `InteractableSfx.cs` | Audio grab, release, snap, scale dengan cooldown |

---

## Data Konten

### `Assets/Resources/organ_data.json`
Berisi 19 entri organ tubuh manusia dalam Bahasa Indonesia:
- Paru-paru (kiri/kanan), Jantung, Hati, Lambung, Usus Halus, Usus Besar, Pankreas, Ginjal, Otak, Kantung Empedu, Trakea, Pembuluh Darah, dll.

```json
{
  "id": "heart",
  "title": "Jantung",
  "description": "Organ yang memompa darah ke seluruh tubuh..."
}
```

### `Assets/Resources/quiz_questions.json`
12+ soal kuis dalam Bahasa Indonesia, format:

```json
{
  "question": "Organ tubuh mana yang memompa darah?",
  "answerId": "heart"
}
```

---

## Cara Menjalankan

### Menggunakan APK (langsung install ke Quest)
1. Aktifkan **Developer Mode** di Meta Quest
2. Hubungkan Quest ke PC via USB
3. Jalankan perintah:
   ```bash
   adb install AnatomyLearning.apk
   ```

### Build dari Unity
1. Buka project di **Unity 6000.0.62f1**
2. Pastikan **Meta XR SDK v83** sudah terinstall via Package Manager
3. Buka `File > Build Settings`, pilih platform **Android**
4. Set **Texture Compression** ke ASTC
5. Klik **Build And Run** dengan Quest terhubung

### Konfigurasi XR
- Buka `Edit > Project Settings > XR Plug-in Management`
- Pastikan **Meta XR** diaktifkan untuk Android
- Verifikasi `Assets/Resources/OculusRuntimeSettings.asset` sudah terkonfigurasi

---

## Package Dependencies Utama

```
com.meta.xr.sdk.all              v83.0.3   (Meta XR lengkap)
com.unity.xr.openxr             v1.16.0
com.unity.xr.management         v4.5.4
com.unity.inputsystem            v1.14.2
com.unity.ugui                   v2.0.0
com.unity.timeline               v1.8.9
```

---

## Alur Aplikasi

```
[Menu Utama]
     |
     +---> [Mode Explore]
     |        - Organ 3D tampil di depan pengguna
     |        - Sentuh organ → label & info muncul
     |        - Bebas eksplorasi tanpa batas
     |
     +---> [Mode Quiz]
              - 10 soal diacak dari JSON
              - Drag organ ke kotak jawaban
              - Feedback langsung (hijau/merah + suara)
              - Skor akhir ditampilkan
```

---

## Kredit & Aset

- Audio SFX: Freesound community (pop, error, correct)
- Narasi: CapCut TTS (Timothy voice)
- SDK: Meta XR SDK — Meta Platforms, Inc.
- Unity Engine — Unity Technologies

---

## Lisensi

Project ini dibuat untuk keperluan edukasi. Seluruh model 3D organ dan konten edukatif merupakan aset project.
